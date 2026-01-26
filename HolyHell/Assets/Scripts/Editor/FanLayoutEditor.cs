using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(FanLayoutGroup))]
public class FanLayoutEditor : Editor
{
    public override void OnInspectorGUI()
    {
        // 1. Draw the default inspector (all your public variables)
        DrawDefaultInspector();

        // Add some spacing
        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Editor Controls", EditorStyles.boldLabel);

        // 2. Reference the original script
        FanLayoutGroup script = (FanLayoutGroup)target;

        // 3. Create the Button
        GUI.backgroundColor = Color.cyan; // Make the button stand out a bit
        if (GUILayout.Button("Refresh Layout", GUILayout.Height(30)))
        {
            script.LayoutCards();

            // Mark the scene as dirty so the changes are saved
            EditorUtility.SetDirty(script);
        }
        GUI.backgroundColor = Color.white;
    }
}
