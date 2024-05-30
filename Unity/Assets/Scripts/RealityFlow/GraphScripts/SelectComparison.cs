using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

// This class is a dropdown to select a boolean node's behavior. It currently only compares
// 0 and 0 (hardcoded in rfgv)
public class SelectComparison : MonoBehaviour
{
    public RealityFlowGraphView rfgv;
    public Dropdown selectionDropdown;
    void Start() {
        selectionDropdown.onValueChanged.AddListener(delegate {
            selectionDropdownValueChangedHandler(selectionDropdown);
        });
    }
    void Destroy() {
        selectionDropdown.onValueChanged.RemoveAllListeners();
    }
    
    private void selectionDropdownValueChangedHandler(Dropdown target) {
        string dropDownText = target.options[target.value].text;
    }

    public void GetDropDownValue()
    {
        this.gameObject.SetActive(false);
        rfgv.BoolNodeStep2(selectionDropdown.options[selectionDropdown.value].text);
    }
}
