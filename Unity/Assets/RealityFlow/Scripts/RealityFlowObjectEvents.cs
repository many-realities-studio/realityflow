using UnityEngine;

public class RealityFlowObjectEvents : MonoBehaviour
{
    public void SendSelectedEvent()
    {
        EventBus<AvatarSelectedObject>.Send(new()
        {
            Selected = gameObject,
        });
    }
}