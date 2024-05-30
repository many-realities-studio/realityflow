using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GraphProcessor;
using NodeGraphProcessor.Examples;
using System.Linq;

[System.Serializable, NodeMenuItem("Custom/ToggleObjectGravity")]
public class ToggleObjectGravityNode : BaseNode
{
	[Input(name = "Game Object"), ShowAsDrawer]
	public GameObject gameObject;

	public override string name => "Toggle Gravity";

	protected override void Process() {
		Rigidbody rb = gameObject.GetComponent<Rigidbody>();
		rb.useGravity = !rb.useGravity;
    }
}