using UnityEngine;

namespace Common.Core
{
    public static class ColorUtils
    {
        private const float ByteMultiplier = 1.0f / 255.0f;

        public static Color FromHashCode(int hashCode, float alpha = 1f)
        {
            if (hashCode == 0)
                return Color.white;

            var rgb = unchecked((uint) hashCode);
            while (rgb < 0x0FFFFF)
                rgb *= 99;

            return FromRgb(rgb, alpha);
        }

        public static Color FromRgb(uint value, float alpha)
        {
            var r = ((value >> 16) & 0xff) * ByteMultiplier;
            var g = ((value >> 8) & 0xff) * ByteMultiplier;
            var b = ((value >> 0) & 0xff) * ByteMultiplier;

            return new Color(r, g, b, alpha);
        }
    }
}