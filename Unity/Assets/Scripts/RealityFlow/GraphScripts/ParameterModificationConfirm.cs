using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

// This class is used upon clicking Modify on an exposed parameter to change its value
// based on its type, works in conjunction with parameterView
public class ParameterModificationConfirm : MonoBehaviour
{
    public TMP_InputField input;
    public Dropdown dropdown;
    public ParameterView pv;
    
    void Start() {
        if(dropdown != null)
        {
            dropdown.onValueChanged.AddListener(delegate {
            DropdownValueChangedHandler(dropdown);
            });
        }
    }

    void Destroy() {
        dropdown.onValueChanged.RemoveAllListeners();
    }

    private void DropdownValueChangedHandler(Dropdown target) {
        string dropDownText = target.options[target.value].text;
    }

    // This method is called for strings, ints, and floats
    public void inputConfirm()
    {
        switch(pv.pn.type)
		{
			default:
			case "System.String, mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089":
                pv.SetParameterValue(input.text);
                break;
			case "System.Single, mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089":
                pv.SetParameterValue(float.Parse(input.text));
                break;
			case "System.Int32, mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089":
                pv.SetParameterValue(int.Parse(input.text));
                break;
		}
        this.gameObject.SetActive(false);
    }

    // This method is caled for bools
    public void dropdownConfirm()
    {
        if(dropdown.options[dropdown.value].text == "True")
        {
            pv.SetParameterValue(true);
        }
        else if(dropdown.options[dropdown.value].text == "False")
        {
            pv.SetParameterValue(false);
        }

        this.gameObject.SetActive(false);
    }

    // This method is called for colors
    public void colorConfirm()
    {
        if(dropdown.options[dropdown.value].text == "Red")
        {
            pv.SetParameterValue(Color.red);
        }
        if(dropdown.options[dropdown.value].text == "Black")
        {
            pv.SetParameterValue(Color.black);
        }
        if(dropdown.options[dropdown.value].text == "Yellow")
        {
            pv.SetParameterValue(Color.yellow);
        }
        if(dropdown.options[dropdown.value].text == "Green")
        {
            pv.SetParameterValue(Color.green);
        }
        if(dropdown.options[dropdown.value].text == "Blue")
        {
            pv.SetParameterValue(Color.blue);
        }
        if(dropdown.options[dropdown.value].text == "White")
        {
            pv.SetParameterValue(Color.white);
        }
        this.gameObject.SetActive(false);
    }
}
