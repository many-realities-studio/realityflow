using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Networking; // Add this directive

public class ModelUI : MonoBehaviour
{
    public GameObject modelPrefab; // Reference to the model prefab
    public Transform contentParent; // Parent transform for the instantiated prefabs
    public TextMeshProUGUI selectedModelText; // Reference to the top right corner text
    public string selectedModelURL; // Store the URL of the selected model

    public DownloadModel downloadModelScript; // Reference to the DownloadModel script

    public void PopulateModels(List<ModelData> models)
    {
        foreach (var modelData in models)
        {
            InstantiateModel(modelData);
        }
    }

    private void InstantiateModel(ModelData modelData)
    {
        GameObject modelInstance = Instantiate(modelPrefab, contentParent);
        modelInstance.transform.Find("ObjectNameText").GetComponent<TextMeshProUGUI>().text = "Name: " + modelData.name;
        modelInstance.transform.Find("ObjectTypeText").GetComponent<TextMeshProUGUI>().text = "Triangles: " + modelData.triangles;

        Button modelButton = modelInstance.GetComponent<Button>();
        if (modelButton != null)
        {
            modelButton.onClick.AddListener(() => OnModelClicked(modelData.name, modelData.downloadURL)); 
        }

        // Check if the URL is a data URL or a regular URL
        if (modelData.thumbnailURL.StartsWith("data:image"))
        {
            LoadImageFromDataURL(modelData.thumbnailURL, modelInstance.transform.Find("ObjectImage").GetComponent<Image>());
        }
        else
        {
            StartCoroutine(LoadImageFromURL(modelData.thumbnailURL, modelInstance.transform.Find("ObjectImage").GetComponent<Image>()));
        }
    }

    private void OnModelClicked(string modelName, string modelURL)
    {
        selectedModelText.text = modelName;
        selectedModelURL = modelURL;
        Debug.Log("Model clicked. Name: " + modelName + ", URL: " + modelURL);
    }

    public void SpawnSelectedModel()
    {
        if (!string.IsNullOrEmpty(selectedModelURL))
        {
            downloadModelScript.StartDownload(selectedModelURL);
        }
        else
        {
            Debug.LogError("No model URL selected!");
        }
    }

    private IEnumerator LoadImageFromURL(string url, Image image)
    {
        using (UnityWebRequest uwr = UnityWebRequestTexture.GetTexture(url))
        {
            yield return uwr.SendWebRequest();

            if (uwr.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError("Failed to load image from URL: " + url);
            }
            else
            {
                Texture2D texture = DownloadHandlerTexture.GetContent(uwr);
                image.sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));
            }
        }
    }

    private void LoadImageFromDataURL(string dataURL, Image image)
    {
        // Extract base64 data from the data URL
        string base64Data = dataURL.Substring(dataURL.IndexOf(",") + 1);
        byte[] imageData = Convert.FromBase64String(base64Data);

        // Create a new texture and load the image data into it
        Texture2D texture = new Texture2D(1, 1);
        texture.LoadImage(imageData);

        // Create a sprite from the texture and assign it to the Image component
        image.sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));
    }
}