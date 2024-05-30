using System.Collections;
using System.Collections.Generic;
using System.Linq;
using GraphProcessor;
using UnityEngine;
using UnityEngine.UI;

// This class takes care of all methods relating to nodeports,
// which is where edges are connected from
public class NodePortView : MonoBehaviour
{
    public Text fieldName;
    public Text currentNodeGUID;
    public NodePort port;
    public EdgeListener listener;
    public string type;

    public List<EdgeView> edges = new List< EdgeView >();

    public void SignalRedrawOnUpdate(bool flag){
        foreach(EdgeView e in edges){ e.ToggleUpdates(flag); }
    }

    public void SignalRedraw(){
        foreach(EdgeView e in edges){ e.RedrawEdge(); }
    }

    void RebuildUI(){
        fieldName.text = port.fieldName;
        string inputGUID, outputGUID;
        if(port.GetEdges().Count > 0) {
            inputGUID = ((BaseNode)port.GetEdges()[0].inputNode).GUID;
            outputGUID = ((BaseNode)port.GetEdges()[0].outputNode).GUID;
            if (inputGUID != port.owner.GUID)
            {
                currentNodeGUID.text = inputGUID.Substring(inputGUID.Length - 5);
            }
            if (outputGUID != port.owner.GUID)
            {
                currentNodeGUID.text = outputGUID.Substring(outputGUID.Length - 5);
            }
        }

        SignalRedraw();
    }

	// how do we call this when we select a a port that we want to connect?
    // is this even the right thing to call in that case?
    public void SelectEdge(){
        switch (type)
        {
            case "input":
                listener.SelectInputPort(this);
                break;
            case "output":
                listener.SelectOutputPort(this);
                break;
        }

    }

    public void DeleteEdge(EdgeView edge){
        edges.Remove(edge); // Remove the edge from the list of tracked edges
    }

    // This method deletes the edges from the basegraph (hard), 
    // while the one below only deletes them from the whiteboard (soft)
    public void Delete(){
        foreach (EdgeView e in edges){
            e.Delete(this); 
        }
        edges.Clear();
        Destroy(this.gameObject);
    }

    public void DeleteFromWhiteBoard(){
        foreach (EdgeView e in edges){
            e.DeleteFromWhiteBoard(this); 
        }
        edges.Clear();
        Destroy(this.gameObject);
    }

    public void Init(NodePort np)
    {
        port = np;
        RebuildUI();
    }
}