using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

// This class is used upon clicking add parameter to open panels and register values
// and add an exposed parameter to the parameter panel based on the information
public class ParameterCreation : MonoBehaviour
{
    public RealityFlowGraphView rfgv;
    public Dropdown typeDropdown;
    public TMP_InputField nameField;
    void Start() {
        typeDropdown.onValueChanged.AddListener(delegate {
            TypeDropdownValueChangedHandler(typeDropdown);
        });
    }
    void Destroy() {
        typeDropdown.onValueChanged.RemoveAllListeners();
    }
    
    private void TypeDropdownValueChangedHandler(Dropdown target) {
        string dropDownText = target.options[target.value].text;
    }

    // Happens upon confirmation, calls the rest of rfgv's AddParameter based on the values
    public void GetDropDownValue()
    {
        this.gameObject.SetActive(false);
        rfgv.AddParameterStep2(typeDropdown.options[typeDropdown.value].text,nameField.text);
    }
}
