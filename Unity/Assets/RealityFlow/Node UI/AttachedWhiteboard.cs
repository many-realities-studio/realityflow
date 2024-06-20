
using System;
using System.Collections;
using System.Collections.Generic;
using RealityFlow.NodeGraph;
using UnityEngine;

namespace RealityFlow.NodeUI
{
    public class AttachedWhiteboard : MonoBehaviour
    {
        [SerializeField]
        GameObject whiteboardPrefab;

        GameObject realityTools;

        void Start()
        {
            realityTools = GameObject.Find("RealityFlow Editor");
            if (!Whiteboard.Instance)
            {
                GameObject whiteboard = Instantiate(whiteboardPrefab, realityTools.transform);
                whiteboard.GetComponent<Whiteboard>().Init();
                whiteboard.SetActive(false);

                realityTools.GetComponentInChildren<MeshSelectionManager>().ObjectSelected.AddListener(obj =>
                {
                    if (!Whiteboard.Instance)
                        return;

                    VisualScript script = obj.GetComponent<VisualScript>();
                    Whiteboard.Instance.ShowForObject(script);
                });
            }
        }
    }
}