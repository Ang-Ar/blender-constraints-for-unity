using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using BlenderConstraints;

[CustomEditor(typeof(BlenderDampedTrack))]
public class BlenderDampedTrackEditor : Editor
{

    public override void OnInspectorGUI()
    {
        BlenderDampedTrack target = (BlenderDampedTrack)this.target;
        if (GUILayout.Button("save constrained rest pose"))
        {
            Undo.RecordObject(target, "save rest pose (constrained)");
            target.data.savedRotationConstrained = target.data.constrained.localEulerAngles;
            Debug.Log($"Saved rest pose of constrained object (unity eulers) {target.data.savedRotationConstrained}");
        }

        DrawDefaultInspector();

        EditorGUILayout.Space();

        if (GUILayout.Button("convert to simple constraint"))
        {
            ConvertComponent(target, useAnimationRigging: false);
        }
    }

    public static void ConvertComponent(BlenderDampedTrack target, bool useAnimationRigging, bool updateInEditMode = true, UpdateMode updateMode = UpdateMode.Update)
    {
        if (useAnimationRigging) return;

        Undo.RecordObject(target.gameObject, "convert to simple constraint");
        var simpleConstraint = target.gameObject.AddComponent<BlenderDampedTrackSimple>();
        simpleConstraint.weight = target.weight;
        simpleConstraint.constrained = target.data.constrained;
        simpleConstraint.target = target.data.target;
        simpleConstraint.constrainedRestPose = Quaternion.Euler(target.data.savedRotationConstrained);
        simpleConstraint.axis = target.data.axis;
        DestroyImmediate(target);

        simpleConstraint.updateInEditMode = updateInEditMode;
        simpleConstraint.updateMode = updateMode;
    }
}

[CustomEditor(typeof(BlenderDampedTrackSimple))]
public class BlenderDampedTrackSimpleEditor : Editor
{

    public override void OnInspectorGUI()
    {
        BlenderDampedTrackSimple target = (BlenderDampedTrackSimple)this.target;
        if (GUILayout.Button("save constrained rest pose"))
        {
            Undo.RecordObject(target, "save rest pose (constrained)");
            target.constrainedRestPose = target.constrained.localRotation;
            Debug.Log($"Saved rest pose of constrained object (unity eulers) {target.constrainedRestPose}");
        }

        DrawDefaultInspector();

        EditorGUILayout.Space();

        if (GUILayout.Button("convert to ainmation rigging constraint"))
        {
            ConvertComponent(target, useAnimationRigging: true);
        }
    }

    public static void ConvertComponent(BlenderDampedTrackSimple target, bool useAnimationRigging, bool updateInEditMode = true, UpdateMode updateMode = UpdateMode.Update)
    {
        if (!useAnimationRigging)
        {
            Undo.RecordObject(target.gameObject, "change update settings");
            target.updateInEditMode = updateInEditMode;
            target.updateMode = updateMode;
            return;
        }

        Undo.RecordObject(target.gameObject, "convert to animation rigging constraint");
        var simpleConstraint = target.gameObject.AddComponent<BlenderDampedTrack>();
        simpleConstraint.weight = target.weight;
        simpleConstraint.data.constrained = target.constrained;
        simpleConstraint.data.target = target.target;
        simpleConstraint.data.savedRotationConstrained = target.constrainedRestPose.eulerAngles;
        simpleConstraint.data.axis = target.axis;
        DestroyImmediate(target);
    }
}
