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
            ConvertBlenderConstraints.ConvertComponent(target, useAnimationRigging: false);
        }
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
            ConvertBlenderConstraints.ConvertComponent(target, useAnimationRigging: true);
        }
    }
}
