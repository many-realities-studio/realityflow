using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GraphProcessor;
using NodeGraphProcessor.Examples;
using System.Linq;

[System.Serializable, NodeMenuItem("Custom/GameObjectManipulation")]
public class GameObjectManipulationNode : BaseNode
{
    [Input(name = "Boolean")]
    public bool condition = true;

	[Input(name = "Color"), ShowAsDrawer]
	public Color color;

	[Input(name = "Game Object"), ShowAsDrawer]
	public GameObject gameObject;

	public override string name => "Game Object Manipulation";

	protected override void Process() {
		if(condition == true)
		{
			if(gameObject.GetComponent<Renderer>()!=null)
				gameObject.GetComponent<Renderer>().material.color = color;
			else
			{
				gameObject.AddComponent<Renderer>().material.color = color;
			}
            Debug.Log("Color set!");
		}
		else
		{
			Debug.Log("Condition false");
		}
    }
}