using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GraphProcessor;

[System.Serializable, NodeMenuItem("Primitives/Int")]
public class IntNode : BaseNode
{
    [Output("Out")]
	public int		output;
	
    [Input("In")]
	public int		input;

	public override string name => "Int";

	protected override void Process() => output = input;
}