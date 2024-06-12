using System;
using System.IO;
using UnityEngine;

public class EnvConfigManager : MonoBehaviour
{
    public static EnvConfigManager Instance { get; private set; }

    public string OpenAIApiKey { get; private set; }
    public string OpenAIOrganization { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this.gameObject);
        }
        else
        {
            Instance = this;
            DontDestroyOnLoad(this.gameObject);
            LoadEnvVariables();
        }
    }

    private void LoadEnvVariables()
    {
        string envPath = Path.Combine(Application.dataPath, "../.env");
        if (File.Exists(envPath))
        {
            string[] lines = File.ReadAllLines(envPath);
            foreach (string line in lines)
            {
                if (string.IsNullOrWhiteSpace(line) || line.StartsWith("#"))
                    continue;

                string[] keyValue = line.Split('=');
                if (keyValue.Length == 2)
                {
                    string key = keyValue[0].Trim();
                    string value = keyValue[1].Trim();
                    Environment.SetEnvironmentVariable(key, value);

                    if (key == "OPENAI_API_KEY")
                    {
                        OpenAIApiKey = value;
                    }
                    else if (key == "OPENAI_ORGANIZATION")
                    {
                        OpenAIOrganization = value;
                    }
                }
            }
            Debug.Log("Environment variables loaded.");
        }
        else
        {
            Debug.LogWarning(".env file not found.");
        }
    }

    public void UpdateApiKey(string newApiKey)
    {
        OpenAIApiKey = newApiKey;
        Environment.SetEnvironmentVariable("OPENAI_API_KEY", newApiKey);
        WriteEnvFile();
        Debug.Log("API key updated: " + newApiKey);
    }

    private void WriteEnvFile()
    {
        string envPath = Path.Combine(Application.dataPath, "../.env");
        var envLines = new[]
        {
            $"OPENAI_API_KEY={OpenAIApiKey}",
            $"OPENAI_ORGANIZATION={OpenAIOrganization}"
        };

        File.WriteAllLines(envPath, envLines);
        Debug.Log(".env file updated.");
    }
}
