using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// This class allows overriding components such as the TextMeshPro input field which messes with
/// layout properties in odd ways when you try to do more interesting things with it 
/// (such as extra text in the field box). It is needed because LayoutElement does not have a 
/// way to inherit properties like layoutgroups do; it either flatly overrides with a fixed value
/// or does nothing.
/// </summary>
public class PreserveChildLayoutProperties : MonoBehaviour, ILayoutElement
{
    public bool overrideMinWidth;
    float _minWidth;
    public float minWidth => overrideMinWidth ? _minWidth : -1;

    public bool overrideMinHeight;
    float _minHeight;
    public float minHeight => overrideMinHeight ? _minHeight : -1;

    public bool overridePreferredWidth;
    float _preferredWidth;
    public float preferredWidth => overridePreferredWidth ? _preferredWidth : -1;

    public bool overridePreferredHeight;
    float _preferredHeight;
    public float preferredHeight => overridePreferredHeight ? _preferredHeight : -1;

    // these could be implemented but i don't need them right now so I haven't
    // public bool overrideFlexibleWidth;
    // float _flexibleWidth;
    // public float flexibleWidth => overrideFlexibleWidth ? flexibleWidth : -1;

    // public bool overrideFlexibleHeight;
    // float _flexibleHeight;
    // public float flexibleHeight => overrideFlexibleHeight ? _flexibleHeight : -1;

    public float flexibleWidth => -1;
    public float flexibleHeight => -1;

    public int _layoutPriority = 1;
    public int layoutPriority => _layoutPriority;

    RectTransform[] children;

    public void CalculateLayoutInputHorizontal()
    {
        // fun fact: Transform can be enumerated to enumerate its children. Unfortunately not generic
        // but i have to filter by type here anyway.
        children = (transform as IEnumerable)
            .OfType<RectTransform>()
            .ToArray();

        IEnumerable<float> minWidths = children.Select(rt => LayoutUtility.GetMinWidth(rt));
        IEnumerable<float> prefWidths = children.Select(rt => LayoutUtility.GetPreferredWidth(rt));
        if (TryGetComponent<HorizontalLayoutGroup>(out var horizGroup))
        {
            _minWidth = horizGroup.spacing + minWidths.Sum();
            _preferredWidth = horizGroup.spacing + prefWidths.Sum();
        }
        else
        {
            _minWidth = minWidths.Max();
            _preferredWidth = prefWidths.Max();
        }

        if (TryGetComponent<LayoutGroup>(out var group))
        {
            _minWidth += group.padding.left + group.padding.right;
            _preferredWidth += group.padding.left + group.padding.right;
        }
    }

    public void CalculateLayoutInputVertical()
    {
        IEnumerable<float> minHeights = children.Select(rt => LayoutUtility.GetMinHeight(rt));
        IEnumerable<float> prefHeights = children.Select(rt => LayoutUtility.GetPreferredHeight(rt));
        if (TryGetComponent<VerticalLayoutGroup>(out var vertGroup))
        {
            _minHeight = vertGroup.spacing + minHeights.Sum();
            _preferredHeight = vertGroup.spacing + prefHeights.Sum();
        }
        else
        {
            _minHeight = minHeights.Max();
            _preferredHeight = prefHeights.Max();
        }

        if (TryGetComponent<LayoutGroup>(out var group))
        {
            _minHeight += group.padding.top + group.padding.bottom;
            _preferredHeight += group.padding.top + group.padding.bottom;
        }
    }
}
