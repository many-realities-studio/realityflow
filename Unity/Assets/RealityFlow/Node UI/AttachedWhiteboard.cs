using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.MixedReality.GraphicsTools;
using Microsoft.MixedReality.Toolkit.SpatialManipulation;
using RealityFlow.NodeGraph;
using UnityEngine;

namespace RealityFlow.NodeUI
{
    public class AttachedWhiteboard : MonoBehaviour
    {
        GameObject realityTools;

        void Start()
        {
            realityTools = GameObject.Find("RealityFlow Editor");
            if (!Whiteboard.Instance)
            {
                GameObject whiteboard = Instantiate(RealityFlowAPI.Instance.whiteboardPrefab, realityTools.transform);
                whiteboard.GetComponent<Whiteboard>().Init();
                whiteboard.SetActive(false);

                // realityTools
                //     .GetComponentInChildren<MeshSelectionManager>()
                //     .ObjectSelected
                //     .AddListener(ShowWhiteboard);
            }

            GetComponent<ObjectManipulator>().firstSelectEntered.AddListener(_ => 
            {
                ShowWhiteboard(gameObject);
            });
            Debug.Log("Added listener!");
        }

        void ShowWhiteboard(GameObject obj)
        {
            if (!Whiteboard.Instance)
                return;

            VisualScript script = obj.EnsureComponent<VisualScript>();
            Whiteboard.Instance.ShowForObject(script);
        }
    }
}