using Newtonsoft.Json;
using Packages.realityflow_package.Runtime.scripts;
//using Packages.realityflow_package.Runtime.scripts.Managers;
using System;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

namespace RealityFlow.Plugin.Scripts
{
    [System.Serializable]
    public class FlowTObject
    {
        [SerializeField]
        public static SerializableDictionary<string, FlowTObject> idToGameObjectMapping = new SerializableDictionary<string, FlowTObject>();

        public bool CanBeModified { get => _canBeModified; set => _canBeModified = value; }

        [JsonProperty("Id")]
        public string Id { get => _id; set => _id = value; }

        [JsonIgnore]
        private GameObject _AttachedGameObject = null;

        [JsonIgnore]
        public GameObject AttachedGameObject
        {
            get
            {
                if (_AttachedGameObject == null)
                {
                    // The game object already exists
                    if (idToGameObjectMapping.ContainsKey(Id))
                    {
                        if (idToGameObjectMapping[Id]._AttachedGameObject == null)
                        {
                            UnityEngine.Object prefabReference = Resources.Load(idToGameObjectMapping[Id].Prefab);
                            if (prefabReference == null)
                            {
                                Debug.Log("cannot load prefab");
                            }
                            idToGameObjectMapping[Id]._AttachedGameObject = GameObject.Instantiate(prefabReference) as GameObject;
                        }

                        _AttachedGameObject = idToGameObjectMapping[Id]._AttachedGameObject;
                    }

                    // The game object doesn't exist, but it should by this point
                    // Can happen when a client receives a create object request when another user created an object
                    else
                    {
                        UnityEngine.Object prefabReference = Resources.Load(Prefab);
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

        private Color _ObjectColor;
        private bool _outgoingRequest;

        [SerializeField]
        private string _id = Guid.NewGuid().ToString();

        [JsonIgnore]
        public Color ObjectColor
        {
            get => _ObjectColor;
            set
            {
                _ObjectColor = new Color(value.r, value.g, value.b, value.a);
                R = value.r;
                G = value.g;
                B = value.b;
                A = value.a;
            }
        }

        [SerializeField]
        private float _X;

        [SerializeField]
        private bool _canBeModified;

        [JsonProperty("X")]
        public float X
        {
            get
            {
                _X = AttachedGameObject.transform.localPosition.x;
                return _X;
            }
            set
            {
                _X = value;
                AttachedGameObject.transform.localPosition = new Vector3(value, Y, Z);
            }
        }

        [JsonProperty("Y")]
        public float Y
        {
            get => AttachedGameObject.transform.localPosition.y;
            set => AttachedGameObject.transform.localPosition = new Vector3(X, value, Z);
        }

        [JsonProperty("Z")]
        public float Z
        {
            get => AttachedGameObject.transform.localPosition.z;
            set => AttachedGameObject.transform.localPosition = new Vector3(X, Y, value);
        }

        [JsonProperty("Q_x")]
        public float Q_x
        {
            get => AttachedGameObject.transform.localRotation.x;
            set => AttachedGameObject.transform.localRotation = new Quaternion(value, Q_y, Q_z, Q_w);
        }

        [JsonProperty("Q_y")]
        public float Q_y
        {
            get => AttachedGameObject.transform.localRotation.y;
            set => AttachedGameObject.transform.localRotation = new Quaternion(Q_x, value, Q_z, Q_w);
        }

        [JsonProperty("Q_z")]
        public float Q_z
        {
            get => AttachedGameObject.transform.localRotation.z;
            set => AttachedGameObject.transform.localRotation = new Quaternion(Q_x, Q_y, value, Q_w);
        }

        [JsonProperty("Q_w")]
        public float Q_w
        {
            get => AttachedGameObject.transform.localRotation.w;
            set => AttachedGameObject.transform.localRotation = new Quaternion(Q_x, Q_y, Q_z, value);
        }

        [JsonProperty("S_x")]
        public float S_x
        {
            get => AttachedGameObject.transform.localScale.x;
            set => AttachedGameObject.transform.localScale = new Vector3(value, S_y, S_z);
        }

        [JsonProperty("S_y")]
        public float S_y
        {
            get => AttachedGameObject.transform.localScale.y;
            set => AttachedGameObject.transform.localScale = new Vector3(S_x, value, S_z);
        }

        [JsonProperty("S_z")]
        public float S_z
        {
            get => AttachedGameObject.transform.localScale.z;
            set => AttachedGameObject.transform.localScale = new Vector3(S_x, S_y, value);
        }

        [JsonProperty("R")]
        public float R
        {
            get => ObjectColor.r;
            set
            {
                if (_ObjectColor == null)
                    _ObjectColor = new Color();
                _ObjectColor.r = value;
            }
        }

        [JsonProperty("G")]
        public float G
        {
            get => ObjectColor.g;
            set
            {
                if (_ObjectColor == null)
                    _ObjectColor = new Color();
                _ObjectColor.g = value;
            }
        }

        [JsonProperty("B")]
        public float B
        {
            get => ObjectColor.b;
            set
            {
                if (_ObjectColor == null)
                    _ObjectColor = new Color();
                _ObjectColor.b = value;
            }
        }

        [JsonProperty("A")]
        public float A
        {
            get => ObjectColor.a;
            set
            {
                if (_ObjectColor == null)
                    _ObjectColor = new Color();
                _ObjectColor.a = value;
            }
        }

        [JsonProperty("Name")]
        public string Name { get; set; }

        [JsonIgnore]
        public Vector3 Position
        {
            get => new Vector3(X, Y, Z);
            set
            {
                X = value.x;
                Y = value.y;
                Z = value.z;
            }
        }

        [JsonIgnore]
        public Vector3 Scale
        {
            get => new Vector3(S_x, S_y, S_z);
            set
            {
                S_x = value.x;
                S_y = value.y;
                S_z = value.z;
            }
        }

        [JsonIgnore]
        public Quaternion Rotation
        {
            get => new Quaternion(Q_x, Q_y, Q_z, Q_w);
            set
            {
                Q_x = value.x;
                Q_y = value.y;
                Q_z = value.z;
                Q_w = value.w;
            }
        }

        [SerializeField]
        private string _Prefab;

        public string Prefab
        {
            get { return _Prefab; }
            set { _Prefab = value; }
        }

        public static void DestroyObject(string idOfObjectToDestroy)
        {
            try
            {
                GameObject objectToDestroy = idToGameObjectMapping[idOfObjectToDestroy].AttachedGameObject;
                idToGameObjectMapping.Remove(idOfObjectToDestroy);
                UnityEngine.Object.Destroy(objectToDestroy);
            }
            catch (Exception e)
            {
                Debug.LogError(e.ToString());
            }
        }

        public FlowTObject(string name, Vector3 position, Quaternion rotation, Vector3 scale, Color color, string ObjectPrefab)
        {
            this.Prefab = ObjectPrefab;
            Debug.Log("prefab is " + Prefab);
            this.Name = name;
            this.Position = position;
            this.Rotation = rotation;
            this.Scale = scale;
            this.ObjectColor = color;

            idToGameObjectMapping.Add(Id, this);
            AttachedGameObject.transform.hasChanged = false;
            AttachedGameObject.name = name;
            AttachedGameObject.layer = 9;

            AttachedGameObject.AddComponent<FlowObject_Monobehaviour>();
//            ObjectManipulator om = AttachedGameObject.AddComponent<ObjectManipulator>();
            XRGrabInteractable om = AttachedGameObject.AddComponent<XRGrabInteractable>();
            Rigidbody ObjectRigidBody = AttachedGameObject.AddComponent<Rigidbody>();
            ObjectRigidBody.useGravity = false;
            ObjectRigidBody.isKinematic = true;
            FlowObject_Monobehaviour monoBehaviour = AttachedGameObject.GetComponent<FlowObject_Monobehaviour>();

            monoBehaviour.underlyingFlowObject = this;
            monoBehaviour.objectRigidbody = ObjectRigidBody;
            om.selectEntered.RemoveListener(ManipulationStart);
            om.selectExited.RemoveListener(ManipulationEnd);
            om.selectEntered.AddListener(ManipulationStart);
            om.selectExited.AddListener(ManipulationEnd);
        }

        private void ManipulationStart(SelectEnterEventArgs eventData)
        {
            this.CheckOut();
        }
        private void ManipulationEnd(SelectExitEventArgs eventData)
        {
            this.CheckIn();
        }

        [JsonConstructor]
        public FlowTObject(string id, float x, float y, float z, float q_x, float q_y, float q_z, float q_w, float s_x, float s_y, float s_z, float r, float g, float b, float a, string name, string prefab)
        {
            Id = id;
            this.Prefab = prefab;
            X = x;
            Y = y;
            Z = z;
            Q_x = q_x;
            Q_y = q_y;
            Q_z = q_z;
            Q_w = q_w;
            S_x = s_x;
            S_y = s_y;
            S_z = s_z;
            R = r;
            G = g;
            B = b;
            A = a;
            Name = name;

            if (idToGameObjectMapping.ContainsKey(id))
            {
                idToGameObjectMapping[id].UpdateObjectLocally(this);
            }
            else // Create game object if it doesn't exist
            {
                idToGameObjectMapping.Add(Id, this);
                AttachedGameObject.name = name;
                AttachedGameObject.AddComponent<FlowObject_Monobehaviour>();
                XRGrabInteractable om = AttachedGameObject.AddComponent<XRGrabInteractable>();
                AttachedGameObject.AddComponent<XRGrabInteractable>();
                AttachedGameObject.transform.hasChanged = false;
                AttachedGameObject.layer = 9;

                var monoBehaviour = AttachedGameObject.GetComponent<FlowObject_Monobehaviour>();
                monoBehaviour.underlyingFlowObject = this;
                var ObjectRigidBody = AttachedGameObject.AddComponent<Rigidbody>();
                ObjectRigidBody.useGravity = false;
                ObjectRigidBody.isKinematic = true;
                monoBehaviour.objectRigidbody = ObjectRigidBody;
                om.selectEntered.RemoveListener(ManipulationStart);
                om.selectExited.RemoveListener(ManipulationEnd);
                om.selectEntered.AddListener(ManipulationStart);
                om.selectExited.AddListener(ManipulationEnd);
            }
        }

        /// <summary>
        /// saves the current flowTObject properties to a Unity GameObject
        /// </summary>
        /// <param name="gameObject">GameObject that the information is to be saved to</param>
        public void SavePropertiesToGameObject(GameObject gameObject)
        {
            // TODO: add all properties to the conversion
            gameObject.gameObject.transform.position = new Vector3(X, Y, Z);
            gameObject.gameObject.transform.rotation = new Quaternion(Q_x, Q_y, Q_z, Q_w);
            gameObject.gameObject.transform.localScale = new Vector3(S_x, S_y, S_z);
        }

        /// <summary>
        /// Copy all new values into this object
        /// </summary>
        /// <param name="newValues">The values that should be copied over into this object</param>
        public void UpdateObjectGlobally(FlowTObject newValues)
        {
            if (AttachedGameObject.transform.hasChanged == true)
            {
                bool tempCanBeModified = this.CanBeModified;
                PropertyCopier<FlowTObject, FlowTObject>.Copy(newValues, this);
                this.CanBeModified = tempCanBeModified;

                if (CanBeModified == true)
                {
                    Operations.UpdateObject(this, ConfigurationSingleton.SingleInstance.CurrentUser, ConfigurationSingleton.SingleInstance.CurrentProject.Id, 
                                            ConfigurationSingleton.SingleInstance.CurrentUser.Username, (_, e) => {/* Debug.Log(e.message);*/ });
                }

                AttachedGameObject.transform.hasChanged = false;
            }
        }

        private void UpdateObjectLocally(FlowTObject newValues)
        {
            if (idToGameObjectMapping[newValues.Id].CanBeModified == false)
            {
                bool tempCanBeModified = this.CanBeModified;
                PropertyCopier<FlowTObject, FlowTObject>.Copy(newValues, this);
                this.CanBeModified = tempCanBeModified;
            }
        }

        public void CheckIn()
        {
            if (CanBeModified == true)
            {
                // TODAY!-want to add graphql functionality.
                Operations.CheckinObject(Id, ConfigurationSingleton.SingleInstance.CurrentProject.Id, 
                                        ConfigurationSingleton.SingleInstance.CurrentUser.Username, (_, e) =>
                {
                    // On successful checkin (with GQL solution we dont set the flag here.)
                    if (e.message.WasSuccessful == true)
                    {
                        _canBeModified = false;
                    }
                });
            }
        }

        public void CheckOut()
        {
            if (CanBeModified == false)
            {
                Operations.CheckoutObject(Id, ConfigurationSingleton.SingleInstance.CurrentProject.Id, (_, e) =>
                    {
                        // On successful checkout
                        if (e.message.WasSuccessful == true)
                        {
                            _canBeModified = true;
                        }
                    });
            }
        }

        public static void RemoveAllObjectsFromScene()
        {
            foreach (FlowTObject flowObject in idToGameObjectMapping.Values)
            {
                UnityEngine.Object.DestroyImmediate(flowObject.AttachedGameObject);
            }
            FlowTObject.idToGameObjectMapping = new SerializableDictionary<string, FlowTObject>();
        }
    }
}