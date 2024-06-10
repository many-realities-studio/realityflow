using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace RealityFlow.NodeUI
{
    [RequireComponent(typeof(RectTransform))]
    [ExecuteAlways]
    [AddComponentMenu("Layout/Overlap Layout Group")]
    public class OverlapLayoutGroup : LayoutGroup
    {
        public int _layoutPriority = 1;
 
        RectTransform rect;

        new void Start()
        {
            base.Start();
            rect = GetComponent<RectTransform>();
        }

        public override void CalculateLayoutInputHorizontal()
        {
            base.CalculateLayoutInputHorizontal();

            float minWidth = 0;
            float prefWidth = 0;
            float flexWidth = -1;
            foreach (RectTransform child in transform)
            {
                minWidth = Mathf.Max(minWidth, LayoutUtility.GetMinWidth(child));
                prefWidth = Mathf.Max(prefWidth, LayoutUtility.GetPreferredWidth(child));
                flexWidth = Mathf.Max(flexWidth, LayoutUtility.GetFlexibleWidth(child));
            }

            SetLayoutInputForAxis(minWidth + padding.horizontal, prefWidth + padding.horizontal, flexWidth, 0);
        }

        public override void CalculateLayoutInputVertical()
        {
            float minHeight = 0;
            float prefHeight = 0;
            float flexHeight = -1;
            foreach (RectTransform child in transform)
            {
                minHeight = Mathf.Max(minHeight, LayoutUtility.GetMinHeight(child));
                prefHeight = Mathf.Max(prefHeight, LayoutUtility.GetPreferredHeight(child));
                flexHeight = Mathf.Max(flexHeight, LayoutUtility.GetFlexibleHeight(child));
            }

            SetLayoutInputForAxis(minHeight + padding.vertical, prefHeight + padding.vertical, flexHeight, 1);
        }

        public override void SetLayoutHorizontal()
        {
            bool isFlexible = LayoutUtility.GetFlexibleWidth(rect) > 0;
            float width = isFlexible ? rect.rect.width : preferredWidth;
            width -= padding.horizontal;

            foreach (RectTransform child in transform)
            {
                SetChildAlongAxis(child, 0, padding.left, width);
            }
        }

        public override void SetLayoutVertical()
        {
            bool isFlexible = LayoutUtility.GetFlexibleHeight(rect) > 0;
            float height = isFlexible ? rect.rect.height : preferredHeight;
            height -= padding.vertical;

            foreach (RectTransform child in transform)
            {
                SetChildAlongAxis(child, 1, padding.top, height);
            }
        }
    }
}
