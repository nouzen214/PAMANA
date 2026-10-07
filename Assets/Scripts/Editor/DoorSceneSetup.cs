using UnityEditor;
using UnityEngine;

namespace TopDownGame
{
    public static class DoorSceneSetup
    {
        [MenuItem("Tools/Setup All Doors")]
        public static void SetupDoors()
        {
            // --- Configure DoorClosed texture as sprite ---
            string[] texGuids = AssetDatabase.FindAssets("DoorClosed t:Texture2D", new[] { "Assets/Sprites" });
            Sprite doorSprite = null;
            if (texGuids.Length > 0)
            {
                string texPath = AssetDatabase.GUIDToAssetPath(texGuids[0]);
                var importer = (TextureImporter)AssetImporter.GetAtPath(texPath);
                importer.textureType = TextureImporterType.Sprite;
                importer.spritePixelsPerUnit = 100;
                importer.filterMode = FilterMode.Point;
                importer.textureCompression = TextureImporterCompression.Uncompressed;
                importer.SaveAndReimport();
                doorSprite = AssetDatabase.LoadAssetAtPath<Sprite>(texPath);
            }
            else
            {
                Debug.LogError("DoorClosed texture not found in Assets/Sprites!");
                return;
            }

            // --- Find hallway backgrounds for positioning reference ---
            GameObject leftHallway = GameObject.Find("LeftHallway");
            GameObject rightHallway = GameObject.Find("RightHallway");

            if (leftHallway == null || rightHallway == null)
            {
                Debug.LogError("Hallway backgrounds not found!");
                return;
            }

            float leftX = leftHallway.transform.position.x;   // -17.85
            float rightX = rightHallway.transform.position.x;  // 17.85
            float hallScale = leftHallway.transform.localScale.x; // 2.90176

            // --- Create parent container ---
            GameObject doorsParent = new GameObject("Doors");
            Undo.RegisterCreatedObjectUndo(doorsParent, "Create Doors");

            // --- Door positions ---
            // Left Hallway: 3 top doors, 3 bottom doors
            // Looking at the hallway sprites, doors are evenly spaced
            // Left hallway image is ~710x400 px, centered at (-17.85, 0)
            // At scale 2.90176 and 100 PPU, that's ~20.6 x 11.6 world units
            // The hallway extends from about x = -28.2 to x = -7.5

            // Top row doors (y ~= 2.5 in the hallway) 
            // Bottom row doors (y ~= -3.8 in the hallway)

            // Approximate door positions from the background art:
            // Left Hallway top doors:
            Vector3[] leftTopDoors = new Vector3[]
            {
                new Vector3(leftX - 5.0f, 2.8f, 0f),
                new Vector3(leftX - 0.5f, 2.8f, 0f),
                new Vector3(leftX + 3.8f, 2.8f, 0f),
            };

            // Left Hallway bottom doors:
            Vector3[] leftBottomDoors = new Vector3[]
            {
                new Vector3(leftX - 5.0f, -3.5f, 0f),
                new Vector3(leftX - 0.5f, -3.5f, 0f),
                new Vector3(leftX + 3.8f, -3.5f, 0f),
            };

            // Right Hallway top doors:
            Vector3[] rightTopDoors = new Vector3[]
            {
                new Vector3(rightX - 3.8f, 2.8f, 0f),
                new Vector3(rightX + 0.5f, 2.8f, 0f),
                new Vector3(rightX + 5.0f, 2.8f, 0f),
            };

            // Right Hallway bottom doors:
            Vector3[] rightBottomDoors = new Vector3[]
            {
                new Vector3(rightX - 3.8f, -3.5f, 0f),
                new Vector3(rightX + 0.5f, -3.5f, 0f),
                new Vector3(rightX + 5.0f, -3.5f, 0f),
            };

            float doorScale = 0.7f; // Scale to match the background door size

            int doorIndex = 0;
            CreateDoorGroup("LeftHall_TopDoor", leftTopDoors, doorSprite, doorScale, doorsParent, ref doorIndex, Vector2.up);
            CreateDoorGroup("LeftHall_BottomDoor", leftBottomDoors, doorSprite, doorScale, doorsParent, ref doorIndex, Vector2.down);
            CreateDoorGroup("RightHall_TopDoor", rightTopDoors, doorSprite, doorScale, doorsParent, ref doorIndex, Vector2.up);
            CreateDoorGroup("RightHall_BottomDoor", rightBottomDoors, doorSprite, doorScale, doorsParent, ref doorIndex, Vector2.down);

            // --- Setup DoorInteractor on Player ---
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                // Check if interactor child already exists
                Transform existingInteractor = player.transform.Find("InteractRange");
                if (existingInteractor == null)
                {
                    GameObject interactRange = new GameObject("InteractRange");
                    Undo.RegisterCreatedObjectUndo(interactRange, "Create InteractRange");
                    interactRange.transform.SetParent(player.transform);
                    interactRange.transform.localPosition = Vector3.zero;

                    var circle = interactRange.AddComponent<CircleCollider2D>();
                    circle.radius = 1.2f;
                    circle.isTrigger = true;

                    interactRange.AddComponent<DoorInteractor>();

                    Debug.Log("DoorInteractor added to Player!");
                }
                else
                {
                    Debug.Log("InteractRange already exists on Player.");
                }
            }

            // --- Save ---
            UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(
                UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene());

            Debug.Log($"Door setup complete! Created {doorIndex} doors.");
        }

        private static void CreateDoorGroup(string baseName, Vector3[] positions, Sprite sprite,
                                             float scale, GameObject parent, ref int index, Vector2 slideDir)
        {
            for (int i = 0; i < positions.Length; i++)
            {
                index++;
                string doorName = $"{baseName}_{i + 1}";

                // Door container (has the DoorController + trigger collider for interaction)
                GameObject doorObj = new GameObject(doorName);
                Undo.RegisterCreatedObjectUndo(doorObj, "Create Door " + doorName);
                doorObj.transform.SetParent(parent.transform);
                doorObj.transform.position = positions[i];

                // Door visual (child - this is what slides/moves)
                GameObject doorVisual = new GameObject("DoorVisual");
                doorVisual.transform.SetParent(doorObj.transform);
                doorVisual.transform.localPosition = Vector3.zero;
                doorVisual.transform.localScale = new Vector3(scale, scale, 1f);

                var sr = doorVisual.AddComponent<SpriteRenderer>();
                sr.sprite = sprite;
                sr.sortingOrder = 5; // Above background but below UI

                // Add DoorController to the visual (the part that actually moves)
                var door = doorVisual.AddComponent<DoorController>();

                // Add trigger collider to parent for interaction detection
                var triggerCol = doorObj.AddComponent<BoxCollider2D>();
                triggerCol.isTrigger = true;
                triggerCol.size = new Vector2(1.5f, 2.0f);

                // Add a solid collider to the visual to block movement when closed
                var solidCol = doorVisual.AddComponent<BoxCollider2D>();
                solidCol.isTrigger = false;
                solidCol.size = new Vector2(1.2f, 1.6f);
            }
        }
    }
}
