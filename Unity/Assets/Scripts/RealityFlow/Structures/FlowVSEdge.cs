using Newtonsoft.Json;
using System.Collections.Generic;
using Packages.realityflow_package.Runtime.scripts;
using System.Linq;
//using Packages.realityflow_package.Runtime.scripts.Managers;
using System;
using UnityEngine;
using GraphProcessor;

namespace RealityFlow.Plugin.Scripts
{
    [System.Serializable]
    public class FlowVSEdge : SerializableEdge
    {
        // public override string inputNodeGUID {get; protected set;}
        // public override string outputNodeGUID {get; protected set;}

        public void OverwriteFieldValues(BaseGraph g, string outputGUID, string inputGUID){
            var flowEdge = new FlowVSEdge();
            flowEdge.GetType().GetField("inputNodeGUID").SetValueDirect(__makeref(flowEdge), inputGUID);
            flowEdge.GetType().GetField("outputNodeGUID").SetValueDirect(__makeref(flowEdge), outputGUID);
            flowEdge.GetType().GetField("owner").SetValueDirect(__makeref(flowEdge), g);
        }

        // public void SetOutputNodeGUID(string guid){
        //     // this.outputNodeGUID = guid;
        // }

        // public void SetOwner(BaseGraph graph){
        //     // base.owner = graph;
        // }
    }
}