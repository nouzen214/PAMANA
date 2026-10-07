using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace TopDownGame.Editor
{
    /// <summary>
    /// Ensures that playing the game in the Unity Editor or in builds always starts from MainMenu.
    /// </summary>
    [InitializeOnLoad]
    public static class GameStartupConfig
    {
        public const string MainMenuScenePath = "Assets/Scenes/MainMenu.unity";

        static GameStartupConfig()
        {
            ApplyPlayModeStartScene();
        }

        [MenuItem("Tools/Game/Set MainMenu as Play Mode Start Scene")]
        public static void ApplyPlayModeStartScene()
        {
            var sceneAsset = AssetDatabase.LoadAssetAtPath<SceneAsset>(MainMenuScenePath);
            if (sceneAsset != null)
            {
                EditorSceneManager.playModeStartScene = sceneAsset;
                // Debug.Log("[GameStartupConfig] MainMenu is set as the Play Mode Start Scene.");
            }
        }

        [MenuItem("Tools/Game/Clear Play Mode Start Scene (Play Current Active Scene)")]
        public static void ClearPlayModeStartScene()
        {
            EditorSceneManager.playModeStartScene = null;
            Debug.Log("[GameStartupConfig] Play Mode Start Scene cleared. Unity will play whichever scene is currently open.");
        }
    }
}
