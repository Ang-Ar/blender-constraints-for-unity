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

        EditorGUILayout.Space();

        if (GUILayout.Button("convert to simple constraint"))
        {
            ConvertBlenderConstraints.ConvertComponent(target, useAnimationRigging: false);
        }
    }
}

[CustomEditor(typeof(BlenderCopyRotationSimple))]
public class BlenderCopyRotationSimpleInspector : Editor
{
    public override void OnInspectorGUI()
    {
        BlenderCopyRotationSimple target = (BlenderCopyRotationSimple)this.target;
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("save constrained rest pose"))
        {
            Undo.RecordObject(target, "save rest pose (constrained)");
            target.constrainedRestPose = target.constrained.localRotation;
            Debug.Log($"Saved rest pose of constrained object (unity eulers) {target.constrainedRestPose}");
        }
        if (GUILayout.Button("save target rest pose"))
        {
            Undo.RecordObject(target, "save rest pose (target)");
            target.targetRestPose = target.target.localRotation;
            Debug.Log($"Saved rest pose of target object (unity eulers) {target.targetRestPose}");
        }
        EditorGUILayout.EndHorizontal();

        DrawDefaultInspector();

        EditorGUILayout.Space();

        if (GUILayout.Button("convert to animation rigging constraint"))
        {
           ConvertBlenderConstraints.ConvertComponent(target, useAnimationRigging: true);
        }
    }
}
