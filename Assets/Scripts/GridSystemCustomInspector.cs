using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(GridSystem))]
public class GridSystemCustomInspector : Editor
{
    GridSystem gridSystem;
    private void OnEnable()
    {
        gridSystem = (GridSystem)target;
    }
    
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        EditorGUILayout.BeginVertical();
        GUI.enabled = !Application.isPlaying;
        gridSystem.isDebugOn = EditorGUILayout.Toggle("DebugOn", gridSystem.isDebugOn);
        if (gridSystem.isDebugOn)
        {
            EditorGUI.indentLevel += 1;
            gridSystem.isImageOn = EditorGUILayout.Toggle("ImageMode", gridSystem.isImageOn);
            if (gridSystem.isImageOn)
            {
                gridSystem.isLineOn = false;
            }
            else
            {
                gridSystem.isLineOn = true;
            }
            gridSystem.isLineOn = EditorGUILayout.Toggle("LineMode", gridSystem.isLineOn);
            if (gridSystem.isLineOn)
            {
                gridSystem.isImageOn = false;
            }
            else
            {
                gridSystem.isImageOn = true;
            }
            EditorGUI.indentLevel -= 1;
        }
        GUI.enabled = true;
        EditorGUILayout.EndVertical();
    }

}
