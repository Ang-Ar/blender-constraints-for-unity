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
    }
}
