using System.Linq;
using DilmerGames.Core.Singletons;
using TMPro;
using UnityEngine;
using System;

public class Logger : Singleton<Logger>
{
    [SerializeField]
    private TextMeshProUGUI debugAreaText = null;

    [SerializeField]
    private bool enableDebug = false;

    [SerializeField]
    private int maxLines = 15;

    private int actionCount = 0; // Counter for numbering actions

    void Awake()
    {
        if (debugAreaText == null)
        {
            debugAreaText = GetComponent<TextMeshProUGUI>();
        }
        debugAreaText.text = string.Empty;
    }

    void OnEnable()
    {
        debugAreaText.enabled = enableDebug;
        enabled = enableDebug;

        if (enabled)
        {
            debugAreaText.text += $"<color=\"white\">Logger enabled</color>\n";
        }
    }

    public void Clear() => debugAreaText.text = string.Empty;

    public void LogInfo(string message)
    {
        ClearLines();
        actionCount++;
        debugAreaText.text += $"<size=150%><color=\"green\">{actionCount}. {message}</color></size>\n";
    }

    public void LogError(string message)
    {
        ClearLines();
        debugAreaText.text += $"<color=\"red\">{message}</color>\n";
    }

    public void LogWarning(string message)
    {
        ClearLines();
        debugAreaText.text += $"<color=\"yellow\">{message}</color>\n";
    }

    private void ClearLines()
    {
        if (debugAreaText.text.Split('\n').Count() >= maxLines)
        {
            debugAreaText.text = string.Empty;
            actionCount = 0; // Reset action counter when clearing
        }
    }
}
