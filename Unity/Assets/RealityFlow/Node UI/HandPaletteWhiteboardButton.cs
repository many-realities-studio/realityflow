using RealityFlow.NodeGraph;
using RealityFlow.NodeUI;
using UnityEngine;

public class HandPaletteWhiteboardButton : MonoBehaviour
{
    bool toggled;

    public void OnClick()
    {
        toggled = !toggled;

        if (toggled)
            Subscribe();
        else
            Unsubscribe();
    }

    public void Subscribe()
    {
        if (!Whiteboard.Instance.gameObject.activeSelf)
            Whiteboard.Instance.Show();
        EventBus<AvatarSelectedObject>.Subscribe(OnSelect);
    }

    public void Unsubscribe()
    {
        EventBus<AvatarSelectedObject>.Unsubscribe(OnSelect);
    }

    void OnSelect(AvatarSelectedObject ev)
    {
        if (
            AttachedWhiteboard.PlayManager != null 
            && !AttachedWhiteboard.PlayManager.playMode 
            && RealityFlowAPI.Instance.SpawnedObjects.ContainsKey(ev.Selected) 
            && ev.Selected.GetComponent<VisualScript>() is VisualScript s
        )
            Whiteboard.Instance.SetAttachedObj(s);
    }
}