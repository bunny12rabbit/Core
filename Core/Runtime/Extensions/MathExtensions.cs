using Unity.Mathematics;
using UnityEngine;

namespace Common.Core
{
    public static class MathExtensions
    {
        public static Vector2 ToXY(this Vector3 vector3) => new Vector2(vector3.x, vector3.y);

        public static Vector2 ToXZ(this Vector3 vector3) => new Vector2(vector3.x, vector3.z);

        public static Vector2Int ToXZInt(this Vector3 vector3) => new Vector2Int((int) vector3.x, (int) vector3.z);

        public static Vector3Int ToVector3Int(this Vector3 vector3) => new Vector3Int((int) vector3.x, (int) vector3.y, (int) vector3.z);

        public static AnimationCurve Inverse(this AnimationCurve originalCurve, bool smoothTangents = true, int samplesCount = 20)
        {
            samplesCount = math.max(samplesCount, 2);

            var invertedCurve = new AnimationCurve();

            var totalTime = originalCurve.keys[originalCurve.length - 1].time;
            var sampleInterval = totalTime / samplesCount;
            var prevValue = originalCurve.Evaluate(0);

            for (var i = 0; i < samplesCount; ++i)
            {
                var t = totalTime * i / (samplesCount - 1);
                var value = originalCurve.Evaluate(t);
                var deltaValue = value - prevValue;
                var tangent = sampleInterval / deltaValue;
                Keyframe invertedKey = new Keyframe(value, t, tangent, tangent);
                invertedCurve.AddKey(invertedKey);
            }

            if (smoothTangents)
            {
                for (var i = 0; i < invertedCurve.length; i++)
                    invertedCurve.SmoothTangents(i, 0.1f);
            }

            return invertedCurve;
        }

        public static Quaternion SmoothDampTo(this Quaternion rot, Quaternion target, ref Quaternion deriv, float time) {
            if (Time.deltaTime < Mathf.Epsilon)
                return rot;
            // account for double-cover
            var Dot = Quaternion.Dot(rot, target);
            var Multi = Dot > 0f ? 1f : -1f;
            target.x *= Multi;
            target.y *= Multi;
            target.z *= Multi;
            target.w *= Multi;
            // smooth damp (nlerp approx)
            var Result = new Vector4(
                Mathf.SmoothDamp(rot.x, target.x, ref deriv.x, time),
                Mathf.SmoothDamp(rot.y, target.y, ref deriv.y, time),
                Mathf.SmoothDamp(rot.z, target.z, ref deriv.z, time),
                Mathf.SmoothDamp(rot.w, target.w, ref deriv.w, time)
            ).normalized;

            // ensure deriv is tangent
            var derivError = Vector4.Project(new Vector4(deriv.x, deriv.y, deriv.z, deriv.w), Result);
            deriv.x -= derivError.x;
            deriv.y -= derivError.y;
            deriv.z -= derivError.z;
            deriv.w -= derivError.w;

            return new Quaternion(Result.x, Result.y, Result.z, Result.w);
        }
    }
}