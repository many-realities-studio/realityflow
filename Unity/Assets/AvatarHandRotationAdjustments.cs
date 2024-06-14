using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AvatarHandRotationAdjustments : MonoBehaviour
{
    public enum SideOfHand
        {
            Left,
            Right
        }

        public SideOfHand side;
    // Start is called before the first frame update
    void Start()
    {
        if(UnityEngine.XR.XRSettings.enabled)
        {
            if(side == SideOfHand.Left)
            {
                gameObject.transform.localRotation = Quaternion.Euler((float)31.684, (float)171.952, (float)-73.121);

            } else
            {
                gameObject.transform.localRotation = Quaternion.Euler((float)-31.684, (float)8.048, (float)-73.121);
            }
        } else
        {
            if(side == SideOfHand.Left)
            {
                gameObject.transform.localRotation = Quaternion.Euler((float)85.26, (float)-15.62, 90);

            } else
            {
                gameObject.transform.localRotation = Quaternion.Euler((float)-94.74, (float)15.62, -90);
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
