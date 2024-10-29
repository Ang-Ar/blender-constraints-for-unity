using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using BlenderConstraints;

[CustomEditor(typeof(BlenderCopyRotation))]
public class BlenderCopyRotationInspector : Editor
{

    public override void OnInspectorGUI()
    {
        BlenderCopyRotation target = (BlenderCopyRotation)this.target;
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("save constrained rest pose"))
        {
            Undo.RecordObject(target, "save rest pose (constrained)");
            target.data.savedRotationConstrained = target.data.constrained.localEulerAngles;
            Debug.Log($"Saved rest pose of constrained object (unity eulers) {target.data.savedRotationConstrained}");
        }
        if (GUILayout.Button("save target rest pose"))
        {
            Undo.RecordObject(target, "save rest pose (target)");
            target.data.savedRotationTarget = target.data.target.localEulerAngles;
            Debug.Log($"Saved rest pose of target object (unity eulers) {target.data.savedRotationTarget}");
        }
        EditorGUILayout.EndHorizontal();

        DrawDefaultInspector();
    }
}
