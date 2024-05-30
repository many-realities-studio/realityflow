using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class NodeData
{
  public Vector2 Position;
}

[Serializable]
public class NodeDataModel : ScriptableObject
{
  public List<NodeData> Nodes;

  public event Action OnDataChanged;

  public void SetNodePosition(int index, Vector2 position)
  {
    if (index < 0 || index >= Nodes.Count)
      return;

    Nodes[index].Position = position;
    OnDataChanged?.Invoke();
  }

  public Vector2 GetNodePosition(int index)
  {
    if (index < 0 || index >= Nodes.Count)
      return Vector2.zero;

    return Nodes[index].Position;
  }
}