using UnityEditor;
using UnityEngine;
using System.Collections.Generic;

public class UnusedAssetsWindow : EditorWindow
{
    private List<string> unusedAssets = new List<string>();
    private Vector2 scrollPosition;

    // Function to open the window and pass in the unused assets list
    public static void ShowWindow(List<string> unusedAssetsList)
    {
        UnusedAssetsWindow window = GetWindow<UnusedAssetsWindow>("Unused Assets");
        window.unusedAssets = unusedAssetsList;
        window.minSize = new Vector2(400, 300);
    }

    // Create the GUI for displaying unused assets
    private void OnGUI()
    {
        GUILayout.Label("Unused Assets", EditorStyles.boldLabel);

        // Display the total number of unused assets
        GUILayout.Label($"Total Unused Assets: {unusedAssets.Count}", EditorStyles.label);

        GUILayout.Space(10);

        if (unusedAssets.Count == 0)
        {
            GUILayout.Label("No unused assets found!");
        }
        else
        {
            // Scrollable view for unused assets
            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);
            foreach (var asset in unusedAssets)
            {
                EditorGUILayout.BeginHorizontal(EditorStyles.helpBox);

                GUILayout.Label(asset, EditorStyles.label);

                // Button to ping asset in the project window
                if (GUILayout.Button("Ping", GUILayout.Width(50)))
                {
                    PingAsset(asset);
                }

                EditorGUILayout.EndHorizontal();
            }
            EditorGUILayout.EndScrollView();
        }
    }

    // Function to ping the asset in the Unity Editor
    private void PingAsset(string assetPath)
    {
        Object obj = AssetDatabase.LoadAssetAtPath<Object>(assetPath);
        EditorGUIUtility.PingObject(obj);
    }
}
