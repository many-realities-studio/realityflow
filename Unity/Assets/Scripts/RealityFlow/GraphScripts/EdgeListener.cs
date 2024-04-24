using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GraphProcessor;


/* This class is a refactor of EdgeConnectorListener from NGP.
   Unlike the other class this is designed to work with VR inputs instead of the UnityEditor */
public class EdgeListener : MonoBehaviour
{
    public RealityFlowGraphView graphView;

    public NodePortView inputView, outputView;
    [SerializeField]
    public NodePort input, output;

    // Constructor
    public EdgeListener(RealityFlowGraphView graphView)
    {
        this.graphView = graphView;
    }

    public void AddEdgeToGraph(){
        graphView.graph.Connect (input, output);
    }

    public void SelectInputPort(NodePortView input){
        this.inputView = input;
        CheckForConnection();
    }

    public void SelectOutputPort(NodePortView output){
        this.outputView = output;
        CheckForConnection();
    }

    private void CheckForConnection(){
        // TODO: We need to make sure this is a valid connection (get the port types and make sure they can be connected)
        // TODO: make sure we don't allow edges to be connected if they are trying to use a port that already has an edge
        if ( inputView != null && outputView != null){ // use the NodePortViews instead
            if ((this.inputView.port.portData.displayType == this.outputView.port.portData.displayType) || (this.inputView.port.portData.displayType == typeof(object) || this.outputView.port.portData.displayType == typeof(object)))
                graphView.ConnectEdges(inputView, outputView);
            else
                Debug.Log("The types of ports are not connectable");
            inputView = null;
            outputView = null;
        }
        // Set the views to be null so we don't keep drawing lines between the previously clicked nodes
    }
}
