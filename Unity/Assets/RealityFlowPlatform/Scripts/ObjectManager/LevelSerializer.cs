using UnityEngine;
using System;
using System.Collections.Generic;

namespace RealityFlowPlatform
{
    [System.Serializable]
    public class LevelObjectData
    {
        public string rf_id;           // ID of the object
        public string name;         // Name of the object
        public Vector3 position;    // Position of the object
        public Quaternion rotation; // Rotation of the object
        public Vector3 scale;       // Scale of the object
        
        public LevelObjectData()
        {
            rf_id = Guid.NewGuid().ToString(); // Generate a unique ID
        }
    }

    [System.Serializable]
    public class LevelData
    {
        public List<LevelObjectData> objects = new List<LevelObjectData>();
    }

    public class LevelSerializer : MonoBehaviour
    {
        public List<string> objectTagsToSave;  // List of object tags to save

        public LevelData CollectLevelData()   // Change the return type to LevelData
        {
            LevelData levelData = new LevelData();   // Create a new LevelData object
            foreach (string tag in objectTagsToSave)
            {
                GameObject[] objects = GameObject.FindGameObjectsWithTag(tag);   // Find all objects with the specified tag
                foreach (GameObject obj in objects)
                {
                    LevelObjectData objectData = new LevelObjectData   // Create a new LevelObjectData object
                    {
                        name = obj.name,
                        position = obj.transform.position,
                        rotation = obj.transform.rotation,
                        scale = obj.transform.localScale
                    };
                    levelData.objects.Add(objectData);   // Add the object data to the list
                }
            }
            return levelData;
        }

        public string SerializeLevelToJson()
        {
            LevelData levelData = CollectLevelData();     // Collect the level data
            string jsonData = JsonUtility.ToJson(levelData, true);   // Serialize the level data to JSON
            return jsonData;
        }

        public void SendJsonToDatabase(string jsonData)
        {
            // Send the JSON data to the database
            
        }

        public void DeserializeJsonToLevel(string jsonData)
        {
            LevelData levelData = JsonUtility.FromJson<LevelData>(jsonData);   // Deserialize the JSON data back to LevelData

            foreach (var objData in levelData.objects)
            {
                // Example: Creating a new GameObject for each LevelObjectData
                GameObject newObject = new GameObject(objData.name);
                newObject.transform.position = objData.position;
                newObject.transform.rotation = objData.rotation;
                newObject.transform.localScale = objData.scale;
                
                // Optionally, set other properties or add components to newObject based on objData
            }
        }

    }
}