using System.IO;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace TopDownGame.Editor
{
    public static class AltRealmSetup
    {
        [MenuItem("Tools/Setup/Generate Alternate Realm Scenes")]
        public static void GenerateAltRealmScenes()
        {
            SetupAltLobby();
            SetupAltLeftHallway();
            SetupAltRightHallway();
            UpdateNormalLobbyPortal();
            UpdateBuildSettings();
            Debug.Log("[AltRealmSetup] Successfully generated and configured AltLobby, AltLeftHallway, and AltRightHallway!");
        }

        public static void UpdateNormalLobbyPortal()
        {
            EditorSceneManager.OpenScene("Assets/Scenes/Lobby.unity", OpenSceneMode.Single);

            var portal = GameObject.Find("Portal");
            if (portal != null)
            {
                var pc = portal.GetComponent<PortalController>();
                if (pc != null)
                {
                    pc.SetConfig("AltLobby", new Vector2(0f, 2.5f), CharacterAnimator2D.FacingDirection.Down);
                    EditorUtility.SetDirty(pc);
                }
            }

            EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene(), "Assets/Scenes/Lobby.unity");
            Debug.Log("[AltRealmSetup] Updated normal Lobby portal destination to AltLobby!");
        }

        public static void SetupAltLobby()
        {
            EditorSceneManager.OpenScene("Assets/Scenes/Lobby.unity", OpenSceneMode.Single);

            // 1. Swap background sprite to AltLobbyBackground
            var bgObj = GameObject.Find("LobbyBackground");
            if (bgObj != null)
            {
                var sr = bgObj.GetComponent<SpriteRenderer>();
                var altSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/AltLobbyBackground.png");
                if (sr != null && altSprite != null)
                {
                    sr.sprite = altSprite;
                    bgObj.transform.localScale = new Vector3(2f, 2f, 1f);
                    EditorUtility.SetDirty(sr);
                }
            }

            // 2. Configure Portal to return to normal Lobby
            var portal = GameObject.Find("Portal");
            if (portal != null)
            {
                var pc = portal.GetComponent<PortalController>();
                if (pc != null)
                {
                    pc.SetConfig("Lobby", new Vector2(0f, 2.5f), CharacterAnimator2D.FacingDirection.Down);
                    EditorUtility.SetDirty(pc);
                }
            }

            // 3. Connect passages to AltLeftHallway and AltRightHallway
            var toLeft = GameObject.Find("ToLeftHallway");
            if (toLeft != null)
            {
                toLeft.name = "ToAltLeftHallway";
                var trans = toLeft.GetComponent<SceneTransitionTrigger>();
                if (trans != null)
                {
                    trans.SetConfig("AltLeftHallway", new Vector2(7.5f, -0.5f), CharacterAnimator2D.FacingDirection.SideLeft);
                    EditorUtility.SetDirty(trans);
                }
            }

            var toRight = GameObject.Find("ToRightHallway");
            if (toRight != null)
            {
                toRight.name = "ToAltRightHallway";
                var trans = toRight.GetComponent<SceneTransitionTrigger>();
                if (trans != null)
                {
                    trans.SetConfig("AltRightHallway", new Vector2(-7.5f, -0.5f), CharacterAnimator2D.FacingDirection.SideRight);
                    EditorUtility.SetDirty(trans);
                }
            }

            // 4. Atmosphere: warm atmospheric tint on Global Light 2D
            var globalLight = GameObject.Find("Global Light 2D");
            if (globalLight != null)
            {
                var light2d = globalLight.GetComponent<Light2D>();
                if (light2d != null)
                {
                    light2d.color = new Color(1.0f, 0.72f, 0.55f, 1f);
                    light2d.intensity = 1.05f;
                    EditorUtility.SetDirty(light2d);
                }
            }

            EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene(), "Assets/Scenes/AltLobby.unity");
            Debug.Log("[AltRealmSetup] Saved Assets/Scenes/AltLobby.unity successfully!");
        }

        public static void SetupAltLeftHallway()
        {
            EditorSceneManager.OpenScene("Assets/Scenes/LeftHallway.unity", OpenSceneMode.Single);

            // 1. Swap background sprite to AltLeftHallway
            var bgObj = GameObject.Find("HallwayBackground");
            if (bgObj != null)
            {
                var sr = bgObj.GetComponent<SpriteRenderer>();
                var altSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/AltLeftHallway.png");
                if (sr != null && altSprite != null)
                {
                    sr.sprite = altSprite;
                    bgObj.transform.localScale = new Vector3(2f, 2f, 1f);
                    EditorUtility.SetDirty(sr);
                }
            }

            // 2. Connect passage back to AltLobby
            var toLobby = GameObject.Find("ToLobby");
            if (toLobby != null)
            {
                toLobby.name = "ToAltLobby";
                var trans = toLobby.GetComponent<SceneTransitionTrigger>();
                if (trans != null)
                {
                    trans.SetConfig("AltLobby", new Vector2(-8.6f, -0.5f), CharacterAnimator2D.FacingDirection.SideRight);
                    EditorUtility.SetDirty(trans);
                }
            }

            // 3. Atmosphere
            var globalLight = GameObject.Find("Global Light 2D");
            if (globalLight != null)
            {
                var light2d = globalLight.GetComponent<Light2D>();
                if (light2d != null)
                {
                    light2d.color = new Color(1.0f, 0.72f, 0.55f, 1f);
                    light2d.intensity = 1.05f;
                    EditorUtility.SetDirty(light2d);
                }
            }

            EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene(), "Assets/Scenes/AltLeftHallway.unity");
            Debug.Log("[AltRealmSetup] Saved Assets/Scenes/AltLeftHallway.unity successfully!");
        }

        public static void SetupAltRightHallway()
        {
            EditorSceneManager.OpenScene("Assets/Scenes/RightHallway.unity", OpenSceneMode.Single);

            // 1. Swap background sprite to AltRightHallway
            var bgObj = GameObject.Find("HallwayBackground");
            if (bgObj != null)
            {
                var sr = bgObj.GetComponent<SpriteRenderer>();
                var altSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/AltRightHallway.png");
                if (sr != null && altSprite != null)
                {
                    sr.sprite = altSprite;
                    bgObj.transform.localScale = new Vector3(2f, 2f, 1f);
                    EditorUtility.SetDirty(sr);
                }
            }

            // 2. Connect passage back to AltLobby
            var toLobby = GameObject.Find("ToLobby");
            if (toLobby != null)
            {
                toLobby.name = "ToAltLobby";
                var trans = toLobby.GetComponent<SceneTransitionTrigger>();
                if (trans != null)
                {
                    trans.SetConfig("AltLobby", new Vector2(8.6f, -0.5f), CharacterAnimator2D.FacingDirection.SideLeft);
                    EditorUtility.SetDirty(trans);
                }
            }

            // 3. Atmosphere
            var globalLight = GameObject.Find("Global Light 2D");
            if (globalLight != null)
            {
                var light2d = globalLight.GetComponent<Light2D>();
                if (light2d != null)
                {
                    light2d.color = new Color(1.0f, 0.72f, 0.55f, 1f);
                    light2d.intensity = 1.05f;
                    EditorUtility.SetDirty(light2d);
                }
            }

            EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene(), "Assets/Scenes/AltRightHallway.unity");
            Debug.Log("[AltRealmSetup] Saved Assets/Scenes/AltRightHallway.unity successfully!");
        }

        public static void UpdateBuildSettings()
        {
            string[] requiredScenes = new string[]
            {
                "Assets/Scenes/MainMenu.unity",
                "Assets/Scenes/CharacterSelect.unity",
                "Assets/Scenes/Lobby.unity",
                "Assets/Scenes/LeftHallway.unity",
                "Assets/Scenes/RightHallway.unity",
                "Assets/Scenes/AltLobby.unity",
                "Assets/Scenes/AltLeftHallway.unity",
                "Assets/Scenes/AltRightHallway.unity",
                "Assets/Scenes/Room_1.unity",
                "Assets/Scenes/Room_2.unity",
                "Assets/Scenes/Room_3.unity",
                "Assets/Scenes/Room_4.unity",
                "Assets/Scenes/Room_5.unity",
                "Assets/Scenes/Room_6.unity",
                "Assets/Scenes/Room_7.unity",
                "Assets/Scenes/Room_8.unity",
                "Assets/Scenes/Room_9.unity",
                "Assets/Scenes/Room_10.unity",
                "Assets/Scenes/Room_11.unity",
                "Assets/Scenes/Room_12.unity"
            };

            List<EditorBuildSettingsScene> sceneList = new List<EditorBuildSettingsScene>();
            foreach (var path in requiredScenes)
            {
                if (File.Exists(path))
                {
                    sceneList.Add(new EditorBuildSettingsScene(path, true));
                }
            }

            EditorBuildSettings.scenes = sceneList.ToArray();
            Debug.Log($"[AltRealmSetup] Build Settings updated with {sceneList.Count} scenes (including AltLobby, AltLeftHallway, and AltRightHallway)!");
        }
    }
}
