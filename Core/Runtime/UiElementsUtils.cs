using UnityEngine.UIElements;

namespace Common.Core
{
	public static class UiElementsUtils
	{
#if UNITY_EDITOR
        // Иногда юнити вызывает методы, котоыре используют EditorGUIUtility.labelWidth не из OnGUI и это приводит к крашу.
        // Поэтому запоминаем это свойство пока можно.
        private static float? s_labelWidth;
        public static float LabelWidth
        {
            get => s_labelWidth ??= UnityEditor.EditorGUIUtility.labelWidth;
        }

		public static Label CreatePropertyLabel(string text)
		{
			var label = new Label(text)
			{
				style =
				{
					width = LabelWidth,
					alignSelf = Align.Center
				}
			};

			return label;
		}
#endif
	}
}