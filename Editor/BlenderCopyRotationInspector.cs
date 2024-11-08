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

        EditorGUILayout.Space();

        if (GUILayout.Button("convert to simple constraint"))
        {
            ConvertComponent(target, useAnimationRigging: false);
        }
    }

    public static void ConvertComponent(BlenderCopyRotation target, bool useAnimationRigging, bool updateInEditMode=true, UpdateMode updateMode=UpdateMode.Update)
    {
        if (useAnimationRigging) return;

        Undo.RecordObject(target.gameObject, "convert to simple constraint");
        var simpleConstraint = target.gameObject.AddComponent<BlenderCopyRotationSimple>();
        simpleConstraint.weight = target.weight;
        simpleConstraint.constrained = target.data.constrained;
        simpleConstraint.target = target.data.target;
        simpleConstraint.constrainedRestPose = Quaternion.Euler(target.data.savedRotationConstrained);
        simpleConstraint.targetRestPose = Quaternion.Euler(target.data.savedRotationTarget);
        simpleConstraint.order = target.data.eulerAxisOrder;
        simpleConstraint.mask = target.data.includedAxes;
        DestroyImmediate(target);

        simpleConstraint.updateInEditMode = updateInEditMode;
        simpleConstraint.updateMode = updateMode;
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
            ConvertComponent(target, useAnimationRigging: true);
        }
    }

    public static void ConvertComponent(BlenderCopyRotationSimple target, bool useAnimationRigging, bool updateInEditMode = true, UpdateMode updateMode = UpdateMode.Update)
    {
        if (!useAnimationRigging)
        {
            Undo.RecordObject(target.gameObject, "change update settings");
            target.updateInEditMode = updateInEditMode;
            target.updateMode = updateMode;
            return;
        }

        Undo.RecordObject(target.gameObject, "convert to animation rigging constraint");
        var animRigConstraint = target.gameObject.AddComponent<BlenderCopyRotation>();
        animRigConstraint.weight = target.weight;
        animRigConstraint.data.constrained = target.constrained;
        animRigConstraint.data.target = target.target;
        animRigConstraint.data.savedRotationConstrained = target.constrainedRestPose.eulerAngles;
        animRigConstraint.data.savedRotationTarget = target.targetRestPose.eulerAngles;
        animRigConstraint.data.eulerAxisOrder = target.order;
        animRigConstraint.data.includedAxes = target.mask;
        DestroyImmediate(target);
    }
}
