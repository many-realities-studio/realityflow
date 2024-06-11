using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Animations;
using Ubiq.Messaging;
using Ubiq.Networking;
using Microsoft.MixedReality.Toolkit;
using Microsoft.MixedReality.Toolkit.UX;
using Ubiq.Avatars;

/// <summary>
/// Class PaletteHandManager manages which hand is the dominant one and attaches the palette to it.
/// </summary>
public class PaletteHandManager : MonoBehaviour
{
    [SerializeField] private PaletteSpawner paletteSpawner;
    public GameObject paletteManager;
    public StatefulInteractable isLeftHandDominant;
    private ConstraintSource constraintSource;

    private GameObject leftHandRay;
    private GameObject leftHandPokeInteractor;
    private GameObject rightHandRay;
    private GameObject rightHandPokeInteractor;

    public static event Action<bool> OnHandChange;

    void Awake()
    {   
        leftHandRay = GameObject.Find("Player (XRI + WebXR)/MRTK XR Rig/Camera Offset/MRTK LeftHand Controller/Far Ray");
        leftHandPokeInteractor = GameObject.Find("Player (XRI + WebXR)/MRTK XR Rig/Camera Offset/MRTK LeftHand Controller/IndexTip PokeInteractor");
        rightHandRay = GameObject.Find("Player (XRI + WebXR)/MRTK XR Rig/Camera Offset/MRTK RightHand Controller/Far Ray");
        rightHandPokeInteractor = GameObject.Find("Player (XRI + WebXR)/MRTK XR Rig/Camera Offset/MRTK RightHand Controller/IndexTip PokeInteractor");
    }

     public void UpdateHand(NetworkContext context, ParentConstraint parentConstraint, Ubiq.Avatars.Avatar[] avatars, ParentConstraint otherPaletteParentConstraint, string paletteType, bool handStateAlreadyFound)
    {
        // Debug.Log("Called UpdateHand() from palette " + gameObject.name + " in scene " + gameObject.transform.parent.parent.parent.name);
        // Go through every avatar looking for the palette owner's avatar.
        // Only put a parent constraint for the owner's palette (if the first avatar found is the owner)
        for (int i = 0; i < avatars.Length; i++)
        {
            /*
                if (isLeftHandDominant == null && !handStateAlreadyFound)
                {
                    for (int j = 0; j < dominantHandManagers.Length; j++)
                    {                      
                        if (paletteType == "Palette" && dominantHandManagers[j].transform.parent.parent.parent.GetComponent<NetworkedPalette>() != null && dominantHandManagers[j].transform.parent.parent.parent.GetComponent<NetworkedPalette>().ownerName.Contains(AvatarManager.UUID)
                            && dominantHandManagers[j].transform.parent.parent.parent.GetComponent<NetworkedPalette>().gameObject == gameObject)
                        {
                            // Debug.Log("Successfully got the right hand manager");
                            isLeftHandDominant = dominantHandManagers[j].GetComponent<PressableButton>();
                            // return;
                        }

                        if (paletteType == "PlayPalette" && dominantHandManagers[j].transform.parent.parent.parent.GetComponent<NetworkedPlayPalette>() != null && dominantHandManagers[j].transform.parent.parent.parent.GetComponent<NetworkedPlayPalette>().ownerName.Contains(AvatarManager.UUID)
                            && dominantHandManagers[j].transform.parent.parent.parent.GetComponent<NetworkedPlayPalette>().gameObject == gameObject)
                        {
                            // Debug.Log("Successfully got the right hand manager");
                            isLeftHandDominant = dominantHandManagers[j].GetComponent<PressableButton>();
                            // return;
                        }
                    }
                }
                // if (handStateAlreadyFound)
                // {
                //     Debug.Log("handStateAlreadyFound");
                //     isLeftHandDominant.ForceSetToggled(handStateAlreadyFound);
                // }

                // By default the dominant hand is assigned to the right hand
                Transform dominantHand = avatars[i].transform.Find("Body/LeftHand IK Target");

                // Update the dominant hand based on the toggle state of the Switch Hands Button
                if (isLeftHandDominant.IsToggled)
                {
                    dominantHand = avatars[i].transform.Find("Body/RightHand IK Target");

                    // If the rotation offset y is negative then make it positive to reflect the orientation of a left hand perspective
                    if (Mathf.Sign(parentConstraint.GetRotationOffset(0).y) == -1)
                    {
                        parentConstraint.SetRotationOffset(0, Vector3.Scale(parentConstraint.GetRotationOffset(0), new Vector3(1, -1, 1)));
                    }

                    // Ensure the other palette is in the same dominant hand
                    if (otherPaletteParentConstraint != null)
                    {
                        if (Mathf.Sign(otherPaletteParentConstraint.GetRotationOffset(0).y) == -1)
                        {
                            otherPaletteParentConstraint.SetRotationOffset(0, Vector3.Scale(otherPaletteParentConstraint.GetRotationOffset(0), new Vector3(1, -1, 1)));
                        }
                    }

                    // Debug.Log("Turn on left hand interactors");
                    // Disable and enable the appropriate selectors for a dominant left hand
                    leftHandRay.SetActive(true);
                    leftHandPokeInteractor.SetActive(true);

                    StartCoroutine(disableController(0.25f, GameObject.Find("MRTK Player/MRTK XR Rig/Camera Offset/MRTK RightHand Controller/Far Ray"),
                                    GameObject.Find("MRTK Player/MRTK XR Rig/Camera Offset/MRTK RightHand Controller/IndexTip PokeInteractor")));
                }
                else if (!isLeftHandDominant.IsToggled)
                {
                    dominantHand = avatars[i].transform.Find("Body/LeftHand IK Target");

                    // If the rotation offset y is positive then make it negative to reflect the orientation of a right hand perspective
                    if (Mathf.Sign(parentConstraint.GetRotationOffset(0).y) == 1)
                    {
                        parentConstraint.SetRotationOffset(0, Vector3.Scale(parentConstraint.GetRotationOffset(0), new Vector3(1, -1, 1)));
                    }

                    // Ensure the other palette is in the same dominant hand
                    if (otherPaletteParentConstraint != null)
                    {
                        if (Mathf.Sign(otherPaletteParentConstraint.GetRotationOffset(0).y) == 1)
                        {
                            otherPaletteParentConstraint.SetRotationOffset(0, Vector3.Scale(otherPaletteParentConstraint.GetRotationOffset(0), new Vector3(1, -1, 1)));
                        }
                    }

                    // Debug.Log("Turn on right hand interactors");
                    // Disable and enable the appropriate selectors for a dominant right hand
                    rightHandRay.SetActive(true);
                    rightHandPokeInteractor.SetActive(true);

                    StartCoroutine(disableController(0.25f, GameObject.Find("MRTK Player/MRTK XR Rig/Camera Offset/MRTK LeftHand Controller/Far Ray"),
                                    GameObject.Find("MRTK Player/MRTK XR Rig/Camera Offset/MRTK LeftHand Controller/IndexTip PokeInteractor")));
                }

                // By default, parent constraint is set to false in the inspector. Turn it on only for the client. If parent
                // constraints are on for both client and server-side peers the palette will not function correctly and flicker.
                parentConstraint.enabled = true;

                // Add the LeftHand Controller as an argument for Parent Constraint
                constraintSource.sourceTransform = dominantHand;

                // Set the weight to 1 instead of default 0
                constraintSource.weight = 1;
                parentConstraint.SetSource(0, constraintSource);

                if (otherPaletteParentConstraint != null)
                {
                    otherPaletteParentConstraint.enabled = true;

                    otherPaletteParentConstraint.SetSource(0, constraintSource);
                }

                break;
            }
            else 
            {
                i++;
            }

            if (avatars[i].ToString().Contains(AvatarManager.UUID))
            {
                GameObject[] dominantHandManagers = GameObject.FindGameObjectsWithTag("DominantHandManager");

                // foreach (GameObject go in dominantHandManagers)
                // {
                //     Debug.Log(go.name + " in palette " + go.transform.parent.parent.parent.name);
                // }
            */
            if (avatars[i].ToString().Contains("My Avatar"))
            {
                // By default the dominant hand is assigned to the right hand
                // This is very dependent on Ubiq implementation but likely has to be.
                Transform dominantHand = avatars[i].transform.Find("Body/Floating_LeftHand_A");

                // Update the dominant hand based on the toggle state of the Switch Hands Button
                if (isLeftHandDominant.IsToggled)
                {
                    dominantHand = avatars[i].transform.Find("Body/Floating_RightHand_A");

                    // If the rotation offset y is negative then make it positive to reflect the orientation of a left hand perspective
                    if (Mathf.Sign(parentConstraint.GetRotationOffset(0).y) == -1)
                    {
                        parentConstraint.SetRotationOffset(0, Vector3.Scale(parentConstraint.GetRotationOffset(0), new Vector3(1, -1, 1)));
                    }

                    // Disable and enable the appropriate selectors for a dominant left hand
                    leftHandRay.SetActive(true);
                    leftHandPokeInteractor.SetActive(true);

                    StartCoroutine(disableController(0.25f, GameObject.Find("Player (XRI + WebXR)/MRTK XR Rig/Camera Offset/MRTK RightHand Controller/Far Ray"),
                                    GameObject.Find("Player (XRI + WebXR)/MRTK XR Rig/Camera Offset/MRTK RightHand Controller/IndexTip PokeInteractor")));
                }
                else if (!isLeftHandDominant.IsToggled)
                {
                    dominantHand = avatars[i].transform.Find("Body/Floating_LeftHand_A");

                    // If the rotation offset y is positive then make it negative to reflect the orientation of a right hand perspective
                    if (Mathf.Sign(parentConstraint.GetRotationOffset(0).y) == 1)
                    {
                        parentConstraint.SetRotationOffset(0, Vector3.Scale(parentConstraint.GetRotationOffset(0), new Vector3(1, -1, 1)));
                    }

                    // Disable and enable the appropriate selectors for a dominant right hand
                    rightHandRay.SetActive(true);
                    rightHandPokeInteractor.SetActive(true);

                    StartCoroutine(disableController(0.25f, GameObject.Find("Player (XRI + WebXR)/MRTK XR Rig/Camera Offset/MRTK LeftHand Controller/Far Ray"),
                                    GameObject.Find("Player (XRI + WebXR)/MRTK XR Rig/Camera Offset/MRTK LeftHand Controller/IndexTip PokeInteractor")));
                }

                // By default, parent constraint is set to false in the inspector. Turn it on only for the client. If parent
                // constraints are on for both client and server-side peers the palette will not function correctly and flicker.
                parentConstraint.constraintActive = true;

                // Add the LeftHand Controller as an argument for Parent Constraint
                constraintSource.sourceTransform = dominantHand;

                // Set the weight to 1 instead of default 0
                constraintSource.weight = 1;
                parentConstraint.SetSource(0, constraintSource);

                // Change which grip button determines the toggle ability of the palette
                ChangeGripButton();
                break;
            }
            else 
            {
                i++;
            }
                
        }

        OnHandChange?.Invoke(isLeftHandDominant.IsToggled);
    }

    public OnButtonPress left;
    public OnButtonPress right;
    public void ChangeGripButton()
    {
        try
        {
            if (isLeftHandDominant.IsToggled)
            {
                left.enabled = false;
                right.enabled = true;
                // paletteManager.GetComponents<OnButtonPress>()[1].enabled = true;
            }
            else if (!isLeftHandDominant.IsToggled)
            {
                left.enabled = true;
                right.enabled = false;
                // paletteManager.GetComponents<OnButtonPress>()[0].enabled = true;
                // paletteManager.GetComponents<OnButtonPress>()[1].enabled = false;
            }
        }
        catch (NullReferenceException e)
        {
            Debug.Log(e);
        }
    }

    // Disable the previous dominant controller after a slight interval to avoid button states locking into place
    IEnumerator disableController(float secs, GameObject farRay, GameObject pokeInteractor)
    {
        yield return new WaitForSeconds(secs);
        farRay.SetActive(false);
        pokeInteractor.SetActive(false);
    }
}
