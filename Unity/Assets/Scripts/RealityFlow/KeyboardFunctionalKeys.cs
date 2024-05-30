using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class KeyboardFunctionalKeys : MonoBehaviour
{
    /// <summary>
    /// Possible functionality for a button.
    /// </summary>
    public enum Function
    {
        // Commands
        Enter,
        Tab,
        ABC,
        Symbol,
        Previous,
        Next,
        Close,
        Dictate,

        // Editing
        Shift,
        CapsLock,
        Space,
        Backspace,

        UNDEFINED,
    }

    //[System.Serializable]
    //private Function buttonFunction = Function.UNDEFINED;
    public Function buttonFunction = Function.UNDEFINED;
    public Function ButtonFunction => buttonFunction;


    private void Start()
    {
        Button m_Button = GetComponent<Button>();
        m_Button.onClick.RemoveAllListeners();
        m_Button.onClick.AddListener(FireFunctionKey);
    }

    /// <summary>
    /// Method injected into the button's onClick listener.
    /// </summary>
    private void FireFunctionKey()
    {
        Debug.Log("Pressed a: "+this.buttonFunction.ToString() +" key type");
        KeyboardManager.Instance.FunctionKey(this);
    }
}
