using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

namespace Common.Core
{
    /// <summary>
    /// У Rigidbody дочерние коллайдеры не получают колбеков OnCollision/TriggerEnter/Stay/Exit.
    /// Их получают монобехи только на корневом объекте (рядом с Rigidbody).
    /// Данный класс прокидывает эти колбеки в дочерние объекты.
    /// </summary>
    [RequireComponent(typeof(Rigidbody))]
    public class CollisionEventsSender : MonoBehaviour
    {
        private struct EventListener<TListener>
        {
            public TListener Listener;
            public List<ContactPoint> ContactPoints;
        }

        [SerializeField, TypeRestriction(typeof(ICollisionEnterListener))]
        private GameObject[] _collisionEnterListeners = Array.Empty<GameObject>();

        [SerializeField, TypeRestriction(typeof(ICollisionStayListener))]
        private GameObject[] _collisionStayListeners = Array.Empty<GameObject>();

        [SerializeField, TypeRestriction(typeof(ICollisionExitListener))]
        private GameObject[] _collisionExitListeners = Array.Empty<GameObject>();

        private Dictionary<Collider, EventListener<ICollisionEnterListener>> _colliderToEnterListener;
        private Dictionary<Collider, EventListener<ICollisionEnterListener>> ColliderToEnterListener => GetOrCreateListeners(_collisionEnterListeners, ref _colliderToEnterListener);

        private Dictionary<Collider, EventListener<ICollisionStayListener>> _colliderToStayListener;
        private Dictionary<Collider, EventListener<ICollisionStayListener>> ColliderToStayListener => GetOrCreateListeners(_collisionStayListeners, ref _colliderToStayListener);

        private Dictionary<Collider, EventListener<ICollisionExitListener>> _colliderToExitListener;
        private Dictionary<Collider, EventListener<ICollisionExitListener>> ColliderToExitListener => GetOrCreateListeners(_collisionExitListeners, ref _colliderToExitListener);

        private static Dictionary<Collider, EventListener<TListener>> GetOrCreateListeners<TListener>(IReadOnlyList<GameObject> serializedListeners, ref Dictionary<Collider, EventListener<TListener>> colliderToListener)
            where TListener : IColliderProvider
        {
            if (colliderToListener == null)
            {
                colliderToListener = new Dictionary<Collider, EventListener<TListener>>();

                foreach (var listenerGameObject in serializedListeners)
                {
                    var listener = listenerGameObject.GetComponent<TListener>();
                    colliderToListener.Add(listener.Collider, new EventListener<TListener>
                    {
                        Listener = listener,
                        ContactPoints = new List<ContactPoint>(),
                    });
                }
            }

            return colliderToListener;
        }

        private void OnCollisionEnter(Collision other)
        {
            ForEachContact(other, ColliderToEnterListener, eventListener =>
            {
                eventListener.Listener.OnCollisionEnterEvent(other, eventListener.ContactPoints);
            });
        }

        private void OnCollisionStay(Collision other)
        {
            ForEachContact(other, ColliderToStayListener, eventListener => eventListener.Listener.OnCollisionStayEvent(other, eventListener.ContactPoints));
        }

        private void OnCollisionExit(Collision other)
        {
            foreach (var value in ColliderToExitListener.Values)
                value.Listener.OnCollisionExitEvent();
        }

        private static void ForEachContact<TListener>(Collision collision, Dictionary<Collider, EventListener<TListener>> colliderToListener,
            Action<EventListener<TListener>> callback)
            where TListener : IColliderProvider
        {
            if (colliderToListener.Count == 0)
                return;

            using (ListPool<ContactPoint>.Get(out var contactPoints))
            {
                for (var i = 0; i < collision.contactCount; i++)
                {
                    var contactPoint = collision.GetContact(i);

                    if (contactPoint.thisCollider == null)
                        continue;

                    if (colliderToListener.TryGetValue(contactPoint.thisCollider, out var eventListener))
                        eventListener.ContactPoints.Add(contactPoint);
                }
            }

            foreach (var eventListener in colliderToListener.Values)
            {
                if (eventListener.ContactPoints.Count > 0)
                    callback(eventListener);

                eventListener.ContactPoints.Clear();
            }
        }
    }

    public interface IColliderProvider
    {
        Collider Collider { get; }
    }

    public interface ICollisionEnterListener : IColliderProvider
    {
        void OnCollisionEnterEvent(Collision collision, IReadOnlyList<ContactPoint> contactPoints);
    }

    public interface ICollisionStayListener : IColliderProvider
    {
        void OnCollisionStayEvent(Collision collision, IReadOnlyList<ContactPoint> contactPoints);
    }

    public interface ICollisionExitListener : IColliderProvider
    {
        void OnCollisionExitEvent();
    }
}