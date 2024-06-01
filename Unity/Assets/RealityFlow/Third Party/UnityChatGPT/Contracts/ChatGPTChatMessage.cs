using System;
using UnityEngine;

[Serializable]
public class ChatGPTChatMessage
{
    [field: SerializeField]
    public string role { get; set; }

    [field: SerializeField]
    public string content { get; set; }
}