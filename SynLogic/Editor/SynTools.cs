using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.Animations;

public static class SynTools
{
    static string[] objectNames = new string[] { "LeftArm", "LeftElbow", "RightArm", "RightElbow"};
    static string[] twistNames = new string[] {"LeftElbowTwist", "LeftHandTwist", "RightElbowTwist", "RightHandTwist"};
    static string[] siblingNames = new string[] {"LeftHand", "RightHand", "LeftElbow", "RightElbow"};

    [MenuItem("GameObject/Setup Twistbone <3", false, 0)]
    static void SetupTwistBone()
   {
       try {
        GameObject root = Selection.activeGameObject;
            foreach (Transform child in root.GetComponentsInChildren<Transform>())
            {
                foreach (string name in objectNames)
                {
                    if (name == child.name)
                    {
                        foreach (Transform bone in child)
                        {
                            foreach (string twistName in twistNames)
                            {
                                if (bone.name == twistName)
                                {
                                    RotationConstraint rc = Undo.AddComponent<RotationConstraint>(bone.gameObject);
                                    rc.weight = 0.5f;
                                    ConstraintSource source = new ConstraintSource();
                                    foreach (string sibling in siblingNames)
                                    {
                                        foreach (Transform siblingBone in child)
                                        {
                                            if (sibling == siblingBone.name)
                                            source.sourceTransform = siblingBone;
                                        }
                                    }
                                    source.weight = 1f;
                                    rc.AddSource(source);
                                    rc.rotationAxis = Axis.Y;
                                    rc.locked = true;
                                    rc.constraintActive = true;
                                    
                                    EditorUtility.SetDirty(bone);
                                }
                            }
                        }
                    }
                }
            }
            Debug.Log("Setup Twistbones Succesfully (probably)");
       } 
       catch (System.Exception e)  {
           Debug.Log("Something went wrong setting twist bones :(");
           Debug.LogError(e.ToString());
       }
   }

}

