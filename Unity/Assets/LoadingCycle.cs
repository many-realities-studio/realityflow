using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LoadingCycle : MonoBehaviour
{
    void Update()
    {
        transform.Rotate(0, 0, -270.0f * Time.deltaTime);
    }
}
