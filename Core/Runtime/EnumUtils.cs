using System;

namespace Common.Core
{
    /// <summary>
    /// В отличии от <see cref="Enum.HasFlag"/> не имеет аллокаций.
    /// </summary>
	public static class EnumUtils
	{
		public static unsafe bool HasFlag<T>(T x, T y) where T : unmanaged, Enum
		{
			switch (sizeof(T))
			{
				case sizeof(byte):
					return (*(byte*) &x & *(byte*) &y) != 0;

				case sizeof(short):
					return (*(short*) &x & *(short*) &y) != 0;

				case sizeof(int):
					return (*(int*) &x & *(int*) &y) != 0;

				case sizeof(long):
					return (*(long*) &x & *(long*) &y) != 0L;

				default:
					return false;
			}
		}

		public static T[] Values<T>()
		{
			return (T[]) Enum.GetValues(typeof(T));
		}

		public static string FlagsToString(object flags)
		{
			switch ((int) flags)
			{
				case 0:
					return "None";

				case -1:
					return "All";
			}

			return flags.ToString();
		}

		public static bool TryGetEnum<T>(int value, out T @enum) where T : Enum
		{
			if (!Enum.IsDefined(typeof(T), value))
			{
				@enum = default;
				return false;
			}

			@enum = (T) Enum.ToObject(typeof(T), value);
			return true;
		}
	}
}