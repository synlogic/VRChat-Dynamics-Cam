using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class DynamicsCamHelper : MonoBehaviour
{

    
    [MenuItem("GameObject/Set DynamicsCam Focus", false, 0)]
    static void setFocus()
    {
        bool foundHead = false;
        foreach (DynamicsCam dcam in GameObject.FindObjectsOfType<DynamicsCam>())
        {
            Debug.Log("Found DynamicsCam");
            if (dcam.focusHead)
            {
                Debug.Log("Focus head is true!");
                foreach (Transform t in Selection.activeTransform.GetComponentsInChildren<Transform>())
                {
                    if (t.name.ToLower() == "head")
                    {
                        Debug.Log("Setting focus");
                        dcam.SetFocus(t);
                        foundHead = true;
                        break;
                    }
                }
            }
            if (!foundHead) dcam.SetFocus(Selection.activeGameObject.transform);
        }
    }
}
