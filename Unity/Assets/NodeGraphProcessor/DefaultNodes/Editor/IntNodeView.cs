using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEditor.Experimental.GraphView;
using UnityEngine.UIElements;
using GraphProcessor;

[NodeCustomEditor(typeof(IntNode))]
public class IntNodeView : BaseNodeView
{
	public override void Enable()
	{
		var intNode = nodeTarget as IntNode;

		DoubleField intField = new DoubleField
		{
			value = intNode.input
		};

		intNode.onProcessed += () => intField.value = intNode.input;

		intField.RegisterValueChangedCallback((v) => {
			owner.RegisterCompleteObjectUndo("Updated intNode input");
			intNode.input = (int)v.newValue;
		});

		controlsContainer.Add(intField);
	}
}