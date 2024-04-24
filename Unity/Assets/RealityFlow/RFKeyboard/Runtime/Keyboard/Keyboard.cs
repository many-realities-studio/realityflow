using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using Microsoft.MixedReality.Toolkit.UX;
using SIPSorcery.Sys;
using TMPro;

namespace VrKeyboard
{

    public enum KeyboardStatusTypes
    {
        upper,
        lower,
        other
    }

    /// <summary>
    /// Class thatcontrols the keyboard
    /// </summary>
    public class Keyboard : MonoBehaviour
    {
        public MRTKTMPInputField preview;
        public MRTKUGUIInputField field;
        public UnityEvent<string> KeyPressed = new UnityEvent<string>();
        public UnityEvent EnterPressed = new UnityEvent();
        public UnityEvent BackspacePressed = new UnityEvent();
        public UnityEvent<string> SubmitString = new UnityEvent<string>();
        public UnityEvent<KeyboardStatusTypes> Status = new UnityEvent<KeyboardStatusTypes>();
        public string curString;
        public void Start() {
            preview.ActivateMRTKTMPInputField();
        }
        public void onKey(string text) {
            int caretMove = 0;
            switch(text)
            {
                case "Enter":
                    onEnter();
                    curString = "";
                    break;
                case "Backspace":
                    curString = curString.Remove(preview.caretPosition-1, 1);
                    onBackspace();
                    break;
                case "%#":
                    Status.Invoke(KeyboardStatusTypes.other);
                    break;
                case "Ab":
                    Status.Invoke(KeyboardStatusTypes.lower);
                    break;
                case "Caps":
                    Status.Invoke(KeyboardStatusTypes.upper);
                    break;
                case "Clear":
                    curString = "";
                    break;
                case "Norm":
                    Status.Invoke(KeyboardStatusTypes.lower);
                    break;
                case "Space":
                    KeyPressed.Invoke(" ");
                    curString += " ";
                    caretMove = 1;
                    break;
                default:
                    curString += text;
                    KeyPressed.Invoke(text);
                    caretMove = 1;
                    break;
            }
            preview.text = curString;
            preview.onValueChanged.Invoke(curString);
            preview.caretPosition += caretMove;

            field.text = curString;
            field.onValueChanged.Invoke(curString);
            field.caretPosition += caretMove;

        }

        public void onEnter() {
            EnterPressed.Invoke();
        }

        public void onBackspace() {
            BackspacePressed.Invoke();
            SubmitString.Invoke(curString);
        }

        public Transform spawnRelativeTransform;
        public void Request()
        {
            var cam = Camera.main.transform;

            // NetworkSpawnManager.Find(this).SpawnWithRoomScope(VRWhiteBoard);
            transform.position = cam.TransformPoint(spawnRelativeTransform.localPosition);
            transform.rotation = cam.rotation * spawnRelativeTransform.localRotation;
            gameObject.SetActive(true);
            field = GameObject.Find ("InputField").GetComponent<MRTKUGUIInputField>();
        }

        public void Hide()
        {
            gameObject.SetActive(false);
        }
    }
}
