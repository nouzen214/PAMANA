using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using TMPro;

namespace TopDownGame.Editor
{
    public static class RoomSceneGenerator
    {
        [MenuItem("Tools/Generate All 12 Room Scenes")]
        public static void GenerateAllRooms()
        {
            string originalScenePath = EditorSceneManager.GetActiveScene().path;

            // Ensure directory exists
            if (!Directory.Exists("Assets/Scenes"))
                Directory.CreateDirectory("Assets/Scenes");

            // Load prefabs and sprites
            var playerPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Player.prefab");
            var canvasPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/InGameCanvas.prefab");
            var tablePrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/TablePedestal.prefab");
            var roomSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/Room_Area.png");

            if (playerPrefab == null || canvasPrefab == null || roomSprite == null || tablePrefab == null)
            {
                Debug.LogError($"[RoomSceneGenerator] Missing required asset! Player: {playerPrefab != null}, Canvas: {canvasPrefab != null}, Sprite: {roomSprite != null}, Table: {tablePrefab != null}");
                return;
            }

            List<string> generatedPaths = new List<string>();

            for (int i = 1; i <= 12; i++)
            {
                string scenePath = $"Assets/Scenes/Room_{i}.unity";
                GenerateSingleRoom(i, scenePath, playerPrefab, canvasPrefab, tablePrefab, roomSprite);
                generatedPaths.Add(scenePath);
            }

            // Register in Build Settings
            UpdateBuildSettings(generatedPaths);

            // Reopen original scene
            if (!string.IsNullOrEmpty(originalScenePath))
            {
                EditorSceneManager.OpenScene(originalScenePath);
            }

            Debug.Log($"[RoomSceneGenerator] Successfully created and registered all 12 Room scenes in Build Settings!");
        }

        private static void GenerateSingleRoom(int roomNumber, string scenePath, GameObject playerPrefab, GameObject canvasPrefab, GameObject tablePrefab, Sprite roomSprite)
        {
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            // 1. Camera
            var camObj = new GameObject("Main Camera", typeof(Camera), typeof(AudioListener));
            camObj.tag = "MainCamera";
            camObj.transform.position = new Vector3(0f, 0f, -10f);
            var cam = camObj.GetComponent<Camera>();
            cam.orthographic = true;
            cam.orthographicSize = 4.58f;
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = new Color(0.08f, 0.07f, 0.06f, 1f);
            camObj.AddComponent<UniversalAdditionalCameraData>();

            // 2. Global Light 2D
            var lightObj = new GameObject("Global Light 2D");
            var globalLight = lightObj.AddComponent<Light2D>();
            globalLight.lightType = Light2D.LightType.Global;
            globalLight.intensity = 0.85f;
            globalLight.color = new Color(1f, 0.95f, 0.88f, 1f);

            // 3. Torches
            var torchesParent = new GameObject("Torches");
            Vector3[] torchPositions = new Vector3[]
            {
                new Vector3(-3.7f, 2.1f, 0f),
                new Vector3(3.7f, 2.1f, 0f),
                new Vector3(-6.0f, 0.4f, 0f),
                new Vector3(6.0f, 0.4f, 0f)
            };
            string[] torchNames = new string[] { "Torch_TopLeft", "Torch_TopRight", "Torch_Left", "Torch_Right" };
            for (int t = 0; t < torchPositions.Length; t++)
            {
                var tObj = new GameObject(torchNames[t]);
                tObj.transform.SetParent(torchesParent.transform);
                tObj.transform.position = torchPositions[t];
                var pLight = tObj.AddComponent<Light2D>();
                pLight.lightType = Light2D.LightType.Point;
                pLight.pointLightInnerRadius = 0.5f;
                pLight.pointLightOuterRadius = 3.0f;
                pLight.color = new Color(1f, 0.65f, 0.25f, 1f);
                pLight.intensity = 1.3f;
            }

            // 4. Room Background
            var bgObj = new GameObject("RoomBackground");
            bgObj.transform.position = new Vector3(0f, 0f, 0.05f);
            bgObj.transform.localScale = new Vector3(2.5f, 2.5f, 1f);
            var bgSr = bgObj.AddComponent<SpriteRenderer>();
            bgSr.sprite = roomSprite;
            bgSr.sortingOrder = -10;

            // 5. Room Walls
            var wallsParent = new GameObject("RoomWalls");
            CreateWall("TopWall", new Vector2(0f, 3.55f), new Vector2(14.0f, 3.0f), wallsParent);
            CreateWall("LeftWall", new Vector2(-7.85f, 0f), new Vector2(3.0f, 10.0f), wallsParent);
            CreateWall("RightWall", new Vector2(7.85f, 0f), new Vector2(3.0f, 10.0f), wallsParent);
            CreateWall("BottomWall_Left", new Vector2(-3.65f, -5.1f), new Vector2(5.5f, 2.0f), wallsParent);
            CreateWall("BottomWall_Right", new Vector2(3.65f, -5.1f), new Vector2(5.5f, 2.0f), wallsParent);

            // 6. Exit Door
            var exitObj = new GameObject("ExitDoor");
            exitObj.transform.position = new Vector3(0f, -4.4f, 0f);
            var exitCol = exitObj.AddComponent<BoxCollider2D>();
            exitCol.isTrigger = true;
            exitCol.size = new Vector2(1.6f, 1.0f);
            exitObj.AddComponent<RoomExit>();

            // 7. Tables
            if (tablePrefab != null)
            {
                var tablesParent = new GameObject("Tables");
                Vector3[] tablePositions = new Vector3[]
                {
                    new Vector3(-4.6f, 1.25f, 0f),  // Top-Left
                    new Vector3(4.6f, 1.25f, 0f),   // Top-Right
                    new Vector3(0.0f, 1.25f, 0f),   // Middle
                    new Vector3(-4.6f, -2.5f, 0f),  // Bottom-Left
                    new Vector3(4.6f, -2.5f, 0f)    // Bottom-Right
                };
                string[] tableNames = new string[]
                {
                    "Table_TopLeft", "Table_TopRight", "Table_Middle", "Table_BottomLeft", "Table_BottomRight"
                };

                for (int t = 0; t < tablePositions.Length; t++)
                {
                    var tInst = (GameObject)PrefabUtility.InstantiatePrefab(tablePrefab);
                    tInst.name = tableNames[t];
                    tInst.transform.SetParent(tablesParent.transform);
                    tInst.transform.position = tablePositions[t];
                }
            }

            // 8. Player
            var playerInst = (GameObject)PrefabUtility.InstantiatePrefab(playerPrefab);
            playerInst.transform.position = new Vector3(0f, -2.0f, 0f);
            playerInst.transform.rotation = Quaternion.identity;

            // 8. Canvas
            var canvasInst = (GameObject)PrefabUtility.InstantiatePrefab(canvasPrefab);

            // Wire Joystick
            var vJoy = canvasInst.GetComponentInChildren<VirtualJoystick>(true);
            var inputHandler = playerInst.GetComponent<PlayerInputHandler>();
            if (vJoy != null && inputHandler != null)
            {
                inputHandler.SetVirtualJoystick(vJoy);
            }

            // Wire Health UI
            var health = playerInst.GetComponent<Health>();
            var heartUI = canvasInst.GetComponentInChildren<HeartHealthUI>(true);
            if (heartUI != null && health != null)
            {
                var hField = typeof(HeartHealthUI).GetField("playerHealth", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                if (hField != null) hField.SetValue(heartUI, health);
            }
            var generalHealthUI = canvasInst.GetComponentInChildren<HealthUI>(true);
            if (generalHealthUI != null && health != null)
            {
                var ghField = typeof(HealthUI).GetField("playerHealth", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                if (ghField != null) ghField.SetValue(generalHealthUI, health);
            }

            // Wire Prompt Label
            var promptTr = canvasInst.transform.Find("InteractPrompt");
            TextMeshProUGUI promptTmp = promptTr != null ? promptTr.GetComponent<TextMeshProUGUI>() : null;
            var interactor = playerInst.GetComponentInChildren<DoorInteractor>(true);
            if (interactor != null && promptTmp != null)
            {
                interactor.SetPromptLabel(promptTmp);
            }

            // 9. Room Banner HUD
            var bannerObj = new GameObject("RoomBanner", typeof(RectTransform), typeof(CanvasGroup));
            bannerObj.transform.SetParent(canvasInst.transform, false);
            var bannerRt = bannerObj.GetComponent<RectTransform>();
            bannerRt.anchorMin = new Vector2(0.5f, 1f);
            bannerRt.anchorMax = new Vector2(0.5f, 1f);
            bannerRt.pivot = new Vector2(0.5f, 1f);
            bannerRt.anchoredPosition = new Vector2(0f, -25f);
            bannerRt.sizeDelta = new Vector2(400f, 90f);
            var bannerGroup = bannerObj.GetComponent<CanvasGroup>();

            var bannerTextObj = new GameObject("BannerText", typeof(RectTransform), typeof(TextMeshProUGUI));
            bannerTextObj.transform.SetParent(bannerObj.transform, false);
            var bTextRt = bannerTextObj.GetComponent<RectTransform>();
            bTextRt.anchorMin = Vector2.zero;
            bTextRt.anchorMax = Vector2.one;
            bTextRt.sizeDelta = Vector2.zero;
            var bannerTmp = bannerTextObj.GetComponent<TextMeshProUGUI>();
            bannerTmp.text = $"ROOM {roomNumber}";
            bannerTmp.alignment = TextAlignmentOptions.Center;
            bannerTmp.fontSize = 38f;
            bannerTmp.fontStyle = FontStyles.Bold;
            bannerTmp.color = new Color(0.96f, 0.82f, 0.35f, 1f);

            // Subtitle
            var subTextObj = new GameObject("SubtitleText", typeof(RectTransform), typeof(TextMeshProUGUI));
            subTextObj.transform.SetParent(bannerObj.transform, false);
            var sTextRt = subTextObj.GetComponent<RectTransform>();
            sTextRt.anchorMin = new Vector2(0f, 0f);
            sTextRt.anchorMax = new Vector2(1f, 0.35f);
            sTextRt.sizeDelta = Vector2.zero;
            var subTmp = subTextObj.GetComponent<TextMeshProUGUI>();
            subTmp.text = "ANCIENT CHAMBER";
            subTmp.alignment = TextAlignmentOptions.Center;
            subTmp.fontSize = 18f;
            subTmp.color = new Color(0.85f, 0.85f, 0.85f, 0.85f);

            // 10. RoomManager
            var roomMgrObj = new GameObject("RoomManager");
            var roomCtrl = roomMgrObj.AddComponent<RoomController>();
            roomCtrl.SetRoomIdentity(roomNumber, $"Room {roomNumber}");

            var rmFieldBanner = typeof(RoomController).GetField("roomBannerText", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (rmFieldBanner != null) rmFieldBanner.SetValue(roomCtrl, bannerTmp);
            var rmFieldGroup = typeof(RoomController).GetField("roomBannerGroup", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (rmFieldGroup != null) rmFieldGroup.SetValue(roomCtrl, bannerGroup);
            var rmFieldPrompt = typeof(RoomController).GetField("interactPromptText", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (rmFieldPrompt != null) rmFieldPrompt.SetValue(roomCtrl, promptTmp);

            // 11. EventSystem
            var esObj = new GameObject("EventSystem", typeof(UnityEngine.EventSystems.EventSystem), typeof(UnityEngine.InputSystem.UI.InputSystemUIInputModule));

            // Save scene
            EditorSceneManager.SaveScene(scene, scenePath);
        }

        private static void CreateWall(string name, Vector2 center, Vector2 size, GameObject parent)
        {
            var w = new GameObject(name);
            w.transform.SetParent(parent.transform);
            w.transform.position = new Vector3(center.x, center.y, 0f);
            var col = w.AddComponent<BoxCollider2D>();
            col.size = size;
        }

        private static void UpdateBuildSettings(List<string> roomScenePaths)
        {
            List<EditorBuildSettingsScene> buildScenes = new List<EditorBuildSettingsScene>(EditorBuildSettings.scenes);

            // Ensure base scenes exist first
            string[] baseScenes = new string[]
            {
                "Assets/Scenes/MainMenu.unity",
                "Assets/Scenes/CharacterSelect.unity",
                "Assets/Scenes/Lobby.unity"
            };

            foreach (var bPath in baseScenes)
            {
                if (!buildScenes.Exists(s => s.path == bPath) && File.Exists(bPath))
                {
                    buildScenes.Add(new EditorBuildSettingsScene(bPath, true));
                }
            }

            // Add room scenes
            foreach (var rPath in roomScenePaths)
            {
                if (!buildScenes.Exists(s => s.path == rPath))
                {
                    buildScenes.Add(new EditorBuildSettingsScene(rPath, true));
                }
            }

            EditorBuildSettings.scenes = buildScenes.ToArray();
            AssetDatabase.SaveAssets();
        }
    }
}
