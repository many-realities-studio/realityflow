using System.Collections.Generic;
using UnityEngine;

public class RealityFlowAPI : MonoBehaviour
{
    // Singleton instance
    private static RealityFlowAPI _instance;
    private static readonly object _lock = new object();

    // Dictionary to store GameObjects with their associated string identifiers
    private Dictionary<string, GameObject> _gameObjectDictionary;
    public GameObject testObject;

    // Property to access the singleton instance
    public static RealityFlowAPI Instance
    {
        get
        {
            lock (_lock)
            {
                if (_instance == null)
                {
                    // Search for existing instance in the scene or create a new one
                    _instance = FindObjectOfType<RealityFlowAPI>() ?? new GameObject("RealityFlowAPI").AddComponent<RealityFlowAPI>();
                }
                return _instance;
            }
        }
    }

    void Awake()
    {
        // Ensure the instance is singleton
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            _instance = this;
            _gameObjectDictionary = new Dictionary<string, GameObject>();
            DontDestroyOnLoad(gameObject); // Optional: Makes the object persistent across scenes
        }
        AddGameObject("Test Object", testObject);
    }

    // Function to add a GameObject with an identifier
    public void AddGameObject(string identifier, GameObject gameObject)
    {
        if (!_gameObjectDictionary.ContainsKey(identifier))
        {
            _gameObjectDictionary.Add(identifier, gameObject);
        }
        else
        {
            Debug.LogWarning("Identifier already exists. GameObject not added.");
        }
    }

    // Function to retrieve a GameObject by identifier
    public GameObject GetGameObject(string identifier)
    {
        if (_gameObjectDictionary.TryGetValue(identifier, out GameObject gameObject))
        {
            return gameObject;
        }
        else
        {
            Debug.LogWarning("Identifier not found.");
            return null;
        }
    }
}
