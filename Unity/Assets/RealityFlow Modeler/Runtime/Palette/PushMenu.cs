using UnityEngine;

/// <summary>
/// Class PushMenu pushes a Menu up or down a specfified amount of units to compensate
/// potentially for the inclusion of an additional menu panel.
/// </summary>
public class PushMenu : MonoBehaviour
{
    public void MoveMenuPositionY(int units)
    {
        gameObject.GetComponent<RectTransform>().anchoredPosition3D += new Vector3(0, units, 0);
    }
}
