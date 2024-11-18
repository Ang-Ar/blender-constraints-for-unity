using BlenderConstraints;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

public class ConvertBlenderConstraints : EditorWindow
{
    static bool useAnimationRigging = false;
    static bool updateInEditMode = true;
    static UpdateMode updateMode;

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
            updateMode = (UpdateMode) EditorGUILayout.EnumPopup("update mode", updateMode);
        }

        EditorGUILayout.Space();

        if (GUILayout.Button("Convert Selected Only"))
        {
            ConvertSelected(recursive: false);
            // this.Close();
        }
        if (GUILayout.Button("Convert Selected & Children"))
        {
            ConvertSelected(recursive: true);
            // this.Close();
        }
    }

    static public void ConvertSelected(bool recursive)
    {
        GameObject[] selected = Selection.gameObjects;
        List<IBlenderConstraint> constraintsForConversion = new();
        foreach (GameObject go in selected)
        {
            IBlenderConstraint[] blenderConstraintsInGO =
                recursive ?
                go.GetComponentsInChildren<IBlenderConstraint>(includeInactive: true) :
                go.GetComponents<IBlenderConstraint>();
            constraintsForConversion.AddRange(blenderConstraintsInGO);
            Debug.Log($"Found {blenderConstraintsInGO.Length} constraints in \"{go.name}\"");
        }
        ConvertComponents(constraintsForConversion, useAnimationRigging, updateInEditMode, updateMode);
        Debug.Log($"Converted {constraintsForConversion.Count} constraints total");
    }

    /// <summary>
    /// Identical to calling target.ConvertComponent() with the same settings.
    /// </summary>
    static public void ConvertComponent(
        IBlenderConstraint target,
        bool useAnimationRigging,
        bool updateInEditMode = true,
        UpdateMode updateMode = UpdateMode.Update)
    {
        target = target.ConvertComponent(useAnimationRigging, updateInEditMode, updateMode);
    }

    /// <summary>
    /// Identical to calling target.ConvertComponent() with the same settings.
    /// The conversions are grouped into a single undo operation.
    /// </summary>
    static public void ConvertComponents(
        IEnumerable<IBlenderConstraint> targets, 
        bool useAnimationRigging,
        bool updateInEditMode = true,
        UpdateMode updateMode = UpdateMode.Update)
    {
        foreach (var target in targets)
        {
            ConvertComponent(target, useAnimationRigging, updateInEditMode, updateMode);
        }
        Undo.SetCurrentGroupName("batch convert blender constraints");
    }
}
