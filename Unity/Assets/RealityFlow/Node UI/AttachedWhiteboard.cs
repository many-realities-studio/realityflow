
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

        static NetworkedPlayManager _playManager;
        static NetworkedPlayManager PlayManager
        {
            get
            {
                if (!_playManager)
                {
                    _playManager = FindObjectOfType<NetworkedPlayManager>();
                    _playManager.enterPlayMode.AddListener(OnEnterPlayMode);
                    _playManager.exitPlayMode.AddListener(OnExitPlayMode);
                }

                return _playManager;
            }
        }

        static bool whiteboardActiveBeforePlay;

        static void OnEnterPlayMode()
        {
            if (Whiteboard.Instance)
            {
                whiteboardActiveBeforePlay = Whiteboard.Instance.gameObject.activeSelf;
                Whiteboard.Instance.gameObject.SetActive(false);
            }
        }

        static void OnExitPlayMode()
        {
            if (Whiteboard.Instance && whiteboardActiveBeforePlay)
                Whiteboard.Instance.gameObject.SetActive(true);
        }

        void Start()
        {
            realityTools = GameObject.Find("RealityFlow Editor");
            if (!Whiteboard.Instance)
            {
                GameObject whiteboard = Instantiate(RealityFlowAPI.Instance.whiteboardPrefab, realityTools.transform);
                whiteboard.GetComponent<Whiteboard>().Init();
                whiteboard.SetActive(false);
            }

            GetComponent<ObjectManipulator>().selectExitedgit .AddListener(_ => 
            {
                if (PlayManager == null || !PlayManager.playMode || RealityFlowAPI.Instance.SpawnedObjects[gameObject]!=null)
                    ShowWhiteboard(gameObject);
            });
        }

        void ShowWhiteboard(GameObject obj)
        {
            Debug.Log("Showing whiteboard for this");
            if (!Whiteboard.Instance)
                return;

            VisualScript script = obj.EnsureComponent<VisualScript>();
            Whiteboard.Instance.ShowForObject(script);
        }
    }
}