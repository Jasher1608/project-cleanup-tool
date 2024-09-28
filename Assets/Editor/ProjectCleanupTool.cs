using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

public class ProjectCleanupTool : EditorWindow
{
    private bool findUnusedAssets = false;
    private bool findDuplicateAssets = false;
    private bool optimizeTexturesAndAudio = false;

    private Color headerColor = new Color(0.2f, 0.4f, 0.6f);
    private Color backgroundColor = new Color(0.85f, 0.85f, 0.85f);

    [MenuItem("Tools/Project Cleanup Tool")]
    public static void ShowWindow()
    {
        ProjectCleanupTool window = GetWindow<ProjectCleanupTool>("Project Cleanup Tool");
        window.minSize = new Vector2(350, 250);
    }

    private void OnGUI()
    {
        GUI.backgroundColor = backgroundColor;
        EditorGUILayout.BeginVertical(EditorStyles.helpBox);

        GUILayout.Space(10);
        DrawHeader();

        GUILayout.Space(20);

        findUnusedAssets = GUILayout.Toggle(findUnusedAssets, "Find Unused Assets", GetToggleStyle());
        GUILayout.Space(10);
        findDuplicateAssets = GUILayout.Toggle(findDuplicateAssets, "Find Duplicate Assets", GetToggleStyle());
        GUILayout.Space(10);
        optimizeTexturesAndAudio = GUILayout.Toggle(optimizeTexturesAndAudio, "Texture/Audio Optimisation Checker", GetToggleStyle());

        GUILayout.Space(20);

        if (GUILayout.Button("Run Cleanup", GetButtonStyle()))
        {
            RunCleanup();
        }

        EditorGUILayout.EndVertical();
    }

    private void DrawHeader()
    {
        Color previousColor = GUI.color;
        GUI.color = headerColor;

        EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);
        GUILayout.FlexibleSpace();
        GUILayout.Label("Project Cleanup Tool", EditorStyles.boldLabel);
        GUILayout.FlexibleSpace();
        EditorGUILayout.EndHorizontal();

        GUI.color = previousColor;
    }

    // Function to handle the logic when 'Run Cleanup' is clicked
    private void RunCleanup()
    {
        if (findUnusedAssets)
        {
            Debug.Log("Finding unused assets...");
            FindUnusedAssets();
        }

        if (findDuplicateAssets)
        {
            Debug.Log("Finding duplicate assets...");
        }

        if (optimizeTexturesAndAudio)
        {
            Debug.Log("Optimising textures and audio...");
        }

        // If no option is selected
        if (!findUnusedAssets && !findDuplicateAssets && !optimizeTexturesAndAudio)
        {
            Debug.LogWarning("Please select at least one option.");
        }
    }

    // Function to find unused assets
    private void FindUnusedAssets()
    {
        // Get all assets in the project
        string[] allAssets = AssetDatabase.GetAllAssetPaths();

        List<string> unusedAssets = new List<string>();

        // Check each asset to see if it is used in any scenes, prefabs, etc.
        foreach (var assetPath in allAssets)
        {
            if (IsAssetUnused(assetPath))
            {
                unusedAssets.Add(assetPath);
            }
        }

        // Display results in a new window
        UnusedAssetsWindow.ShowWindow(unusedAssets);
    }

    // Function to determine if an asset is unused
    private bool IsAssetUnused(string assetPath)
    {
        // Exclude certain directories and files (e.g., Editor folders, Resources, StreamingAssets, Packages, .prefs files)
        if (assetPath.StartsWith("Assets/Editor") ||
            assetPath.Contains("/Resources/") ||
            assetPath.Contains("/StreamingAssets/") ||
            assetPath.StartsWith("Packages/") ||                 // Exclude default packages
            assetPath.StartsWith("ProjectSettings/") ||          // Exclude project settings files
            assetPath.StartsWith("Assets/Standard Assets/") ||   // Exclude standard assets
            assetPath.StartsWith("Assets/TextMesh Pro/") ||      // Exclude TextMeshPro package
            assetPath.StartsWith("Assets/PostProcessing/") ||    // Exclude PostProcessing package
            assetPath.EndsWith(".prefs"))                        // Exclude .prefs files
        {
            return false;
        }

        // Check if the asset is a folder or meta file
        if (System.IO.Directory.Exists(assetPath) || assetPath.EndsWith(".meta"))
        {
            return false;
        }

        // If the asset is a sprite sheet (Texture2D), check if its sprites are used
        if (AssetDatabase.GetMainAssetTypeAtPath(assetPath) == typeof(Texture2D))
        {
            // Get the sprites from the sprite sheet
            Object[] sprites = AssetDatabase.LoadAllAssetRepresentationsAtPath(assetPath);

            // Check if any of the sprites are used
            foreach (Object sprite in sprites)
            {
                if (sprite is not Sprite)
                {
                    continue;
                }
                if (IsSpriteUsed((Sprite)sprite))
                {
                    return false;  // If any sprite is used, the whole sprite sheet is considered used
                }
            }

            // If no sprites are used, consider the sprite sheet unused
            return true;
        }

        // Check if the asset is referenced anywhere in the project
        string[] assetDependencies = AssetDatabase.GetDependencies(assetPath);
        if (assetDependencies.Length <= 1)  // If the only dependency is itself
        {
            return true;
        }

        return false;
    }

    // Function to check if a specific sprite is used
    private bool IsSpriteUsed(Sprite sprite)
    {
        string spritePath = AssetDatabase.GetAssetPath(sprite);

        // Check if the sprite is used in any animations, prefabs, or scenes
        string[] allAssets = AssetDatabase.GetAllAssetPaths();
        foreach (string asset in allAssets)
        {
            if (AssetDatabase.GetDependencies(asset).Contains(spritePath))
            {
                return true;  // The sprite is used in this asset
            }
        }

        return false;
    }

    // Custom styling for toggle buttons
    private GUIStyle GetToggleStyle()
    {
        GUIStyle toggleStyle = new GUIStyle(GUI.skin.toggle);
        toggleStyle.fontSize = 14;
        toggleStyle.fontStyle = FontStyle.Bold;

        // Adjust padding to avoid overlap with the checkbox
        toggleStyle.padding = new RectOffset(20, 10, 5, 5);
        toggleStyle.margin = new RectOffset(10, 10, 5, 5);

        return toggleStyle;
    }

    // Custom styling for the 'Run Cleanup' button
    private GUIStyle GetButtonStyle()
    {
        GUIStyle buttonStyle = new GUIStyle(GUI.skin.button);
        buttonStyle.fontSize = 16;
        buttonStyle.fontStyle = FontStyle.Bold;
        buttonStyle.padding = new RectOffset(10, 10, 10, 10);
        buttonStyle.normal.textColor = Color.white;
        buttonStyle.normal.background = MakeTex(600, 1, new Color(0.1f, 0.5f, 0.8f));
        return buttonStyle;
    }

    // Helper function to create a 1x1 texture with a specific color (for button backgrounds)
    private Texture2D MakeTex(int width, int height, Color col)
    {
        Color[] pix = new Color[width * height];
        for (int i = 0; i < pix.Length; i++)
        {
            pix[i] = col;
        }
        Texture2D result = new Texture2D(width, height);
        result.SetPixels(pix);
        result.Apply();
        return result;
    }
}
