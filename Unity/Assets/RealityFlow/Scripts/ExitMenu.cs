using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExitMenu : MonoBehaviour
{
    // This method will be called on button click
    public void DisableParentWithMenuTag()
    {
        // Check if the current GameObject has the "Menu" tag
        if (gameObject.CompareTag("Menu"))
        {
            Debug.Log("Current GameObject has 'Menu' tag: " + gameObject.name);
            gameObject.SetActive(false);
            return;
        }

        Transform currentParent = transform.parent;

        // Traverse up the parent hierarchy to find a parent with the "Menu" tag
        while (currentParent != null)
        {
            Debug.Log("Checking parent: " + currentParent.name);

            if (currentParent.CompareTag("Menu"))
            {
                Debug.Log("Found parent with 'Menu' tag: " + currentParent.name);
                currentParent.gameObject.SetActive(false);
                return;
            }

            currentParent = currentParent.parent;
        }

        Debug.LogWarning("No parent with 'Menu' tag found.");
    }
}
