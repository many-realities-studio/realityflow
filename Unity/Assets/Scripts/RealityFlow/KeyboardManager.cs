using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class KeyboardManager : MonoBehaviour
{
    public static KeyboardManager Instance { get; private set; }

    /// <summary>
    /// Layout type enum for the type of keyboard layout to use.  
    /// This is used when spawning to enable the correct keys based on layout type.
    /// </summary>
    public enum LayoutType
    {
        Alpha,
        Symbol,
        URL,
        Email,
    }

    #region Callbacks

        /// <summary>
        /// Sent when the 'Enter' button is pressed. To retrieve the text from the event,
        /// cast the sender to 'Keyboard' and get the text from the TextInput field.
        /// (Cleared when keyboard is closed.)
        /// </summary>
        public event EventHandler OnTextSubmitted = delegate { };

        /// <summary>
        /// Fired every time the text in the InputField changes.
        /// (Cleared when keyboard is closed.)
        /// </summary>
        public event Action<string> OnTextUpdated = delegate { };

        /// <summary>
        /// Event fired when shift key on keyboard is pressed.
        /// </summary>
        public event Action<bool> OnKeyboardShifted = delegate { };

    #endregion

    public TMP_InputField InputField = null;

    /// <summary>
    /// Bool to flag submitting on enter
    /// </summary>
    public bool SubmitOnEnter = true;

    /// <summary>
    /// The panel that contains the alpha keys.
    /// </summary>
    public Image AlphaKeyboard = null;

    /// <summary>
    /// The panel that contains the number and symbol keys.
    /// </summary>
    public Image SymbolKeyboard = null;

    /// <summary>
    /// References abc bottom panel.
    /// </summary>
    public Image AlphaSubKeys = null;

    /// <summary>
    /// References .com bottom panel.
    /// </summary>
    public Image AlphaWebKeys = null;

    /// <summary>
    /// References @ bottom panel.
    /// </summary>
    public Image AlphaMailKeys = null;

    private LayoutType m_LastKeyboardLayout = LayoutType.Alpha;

    /// <summary>
    /// Current shift state of keyboard.
    /// </summary>
    private bool m_IsShifted = false;

    /// <summary>
    /// Current caps lock state of keyboard.
    /// </summary>
    private bool m_IsCapslocked = false;

    /// <summary>
    /// Accessor reporting shift state of keyboard.
    /// </summary>
    public bool IsShifted
    {
        get { return m_IsShifted; }
    }

    /// <summary>
    /// Accessor reporting caps lock state of keyboard.
    /// </summary>
    public bool IsCapsLocked
    {
        get { return m_IsCapslocked; }
    }

    void Awake()
    {
         Instance = this;

        // Setting the keyboardType to an undefined TouchScreenKeyboardType,
        // which prevents the MRTK keyboard from triggering the system keyboard itself.
        //InputField.keyboardType = (TouchScreenKeyboardType)(int.MaxValue);

        
        // Keep keyboard deactivated until needed
        gameObject.SetActive(false);
    }

    void Start()
    {
        // Delegate Subscription
        //InputField.onValueChanged.AddListener(DoTextUpdated);
    }

    // This functions presents the Keyboard in its basic state: Alpha characters.
    public void PresentKeyboard()
    {
        //ResetClosingTime();
        gameObject.SetActive(true);
        ActivateSpecificKeyboard(LayoutType.Alpha);

        //OnPlacement(this, EventArgs.Empty);

        // TODO: Get InputField and set the output to point there.
        InputField = (TMP_InputField)FindObjectOfType(typeof(TMP_InputField));

        if (InputField != null)
        {
            Debug.Log("InputField object found: " + InputField.name);
            InputField.onValueChanged.AddListener(DoTextUpdated);
        }
        else
        {
            Debug.Log("No InputField object could be found");
        }

    }

    private void ActivateSpecificKeyboard(LayoutType keyboardType)
    {
        DisableAllKeyboards();
        ResetKeyboardState();

        switch (keyboardType)
        {
            case LayoutType.URL:
                {
                    ShowAlphaKeyboard();
                    TryToShowURLSubkeys();
                    break;
                }

            case LayoutType.Email:
                {
                    ShowAlphaKeyboard();
                    TryToShowEmailSubkeys();
                    break;
                }

            case LayoutType.Symbol:
                {
                    ShowSymbolKeyboard();
                    break;
                }

            case LayoutType.Alpha:
            default:
                {
                    ShowAlphaKeyboard();
                    TryToShowAlphaSubkeys();
                    break;
                }
        }
    }

    // Keyboard Layout Functions
    public void ShowAlphaKeyboard()
    {
        AlphaKeyboard.gameObject.SetActive(true);
        m_LastKeyboardLayout = LayoutType.Alpha;
    }

    /// <summary>
    /// Show the default subkeys only on the Alphanumeric keyboard.
    /// </summary>
    /// <returns>Returns true if default subkeys were activated, false if alphanumeric keyboard isn't active</returns>
    private bool TryToShowAlphaSubkeys()
    {
        if (AlphaKeyboard.IsActive())
        {
            AlphaSubKeys.gameObject.SetActive(true);
            return true;
        }
        else
        {
            return false;
        }
    }

    /// <summary>
    /// Show the email subkeys only on the Alphanumeric keyboard.
    /// </summary>
    /// <returns>Returns true if the email subkey was activated, false if alphanumeric keyboard is not active and key can't be activated</returns>
    private bool TryToShowEmailSubkeys()
    {
        if (AlphaKeyboard.IsActive())
        {
            AlphaMailKeys.gameObject.SetActive(true);
            m_LastKeyboardLayout = LayoutType.Email;
            return true;
        }
        else
        {
            return false;
        }
    }

    /// <summary>
    /// Show the URL subkeys only on the Alphanumeric keyboard.
    /// </summary>
    /// <returns>Returns true if the URL subkey was activated, false if alphanumeric keyboard is not active and key can't be activated</returns>
    private bool TryToShowURLSubkeys()
    {
        if (AlphaKeyboard.IsActive())
        {
            AlphaWebKeys.gameObject.SetActive(true);
            m_LastKeyboardLayout = LayoutType.URL;
            return true;
        }
        else
        {
            return false;
        }
    }

    /// <summary>
    /// Enable the symbol keyboard.
    /// </summary>
    public void ShowSymbolKeyboard()
    {
        SymbolKeyboard.gameObject.SetActive(true);
    }

    /// <summary>
    /// Disable GameObjects for all keyboard elements.
    /// </summary>
    private void DisableAllKeyboards()
    {
        AlphaKeyboard.gameObject.SetActive(false);
        SymbolKeyboard.gameObject.SetActive(false);

        AlphaWebKeys.gameObject.SetActive(false);
        AlphaMailKeys.gameObject.SetActive(false);
        AlphaSubKeys.gameObject.SetActive(false);
    }

    /// <summary>
    /// Reset temporary states of keyboard.
    /// </summary>
    private void ResetKeyboardState()
    {
        CapsLock(false);
    }

    // End of Keyboard Layout Functions

    /// <summary>
    /// Intermediary function for text update events.
    /// Workaround for strange leftover reference when unsubscribing.
    /// </summary>
    /// <param name="value">String value.</param>
    private void DoTextUpdated(string value) => OnTextUpdated?.Invoke(value);

    // Functional Keys

    /// <summary>
        /// Trigger specific keyboard functionality.
        /// </summary>
        /// <param name="functionKey">The functionKey of the pressed key.</param>
        public void FunctionKey(KeyboardFunctionalKeys functionKey)
        {
            // IndicateActivity();
            // OnKeyboardFunctionKeyPressed(functionKey);
            switch (functionKey.ButtonFunction)
            {
                case KeyboardFunctionalKeys.Function.Enter:
                    {
                        Enter();
                        break;
                    }

                case KeyboardFunctionalKeys.Function.Tab:
                    {
                        Tab();
                        break;
                    }

                case KeyboardFunctionalKeys.Function.ABC:
                    {
                        ActivateSpecificKeyboard(m_LastKeyboardLayout);
                        break;
                    }

                case KeyboardFunctionalKeys.Function.Symbol:
                    {
                        ActivateSpecificKeyboard(LayoutType.Symbol);
                        break;
                    }

                case KeyboardFunctionalKeys.Function.Previous:
                    {
                        // MoveCaretLeft();
                        break;
                    }

                case KeyboardFunctionalKeys.Function.Next:
                    {
                        // MoveCaretRight();
                        break;
                    }

                case KeyboardFunctionalKeys.Function.Close:
                    {
                        Close();
                        break;
                    }

                case KeyboardFunctionalKeys.Function.Shift:
                    {
                        Shift(!m_IsShifted);
                        break;
                    }

                case KeyboardFunctionalKeys.Function.CapsLock:
                    {
                        CapsLock(!m_IsCapslocked);
                        break;
                    }

                case KeyboardFunctionalKeys.Function.Space:
                    {
                        Space();
                        break;
                    }

                case KeyboardFunctionalKeys.Function.Backspace:
                    {
                        Backspace();
                        break;
                    }

                case KeyboardFunctionalKeys.Function.UNDEFINED:
                    {
                        Debug.LogErrorFormat("The {0} key on this keyboard hasn't been assigned a function.", functionKey.name);
                        break;
                    }

                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        /// <summary>
        /// Delete the character before the caret.
        /// </summary>
        public void Backspace()
        {
            /*
            // check if text is selected
            if (InputField.selectionFocusPosition != InputField.caretPosition || InputField.selectionAnchorPosition != InputField.caretPosition)
            {
                if (InputField.selectionAnchorPosition > InputField.selectionFocusPosition) // right to left
                {
                    InputField.text = InputField.text.Substring(0, InputField.selectionFocusPosition) + InputField.text.Substring(InputField.selectionAnchorPosition);
                    InputField.caretPosition = InputField.selectionFocusPosition;
                }
                else // left to right
                {
                    InputField.text = InputField.text.Substring(0, InputField.selectionAnchorPosition) + InputField.text.Substring(InputField.selectionFocusPosition);
                    InputField.caretPosition = InputField.selectionAnchorPosition;
                }

                m_CaretPosition = InputField.caretPosition;
                InputField.selectionAnchorPosition = m_CaretPosition;
                InputField.selectionFocusPosition = m_CaretPosition;
            }
            else
            {
                m_CaretPosition = InputField.caretPosition;

                if (m_CaretPosition > 0)
                {
                    --m_CaretPosition;
                    InputField.text = InputField.text.Remove(m_CaretPosition, 1);
                    UpdateCaretPosition(m_CaretPosition);
                }
            }
            */
            InputField.text = InputField.text.Remove(InputField.text.Length - 1, 1);
            Debug.Log(KeyboardManager.Instance.InputField.text.ToString());
        }
/*
        /// <summary>
        /// Send the "previous" event.
        /// </summary>
        public void Previous()
        {
            OnPrevious(this, EventArgs.Empty);
        }

        /// <summary>
        /// Send the "next" event.
        /// </summary>
        public void Next()
        {
            OnNext(this, EventArgs.Empty);
        }

*/
        /// <summary>
        /// Fire the text entered event for objects listening to keyboard.
        /// Immediately closes keyboard.
        /// </summary>
        public void Enter()
        {
            if (SubmitOnEnter)
            {
                // Send text entered event and close the keyboard
                if (OnTextSubmitted != null)
                {
                    OnTextSubmitted(this, EventArgs.Empty);
                }

                Close();
            }
            else
            {
                string enterString = "\n";

                //m_CaretPosition = InputField.caretPosition;

                //InputField.text = InputField.text.Insert(m_CaretPosition, enterString);
                //m_CaretPosition += enterString.Length;

                //UpdateCaretPosition(m_CaretPosition);
            }

        }

        /// <summary>
        /// Set the keyboard to a single action shift state.
        /// </summary>
        /// <param name="newShiftState">value the shift key should have after calling the method</param>
        public void Shift(bool newShiftState)
        {
            m_IsShifted = newShiftState;
            //OnKeyboardShifted(m_IsShifted);

            if (m_IsCapslocked && !newShiftState)
            {
                m_IsCapslocked = false;
            }
        }

        /// <summary>
        /// Set the keyboard to a permanent shift state.
        /// </summary>
        /// <param name="newCapsLockState">Caps lock state the method is switching to</param>
        public void CapsLock(bool newCapsLockState)
        {
            m_IsCapslocked = newCapsLockState;
            Shift(newCapsLockState);
        }

        /// <summary>
        /// Insert a space character.
        /// </summary>
        public void Space()
        {
            string spaceString = " ";
            //m_CaretPosition = InputField.caretPosition;
            //InputField.text = InputField.text.Insert(m_CaretPosition++, " ");

            //UpdateCaretPosition(m_CaretPosition);
            InputField.text = InputField.text + spaceString;
            Debug.Log(KeyboardManager.Instance.InputField.text.ToString());
        }

        /// <summary>
        /// Insert a tab character.
        /// </summary>
        public void Tab()
        {
            string tabString = "\t";

            //m_CaretPosition = InputField.caretPosition;

            //InputField.text = InputField.text.Insert(m_CaretPosition, tabString);
            InputField.text = InputField.text + tabString;
            Debug.Log(KeyboardManager.Instance.InputField.text.ToString());
        }

        /// <summary>
        /// Close the keyboard.
        /// </summary>
        public void Close()
        {
            gameObject.SetActive(false);
        }

        /// <summary>
        /// Clear the text input field.
        /// </summary>
        public void Clear()
        {
            ResetKeyboardState();
            InputField.text = "";
        }

}
