using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEditor.Experimental.GraphView;
using UnityEngine.UIElements;
using GraphProcessor;
using NodeGraphProcessor.Examples;

[NodeCustomEditor(typeof(BoolNode))]
public class BoolNodeView : BaseNodeView
{
	public override void Enable()
	{
		BoolNode comparisonNode = nodeTarget as BoolNode;
		DrawDefaultInspector();
		
		var inputA = new FloatField("In A") { value = comparisonNode.inA };
		var inputB = new FloatField("In B") { value = comparisonNode.inB };
		inputA.RegisterValueChangedCallback(v => {
			owner.RegisterCompleteObjectUndo("Change InA value");
			comparisonNode.inA = v.newValue;
		});
		inputB.RegisterValueChangedCallback(v => {
			owner.RegisterCompleteObjectUndo("Change InB value");
			comparisonNode.inB = v.newValue;
		});

		nodeTarget.onAfterEdgeConnected += UpdateVisibleFields;
		nodeTarget.onAfterEdgeDisconnected += UpdateVisibleFields;

		UpdateVisibleFields(null);

		void UpdateVisibleFields(SerializableEdge _)
		{
			var inA = nodeTarget.GetPort(nameof(comparisonNode.inA), null);
			var inB = nodeTarget.GetPort(nameof(comparisonNode.inB), null);

			controlsContainer.Add(inputA);
			controlsContainer.Add(inputB);

			if (inA.GetEdges().Count > 0)
				controlsContainer.Remove(inputA);
			if (inB.GetEdges().Count > 0)
				controlsContainer.Remove(inputB);
		}
	}
}