using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GraphProcessor;
using UnityEngine.EventSystems;
using UnityEngine.XR.Interaction.Toolkit;

// This class is in charge of adding a node to the graph
// after its nodebrush counterpart is dragged and dropped onto the whiteboard
public class NodeManipulation : MonoBehaviour
{
    Vector3 position;
    Quaternion rotation;
    public static NodeManipulation instance;
    GameObject rfgvGameObject;
    public RealityFlowGraphView rfgv;
    BaseGraph graph;

    void Awake()
    {
        instance = this;
        Physics.IgnoreLayerCollision(3, 3);
    }

    // TODO: REDO THIS - Is this even used?
    // public void NodeRaycastCreation(IXRHoverInteractor ptr)
    // {
    //     GameObject node = gameObject.transform.GetChild(0).gameObject;
    //     // Next, perform a raycast
    //     // This one uses an infinite range going forward from the pointer, and should only hit canvas layer (whiteboards)
    //     RaycastHit hit;
    //     if( Physics.Raycast(node.transform.position, ptr.transform.forward, out hit, 1<<6) && ptr != null )
    //     {
    //         // Then set the whiteboard to be the hit object; we want the second child
    //         // as that is RunTimeGraph, where the rfgv script is located
    //         rfgvGameObject = hit.collider.gameObject.transform.GetChild(2).gameObject;
    //         rfgv = rfgvGameObject.GetComponent<RealityFlowGraphView>();
    //         graph = rfgv.graph;

    //         // Then convert the world space position of the new node to local space
    //         Vector3 [] cornerPos = new Vector3[4];
    //         hit.collider.gameObject.GetComponent<RectTransform>().GetWorldCorners(cornerPos);
    //         Vector3 worldNode = node.transform.position;
    //         Vector2 newPos;
    //         newPos = (worldNode - cornerPos[1]);
    //         Vector2 canvasDimensions = (cornerPos[2] - cornerPos[0]);
    //         Vector2 distFromUpperLeftCorner = newPos / canvasDimensions;
    //         AttachNodeToGraph(distFromUpperLeftCorner);
    //     }
    //     else
    //     {
    //         // if it didn't hit, we make sure to destroy it so there isn't a floating button
    //         Destroy(node);
    //         Debug.Log("Node did not hit whiteboard");
    //     }
    // }

    // // This method then signals rfgv to add the node to the whiteboard at the specified location,
    // // and still destroys the button upon completion
    // public void AttachNodeToGraph(Vector2 dist)
    // {
    //     rfgv.SetNewNodeLocation(dist);
    //     rfgv.AddNodeCommand(this.transform.GetChild(0).tag);
    //     Destroy(this.gameObject);
    // }

    // Once a node is dragged off of the command palette,
    // this method will add a new one to take its place immediately
    // Every time, this will add a (Clone) suffix to the object name in the hierarchy;
    // a minor inconvenience that would require a lot of renaming to fix,
    // as all nodebrush nodes have different names
    public void RefreshPalette()
    {
        GameObject o = Instantiate(this.gameObject,this.transform.position,this.transform.rotation,this.transform.parent);
    }
}
