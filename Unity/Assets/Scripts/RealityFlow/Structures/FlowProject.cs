using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using UnityEngine;
using GraphProcessor;

namespace RealityFlow.Plugin.Scripts
{
    /// <summary>
    /// The purpose of this class is to hold all the information that is necessary to define a project.
    /// The information from this class gets serialized and sent to the server.
    /// Anything marked with a [DataMember] annotation is marked to be serialized and
    /// sent to the server.
    /// </summary>
    [Serializable]
    public class FlowProject
    {
        [SerializeField]
        private string _id;

        [SerializeField]
        private string _description;

        [SerializeField]
        private long _dateModified;

        [SerializeField]
        private string _projectName;

        [SerializeField]
        private IEnumerable<FlowTObject> _objectList;

        [SerializeField]
        private IEnumerable<FlowVSGraph> _vsGraphList;

        [JsonProperty("Id")]
        public string Id { get => _id; set => _id = value; } // The unique ID of the project

        [JsonProperty("Description")]
        public string Description { get => _description; set => _description = value; } // Description of the project

        [JsonProperty("DateModified")]
        public long DateModified { get => _dateModified; set => _dateModified = value; } // The last time this project was modified

        [JsonProperty("ProjectName")]
        public string ProjectName { get => _projectName; set => _projectName = value; } // Name of the project

        [JsonProperty("_ObjectList")]
        public IEnumerable<FlowTObject> _ObjectList { get => _objectList; set => _objectList = value; }

        [JsonProperty("_VSGraphList")]
        public IEnumerable<FlowVSGraph> _VSGraphList { get => _vsGraphList; set => _vsGraphList = value; }

        [JsonProperty("_BehaviourList")]
        public List<FlowBehaviour> behaviourList { get; set; }

        public FlowProject(string flowId, string description, long dateModified, string projectName)
        {
            Id = flowId;
            Description = description;
            DateModified = dateModified;
            ProjectName = projectName;
        }
    }
}