using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using GraphProcessor;
using System.Linq;
using TMPro;

[System.Serializable, NodeMenuItem("Custom/OutputNode")]
public class ModalNode: BaseNode
{
	[Input(name = "In")]
    public string                input;
	public GameObject prefab;
	public GameObject modal;
	public TMP_Text displayText;

	public override string		name => "ModalNode";

	public override bool		deletable => false;

	protected override void Process()
	{
		// instantiate Modal prefab from Resources/ folder
		prefab = Resources.Load("Modal") as GameObject;
		modal = GameObject.Instantiate(prefab, new Vector3(0, 0, 0), Quaternion.identity);

		// get the text component of the modal and set it to the input value
		displayText = modal.transform.Find("DescriptionText").GetComponent<TextMeshPro>();
		displayText.text = input;
	}
}
