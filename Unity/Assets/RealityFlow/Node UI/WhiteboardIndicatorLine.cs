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
        if (!target)
        {
            line.enabled = false;
            return;
        }
        else
            line.enabled = true;

        positions[0] = transform.position + transform.forward * 0.1f;
        positions[1] = target.position;
        line.SetPositions(positions);
    }
}
