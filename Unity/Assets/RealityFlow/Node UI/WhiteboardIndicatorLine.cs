using Microsoft.MixedReality.GraphicsTools;
using UnityEngine;

public class WhiteboardIndicatorLine : MonoBehaviour
{
    public Transform target;

    LineRenderer line;

    void Start()
    {
        line = gameObject.EnsureComponent<LineRenderer>();
    }

    readonly Vector3[] positions = new Vector3[2];
    void Update()
    {
        positions[0] = transform.position;
        positions[1] = target.position;
        line.SetPositions(positions);
    }
}
