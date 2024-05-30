using Newtonsoft.Json;
using System.Collections.Generic;
using Packages.realityflow_package.Runtime.scripts;
using System.Linq;
using System;
using System.Runtime.Serialization;
using UnityEngine;
using GraphProcessor;

namespace RealityFlow.Plugin.Scripts
{
    [System.Serializable]
    public class FlowVSGraph : BaseGraph
    {
        // FlowVSGraphs are NodeGraphProcessor BaseGraphs with additional information necessary for handling graphs with Reality Flow.

        [SerializeField]
        public static SerializableDictionary<string, FlowVSGraph> idToVSGraphMapping = new SerializableDictionary<string, FlowVSGraph>();

        [SerializeField]
        public SerializableDictionary<string, string> paramIdToObjId = new SerializableDictionary<string, string>();

        public bool IsUpdated { get => _isUpdated; set => _isUpdated = value; }

        [SerializeField]
        private bool _isUpdated;

        [SerializeField]
        public string Name;

        [SerializeField]
        public string Id;

        [JsonIgnore]
        private GameObject _AttachedGameObject = null;

        // The AttachedGameObject is the GameObject in the scene that represents the FlowVSGraph, which exists to allow for the attaching of
        // a monobehaviour script that attempts to send graph updates to the server + database. 
        [JsonIgnore]
        public GameObject AttachedGameObject
        {
            get
            {
                if (_AttachedGameObject == null)
                {
                    // The game object already exists
                    if (idToVSGraphMapping.ContainsKey(Id))
                    {
                        if (idToVSGraphMapping[Id]._AttachedGameObject == null)
                        {
                            UnityEngine.Object prefabReference = Resources.Load("FlowVSGraph");

                            if (prefabReference == null)
                            {
                                Debug.Log("cannot load prefab");
                            }
                            idToVSGraphMapping[Id]._AttachedGameObject = GameObject.Instantiate(prefabReference) as GameObject;
                        }

                        _AttachedGameObject = idToVSGraphMapping[Id]._AttachedGameObject;
                    }

                    // The game object doesn't exist, but it should by this point
                    // Can happen when a client receives a create object request when another user created an object
                    else
                    {
                        UnityEngine.Object prefabReference = Resources.Load("prefabs/FlowVSGraph");
                        if (prefabReference == null)
                        {
                            Debug.Log("cannot load prefab");
                        }
                        _AttachedGameObject = GameObject.Instantiate(prefabReference) as GameObject;
                    }
                }
                return _AttachedGameObject;
            }

            set { _AttachedGameObject = value; }
        }

        // This prefab property is not currently being used as we always use the same FlowVSGraph prefab.
        [JsonIgnore]
        private string _Prefab;

        public string Prefab
        {
            get { return _Prefab; }
            set { _Prefab = value; }
        }

        // The normal constructor is only ever called when a user creates a FlowVSGraph, and only for the creator's instance. All graph
        // updates are handled by the JsonConstructor.
        public FlowVSGraph(string n)
        {
            this.Name = n;

            this.Id = Guid.NewGuid().ToString();

            idToVSGraphMapping.Add(Id, this);

            AttachedGameObject.AddComponent<FlowVSGraph_Monobehaviour>();
            FlowVSGraph_Monobehaviour monoBehaviour = AttachedGameObject.GetComponent<FlowVSGraph_Monobehaviour>();

            monoBehaviour.underlyingFlowVSGraph = this;
            this.name = (this.Name + " - " + this.Id);
        }

        // The JsonConstructor is used upon deserialization of any message containing a FlowVSGraph. This allows for the creation of a new instance
        // that gets used to either instantiate a FlowVSGraph on a receiving client's side, or to update an existing graph with new data from the server.
        [JsonConstructor]
        public FlowVSGraph(string id, string name, List<JsonElement> SerializedNodes, List<SerializableEdge> Edges,
                          List<Group> Groups, List<BaseStackNode> StackNodes, List<PinnedElement> PinnedElements,
                          string ExposedParameters, List<StickyNote> StickyNotes, Vector3 Position,
                          Vector3 Scale, string ParamIdToObjId)
        {
            Name = name;
            Id = id;
            this.serializedNodes = SerializedNodes;
            edges = Edges;

            foreach (SerializableEdge edge in edges)
            {
                edge.owner = (BaseGraph)this;
            }

            groups = Groups;
            stackNodes = StackNodes;

            // Our graph system does not currently allow users to create pinned Elements. The only way a pinned element will be created is through opening a graph
            // with exposed parameters in the NodeGraphProcessor window itself, after which you can no longer open the project as NewtonSoft does not know how
            // to instantiate pinned Elements using the serialized data. To prevent projects from breaking like this, refrain from opening graphs with parameters
            // in NGP and then sending updates to that graph.
            pinnedElements = PinnedElements;

            var definition = new[] { new { name = "", guid = "", serializedValue = new { serializedType = "", serializedName = "", serializedValue = "" }, type = "" } };
            var exposedParamList = JsonConvert.DeserializeAnonymousType(ExposedParameters, definition);

            // Delete Exposed parameters that shouldn't exist anymore as the server does not know of their existence
            foreach (ExposedParameter expParam in exposedParameters)
            {
                foreach (var param in exposedParamList.ToList())
                {
                    if (expParam.guid.Equals(param.guid))
                    {
                        break;
                    }

                    this.RemoveExposedParameter(expParam.guid);
                }
            }

            paramIdToObjId = JsonUtility.FromJson<SerializableDictionary<string, string>>(ParamIdToObjId);

            // Update graph to add new Exposed Parameters and update still existing ones.
            foreach (var param in exposedParamList.ToList())
            {
                ExposedParameter paramBuilder;
                // First check if each exposed parameter exists in the graph already
                if ((paramBuilder = GetExposedParameterFromGUID(param.guid)) != null)
                {
                    // Update the value of an exposed parameter by setting the serialized values within its SerializableObject using data received from the server
                    if (Type.GetType(param.type).ToString().Equals("UnityEngine.GameObject"))
                    {
                        // Get the object we want to attach to the parameter from the FlowTObject dictionary
                        string newObjGuid = paramIdToObjId[param.guid];
                        FlowTObject updatedFlowTObject = FlowTObject.idToGameObjectMapping[newObjGuid];
                        GameObject newAttachedGameObj = updatedFlowTObject.AttachedGameObject;

                        paramBuilder.serializedValue.serializedName = param.serializedValue.serializedName;
                        paramBuilder.serializedValue.value = new SerializableObject(newAttachedGameObj, typeof(GameObject), null);
                    }
                    else
                    {
                        paramBuilder.serializedValue.serializedType = param.serializedValue.serializedType;
                        paramBuilder.serializedValue.serializedName = param.serializedValue.serializedName;
                        // paramBuilder.serializedValue.serializedValue = param.serializedValue.serializedValue;
                        // paramBuilder.serializedValue.OnAfterDeserialize();
                    }
                }
                else // Exposed parameter does not yet exist, we will have to create it using received data
                {
                    if (Type.GetType(param.type).ToString().Equals("UnityEngine.GameObject"))
                    {
                        // Get the object we want to attach to the parameter from the FlowTObject dictionary
                        if (paramIdToObjId.ContainsKey(param.guid))
                        {
                            string newObjGuid = paramIdToObjId[param.guid];
                            FlowTObject updatedFlowTObject = FlowTObject.idToGameObjectMapping[newObjGuid];
                            GameObject newAttachedGameObj = updatedFlowTObject.AttachedGameObject;

                            exposedParameters.Add(new ExposedParameter
                            {
                                guid = param.guid,
                                name = param.name,
                                type = param.type,
                                settings = new GraphProcessor.ExposedParameter.Settings(),
                                serializedValue = new SerializableObject(newAttachedGameObj, typeof(GameObject), null)
                            });
                        }
                        else
                        {
                            // The GameObject parameter is not linked to any FlowTObjects (yet). Setting the parameter GameObject to null prevents 
                            // the parameter from attempting to use an object instance ID that may not exist.
                            Debug.LogWarning("Cannot create GameObject Exposed Parameter from nonexistent object. Setting parameter GameObject to null instead.");
                            exposedParameters.Add(new ExposedParameter
                            {
                                guid = param.guid,
                                name = param.name,
                                type = param.type,
                                settings = new ExposedParameter.Settings(),
                                serializedValue = new SerializableObject(null, typeof(GameObject), null)
                            });
                        }
                    }
                    else
                    {
                        // For all exposed parameters that are not of type GameObject, create an uninitialized SerializableObject from scratch to avoid using
                        // the SerializableObject constructor, which causes deserialization of graph messages to throw exceptions as some required values
                        // are null.
                        SerializableObject emptyObj = (SerializableObject)FormatterServices.GetUninitializedObject(typeof(SerializableObject));
                        emptyObj.serializedType = param.serializedValue.serializedType;
                        emptyObj.serializedName = param.serializedValue.serializedName;
                        emptyObj.serializedValue = param.serializedValue.serializedValue;
                        // emptyObj.OnAfterDeserialize();

                        exposedParameters.Add(new ExposedParameter
                        {
                            guid = param.guid,
                            name = param.name,
                            type = param.type,
                            settings = new ExposedParameter.Settings(),
                            serializedValue = emptyObj
                        });
                    }
                }
            }

            // Users are currently not allowed to modify these NGP graph values through Reality Flow's graph editing system, but I will keep these fields here
            // in case they ever get used.
            stickyNotes = StickyNotes;
            position = Position;
            scale = Scale;

            if (idToVSGraphMapping.ContainsKey(id))
            {
                Deserialize();
                idToVSGraphMapping[id].UpdateFlowVSGraphLocally(this);
            }
            else // Create graph object if it doesn't exist
            {
                Deserialize();
                idToVSGraphMapping.Add(Id, this);
                AttachedGameObject.name = name;
                AttachedGameObject.AddComponent<FlowVSGraph_Monobehaviour>();

                var monoBehaviour = AttachedGameObject.GetComponent<FlowVSGraph_Monobehaviour>();
                monoBehaviour.underlyingFlowVSGraph = this;
                this.name = (this.Name + " - " + this.Id);
            }

            // Use this foreach to print all nodes in this instance's serializedNodes list for debugging purposes.
            // foreach (var serializedNode in serializedNodes.ToList())
            // {
            //     Debug.Log(serializedNode);
            //     var baseNodeType = Type.GetType(serializedNode.type);
            //     Debug.Log("Type of node: " + baseNodeType);
            // }
        }

        // This function sends graph updates to the server only after the IsUpdated flag gets set.
        public void UpdateFlowVSGraphGlobally(FlowVSGraph newValues)
        {
            if (IsUpdated == true)
            {
                GraphPropertyCopier<FlowVSGraph, FlowVSGraph>.Copy(newValues, this);

                // Operations.FinalizedUpdateVSGraph(this, ConfigurationSingleton.SingleInstance.CurrentUser, ConfigurationSingleton.SingleInstance.CurrentProject.Id, (_, e) => {/* Debug.Log(e.message);*/ });
                // send over ubiq

                _isUpdated = false;
            }
        }

        // This function is called in NodeView.cs after a user finishes dragging a node around the Whiteboard. This is necessary because ending such a manipulation
        // does not trigger the IsUpdated flag to be set.
        public void ManipulationEndGlobalUpdate(FlowVSGraph newValues)
        {
            GraphPropertyCopier<FlowVSGraph, FlowVSGraph>.Copy(newValues, this);

            Operations.FinalizedUpdateVSGraph(this, ConfigurationSingleton.SingleInstance.CurrentUser, ConfigurationSingleton.SingleInstance.CurrentProject.Id, (_, e) => {/* Debug.Log(e.message);*/ });

            _isUpdated = false;
        }

        // Upon receiving a graph update, this function will copy the values of the updated graph onto an existing, outdated graph.
        private void UpdateFlowVSGraphLocally(FlowVSGraph newValues)
        {
            GraphPropertyCopier<FlowVSGraph, FlowVSGraph>.Copy(newValues, this);
        }

        public static void DestroyVSGraph(string idOfObjectToDestroy)
        {
            try
            {
                GameObject objectToDestroy = idToVSGraphMapping[idOfObjectToDestroy].AttachedGameObject;
                idToVSGraphMapping.Remove(idOfObjectToDestroy);
                UnityEngine.Object.Destroy(objectToDestroy);
            }
            catch (Exception e)
            {
                Debug.LogError(e.ToString());
            }
        }

        // Remove all FlowVSGraph objects from a scene and empty the idToVSGraphMapping dictionary.
        public static void RemoveAllGraphsFromScene()
        {
            foreach (FlowVSGraph flowVSGraph in idToVSGraphMapping.Values)
            {
                UnityEngine.Object.DestroyImmediate(flowVSGraph.AttachedGameObject);
            }
            FlowVSGraph.idToVSGraphMapping = new SerializableDictionary<string, FlowVSGraph>();
        }

        // Deprecated function, currently using the GraphPropertyCopier
        public void CopyFromOtherGraph(FlowVSGraph input)
        {
            Name = input.name;
            serializedNodes = input.serializedNodes;
            edges = input.edges;
            groups = input.groups;
            stackNodes = input.stackNodes;
            pinnedElements = input.pinnedElements;
            exposedParameters = input.exposedParameters;
            stickyNotes = input.stickyNotes;
            position = input.position;
            scale = input.scale;
        }

    }
}