using UnityEngine;
using Microsoft.MixedReality.Toolkit.Input;

/// <summary>
/// Class CubePlacer is a placeholder script that creates a cube primitive based off the position of the hand controller.
/// </summary>
public class CubePlacer : MonoBehaviour
{
    private SnapGrid grid;
    [SerializeField] private GameObject rightHandInterator;

    private void Awake()
    {
        grid = FindObjectOfType<SnapGrid>();
    }

    public void PlaceCube()
    {
        PlaceCubeNear(rightHandInterator.transform.position);
    }

    private void PlaceCubeNear(Vector3 clickPoint)
    {
        var finalPosition = grid.GetNearestPointOnGrid(clickPoint);
        GameObject.CreatePrimitive(PrimitiveType.Cube).transform.position = finalPosition;

        //GameObject.CreatePrimitive(PrimitiveType.Sphere).transform.position = nearPoint;
    }
}