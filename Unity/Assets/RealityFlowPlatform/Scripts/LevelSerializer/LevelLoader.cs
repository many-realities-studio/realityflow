using UnityEngine;
using System.Collections.Generic;
using Newtonsoft.Json;
using System.IO;

namespace RealityFlowPlatform
{
    public class LevelLoader : MonoBehaviour
    {
        // Loads the JSON string from a file
        public string LoadJSONFromFile(string filePath)
        {
            try
            {
                using (StreamReader reader = new StreamReader(filePath))
                {
                    string json = reader.ReadToEnd();
                    return json;
                }
            }
            catch (System.Exception ex)
            {
                Debug.LogError("Error loading JSON from file: " + ex.Message);
                return null;
            }
        }

        // Loads objects from a JSON string into the Unity Scene
        public void LoadLevelFromJSON(string jsonString)
        {
            try
            {
                LevelData levelData = JsonConvert.DeserializeObject<LevelData>(jsonString);

                foreach (LevelObjectData objectData in levelData.objects)
                {
                    // Assuming you have a method to instantiate these objects
                    GameObject obj = InstantiatePrefab(objectData.name);
                    if (obj != null)
                    {
                        obj.transform.position = objectData.position;
                        obj.transform.rotation = objectData.rotation;
                        obj.transform.localScale = objectData.scale;
                    }
                    else
                    {
                        Debug.LogWarning("Prefab not found for: " + objectData.name);
                    }
                }
            }
            catch (System.Exception ex)
            {
                Debug.LogError("Error loading level from JSON: " + ex.Message);
            }
        }

        // Helper method to instantiate a prefab by name
        // Use the same prefab catalog and Ubiq
        private GameObject InstantiatePrefab(string prefabName)
        {
            GameObject prefab = Resources.Load<GameObject>("Prefabs/" + prefabName);
            if (prefab != null)
            {
                return Instantiate(prefab);
            }
            else
            {
                return null;
            }
        }
    }
}