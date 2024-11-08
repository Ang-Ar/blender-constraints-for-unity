using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class ConvertBlenderConstraints : EditorWindow
{
    static bool useAnimationRigging = false;
    static bool updateInEditMode = true;
    static BlenderConstraints.UpdateMode updateMode;

    string[] dropdownText = new string[] {"simple update", "animation rigging"};

    [MenuItem("Window/Convert Blender Constaints")]
    public static void ShowWindow()
    {
        EditorWindow.GetWindow<ConvertBlenderConstraints>();
    }

    public void OnGUI()
    {
        GUIStyle multilineLabelStyle = GUI.skin.label;
        multilineLabelStyle.wordWrap = true;
        GUILayout.Label("Convert all Blender Constraint components in selected gameobjects & their children to...", style: multilineLabelStyle);
        
        useAnimationRigging = EditorGUILayout.Popup(useAnimationRigging ? 1 : 0, dropdownText) == 1;

        if(!useAnimationRigging)
        {
            updateInEditMode = EditorGUILayout.Toggle("update in edit mode", updateInEditMode);
            updateMode = (BlenderConstraints.UpdateMode) EditorGUILayout.EnumPopup("update mode", updateMode);
        }

        EditorGUILayout.Space();

        if (GUILayout.Button("Convert"))
        {
            ConvertSelected();
            // this.Close();
        }
    }

    public void ConvertSelected()
    {
        GameObject[] selected = Selection.gameObjects;
        List<GameObject> list = new List<GameObject>();
        foreach (GameObject go in selected)
        {
            int constraintCount = 0;

            // TODO: implement better (polymorphic?) solution
            // TODO: fix double counting bug (converting A->B then B->B)
            foreach (var constraint in go.GetComponentsInChildren<BlenderConstraints.BlenderCopyRotation>(includeInactive: true))
            {
                ++constraintCount;
                BlenderCopyRotationInspector.ConvertComponent(constraint, useAnimationRigging, updateInEditMode, updateMode);
            }
            foreach (var constraint in go.GetComponentsInChildren<BlenderConstraints.BlenderCopyRotationSimple>(includeInactive: true))
            {
                ++constraintCount;
                BlenderCopyRotationSimpleInspector.ConvertComponent(constraint, useAnimationRigging, updateInEditMode, updateMode);
            }
            foreach (var constraint in go.GetComponentsInChildren<BlenderConstraints.BlenderDampedTrack>(includeInactive: true))
            {
                ++constraintCount;
                BlenderDampedTrackEditor.ConvertComponent(constraint, useAnimationRigging, updateInEditMode, updateMode);
            }
            foreach (var constraint in go.GetComponentsInChildren<BlenderConstraints.BlenderDampedTrackSimple>(includeInactive: true))
            {
                ++constraintCount;
                BlenderDampedTrackSimpleEditor.ConvertComponent(constraint, useAnimationRigging, updateInEditMode, updateMode);
            }

            Debug.Log($"Found & converted {constraintCount} constraints in \"{go.name}\"");
        }
    }
}
