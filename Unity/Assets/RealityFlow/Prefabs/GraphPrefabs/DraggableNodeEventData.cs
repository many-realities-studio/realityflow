//
// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.
//

using UnityEngine;

/// <summary>
/// Event data for when a slider's value changes
/// </summary>
public class DraggableNodeEventData
  {
    public DraggableNodeEventData(Vector2 o, Vector2 n)
    {
      OldValue = o;
      NewValue = n;
    }

    /// <summary>
    /// The previous value of the slider
    /// </summary>
    public Vector2 OldValue { get; private set; }

    /// <summary>
    /// The current value of the slider
    /// </summary>
    public Vector2 NewValue { get; private set; }
  }
