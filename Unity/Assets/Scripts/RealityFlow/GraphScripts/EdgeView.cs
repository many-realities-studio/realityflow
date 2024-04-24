using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GraphProcessor;

public class EdgeView : MonoBehaviour
{
    public SerializableEdge edge;
    public RealityFlowGraphView rfgv;

    public NodePortView input;
    public NodePortView output;

    private LineRenderer lr;
    private Transform inputLoc, outputLoc; // exact transform locations where to draw the ends of the edges
    public bool UpdateEdgesPerFrame = true; // flag that is toggled by when the user grabs/lets go of the node view
    public float padding = 0.1f;

    public void Delete(NodePortView portBeingDeleted){
        if (input != null && portBeingDeleted != input) { input.DeleteEdge(this);}
        if (output != null && portBeingDeleted != output) { output.DeleteEdge(this);}
        rfgv.graph.Disconnect(edge);
        Destroy(this?.gameObject);
    }

    public void DeleteFromWhiteBoard(NodePortView portBeingDeleted){
        if (input != null && portBeingDeleted != input) { input.DeleteEdge(this);}
        if (output != null && portBeingDeleted != output) { output.DeleteEdge(this);}
        Destroy(this?.gameObject);
    }

    public void RedrawEdge(){
		// calculate the extra points for a better looking circuit board line
        // switched with explicit references to Transforms since that's wayyyyy more efficient than having to use GetComponent() every Update()
        // set small buffer positions:
        Vector3 outputBuffer = outputLoc.position + Vector3.right * padding;
        Vector3 inputBuffer = inputLoc.position + Vector3.left * padding;
		Vector3 [] edgePoints = new [] {
			outputLoc.position,
            outputBuffer,
            inputBuffer,
			inputLoc.position
			};
		lr.SetPositions(edgePoints);
    }

    public void Init(){
        inputLoc = input.GetComponent<RectTransform>().transform.GetChild(0);
        outputLoc = output.GetComponent<RectTransform>().transform.GetChild(0);
        lr = this.gameObject.GetComponent<LineRenderer>();
        RedrawEdge();
    }

    public void ToggleUpdates(bool flag){
        UpdateEdgesPerFrame = flag;
        RedrawEdge();
    }

    void Update(){
        if (true) { RedrawEdge(); }
    }
}
