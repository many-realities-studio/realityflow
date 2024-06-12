using System.Collections;
using System.Collections.Generic;
using Microsoft.MixedReality.Toolkit;
using RealityFlow.NodeGraph;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.UIElements;

namespace RealityFlow.NodeUI
{
    public class EdgeView : MonoBehaviour
    {
        LineRenderer line;

        public PortIndex from;
        public NodeIndex toNode;
        public int? toPort;

        public Transform target1;
        public Transform target2;
        public Vector3 right = Vector3.right;

        [SerializeField]
        int resolution;
        public int Resolution
        {
            get => resolution;
            set
            {
                resolution = value;
                EnsureValidPointArray();
            }
        }

        public float Width
        {
            get => line.startWidth;
            set
            {
                line.startWidth = value;
                line.endWidth = value;
            }
        }

        public Color Color
        {
            get => line.startColor;
            set
            {
                line.startColor = value;
                line.endColor = value;
            }
        }

        public Style style;

        Vector3[] linePoints;

        Vector3 target1PosLastFrame;
        Vector3 target2PosLastFrame;

        public void Update()
        {
            if (target1PosLastFrame != target1.position || target2PosLastFrame != target2.position)
                Render();

            target1PosLastFrame = transform.position;
            target2PosLastFrame = transform.position;
        }

        [NaughtyAttributes.Button]
        public void Render()
        {
            if (!line)
                line = this.EnsureComponent<LineRenderer>();
            EnsureValidPointArray();

            Assert.IsFalse(line.useWorldSpace, "Line renderers for edge views should use local space");

            Vector3 centerPoint = (target1.position + target2.position) / 2;
            transform.position = centerPoint;

            GeneratePoints();

            line.positionCount = linePoints.Length;
            line.SetPositions(linePoints);
        }

        public void Delete()
        {
            GraphView view = GetComponentInParent<GraphView>();

            if (toPort is int port)
            {
                RealityFlowAPI.Instance.RemoveDataEdgeFromGraph(view.Graph, from, new(toNode, port));
            }
            else
            {
                RealityFlowAPI.Instance.RemoveExecEdgeFromGraph(view.Graph, from, toNode);
            }

            view.MarkDirty();
        }

        void GeneratePoints()
        {
            switch (style)
            {
                case Style.Circuit:
                    Resolution = 4;
                    GenerateCircuitPoints();
                    break;
            }
        }

        // The circuit style is 4 points, one one each endpoint, then 2 halfway between, one on each
        // endpoint's vertical level.
        void GenerateCircuitPoints()
        {
            linePoints[0] = transform.InverseTransformPoint(target1.position);
            Vector3 halfway = (target1.position + target2.position) / 2;
            Vector3 worldSpaceRight = transform.localToWorldMatrix * right;
            Vector3 halfwayRight = Vector3.Project(halfway - target1.position, worldSpaceRight);
            linePoints[1] = transform.InverseTransformPoint(target1.position + halfwayRight);
            linePoints[2] = transform.InverseTransformPoint(target2.position - halfwayRight);
            linePoints[3] = transform.InverseTransformPoint(target2.position);
        }

        void EnsureValidPointArray()
        {
            if (linePoints == null || linePoints.Length != resolution)
                linePoints = new Vector3[resolution];
        }

        public enum Style
        {
            Circuit,
        }
    }
}
