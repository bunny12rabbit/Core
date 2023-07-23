using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UIElements;

namespace Common.Core
{
	public static class UiExtensions
	{
		public static VisualElement AddSpace(this VisualElement visualElement, float height = 10)
		{
			var space = new VisualElement();
			space.style.height = height;
			visualElement.Add(space);
			return space;
		}

		public static void SetBorderRadius(this IStyle style, float radius = 3)
		{
			style.borderBottomLeftRadius = radius;
			style.borderBottomRightRadius = radius;
			style.borderTopLeftRadius = radius;
			style.borderTopRightRadius = radius;
		}

		public static bool IsVisible(this IStyle style)
		{
			return style.display == DisplayStyle.Flex;
		}

		public static void SetVisible(this IStyle style, bool isVisible)
		{
			style.display = isVisible ? DisplayStyle.Flex : DisplayStyle.None;
		}

		public static void ScrollTo(this ScrollRect scrollRect, [CanBeNull] RectTransform target)
		{
			if (target == null)
				return;

			Canvas.ForceUpdateCanvases();

			var contentPanel = scrollRect.content != null ? scrollRect.content : scrollRect.GetComponent<RectTransform>();

			contentPanel.anchoredPosition = (Vector2)scrollRect.transform.InverseTransformPoint(contentPanel.position) -
			                                (Vector2)scrollRect.transform.InverseTransformPoint(target.position);
		}

        public static bool IsRectOverlaps(this RectTransform rectTransform1, RectTransform rectTransform2)
        {
            var rect1 = GetWorldRectFromRectTransform(rectTransform1);
            var rect2 = GetWorldRectFromRectTransform(rectTransform2);

            return rect1.Overlaps(rect2);
        }

        public static Rect GetWorldRectFromRectTransform(this RectTransform rectTransform)
        {
            var shared = System.Buffers.ArrayPool<Vector3>.Shared;
            var corners = shared.Rent(4);
            rectTransform.GetWorldCorners(corners);
            shared.Return(corners);
            return new Rect(corners[0].x, corners[0].y, corners[2].x - corners[0].x, corners[2].y - corners[0].y);
        }
	}
}