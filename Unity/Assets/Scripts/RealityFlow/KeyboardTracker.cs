using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Packages.realityflow_package.Runtime.scripts
{
    public class KeyboardTracker : MonoBehaviour
    {
        //public static KeyboardTracker Instance { get; private set; }
        public string Value;
        public string ShiftValue;

        private Text m_Text;
        private string text;

        private Button m_Button;

        private void Awake()
        {
            m_Button = GetComponent<Button>();
        }

        // Start is called before the first frame update
        void Start()
        {
            m_Text = gameObject.GetComponentInChildren<Text>();
            m_Text.text = Value;

            m_Button.onClick.RemoveAllListeners();
            m_Button.onClick.AddListener(delegate {
                AppendValue(this.Value);
            });
        }

        public void AppendValue(string valueKey)
        {
            if(KeyboardManager.Instance.IsShifted)
            {
                KeyboardManager.Instance.InputField.text = KeyboardManager.Instance.InputField.text + ShiftValue;
                Debug.Log(KeyboardManager.Instance.InputField.text.ToString());
            }
            else
            {
                KeyboardManager.Instance.InputField.text = KeyboardManager.Instance.InputField.text + Value;
                Debug.Log(KeyboardManager.Instance.InputField.text.ToString());
            }
           
        }
    }
}