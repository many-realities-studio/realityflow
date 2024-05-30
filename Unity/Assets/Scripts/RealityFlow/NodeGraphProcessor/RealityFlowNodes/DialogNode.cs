using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using GraphProcessor;
using System.Linq;
using Microsoft.MixedReality.Toolkit.UX;

[System.Serializable, NodeMenuItem("Custom/DialogNode")]
public class DialogNode: BaseNode
{
	[Input(name = "In")]
    public string                input;
	
	public override string		name => "DialogNode";

	public override bool		deletable => true;

	GameObject prefab;
	Dialog dialog;


	protected override void Process()
	{
		prefab = GameObject.Instantiate(Resources.Load("Assets/Resources/Modal.prefab")) as GameObject;
		dialog = prefab.GetComponent<Dialog>();
		Dialog.InstantiateFromPrefab(dialog, new DialogProperty("Dialog!", input, DialogButtonHelpers.OK), true, true);
	}
}