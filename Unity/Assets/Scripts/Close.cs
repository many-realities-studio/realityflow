using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Close : MonoBehaviour
{
    public void CloseModal()
    {
        Destroy(this.gameObject);
    }    
}
