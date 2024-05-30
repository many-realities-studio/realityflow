using System;
using System.Collections;
using UnityEngine;

// This class is used in the demo to run the graph when the pig enters the pen
public class GraphTrigger : MonoBehaviour
{
    public GameObject whiteBoard;
    public GameObject triggerObj;

    // triggerbox, collider, etc
    void OnTriggerEnter(Collider other) {
        if(triggerObj != null){ // now we have to test if the collider is the right GM
            if ( other.gameObject == triggerObj) {
                RunGraph();
            }
        } 
        else {
            RunGraph();
        }
    }

    void RunGraph(){
        whiteBoard.GetComponent<RealityFlowGraphView>().DoProcessing();
    }
}
