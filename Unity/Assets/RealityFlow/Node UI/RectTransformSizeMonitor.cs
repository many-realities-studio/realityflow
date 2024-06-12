using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace RealityFlow.NodeUI
{
    public class RectTransformSizeMonitor : MonoBehaviour
    {
        public UnityEvent onDimensionChange;

        void OnRectTransformDimensionsChange()
        {
            onDimensionChange.Invoke();
        }
    }
}