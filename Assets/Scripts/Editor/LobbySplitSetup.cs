using System.IO;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace TopDownGame.Editor
{
    public static class LobbySplitSetup
    {
        [MenuItem("Tools/Setup/Split Lobby and Hallway Scenes")]
        public static void SplitLobbyAndHallways()
        {
            SetupLeftHallway();
            SetupRightHallway();
            SetupCleanLobby();
            UpdateBuildSettings();
            Debug.Log("[LobbySplitSetup] Successfully separated Lobby, LeftHallway, and RightHallway scenes!");
        }

        public static void SetupLeftHallway()
        {
            EditorSceneManager.OpenScene("Assets/Scenes/Lobby.unity", OpenSceneMode.Single);

            // 1. Remove other area backgrounds and props
            SafeDestroy("LobbyBackground");
            SafeDestroy("RightHallway");
            SafeDestroy("Portal");
            SafeDestroy("HazardZone");

            // 2. Adjust LeftHallway background to (0, 0, 0.05)
            var leftBg = GameObject.Find("LeftHallway");
            if (leftBg != null)
            {
                leftBg.name = "HallwayBackground";
                leftBg.transform.position = new Vector3(0f, 0f, 0.05f);
            }

            // 3. Walls
            var walls = GameObject.Find("RoomWalls");
            if (walls != null)
            {
                SafeDestroyChild(walls, "Lobby_TopWall_Left");
                SafeDestroyChild(walls, "Lobby_TopWall_Right");
                SafeDestroyChild(walls, "Lobby_PortalBackWall");
                SafeDestroyChild(walls, "Lobby_BottomWall");
                SafeDestroyChild(walls, "Lobby_LeftPillar_Top");
                SafeDestroyChild(walls, "Lobby_LeftPillar_Bottom");
                SafeDestroyChild(walls, "Lobby_RightPillar_Top");
                SafeDestroyChild(walls, "Lobby_RightPillar_Bottom");
                SafeDestroyChild(walls, "RightHall_TopWall");
                SafeDestroyChild(walls, "RightHall_BottomWall");
                SafeDestroyChild(walls, "RightHall_FarRightWall");

                ShiftChild(walls, "LeftHall_TopWall", 17.85f);
                ShiftChild(walls, "LeftHall_BottomWall", 17.85f);
                ShiftChild(walls, "LeftHall_FarLeftWall", 17.85f);
            }

            // 4. Doors (keep Room 1..6, remove Room 7..12)
            var doors = GameObject.Find("Doors");
            if (doors != null)
            {
                SafeDestroyChild(doors, "RightHall_TopDoor_1");
                SafeDestroyChild(doors, "RightHall_TopDoor_2");
                SafeDestroyChild(doors, "RightHall_TopDoor_3");
                SafeDestroyChild(doors, "RightHall_BottomDoor_1");
                SafeDestroyChild(doors, "RightHall_BottomDoor_2");
                SafeDestroyChild(doors, "RightHall_BottomDoor_3");

                ShiftChild(doors, "LeftHall_TopDoor_1", 17.85f);
                ShiftChild(doors, "LeftHall_TopDoor_2", 17.85f);
                ShiftChild(doors, "LeftHall_TopDoor_3", 17.85f);
                ShiftChild(doors, "LeftHall_BottomDoor_1", 17.85f);
                ShiftChild(doors, "LeftHall_BottomDoor_2", 17.85f);
                ShiftChild(doors, "LeftHall_BottomDoor_3", 17.85f);
            }

            // 5. Add Passage Trigger to Lobby on right side
            SafeDestroy("ToLobby");
            var toLobby = new GameObject("ToLobby");
            toLobby.transform.position = new Vector3(8.9f, -0.5f, 0f);
            var col = toLobby.AddComponent<BoxCollider2D>();
            col.isTrigger = true;
            col.size = new Vector2(1.2f, 2.6f);
            var trans = toLobby.AddComponent<SceneTransitionTrigger>();
            trans.SetConfig("Lobby", new Vector2(-8.6f, -0.5f), CharacterAnimator2D.FacingDirection.SideRight);

            // 6. Camera Settings
            SetupCameraBounds();

            // 7. Player position and SpawnHandler
            var player = GameObject.FindWithTag("Player");
            if (player != null)
            {
                player.transform.position = new Vector3(7.5f, -0.5f, 0f);
                if (player.GetComponent<LobbySpawnHandler>() == null)
                    player.AddComponent<LobbySpawnHandler>();
            }

            EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene(), "Assets/Scenes/LeftHallway.unity");
            Debug.Log("[LobbySplitSetup] LeftHallway.unity saved successfully!");
        }

        public static void SetupRightHallway()
        {
            EditorSceneManager.OpenScene("Assets/Scenes/Lobby.unity", OpenSceneMode.Single);

            // 1. Remove other area backgrounds and props
            SafeDestroy("LobbyBackground");
            SafeDestroy("LeftHallway");
            SafeDestroy("Portal");
            SafeDestroy("HazardZone");

            // 2. Adjust RightHallway background to (0, 0, 0.05)
            var rightBg = GameObject.Find("RightHallway");
            if (rightBg != null)
            {
                rightBg.name = "HallwayBackground";
                rightBg.transform.position = new Vector3(0f, 0f, 0.05f);
            }

            // 3. Walls
            var walls = GameObject.Find("RoomWalls");
            if (walls != null)
            {
                SafeDestroyChild(walls, "Lobby_TopWall_Left");
                SafeDestroyChild(walls, "Lobby_TopWall_Right");
                SafeDestroyChild(walls, "Lobby_PortalBackWall");
                SafeDestroyChild(walls, "Lobby_BottomWall");
                SafeDestroyChild(walls, "Lobby_LeftPillar_Top");
                SafeDestroyChild(walls, "Lobby_LeftPillar_Bottom");
                SafeDestroyChild(walls, "Lobby_RightPillar_Top");
                SafeDestroyChild(walls, "Lobby_RightPillar_Bottom");
                SafeDestroyChild(walls, "LeftHall_TopWall");
                SafeDestroyChild(walls, "LeftHall_BottomWall");
                SafeDestroyChild(walls, "LeftHall_FarLeftWall");

                ShiftChild(walls, "RightHall_TopWall", -17.85f);
                ShiftChild(walls, "RightHall_BottomWall", -17.85f);
                ShiftChild(walls, "RightHall_FarRightWall", -17.85f);
            }

            // 4. Doors (keep Room 7..12, remove Room 1..6)
            var doors = GameObject.Find("Doors");
            if (doors != null)
            {
                SafeDestroyChild(doors, "LeftHall_TopDoor_1");
                SafeDestroyChild(doors, "LeftHall_TopDoor_2");
                SafeDestroyChild(doors, "LeftHall_TopDoor_3");
                SafeDestroyChild(doors, "LeftHall_BottomDoor_1");
                SafeDestroyChild(doors, "LeftHall_BottomDoor_2");
                SafeDestroyChild(doors, "LeftHall_BottomDoor_3");

                ShiftChild(doors, "RightHall_TopDoor_1", -17.85f);
                ShiftChild(doors, "RightHall_TopDoor_2", -17.85f);
                ShiftChild(doors, "RightHall_TopDoor_3", -17.85f);
                ShiftChild(doors, "RightHall_BottomDoor_1", -17.85f);
                ShiftChild(doors, "RightHall_BottomDoor_2", -17.85f);
                ShiftChild(doors, "RightHall_BottomDoor_3", -17.85f);
            }

            // 5. Add Passage Trigger to Lobby on left side
            SafeDestroy("ToLobby");
            var toLobby = new GameObject("ToLobby");
            toLobby.transform.position = new Vector3(-8.9f, -0.5f, 0f);
            var col = toLobby.AddComponent<BoxCollider2D>();
            col.isTrigger = true;
            col.size = new Vector2(1.2f, 2.6f);
            var trans = toLobby.AddComponent<SceneTransitionTrigger>();
            trans.SetConfig("Lobby", new Vector2(8.6f, -0.5f), CharacterAnimator2D.FacingDirection.SideLeft);

            // 6. Camera Settings
            SetupCameraBounds();

            // 7. Player position and SpawnHandler
            var player = GameObject.FindWithTag("Player");
            if (player != null)
            {
                player.transform.position = new Vector3(-7.5f, -0.5f, 0f);
                if (player.GetComponent<LobbySpawnHandler>() == null)
                    player.AddComponent<LobbySpawnHandler>();
            }

            EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene(), "Assets/Scenes/RightHallway.unity");
            Debug.Log("[LobbySplitSetup] RightHallway.unity saved successfully!");
        }

        public static void SetupCleanLobby()
        {
            EditorSceneManager.OpenScene("Assets/Scenes/Lobby.unity", OpenSceneMode.Single);

            // 1. Remove Hallway backgrounds
            SafeDestroy("LeftHallway");
            SafeDestroy("RightHallway");

            // 2. Remove Hallway walls
            var walls = GameObject.Find("RoomWalls");
            if (walls != null)
            {
                SafeDestroyChild(walls, "LeftHall_TopWall");
                SafeDestroyChild(walls, "LeftHall_BottomWall");
                SafeDestroyChild(walls, "LeftHall_FarLeftWall");
                SafeDestroyChild(walls, "RightHall_TopWall");
                SafeDestroyChild(walls, "RightHall_BottomWall");
                SafeDestroyChild(walls, "RightHall_FarRightWall");
            }

            // 3. Remove all hallway doors from Lobby (now separated in Hallway scenes)
            SafeDestroy("Doors");

            // 4. Add Left Passage Trigger (to LeftHallway)
            SafeDestroy("ToLeftHallway");
            var toLeft = new GameObject("ToLeftHallway");
            toLeft.transform.position = new Vector3(-9.9f, -0.5f, 0f);
            var colLeft = toLeft.AddComponent<BoxCollider2D>();
            colLeft.isTrigger = true;
            colLeft.size = new Vector2(1.0f, 2.4f);
            var transLeft = toLeft.AddComponent<SceneTransitionTrigger>();
            transLeft.SetConfig("LeftHallway", new Vector2(7.5f, -0.5f), CharacterAnimator2D.FacingDirection.SideLeft);

            // 5. Add Right Passage Trigger (to RightHallway)
            SafeDestroy("ToRightHallway");
            var toRight = new GameObject("ToRightHallway");
            toRight.transform.position = new Vector3(9.9f, -0.5f, 0f);
            var colRight = toRight.AddComponent<BoxCollider2D>();
            colRight.isTrigger = true;
            colRight.size = new Vector2(1.0f, 2.4f);
            var transRight = toRight.AddComponent<SceneTransitionTrigger>();
            transRight.SetConfig("RightHallway", new Vector2(-7.5f, -0.5f), CharacterAnimator2D.FacingDirection.SideRight);

            // 6. Camera Settings
            SetupCameraBounds();

            // 7. Player position and SpawnHandler
            var player = GameObject.FindWithTag("Player");
            if (player != null)
            {
                player.transform.position = new Vector3(0f, -1.9f, 0f);
                if (player.GetComponent<LobbySpawnHandler>() == null)
                    player.AddComponent<LobbySpawnHandler>();
            }

            EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene(), "Assets/Scenes/Lobby.unity");
            Debug.Log("[LobbySplitSetup] Lobby.unity cleaned and saved successfully!");
        }

        private static void SetupCameraBounds()
        {
            var cam = Camera.main;
            if (cam == null) cam = Object.FindFirstObjectByType<Camera>();
            if (cam != null)
            {
                var cf = cam.GetComponent<CameraFollow2D>();
                if (cf != null)
                {
                    SetField(cf, "useBounds", true);
                    SetField(cf, "mapMin", new Vector2(-10.24f, -5.76f));
                    SetField(cf, "mapMax", new Vector2(10.24f, 5.76f));
                    SetField(cf, "lockY", true);
                    SetField(cf, "lockedY", 0f);
                    EditorUtility.SetDirty(cf);
                }
            }
        }

        public static void UpdateBuildSettings()
        {
            var currentScenes = new List<EditorBuildSettingsScene>(EditorBuildSettings.scenes);
            string leftPath = "Assets/Scenes/LeftHallway.unity";
            string rightPath = "Assets/Scenes/RightHallway.unity";

            bool hasLeft = false;
            bool hasRight = false;
            foreach (var s in currentScenes)
            {
                if (s.path == leftPath) hasLeft = true;
                if (s.path == rightPath) hasRight = true;
            }

            // Find index of Lobby.unity to place them right after Lobby
            int lobbyIndex = currentScenes.FindIndex(s => s.path == "Assets/Scenes/Lobby.unity");
            int insertIndex = lobbyIndex >= 0 ? lobbyIndex + 1 : currentScenes.Count;

            if (!hasLeft)
            {
                currentScenes.Insert(insertIndex++, new EditorBuildSettingsScene(leftPath, true));
            }
            if (!hasRight)
            {
                currentScenes.Insert(insertIndex++, new EditorBuildSettingsScene(rightPath, true));
            }

            EditorBuildSettings.scenes = currentScenes.ToArray();
            Debug.Log("[LobbySplitSetup] Build Settings updated with LeftHallway and RightHallway!");
        }

        private static void SafeDestroy(string name)
        {
            var go = GameObject.Find(name);
            if (go != null) Object.DestroyImmediate(go);
        }

        private static void SafeDestroyChild(GameObject parent, string childName)
        {
            var t = parent.transform.Find(childName);
            if (t != null) Object.DestroyImmediate(t.gameObject);
        }

        private static void ShiftChild(GameObject parent, string childName, float deltaX)
        {
            var t = parent.transform.Find(childName);
            if (t != null)
            {
                t.position = new Vector3(t.position.x + deltaX, t.position.y, t.position.z);
            }
        }

        private static void SetField(object target, string fieldName, object value)
        {
            var field = target.GetType().GetField(fieldName, System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic);
            if (field != null)
            {
                field.SetValue(target, value);
            }
        }
    }
}
