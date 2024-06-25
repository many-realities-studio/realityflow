using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class TextPrefab : MonoBehaviour
{
    TMP_Text uiText;

    void Awake()
    {
        uiText = GetComponent<TMP_Text>();

        NetworkedPlayManager.Instance.enterPlayMode.AddListener(SetTextEmpty);
        NetworkedPlayManager.Instance.exitPlayMode.AddListener(SetTextText);
    }

    void OnDestroy()
    {
        NetworkedPlayManager.Instance.enterPlayMode.RemoveListener(SetTextEmpty);
        NetworkedPlayManager.Instance.exitPlayMode.RemoveListener(SetTextText);
    }

    void SetTextEmpty()
    {
        RealityFlowAPI.Instance.SetUIText(uiText.gameObject, "");
    }

    void SetTextText()
    {
        RealityFlowAPI.Instance.SetUIText(uiText.gameObject, "text");
    }
}
