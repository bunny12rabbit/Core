using UnityEngine;

namespace Common.Core.Logs
{
    [DisallowMultipleComponent]
    public class TrajectoryDebugDrawer : MonoBehaviour
    {
        [SerializeField, Min(0)]
        private float _duration = 10f;

#if UNITY_EDITOR
        private Vector3 _previousPosition;

        private Transform _transform;

        private void Awake()
        {
            _transform = GetComponent<Transform>();
        }

        private void OnEnable()
        {
            _previousPosition = _transform.position;
        }

        private void Update()
        {
            var currentPosition = _transform.position;
            DebugDraw.Line(_previousPosition, currentPosition, Color.cyan, _duration);
            _previousPosition = currentPosition;
        }
#endif
    }
}