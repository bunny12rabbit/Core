using System.Diagnostics;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace Common.Core.Logs
{
    public static class DebugDraw
    {
        private const string UnityEditorCondition = "UNITY_EDITOR";

        [Conditional(UnityEditorCondition)]
        public static void Cross(Vector3 position, Color color, float duration = 0, float size = 0.5f, bool depthTest = false)
        {
            var halfSize = size / 2;
            Debug.DrawLine(position + Vector3.left * halfSize, position + Vector3.right * halfSize, color, duration, depthTest);
            Debug.DrawLine(position + Vector3.down * halfSize, position + Vector3.up * halfSize, color, duration, depthTest);
            Debug.DrawLine(position + Vector3.forward * halfSize, position + Vector3.back * halfSize, color, duration, depthTest);
        }

        [Conditional(UnityEditorCondition)]
        public static void Arrow(Vector3 startPoint, Vector3 endPoint, Color color, float duration = 0, bool depthTest = false)
        {
            Line(startPoint, endPoint, color, duration, depthTest);
            Cross(endPoint, color, duration, (endPoint - startPoint).magnitude * 0.1f, depthTest);
        }

        [Conditional(UnityEditorCondition)]
        public static void DashedLine(Vector3 startPoint, Vector3 endPoint, Color color, float duration = 0, bool depthTest = false, float dashSize = 1f)
        {
            var distance = (endPoint - startPoint).magnitude;

            for (var length = 0f; length < distance; length += dashSize * 2)
            {
                var dashStartPoint = Vector3.Lerp(startPoint, endPoint, length / distance);
                var dashLength = Mathf.Min(dashSize, distance - length);
                var dashEndPoint = Vector3.Lerp(startPoint, endPoint, (length + dashLength) / distance);
                Debug.DrawLine(dashStartPoint, dashEndPoint, color, duration, depthTest);
            }
        }

        [Conditional(UnityEditorCondition)]
        public static void Line(Vector3 startPoint, Vector3 endPoint, Color color, float duration = 0, bool depthTest = false)
        {
            Debug.DrawLine(startPoint, endPoint, color, duration, depthTest);
        }

        [Conditional(UnityEditorCondition)]
        public static void Ray(Ray ray, Color color, float duration = 0, bool depthTest = false)
        {
            Debug.DrawRay(ray.origin, ray.direction, color, duration, depthTest);
        }

        [Conditional(UnityEditorCondition)]
        public static void Ray(Vector3 origin, Vector3 direction, Color color, float duration = 0, bool depthTest = false)
        {
            Debug.DrawRay(origin, direction, color, duration, depthTest);
        }

        [Conditional(UnityEditorCondition)]
        public static void Plane(Transform transform, Color color, float duration = 0, bool depthTest = false, float planeSize = 10f, float dashSize = 1f)
        {
            Plane(transform.position, transform.up, color, duration, depthTest, planeSize, dashSize);
        }

        [Conditional(UnityEditorCondition)]
        public static void Plane(Vector3 position, Vector3 normal, Color color, float duration = 0, bool depthTest = false, float planeSize = 10f, float dashSize = 1f)
        {
            var crossProduct = Vector3.Cross(normal, normal != Vector3.forward ? Vector3.forward : Vector3.up);

            var p1 = position + crossProduct * planeSize * 0.5f;
            var p3 = position - crossProduct * planeSize * 0.5f;
            var q = Quaternion.AngleAxis(90, normal);
            crossProduct = q * crossProduct;
            var p2 = position + crossProduct * planeSize * 0.5f;
            var p4 = position - crossProduct * planeSize * 0.5f;

            DashedLine(p1, p2, color, duration, depthTest, dashSize);
            DashedLine(p2, p3, color, duration, depthTest, dashSize);
            DashedLine(p3, p4, color, duration, depthTest, dashSize);
            DashedLine(p4, p1, color, duration, depthTest, dashSize);
            DashedLine(p1, p3, color, duration, depthTest, dashSize);
            DashedLine(p2, p4, color, duration, depthTest, dashSize);
        }
    }
}