using UnityEditor;

namespace Common.Generic.Editor
{
	public static class SerializedPropertyExtensions
	{
		public const int INVALID_ENUM_INDEX = -1;

		public static int SafeGetArraySize(this SerializedProperty property)
		{
			try
			{
				return property.arraySize;
			}
			catch
			{
				return -1;
			}
		}

		public static int SafeGetEnumValueIndex(this SerializedProperty property)
		{
			try
			{
				return property.enumValueIndex;
			}
			catch
			{
				return INVALID_ENUM_INDEX;
			}
		}

		public static bool SafeGetBool(this SerializedProperty property)
		{
			try
			{
				return property.boolValue;
			}
			catch
			{
				return false;
			}
		}
	}
}