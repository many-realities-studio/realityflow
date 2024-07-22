
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
        public static NetworkedPlayManager PlayManager
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

        void Awake()
        {
            if (!GetComponent<VisualScript>())
                gameObject.AddComponent<VisualScript>();

            if (!GetComponent<RealityFlowObjectEvents>())
            {
                RealityFlowObjectEvents events = gameObject.AddComponent<RealityFlowObjectEvents>();

                if (GetComponent<ObjectManipulator>() is ObjectManipulator manip)
                    manip.firstSelectEntered.AddListener(_ => events.SendSelectedEvent());
            }
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
        }

        void OnDisable()
        {
            VisualScript script = this.EnsureComponent<VisualScript>();
            if (Whiteboard.Instance.TopLevelGraphView.CurrentObject == script)
            {
                Whiteboard.Instance.TopLevelGraphView.Graph = null;
            }
        }
    }
}