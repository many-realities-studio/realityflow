using Newtonsoft.Json;
using RealityFlow.Plugin.Scripts;
using GraphProcessor;
using UnityEngine;

// This file is currently not being used anywhere as graph message strings are manually formatted in Operations.cs.
// This is because JsonConvert, which is used to serialize and deserialize other messages, does not properly
// serialize and deserialize graph objects as NGP's developer intended.

namespace Packages.realityflow_package.Runtime.scripts.Messages.VSGraphMessages
{
    /// <summary>
    /// Response: <see cref="CreateVSGraph_Received"/>
    /// </summary>
    public class CreateVSGraph_SendToServer : BaseMessage
    {
        [JsonProperty("FlowVSGraph")]
        public FlowVSGraph flowVSGraph { get; set; }

        [JsonProperty("ProjectId")]
        public string projectId { get; set; }

        public CreateVSGraph_SendToServer(FlowVSGraph flowVSGraph, /*FlowUser flowUser,*/ string projectId)
        {
            this.flowVSGraph = flowVSGraph;
            this.projectId = projectId;

            this.MessageType = "CreateVSGraph";
        }
    }
    /// <summary>
    /// Response: <see cref="UpdateVSGraph_Received"/>
    /// </summary>
    public class UpdateVSGraph_SendToServer : BaseMessage
    {
        [JsonProperty("FlowVSGraph")]
        public FlowVSGraph flowVSGraph { get; set; }

        [JsonProperty("ProjectId")]
        public string projectId { get; set; }

        public UpdateVSGraph_SendToServer(FlowVSGraph flowVSGraph, /*FlowUser flowUser,*/ string projectId)
        {
            this.flowVSGraph = flowVSGraph;
            this.projectId = projectId;

            this.MessageType = "UpdateVSGraph";
        }
    }

    /// <summary>
    /// Response: <see cref="DeleteVSGraph_Received"/>
    /// </summary>
    public class DeleteVSGraph_SendToServer : BaseMessage
    {
        [JsonProperty("VSGraphId")]
        public string VSGraphId { get; set; } // Id of the deleted graph

        [JsonProperty("ProjectId")]
        public string ProjectId { get; set; }

        public DeleteVSGraph_SendToServer(string projectId, string vsGraphId)
        {
            ProjectId = projectId;
            this.VSGraphId = vsGraphId;
            this.MessageType = "DeleteVSGraph";
        }
    }

    /// <summary>
    /// Response: <see cref="FinalizedUpdateVSGraph_Received"/>
    /// </summary>
    public class FinalizedUpdateVSGraph_SendToServer : BaseMessage
    {
        [JsonProperty("FlowVSGraph")]
        public FlowVSGraph flowVSGraph { get; set; }

        [JsonProperty("projectId")]
        public string projectId { get; set; }

        public FinalizedUpdateVSGraph_SendToServer(FlowVSGraph flowVSGraph, string projectId)
        {
            this.flowVSGraph = flowVSGraph;
            this.projectId = projectId;

            this.MessageType = "FinalizedUpdateVSGraph";
        }
    }
}