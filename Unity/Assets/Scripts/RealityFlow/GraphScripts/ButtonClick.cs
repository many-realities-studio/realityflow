using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using GraphProcessor;

public class ButtonClick : MonoBehaviour
{
	public GameObject Canvas;
	public static GameObject[] WhiteBoards;

	// This function can be used to instantiate whiteboards into the scene rather than using the one that is currently there
	public void CreateCanvas()
	{
		GameObject WhiteBoard = Instantiate(Canvas, Canvas.transform.position, Canvas.transform.rotation);
		//WhiteBoard.transform.parent = GameObject.FindGameObjectsWithTag("Player")[0].transform;
		// GameObject WhiteBoard2 = Instantiate(Canvas, new Vector3(Canvas.transform.position.x-1.2f,Canvas.transform.position.y,Canvas.transform.position.z), Canvas.transform.rotation);
		//WhiteBoard2.transform.parent = GameObject.FindGameObjectsWithTag("Player")[0].transform;
		WhiteBoards = GameObject.FindGameObjectsWithTag("Canvas");
		// find the graph from the canvas gameobject
		//GameObject rfgvGameObject = GameObject.FindGameObjectWithTag("Canvas").transform.GetChild(5).gameObject;
		//RealityFlowGraphView rfgv = rfgvGameObject.GetComponent<RealityFlowGraphView>();
		//BaseGraph graph = rfgv.graph;
		//NodeUI.instance.Setup(graph);
		//foreach(GameObject w in WhiteBoards)
		//{
		//	GameObject rfgvGameObject = GameObject.FindGameObjectWithTag("Canvas").transform.GetChild(5).gameObject;
		//	RealityFlowGraphView rfgv = rfgvGameObject.GetComponent<RealityFlowGraphView>();
			//BaseGraph graph = rfgv.graph;
			//NodeUI.instance.Setup(rfgv);
		//}
	}
}
