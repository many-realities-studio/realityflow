using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GraphProcessor;
using NodeGraphProcessor.Examples;
using System.Linq;
using Ubiq.Spawning;

[System.Serializable, NodeMenuItem("Custom/DestroyObject")]
public class DestroyObjectNode : BaseNode
{
	[Input(name = "Game Object"), ShowAsDrawer]
	public GameObject gameObject;

	public override string name => "Destroy Object";

	protected override void Process() {
		GameObject.Destroy(gameObject);
		// GameObject spawner = GameObject.Find("Spawn Manager");
		// spawner.GetComponent<NetworkSpawner>().Despawn(gameObject);
    }
}