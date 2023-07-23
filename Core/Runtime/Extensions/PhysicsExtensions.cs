using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

namespace Common.Core
{
    public static class PhysicsExtensions
    {
        public static Vector3 GetHorizontalVelocity(this Rigidbody rigidbody)
        {
            return Vector3.ProjectOnPlane(rigidbody.velocity, Vector3.up);
        }

        public static Vector3 GetLocalAngularVelocity(this Rigidbody rigidbody)
        {
            return rigidbody.transform.InverseTransformDirection(rigidbody.angularVelocity);
        }

        public static void SetLocalAngularVelocity(this Rigidbody rigidbody, Vector3 localAngularVelocity)
        {
            rigidbody.angularVelocity = rigidbody.transform.TransformDirection(localAngularVelocity);
        }

        public static void SetVelocityAlongDirection(this Rigidbody rigidbody, Vector3 forceVector, Vector3 forceDirection)
        {
            rigidbody.velocity += forceVector - Vector3.Project(rigidbody.velocity, forceDirection);
        }

        public static void AddVelocityAlongDirection(this Rigidbody rigidbody, Vector3 velocityVector)
        {
            rigidbody.velocity += Vector3.Project(rigidbody.velocity, velocityVector.normalized) + velocityVector;
        }

        public static void CalcAverageNormalAndPoint(this IReadOnlyList<ContactPoint> contactPoints, out Vector3 averageNormal, out Vector3 averagePoint)
        {
            averageNormal = Vector3.zero;
            averagePoint = Vector3.zero;

            foreach (var contactPoint in contactPoints)
            {
                averageNormal += contactPoint.normal;
                averagePoint += contactPoint.point;
            }

            averageNormal /= contactPoints.Count;
            averagePoint /= contactPoints.Count;
        }

        public static void SetForwardStiffness(this WheelCollider wheelCollider, float value)
        {
            var forwardFriction = wheelCollider.forwardFriction;
            forwardFriction.stiffness = value;
            wheelCollider.forwardFriction = forwardFriction;
        }

        public static void SetSidewaysStiffness(this WheelCollider wheelCollider, float value)
        {
            var sidewaysFriction = wheelCollider.sidewaysFriction;
            sidewaysFriction.stiffness = value;
            wheelCollider.sidewaysFriction = sidewaysFriction;
        }
    }
}