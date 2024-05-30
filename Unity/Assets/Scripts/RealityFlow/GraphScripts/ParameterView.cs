using System.Collections;
using System.Collections.Generic;
using GraphProcessor;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using Packages.realityflow_package.Runtime.scripts;
using RealityFlow.Plugin.Scripts;

// This class is on all exposed parameters and handles value modification, deletion, and graph addition
public class ParameterView : MonoBehaviour
{
    UnityEvent m_ParamDrag;
    public GameObject sphere;
    public GameObject modificationInput;
    public GameObject modificationDropdown;
    public GameObject modificationColor;
    public RealityFlowGraphView rfgv;
    public ExposedParameter pn;
    public GameObject s;

    public Text title;
    public Text type;
    public Text guid;

    // at the start, there is no gameobject parameter to modify, so a modification sphere is not needed
    void start()
    {
        s = null;
    }

    public void AddParameterNodeToGraph(){
        rfgv.AddParameterNodeToGraph(pn.guid);
    }

    // DeleteParam removes the parameter without removing the gameobject,
    // which is handled by Delete
    public void DeleteParam()
    {
        rfgv.RemoveParameter(this);
        this.Delete();
    }

    public void Delete(){
        rfgv.graph.RemoveExposedParameter(this.pn);
        if(this.gameObject != null)
            Destroy(this.gameObject);
    }

    // Delete from whiteboard removes only the gameobject from the whiteboard (soft)
    public void DeleteFromWhiteBoard(){
        if(this.gameObject != null)
            Destroy(this.gameObject);
    }

    public void ResetOrientation(){
        Vector3 localPos = transform.localPosition;
        localPos.z = 0.0f;
        transform.localPosition = localPos;
        transform.localScale = Vector3.one;
    }

    // After ParameterModificationConfirm, this method is called with the new value
    // and simply sets that value, unless it is a gameobject
    public void SetParameterValue(object setValue){
        pn.serializedValue = new SerializableObject(setValue,typeof(object),null);
        s = null;

        // If it is a gameobject, the object to parameter mapping gets tracked in a dictionary
        if (pn.type == "UnityEngine.GameObject, UnityEngine.CoreModule, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null")
        {
            GameObject valueObject = (GameObject)setValue;
            FlowObject_Monobehaviour monoBehaviour = valueObject.GetComponent<FlowObject_Monobehaviour>();
            FlowTObject flowTObj = monoBehaviour.underlyingFlowObject;

            if(rfgv.vsGraph.paramIdToObjId.ContainsKey(pn.guid))
            {
                rfgv.vsGraph.paramIdToObjId[pn.guid] = flowTObj.Id;
            }
            else
            {
                rfgv.vsGraph.paramIdToObjId.Add(pn.guid, flowTObj.Id);
            }
        }
        rfgv.vsGraph.IsUpdated = true;
    }

    // This method initialized the setting of parameter values
    public void ModifyParameterValue()
    {
        // the call to rfgv is simply used to track this as a command
        rfgv.ModifyExposedParameterValue();
        // IF YOU MODIFY THE ORDER OF THESE GAMEOBJECTS ON THE WHITEBOARD PREFAB,
        // BE SURE TO MODIFY THESE CHILD REFERENCES ACCORDINGLY
        // OR PARAMETERS WILL NOT WORK AS INTENDED
        modificationInput = rfgv.gameObject.transform.parent.transform.GetChild(8).gameObject;
        modificationColor = rfgv.gameObject.transform.parent.transform.GetChild(9).gameObject;
        modificationDropdown = rfgv.gameObject.transform.parent.transform.GetChild(10).gameObject;
        if(pn.type == "UnityEngine.GameObject, UnityEngine.CoreModule, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null")
        {
            if(s == null)
            {
                s = Instantiate(sphere, rfgv.gameObject.transform.position, Quaternion.identity);
            }
            s.gameObject.GetComponent<ParameterManipulation>().pv = this;
        }
        else if(pn.type == "System.String, mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089")
        {
            modificationInput.GetComponent<ParameterModificationConfirm>().pv = this;
            // bring up input field to edit string
            modificationInput.SetActive(true);
        }
        else if(pn.type == "System.Int32, mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089")
        {
            modificationInput.GetComponent<ParameterModificationConfirm>().pv = this;
            // bring up input field to edit string
            modificationInput.SetActive(true);
        }
        else if(pn.type == "System.Single, mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089")
        {
            modificationInput.GetComponent<ParameterModificationConfirm>().pv = this;
            // bring up input field to edit string
            modificationInput.SetActive(true);
        }
        else if(pn.type == "System.Boolean, mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089")
        {
            modificationDropdown.GetComponent<ParameterModificationConfirm>().pv = this;
            modificationDropdown.SetActive(true);
        }
        else if(pn.type == "UnityEngine.Color, UnityEngine.CoreModule, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null")
        {
            modificationColor.GetComponent<ParameterModificationConfirm>().pv = this;
            modificationColor.SetActive(true);
        }
        else
        {
            Debug.LogError("invalid type, pn.type is "+pn.type);
        }
    }
}
