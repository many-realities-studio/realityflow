using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Animations;
using Ubiq.Messaging;
using Microsoft.MixedReality.Toolkit;
using Microsoft.MixedReality.Toolkit.UX;
using Ubiq.Avatars;

/// <summary>
/// Class PaletteHandManager manages which hand is the dominant one and attaches the palette to it.
/// </summary>
public class PaletteHandManager : MonoBehaviour
{
    [SerializeField] private PaletteSpawner paletteSpawner;
    private GameObject paletteManager;
    private StatefulInteractable isLeftHandDominant;
    private ConstraintSource constraintSource;

    private GameObject leftHandRay;
    private GameObject leftHandPokeInteractor;
    private GameObject rightHandRay;
    private GameObject rightHandPokeInteractor;

    public static event Action<bool> OnHandChange;

    void Awake()
    {   
        paletteManager = GameObject.Find("Network Scene/Palette Manager");
        leftHandRay = GameObject.Find("Player (XRI + WebXR)/XR Origin (XR Rig)/Camera Offset/Left Controller/Ray Interactor");
        leftHandPokeInteractor = GameObject.Find("Player (XRI + WebXR)/XR Origin (XR Rig)/Camera Offset/Left Controller/Poke Interactor");
        rightHandRay = GameObject.Find("Player (XRI + WebXR)/XR Origin (XR Rig)/Camera Offset/Right Controller/Ray Interactor");
        rightHandPokeInteractor = GameObject.Find("Player (XRI + WebXR)/XR Origin (XR Rig)/Camera Offset/Right Controller/Poke Interactor");
    }

    public void UpdateHand(NetworkContext context, ParentConstraint parentConstraint, Ubiq.Avatars.Avatar[] avatars)
    {
        // Go through every avatar looking for the palette owner's avatar.
        // Only put a parent constraint for the owner's palette (if the first avatar found is the owner)
        for (int i = 0; i < avatars.Length; i++)
        {
            if (avatars[i].ToString().Contains(AvatarManager.UUID))
            {
                GameObject[] dominantHandManagers = GameObject.FindGameObjectsWithTag("DominantHandManager");

                if (isLeftHandDominant == null)
                {
                    for (int j = 0; j < dominantHandManagers.Length; j++)
                    {
                        if (dominantHandManagers[j].transform.parent.parent.parent.GetComponent<NetworkedPalette>().ownerName.Contains(AvatarManager.UUID))
                        {
                            
                            isLeftHandDominant = dominantHandManagers[j].GetComponent<PressableButton>();
                        }
                    }
                }

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

                    StartCoroutine(disableController(0.25f, GameObject.Find("Player (XRI + WebXR)/XR Origin (XR Rig)/Camera Offset/Right Controller/Ray Interactor"),
                                    GameObject.Find("Player (XRI + WebXR)/XR Origin (XR Rig)/Camera Offset/Right Controller/Poke Interactor")));
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

                    StartCoroutine(disableController(0.25f, GameObject.Find("Player (XRI + WebXR)/XR Origin (XR Rig)/Camera Offset/Left Controller/Ray Interactor"),
                                    GameObject.Find("Player (XRI + WebXR)/XR Origin (XR Rig)/Camera Offset/Left Controller/Poke Interactor")));
                }

                // By default, parent constraint is set to false in the inspector. Turn it on only for the client. If parent
                // constraints are on for both client and server-side peers the palette will not function correctly and flicker.
                parentConstraint.enabled = true;

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

    public void ChangeGripButton()
    {
        try
        {
            if (isLeftHandDominant.IsToggled)
            {
                paletteManager.GetComponents<OnButtonPress>()[0].enabled = false;
                paletteManager.GetComponents<OnButtonPress>()[1].enabled = true;
            }
            else if (!isLeftHandDominant.IsToggled)
            {
                paletteManager.GetComponents<OnButtonPress>()[0].enabled = true;
                paletteManager.GetComponents<OnButtonPress>()[1].enabled = false;
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
