using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

namespace TopDownGame
{
    /// <summary>
    /// Integrated in-game tutorial system that guides new players through the 1st gameplay sequence:
    /// 1. Inspect Field Guide pedestal in the Lobby (WASD / Joystick + [E]).
    /// 2. Travel through the West Wing doorway into the hallway.
    /// 3. Enter Room 1 (Pre-Colonial Era).
    /// 4. Inspect missing artifact pedestal to study the historical plaque clues.
    /// 5. Awaken the Alterworld Portal and return to the Main Lobby.
    /// 6. Enter the glowing Alterworld Portal to battle and recover the relics.
    /// 
    /// Features:
    /// - Animated bouncing golden arrow hovering directly over objectives in the world.
    /// - Clear action badges above the arrow (e.g. "[E] READ FIELD GUIDE", "WEST WING", "ROOM 1", "STUDY PLAQUE").
    /// - Pulsing ground target halo highlighting where to stand.
    /// - Sleek top-center Objective HUD Bar with step tracking and a discreet [✕ Skip] button.
    /// - Off-screen edge pointer when target is out of camera view.
    /// - Seamless persistence across scenes until the 1st sequence is finished.
    /// </summary>
    public class GameTutorialGuide : MonoBehaviour
    {
        public static GameTutorialGuide Instance { get; private set; }

        public enum TutorialPhase
        {
            Step1_FieldGuide = 0,         // Lobby: Inspect Book Pedestal
            Step2_LeftWing = 1,           // Lobby: Walk to West Wing doorway
            Step3_EnterRoom1 = 2,         // LeftHallway: Walk to Room 1
            Step4_StudyAllPlaques = 3,    // Room 1: Inspect all 5 artifact plaques to awaken portal
            Step5_GoToPortal = 4,         // Room 1 / LeftHallway / Lobby: Enter glowing portal
            Step6_AlterworldBattle = 5,   // AltLobby / AltLeftHallway / BattleScene: Face guardian in battle
            Step7_RestoreArtifact = 6,    // Lobby / LeftHallway / Room 1: Bring recovered relic to pedestal
            Completed = 7                 // Tutorial finished
        }

        private const string KEY_TUTORIAL_PHASE = "GameProgression_TutorialPhase";

        // ─── Visual Elements ───────────────────────────────────────────────────
        private GameObject _worldArrowContainer;
        private SpriteRenderer _arrowRenderer;
        private Transform _arrowTransform;
        private RectTransform _badgeRect;
        private TMP_Text _badgeText;
        private SpriteRenderer _groundHaloRenderer;
        private Transform _groundHaloTransform;

        private GameObject _objectiveBarObj;
        private CanvasGroup _objectiveBarCg;
        private TMP_Text _objectiveTitleText;
        private TMP_Text _objectiveBodyText;
        private GameObject _skipContainerObj;
        private CanvasGroup _skipContainerCg;
        private TMP_Text _skipPromptText;
        private Button _skipButton;

        private GameObject _offscreenIndicatorObj;
        private RectTransform _offscreenIndicatorRt;
        private TMP_Text _offscreenIndicatorText;

        // ─── State ────────────────────────────────────────────────────────────
        private TutorialPhase _currentPhase = TutorialPhase.Step1_FieldGuide;
        private Vector3 _currentTargetPos;
        private bool _hasTarget = false;
        private string _targetLabel = "";
        private float _arrowAngle = 0f; // 0 = down, 90 = right, -90 = left, 180 = up
        private float _baseArrowHeight = 1.1f;
        private Vector2 _badgeLocalOffset = new Vector2(0f, 0.48f);
        private Vector2 _badgeSizeDelta = new Vector2(250f, 44f);
        private Vector2 _haloLocalOffset = new Vector2(0f, -0.42f);
        private string _currentSceneName = "";

        private Sprite _arrowSprite;
        private Sprite _haloSprite;
        private Sprite _panelSprite;
        private Sprite _bannerSprite;

        public static TutorialPhase CurrentPhase
        {
            get
            {
                if (GameProgression.HasCompletedTutorial) return TutorialPhase.Completed;
                return (TutorialPhase)PlayerPrefs.GetInt(KEY_TUTORIAL_PHASE, 0);
            }
            set
            {
                PlayerPrefs.SetInt(KEY_TUTORIAL_PHASE, (int)value);
                PlayerPrefs.Save();
                if (Instance != null && Instance._currentPhase != value)
                {
                    Instance._currentPhase = value;
                    if (value != TutorialPhase.Completed)
                    {
                        Instance.RefreshPhaseObjectives();
                    }
                }
            }
        }

        // ─── Auto-Initialization ───────────────────────────────────────────────
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Bootstrap()
        {
            SceneManager.sceneLoaded -= OnSceneLoadedStatic;
            SceneManager.sceneLoaded += OnSceneLoadedStatic;
        }

        private static void OnSceneLoadedStatic(Scene scene, LoadSceneMode mode)
        {
            if (GameProgression.HasCompletedTutorial)
            {
                PurgeAllTutorialObjects();
                return;
            }

            if (IsTutorialScene(scene.name))
            {
                EnsureInstance();
                if (Instance != null)
                {
                    Instance.HandleSceneLoaded(scene, mode);
                }
            }
            else
            {
                if (Instance != null)
                {
                    Instance._currentSceneName = scene.name;
                    Instance.SetVisualsVisible(false);
                }
            }
        }

        public static void EnsureInstance()
        {
            if (GameProgression.HasCompletedTutorial)
            {
                PurgeAllTutorialObjects();
                return;
            }

            if (Instance == null)
            {
                var existing = FindFirstObjectByType<GameTutorialGuide>(FindObjectsInactive.Include);
                if (existing != null)
                {
                    Instance = existing;
                }
                else
                {
                    var go = new GameObject("GameTutorialGuide");
                    if (Application.isPlaying) DontDestroyOnLoad(go);
                    Instance = go.AddComponent<GameTutorialGuide>();
                }
            }
        }

        private static bool IsTutorialScene(string scene)
        {
            return scene == "Lobby" || scene == "LeftHallway" || scene == "Room_1" ||
                   scene == "AltLobby" || scene == "AltLeftHallway";
        }

        private void Awake()
        {
            if (GameProgression.HasCompletedTutorial)
            {
                PurgeAllTutorialObjects();
                return;
            }

            if (Instance == null)
            {
                Instance = this;
                if (Application.isPlaying) DontDestroyOnLoad(gameObject);
            }
            else if (Instance != this)
            {
                DestroyImmediate(gameObject);
                return;
            }

            LoadAssets();
            _currentSceneName = SceneManager.GetActiveScene().name;
            _currentPhase = CurrentPhase;
            BuildUI();
        }

        private void OnEnable()
        {
            GameProgression.OnBookUnlocked -= HandleBookUnlocked;
            GameProgression.OnBookUnlocked += HandleBookUnlocked;
            GameProgression.OnBookTopicReviewed -= HandleBookTopicReviewed;
            GameProgression.OnBookTopicReviewed += HandleBookTopicReviewed;
            GameProgression.OnPlaqueRead -= HandlePlaqueRead;
            GameProgression.OnPlaqueRead += HandlePlaqueRead;
            GameProgression.OnPortalUnlocked -= HandlePortalUnlocked;
            GameProgression.OnPortalUnlocked += HandlePortalUnlocked;
            GameProgression.OnPedestalRestored -= HandlePedestalRestored;
            GameProgression.OnPedestalRestored += HandlePedestalRestored;
            PlayerInventory.OnInventoryChanged -= HandleInventoryChanged;
            PlayerInventory.OnInventoryChanged += HandleInventoryChanged;
        }

        private void OnDisable()
        {
            GameProgression.OnBookUnlocked -= HandleBookUnlocked;
            GameProgression.OnBookTopicReviewed -= HandleBookTopicReviewed;
            GameProgression.OnPlaqueRead -= HandlePlaqueRead;
            GameProgression.OnPortalUnlocked -= HandlePortalUnlocked;
            GameProgression.OnPedestalRestored -= HandlePedestalRestored;
            PlayerInventory.OnInventoryChanged -= HandleInventoryChanged;
        }

        private void Start()
        {
            if (GameProgression.HasCompletedTutorial)
            {
                DismissAndDestroy();
                return;
            }

            _currentSceneName = SceneManager.GetActiveScene().name;
            BuildUI();
            RefreshPhaseObjectives();
        }

        private void OnDestroy()
        {
            OnDisable();
            if (Instance == this) Instance = null;
        }

        private void LoadAssets()
        {
#if UNITY_EDITOR
            if (_arrowSprite == null)
                _arrowSprite = UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/Tutorial_Arrow_Down.png");
            if (_bannerSprite == null)
                _bannerSprite = UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>("Assets/UI/NotificationBanner_Frame.png");
            if (_panelSprite == null)
                _panelSprite = UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>("Assets/UI/DialogueBox_Clean.png");
            if (_haloSprite == null)
                _haloSprite = UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>("Assets/UI/SilhouetteGlow.png");
#endif
            if (_arrowSprite == null) _arrowSprite = Resources.Load<Sprite>("Tutorial_Arrow_Down");
            if (_bannerSprite == null) _bannerSprite = Resources.Load<Sprite>("UI/NotificationBanner_Frame");
            if (_panelSprite == null) _panelSprite = Resources.Load<Sprite>("UI/DialogueBox_Clean");
            if (_haloSprite == null) _haloSprite = Resources.Load<Sprite>("UI/SilhouetteGlow");
        }

        private void HandleSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            _currentSceneName = scene.name;

            if (GameProgression.HasCompletedTutorial)
            {
                DismissAndDestroy();
                return;
            }

            if (!IsTutorialScene(scene.name))
            {
                // In non-tutorial scene (e.g. MainMenu, CharacterSelect, BattleScene), hide UI completely
                SetVisualsVisible(false);
                return;
            }

            SetVisualsVisible(true);
            BuildUI();
            RefreshPhaseObjectives();
        }

        // ─── Event Handlers ────────────────────────────────────────────────────
        private void HandleBookUnlocked()
        {
            RefreshPhaseObjectives();
        }

        private void HandleBookTopicReviewed(int topic)
        {
            if (topic == 1)
            {
                RefreshPhaseObjectives();
            }
        }

        private void HandlePlaqueRead(int roomNum, int count)
        {
            if (roomNum == 1)
            {
                RefreshPhaseObjectives();
            }
        }

        private void HandlePortalUnlocked()
        {
            RefreshPhaseObjectives();
        }

        private void HandlePedestalRestored(int roomNum, string artifactName)
        {
            if (roomNum == 1)
            {
                CompleteTutorial();
            }
        }

        private void HandleInventoryChanged()
        {
            RefreshPhaseObjectives();
        }

        // ─── Progression Helpers ───────────────────────────────────────────────
        public static bool HasAnyRoom1ArtifactInInventory()
        {
            if (PlayerInventory.Items == null || PlayerInventory.Items.Count == 0) return false;
            foreach (var item in PlayerInventory.Items)
            {
                if (item == null) continue;
                string id = (item.id ?? "").ToLowerInvariant();
                string name = (item.name ?? "").ToLowerInvariant();
                if (id.StartsWith("room1_") || id.Contains("pen") || id.Contains("seal") ||
                    id.Contains("notebook") || id.Contains("spectacles") || id.Contains("glasses") || id.Contains("locket") ||
                    name.Contains("pen") || name.Contains("seal") || name.Contains("notebook") ||
                    name.Contains("spectacles") || name.Contains("glasses") || name.Contains("locket"))
                {
                    return true;
                }
            }
            return false;
        }

        private static bool IsPlaqueStudied(ArtifactInteractable art)
        {
            if (art == null) return false;
            string key = "GameProgression_Room1_PlaquesList";
            string currentList = PlayerPrefs.GetString(key, "");
            if (string.IsNullOrEmpty(currentList)) return false;

            string[] items = currentList.Split(';');
            string pId = !string.IsNullOrEmpty(art.ArtifactName) ? art.ArtifactName : art.gameObject.name;

            for (int i = 0; i < items.Length; i++)
            {
                if (string.Equals(items[i], pId, StringComparison.OrdinalIgnoreCase) ||
                    string.Equals(items[i], art.gameObject.name, StringComparison.OrdinalIgnoreCase) ||
                    (!string.IsNullOrEmpty(art.ArtifactName) && string.Equals(items[i], art.ArtifactName, StringComparison.OrdinalIgnoreCase)))
                {
                    return true;
                }
            }
            return false;
        }

        private GameObject FindNextUnreadPlaqueTable()
        {
            var interactables = FindObjectsByType<ArtifactInteractable>(FindObjectsSortMode.None);
            if (interactables != null)
            {
                foreach (var art in interactables)
                {
                    if (art.RoomNumber == 1 && !IsPlaqueStudied(art))
                    {
                        return art.gameObject;
                    }
                }
            }
            return null;
        }

        private GameObject FindPedestalToRestore()
        {
            var interactables = FindObjectsByType<ArtifactInteractable>(FindObjectsSortMode.None);
            if (interactables == null || interactables.Length == 0) return null;

            // Check if any unrestored interactable matches an artifact currently held in bag
            if (PlayerInventory.Items != null && PlayerInventory.Items.Count > 0)
            {
                foreach (var invItem in PlayerInventory.Items)
                {
                    if (invItem == null) continue;
                    string invId = (invItem.id ?? "").ToLowerInvariant();
                    string invName = (invItem.name ?? "").ToLowerInvariant();

                    foreach (var art in interactables)
                    {
                        if (art.RoomNumber != 1 || art.IsRestored) continue;
                        string artName = (art.ArtifactName ?? "").ToLowerInvariant();
                        string goName = art.gameObject.name.ToLowerInvariant();

                        if (artName.Contains(invName) || invName.Contains(artName) ||
                            goName.Contains(invName) || invId.Contains(goName))
                        {
                            return art.gameObject;
                        }
                    }
                }
            }

            // Fallback: pick any unrestored pedestal in Room 1
            foreach (var art in interactables)
            {
                if (art.RoomNumber == 1 && !art.IsRestored)
                {
                    return art.gameObject;
                }
            }

            return null;
        }

        // ─── Phase Logic & Target Resolution ──────────────────────────────────
        public void RefreshPhaseObjectives()
        {
            if (GameProgression.HasCompletedTutorial || GameProgression.GetPedestalsRestoredCount(1) >= 1)
            {
                CompleteTutorial();
                return;
            }

            _currentSceneName = SceneManager.GetActiveScene().name;

            bool isBookDone = GameProgression.IsBookUnlocked || GameProgression.IsBookTopicReviewed(1);
            bool isAllPlaquesDone = GameProgression.AreAllPlaquesRead(1) || GameProgression.IsPortalUnlocked;
            bool hasRecoveredArtifact = GameProgression.GetRoomRetrievedCount(1) > 0 ||
                                        GameProgression.HasRetrievedRoom1Artifact ||
                                        HasAnyRoom1ArtifactInInventory();

            // Strict Step-by-Step State Machine
            if (!isBookDone)
            {
                // STEP 1: Must read field guide book first!
                _currentPhase = TutorialPhase.Step1_FieldGuide;
            }
            else if (hasRecoveredArtifact)
            {
                // STEP 7: Reclaimed artifact! Guide back to Room 1 to restore it.
                _currentPhase = TutorialPhase.Step7_RestoreArtifact;
            }
            else if (_currentSceneName == "BattleScene" || _currentSceneName == "BattleScene_1" ||
                     _currentSceneName == "AltLobby" || _currentSceneName == "AltLeftHallway")
            {
                // STEP 6: Inside the Alterworld / Battle!
                _currentPhase = TutorialPhase.Step6_AlterworldBattle;
            }
            else if (isAllPlaquesDone)
            {
                // STEP 5: All 5 plaques studied! Guide to the awakened portal.
                _currentPhase = TutorialPhase.Step5_GoToPortal;
            }
            else if (_currentSceneName == "Room_1" || GameProgression.GetPlaquesReadCount(1) > 0)
            {
                // STEP 4: In Room 1, study all 5 plaques
                _currentPhase = TutorialPhase.Step4_StudyAllPlaques;
            }
            else if (_currentSceneName == "LeftHallway")
            {
                // STEP 3: In LeftHallway, enter Room 1
                _currentPhase = TutorialPhase.Step3_EnterRoom1;
            }
            else
            {
                // STEP 2: In Lobby, head through West Wing doorway
                _currentPhase = TutorialPhase.Step2_LeftWing;
            }

            PlayerPrefs.SetInt(KEY_TUTORIAL_PHASE, (int)_currentPhase);

            BuildUI();

            bool isMobile = MobileControlsManager.IsMobileActive;

            switch (_currentPhase)
            {
                case TutorialPhase.Step1_FieldGuide:
                    ConfigureStep1(isMobile);
                    break;
                case TutorialPhase.Step2_LeftWing:
                    ConfigureStep2(isMobile);
                    break;
                case TutorialPhase.Step3_EnterRoom1:
                    ConfigureStep3(isMobile);
                    break;
                case TutorialPhase.Step4_StudyAllPlaques:
                    ConfigureStep4(isMobile);
                    break;
                case TutorialPhase.Step5_GoToPortal:
                    ConfigureStep5(isMobile);
                    break;
                case TutorialPhase.Step6_AlterworldBattle:
                    ConfigureStep6_Battle(isMobile);
                    break;
                case TutorialPhase.Step7_RestoreArtifact:
                    ConfigureStep7_Restore(isMobile);
                    break;
                case TutorialPhase.Completed:
                    DismissAndDestroy();
                    break;
            }
        }

        private void ConfigureStep1(bool isMobile)
        {
            if (_currentSceneName == "Lobby")
            {
                var book = GameObject.Find("BookPedestal") ?? FindFirstObjectByType<BookPedestal>()?.gameObject;
                Vector3 pos = book != null ? book.transform.position : new Vector3(-3.0f, -0.53f, 0f);

                SetTarget(pos, isMobile ? "TAP [E] TO READ" : "[E] READ FIELD GUIDE", 0f, 1.25f);
                SetObjectiveText(
                    "TUTORIAL • Step 1 of 5: Controls & Field Guide",
                    isMobile
                        ? "Use the Joystick to move. Walk to the Field Guide pedestal on the left and tap [E] to read."
                        : "Use WASD or Arrow Keys to move. Walk to the Field Guide pedestal on the left and press [E] to read."
                );
            }
            else
            {
                GuideBackToLobby("Walk back to the Main Lobby to inspect the Field Guide pedestal.");
            }
        }

        private void ConfigureStep2(bool isMobile)
        {
            if (_currentSceneName == "Lobby")
            {
                var door = GameObject.Find("ToLeftHallway") ?? GameObject.Find("Door_Left");
                Vector3 pos = door != null ? door.transform.position : new Vector3(-8.6f, -0.5f, 0f);

                // Point left toward the doorway
                SetTarget(pos + new Vector3(0.8f, 0f, 0f), "<-- WEST WING", -90f, 0.3f);
                SetObjectiveText(
                    "TUTORIAL • Step 2 of 5: The Exhibition Wing",
                    "Head left through the West Wing doorway to enter the museum exhibition hallway."
                );
            }
            else if (_currentSceneName == "LeftHallway")
            {
                RefreshPhaseObjectives();
            }
            else
            {
                GuideBackToLobby("Return to the Main Lobby to proceed through the West Wing.");
            }
        }

        private void ConfigureStep3(bool isMobile)
        {
            if (_currentSceneName == "LeftHallway")
            {
                var r1Door = GameObject.Find("LeftHall_TopDoor_1");
                if (r1Door == null)
                {
                    var doors = FindObjectsByType<RoomDoor>(FindObjectsSortMode.None);
                    foreach (var d in doors)
                    {
                        if (d.TargetSceneName == "Room_1") { r1Door = d.gameObject; break; }
                    }
                }
                Vector3 pos = r1Door != null ? r1Door.transform.position : new Vector3(-7.94f, 3.36f, 0f);

                // Arrow placed on the right of the door pointing left at the door, and text badge placed to the right of the arrow
                Vector3 targetPos = new Vector3(-7.00f, 2.85f, pos.z);
                SetTarget(targetPos, isMobile ? "TAP ROOM 1" : "[E] ROOM 1", -90f, 0f, new Vector2(1.00f, 0f), new Vector2(120f, 36f), new Vector2(-0.94f, -0.60f));
                SetObjectiveText(
                    "TUTORIAL • Step 3 of 5: Room 1 (Pre-Colonial Era)",
                    isMobile
                        ? "Walk down the hallway and tap [E] in front of Room 1 to enter."
                        : "Walk down the hallway and press [E] in front of Room 1 to enter."
                );
            }
            else if (_currentSceneName == "Room_1")
            {
                RefreshPhaseObjectives();
            }
            else
            {
                // In Lobby: guide to West Wing
                var door = GameObject.Find("ToLeftHallway") ?? GameObject.Find("Door_Left");
                Vector3 pos = door != null ? door.transform.position : new Vector3(-9.9f, -0.5f, 0f);
                SetTarget(new Vector3(-8.7f, pos.y, pos.z), "<-- WEST WING", -90f, 0.3f);
                SetObjectiveText("TUTORIAL • Step 3 of 5: Enter Room 1", "Head through the West Wing door on the left.");
            }
        }

        private void ConfigureStep4(bool isMobile)
        {
            int readCount = GameProgression.GetPlaquesReadCount(1);

            if (_currentSceneName == "Room_1")
            {
                var targetTable = FindNextUnreadPlaqueTable();
                Vector3 pos = targetTable != null ? targetTable.transform.position : new Vector3(-2.8f, 1.5f, 0f);

                SetTarget(pos, isMobile ? $"TAP [E] ({readCount}/5)" : $"[E] STUDY PLAQUE ({readCount}/5)", 0f, 1.15f);
                SetObjectiveText(
                    $"TUTORIAL • Step 4 of 5: Study Relics ({readCount}/5)",
                    isMobile
                        ? $"Inspect all 5 artifact pedestals to uncover clues ({readCount}/5 studied). Studying all plaques awakens the Lobby portal!"
                        : $"Inspect all 5 artifact pedestals to uncover clues ({readCount}/5 studied). Studying all plaques awakens the Lobby portal!"
                );
            }
            else
            {
                // In hallway or lobby, guide into Room 1
                ConfigureStep3(isMobile);
            }
        }

        private void ConfigureStep5(bool isMobile)
        {
            if (_currentSceneName == "Room_1")
            {
                var exit = GameObject.Find("ExitDoor") ?? FindFirstObjectByType<RoomExit>()?.gameObject;
                Vector3 pos = exit != null ? exit.transform.position : new Vector3(0f, -4.4f, 0f);

                SetTarget(new Vector3(pos.x, -3.60f, pos.z), "EXIT TO HALLWAY -->", 0f, 0.50f);
                SetObjectiveText(
                    "TUTORIAL • Step 5 of 5: Alterworld Portal Awakened",
                    "All 5 plaques studied! Exit Room 1 through the bottom door to return to the hallway."
                );
            }
            else if (_currentSceneName == "LeftHallway")
            {
                var toLobby = GameObject.Find("ToLobby");
                Vector3 pos = toLobby != null ? toLobby.transform.position : new Vector3(8.9f, -0.5f, 0f);

                SetTarget(new Vector3(pos.x - 1.2f, pos.y, pos.z), "TO LOBBY -->", 90f, 0.3f);
                SetObjectiveText(
                    "TUTORIAL • Step 5 of 5: Return to Main Lobby",
                    "Head east down the hallway to return to the Main Museum Lobby."
                );
            }
            else if (_currentSceneName == "Lobby")
            {
                var portal = GameObject.Find("Portal") ?? GameObject.Find("Lobby_Portal");
                Vector3 pos = portal != null ? portal.transform.position : new Vector3(0f, 3.65f, 0f);

                bool hasAttempted = GameProgression.HasAttemptedRoomBattle(1);
                string label = hasAttempted ? "RETRY BATTLE" : "ENTER PORTAL";
                string title = hasAttempted ? "TUTORIAL • Retake Alterworld Battle" : "TUTORIAL • Final Step: Enter the Alterworld";
                string body = hasAttempted
                    ? "You recovered 0 artifacts! Step into the glowing blue portal vortex to challenge the guardian again and reclaim the relics."
                    : "Step directly into the glowing blue portal vortex to challenge the shadow guardian and reclaim the relics!";

                // Arrow placed on the right of the portal pointing left at the vortex, and text badge placed to the right of the arrow
                Vector3 targetPos = new Vector3(pos.x + 1.15f, 3.25f, pos.z);
                SetTarget(targetPos, label, -90f, 0f, new Vector2(1.28f, 0f), new Vector2(165f, 36f), new Vector2(-1.15f, -0.85f));
                SetObjectiveText(title, body);
            }
            else
            {
                GuideBackToLobby("Head back to the Main Lobby and enter the glowing portal vortex.");
            }
        }

        private void ConfigureStep6_Battle(bool isMobile)
        {
            if (_currentSceneName == "AltLobby")
            {
                var door = GameObject.Find("ToAltLeftHallway") ?? GameObject.Find("Door_Left");
                Vector3 pos = door != null ? door.transform.position : new Vector3(-8.6f, -0.5f, 0f);

                SetTarget(pos + new Vector3(0.8f, 0f, 0f), "<-- ALT WEST WING", -90f, 0.3f);
                SetObjectiveText(
                    "TUTORIAL • Alterworld: West Wing",
                    "Head left through the doorway to reach the Room 1 guardian trial chamber."
                );
            }
            else if (_currentSceneName == "AltLeftHallway")
            {
                var r1Door = GameObject.Find("LeftHall_TopDoor_1");
                if (r1Door == null)
                {
                    var doors = FindObjectsByType<RoomDoor>(FindObjectsSortMode.None);
                    foreach (var d in doors)
                    {
                        if (d.TargetSceneName == "BattleScene" || d.TargetSceneName == "BattleScene_1")
                        {
                            r1Door = d.gameObject;
                            break;
                        }
                    }
                }
                Vector3 pos = r1Door != null ? r1Door.transform.position : new Vector3(-7.94f, 3.36f, 0f);

                // Arrow placed on the right of the door pointing left at the door, and text badge placed to the right of the arrow
                Vector3 targetPos = new Vector3(-7.00f, 2.85f, pos.z);
                SetTarget(targetPos, isMobile ? "TAP TRIAL 1" : "[E] TRIAL 1", -90f, 0f, new Vector2(1.00f, 0f), new Vector2(120f, 36f), new Vector2(-0.94f, -0.60f));
                SetObjectiveText(
                    "TUTORIAL • Alterworld: Room 1 Trial",
                    isMobile
                        ? "Walk to Room 1 door and tap [E] to begin the Alterworld guardian battle!"
                        : "Walk to Room 1 door and press [E] to begin the Alterworld guardian battle!"
                );
            }
            else if (_currentSceneName == "BattleScene" || _currentSceneName == "BattleScene_1")
            {
                // Inside battle scene: HIDE world arrows and indicators so we don't guide quiz answers!
                _hasTarget = false;
                if (_worldArrowContainer != null) _worldArrowContainer.SetActive(false);
                if (_offscreenIndicatorObj != null) _offscreenIndicatorObj.SetActive(false);

                SetObjectiveText(
                    "TUTORIAL • Alterworld Trial: Room 1 Guardian",
                    "Answer the quiz questions correctly to damage the guardian and reclaim the lost artifacts!"
                );
            }
            else
            {
                ConfigureStep5(isMobile);
            }
        }

        private void ConfigureStep7_Restore(bool isMobile)
        {
            if (_currentSceneName == "Lobby")
            {
                var door = GameObject.Find("ToLeftHallway") ?? GameObject.Find("Door_Left");
                Vector3 pos = door != null ? door.transform.position : new Vector3(-8.6f, -0.5f, 0f);

                SetTarget(pos + new Vector3(0.8f, 0f, 0f), "<-- WEST WING", -90f, 0.3f);
                SetObjectiveText(
                    "TUTORIAL • Return Relic: Exhibition Wing",
                    "You recovered an artifact! Head left through the West Wing doorway to return it to Room 1."
                );
            }
            else if (_currentSceneName == "LeftHallway")
            {
                var r1Door = GameObject.Find("LeftHall_TopDoor_1");
                if (r1Door == null)
                {
                    var doors = FindObjectsByType<RoomDoor>(FindObjectsSortMode.None);
                    foreach (var d in doors)
                    {
                        if (d.TargetSceneName == "Room_1") { r1Door = d.gameObject; break; }
                    }
                }
                Vector3 pos = r1Door != null ? r1Door.transform.position : new Vector3(-7.94f, 3.36f, 0f);

                // Arrow placed on the right of the door pointing left at the door, and text badge placed to the right of the arrow
                Vector3 targetPos = new Vector3(-7.00f, 2.85f, pos.z);
                SetTarget(targetPos, isMobile ? "TAP ROOM 1" : "[E] ROOM 1", -90f, 0f, new Vector2(1.00f, 0f), new Vector2(120f, 36f), new Vector2(-0.94f, -0.60f));
                SetObjectiveText(
                    "TUTORIAL • Return Relic: Enter Room 1",
                    isMobile
                        ? "Walk to Room 1 door and tap [E] to enter the gallery."
                        : "Walk to Room 1 door and press [E] to enter the gallery."
                );
            }
            else if (_currentSceneName == "Room_1")
            {
                var targetTable = FindPedestalToRestore();
                Vector3 pos = targetTable != null ? targetTable.transform.position : new Vector3(-2.8f, 1.5f, 0f);

                SetTarget(pos, isMobile ? "TAP [E] TO RESTORE" : "[E] RESTORE ARTIFACT", 0f, 1.15f);
                SetObjectiveText(
                    "TUTORIAL • Final Step: Restore Artifact",
                    isMobile
                        ? "Approach the empty pedestal, tap [E], and choose 'Place from bag' to restore the relic!"
                        : "Approach the empty pedestal, press [E], and choose 'Place from bag' to restore the relic!"
                );
            }
            else if (_currentSceneName == "AltLobby")
            {
                var portal = GameObject.Find("Portal") ?? GameObject.Find("Lobby_Portal");
                Vector3 pos = portal != null ? portal.transform.position : new Vector3(0f, 3.65f, 0f);

                // Arrow placed on the right of the portal pointing left at the vortex, and text badge placed to the right of the arrow
                Vector3 targetPos = new Vector3(pos.x + 1.15f, 3.25f, pos.z);
                SetTarget(targetPos, "RETURN TO LOBBY", -90f, 0f, new Vector2(1.35f, 0f), new Vector2(180f, 36f), new Vector2(-1.15f, -0.85f));
                SetObjectiveText("TUTORIAL • Return to Real World", "Step into the portal to return to the Museum Lobby.");
            }
            else if (_currentSceneName == "AltLeftHallway")
            {
                var toAltLobby = GameObject.Find("ToAltLobby");
                Vector3 pos = toAltLobby != null ? toAltLobby.transform.position : new Vector3(8.6f, -0.5f, 0f);
                SetTarget(pos + new Vector3(-0.8f, 0f, 0f), "TO ALT LOBBY -->", 90f, 0.3f);
                SetObjectiveText("TUTORIAL • Return to Real World", "Head east to return to the Alterworld portal.");
            }
            else
            {
                GuideBackToLobby("Head back to Room 1 to restore your recovered artifact onto its pedestal.");
            }
        }

        private void GuideBackToLobby(string msg)
        {
            if (_currentSceneName == "Room_1")
            {
                var exit = GameObject.Find("ExitDoor");
                Vector3 pos = exit != null ? exit.transform.position : new Vector3(0f, -4.5f, 0f);
                SetTarget(pos, "EXIT -->", 0f, 0.85f);
                SetObjectiveText("TUTORIAL: Return to Lobby", msg);
            }
            else if (_currentSceneName == "LeftHallway")
            {
                var toLobby = GameObject.Find("ToLobby");
                Vector3 pos = toLobby != null ? toLobby.transform.position : new Vector3(8.6f, -0.5f, 0f);
                SetTarget(pos + new Vector3(-0.8f, 0f, 0f), "TO LOBBY -->", 90f, 0.3f);
                SetObjectiveText("TUTORIAL: Return to Lobby", msg);
            }
            else
            {
                SetTarget(Vector3.zero, "LOBBY", 0f, 1f);
                SetObjectiveText("TUTORIAL", msg);
            }
        }

        private void SetTarget(Vector3 worldPos, string label, float angle, float heightOffset, Vector2? badgeLocalOffset = null, Vector2? badgeSize = null, Vector2? haloLocalOffset = null)
        {
            _currentTargetPos = worldPos;
            _targetLabel = label;
            _arrowAngle = angle;
            _baseArrowHeight = heightOffset;
            _badgeLocalOffset = badgeLocalOffset ?? new Vector2(0f, 0.48f);
            _badgeSizeDelta = badgeSize ?? new Vector2(250f, 44f);
            _haloLocalOffset = haloLocalOffset ?? new Vector2(0f, -0.42f);
            _hasTarget = true;

            if (_badgeText != null) _badgeText.text = label;

            if (_worldArrowContainer != null)
            {
                _worldArrowContainer.transform.position = _currentTargetPos;
                if (_arrowTransform != null)
                {
                    _arrowTransform.localPosition = new Vector3(0f, _baseArrowHeight, 0f);
                    _arrowTransform.localRotation = Quaternion.Euler(0f, 0f, _arrowAngle);
                }
                if (_badgeRect != null)
                {
                    _badgeRect.sizeDelta = _badgeSizeDelta;
                    _badgeRect.localPosition = new Vector3(_badgeLocalOffset.x, _baseArrowHeight + _badgeLocalOffset.y, 0f);
                }
                if (_groundHaloTransform != null)
                {
                    _groundHaloTransform.localPosition = new Vector3(_haloLocalOffset.x, _haloLocalOffset.y, 0f);
                }
            }

            SetVisualsVisible(true);
        }

        private void SetObjectiveText(string title, string body)
        {
            if (_objectiveTitleText != null) _objectiveTitleText.text = title;
            if (_objectiveBodyText != null) _objectiveBodyText.text = body;
        }

        // ─── UI Creation & Management ──────────────────────────────────────────
        private void BuildUI()
        {
            EnsureWorldPointerElements();
            EnsureSkipPromptElement();
            EnsureOffscreenPointerElements();
        }

        private void EnsureWorldPointerElements()
        {
            if (_arrowSprite == null || _panelSprite == null) LoadAssets();

            if (_worldArrowContainer != null)
            {
                return;
            }

            // Clean up any stale or orphaned pointers to guarantee a single instance
            var existingArrows = FindObjectsByType<GameObject>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            for (int i = 0; i < existingArrows.Length; i++)
            {
                var go = existingArrows[i];
                if (go != null && go.name == "Tutorial_WorldArrow")
                {
                    if (Application.isPlaying) Destroy(go);
                    else DestroyImmediate(go);
                }
            }

            _worldArrowContainer = new GameObject("Tutorial_WorldArrow");
            if (Application.isPlaying) DontDestroyOnLoad(_worldArrowContainer);

            // 1. Arrow Sprite
            var arrowChild = new GameObject("ArrowSprite", typeof(SpriteRenderer));
            arrowChild.transform.SetParent(_worldArrowContainer.transform, false);
            _arrowRenderer = arrowChild.GetComponent<SpriteRenderer>();
            _arrowRenderer.sprite = _arrowSprite;
            _arrowRenderer.sortingOrder = 3000;
            _arrowRenderer.color = Color.white;
            _arrowTransform = arrowChild.transform;
            _arrowTransform.localScale = new Vector3(0.18f, 0.18f, 1f);
            _arrowTransform.localPosition = new Vector3(0f, _baseArrowHeight, 0f);

            // 2. Action Badge (World Canvas)
            var badgeCanvasGo = new GameObject("ActionBadgeCanvas", typeof(RectTransform), typeof(Canvas));
            badgeCanvasGo.transform.SetParent(_worldArrowContainer.transform, false);
            var c = badgeCanvasGo.GetComponent<Canvas>();
            c.renderMode = RenderMode.WorldSpace;
            c.sortingOrder = 3005;

            _badgeRect = badgeCanvasGo.GetComponent<RectTransform>();
            _badgeRect.sizeDelta = _badgeSizeDelta;
            _badgeRect.localScale = new Vector3(0.010f, 0.010f, 1f);
            _badgeRect.localPosition = new Vector3(_badgeLocalOffset.x, _baseArrowHeight + _badgeLocalOffset.y, 0f);

            // Badge background image
            var bgObj = new GameObject("BadgeBg", typeof(RectTransform), typeof(Image));
            bgObj.transform.SetParent(badgeCanvasGo.transform, false);
            var bgRt = bgObj.GetComponent<RectTransform>();
            bgRt.anchorMin = Vector2.zero;
            bgRt.anchorMax = Vector2.one;
            bgRt.sizeDelta = Vector2.zero;
            var bgImg = bgObj.GetComponent<Image>();
            bgImg.color = new Color(0.18f, 0.10f, 0.05f, 0.95f); // Deep wood-toned pill
            if (_panelSprite != null)
            {
                bgImg.sprite = _panelSprite;
                bgImg.type = Image.Type.Sliced;
            }

            // Badge text
            var txtObj = new GameObject("BadgeText", typeof(RectTransform), typeof(TextMeshProUGUI));
            txtObj.transform.SetParent(badgeCanvasGo.transform, false);
            var tRt = txtObj.GetComponent<RectTransform>();
            tRt.anchorMin = Vector2.zero;
            tRt.anchorMax = Vector2.one;
            tRt.sizeDelta = Vector2.zero;
            _badgeText = txtObj.GetComponent<TextMeshProUGUI>();
            _badgeText.text = _targetLabel;
            _badgeText.fontSize = 17f;
            _badgeText.fontStyle = FontStyles.Bold;
            _badgeText.alignment = TextAlignmentOptions.Center;
            _badgeText.enableWordWrapping = false;
            _badgeText.overflowMode = TextOverflowModes.Ellipsis;
            _badgeText.color = new Color(1f, 0.88f, 0.45f, 1f); // Vibrant gold
            _badgeText.raycastTarget = false;

            // 3. Ground Halo
            var haloGo = new GameObject("GroundHalo", typeof(SpriteRenderer));
            haloGo.transform.SetParent(_worldArrowContainer.transform, false);
            _groundHaloRenderer = haloGo.GetComponent<SpriteRenderer>();
            _groundHaloRenderer.sprite = _haloSprite != null ? _haloSprite : _arrowSprite;
            _groundHaloRenderer.sortingOrder = 2999;
            _groundHaloRenderer.color = new Color(1f, 0.82f, 0.25f, 0.45f);
            _groundHaloTransform = haloGo.transform;
            _groundHaloTransform.localScale = new Vector3(0.35f, 0.18f, 1f); // Ellipse on ground
            _groundHaloTransform.localPosition = new Vector3(_haloLocalOffset.x, _haloLocalOffset.y, 0f);
        }

        private void EnsureSkipPromptElement()
        {
            if (GameProgression.HasCompletedTutorial || _currentPhase == TutorialPhase.Completed)
            {
                if (_skipContainerObj != null)
                {
                    if (Application.isPlaying) Destroy(_skipContainerObj);
                    else DestroyImmediate(_skipContainerObj);
                    _skipContainerObj = null;
                }
                return;
            }

            Canvas targetCanvas = FindActiveScreenCanvas();
            if (targetCanvas == null) return;

            // If skip container already exists under this canvas and is valid, keep it active
            if (_skipContainerObj != null && _skipContainerObj.transform.parent == targetCanvas.transform)
            {
                _skipContainerObj.SetActive(true);
                return;
            }

            // Clean up any legacy or existing bars/prompts
            Transform existingBar = targetCanvas.transform.Find("Tutorial_ObjectiveBar");
            if (existingBar != null)
            {
                if (Application.isPlaying) Destroy(existingBar.gameObject); else DestroyImmediate(existingBar.gameObject);
            }

            Transform existingPrompt = targetCanvas.transform.Find("Tutorial_SkipPrompt");
            if (existingPrompt != null)
            {
                if (Application.isPlaying) Destroy(existingPrompt.gameObject); else DestroyImmediate(existingPrompt.gameObject);
            }

            // Clean, discreet skip button at the bottom center of the screen
            var skipContainerGo = new GameObject("Tutorial_SkipPrompt", typeof(RectTransform), typeof(CanvasGroup), typeof(Image), typeof(Button));
            skipContainerGo.transform.SetParent(targetCanvas.transform, false);

            var scRt = skipContainerGo.GetComponent<RectTransform>();
            scRt.anchorMin = new Vector2(0.5f, 0f);
            scRt.anchorMax = new Vector2(0.5f, 0f);
            scRt.pivot = new Vector2(0.5f, 0f);
            scRt.anchoredPosition = new Vector2(0f, 22f);
            scRt.sizeDelta = new Vector2(136f, 36f);

            var sImg = skipContainerGo.GetComponent<Image>();
            sImg.color = new Color(0.20f, 0.12f, 0.05f, 0.92f);
            sImg.raycastTarget = true;

            _skipContainerObj = skipContainerGo;
            _skipContainerCg = skipContainerGo.GetComponent<CanvasGroup>();
            if (_skipContainerCg != null)
            {
                _skipContainerCg.alpha = 1f;
                _skipContainerCg.blocksRaycasts = true;
                _skipContainerCg.interactable = true;
            }

            _skipButton = skipContainerGo.GetComponent<Button>();
            var colors = _skipButton.colors;
            colors.normalColor = Color.white;
            colors.highlightedColor = new Color(1.25f, 1.25f, 1.25f, 1f);
            colors.pressedColor = new Color(0.75f, 0.75f, 0.75f, 1f);
            _skipButton.colors = colors;
            _skipButton.onClick.RemoveAllListeners();
            _skipButton.onClick.AddListener(SkipTutorial);

            var outline = skipContainerGo.AddComponent<Outline>();
            outline.effectColor = new Color(0.65f, 0.48f, 0.22f, 0.85f);
            outline.effectDistance = new Vector2(1.5f, -1.5f);

            var skipTxtGo = new GameObject("Text", typeof(RectTransform), typeof(TextMeshProUGUI));
            skipTxtGo.transform.SetParent(skipContainerGo.transform, false);
            var stRt = skipTxtGo.GetComponent<RectTransform>();
            stRt.anchorMin = Vector2.zero;
            stRt.anchorMax = Vector2.one;
            stRt.sizeDelta = Vector2.zero;

            var stTmp = skipTxtGo.GetComponent<TextMeshProUGUI>();
            stTmp.text = "Skip Tutorial ✕";
            stTmp.fontSize = 13.5f;
            stTmp.fontStyle = FontStyles.Bold;
            stTmp.alignment = TextAlignmentOptions.Center;
            stTmp.color = new Color(1f, 0.92f, 0.78f, 1f);
            stTmp.raycastTarget = false;
        }

        private void EnsureOffscreenPointerElements()
        {
            Canvas targetCanvas = FindActiveScreenCanvas();
            if (targetCanvas == null) return;

            Transform existing = targetCanvas.transform.Find("Tutorial_OffscreenIndicator");
            if (existing != null)
            {
                _offscreenIndicatorObj = existing.gameObject;
                _offscreenIndicatorRt = existing.GetComponent<RectTransform>();
                _offscreenIndicatorText = existing.Find("Text")?.GetComponent<TMP_Text>();
                return;
            }

            _offscreenIndicatorObj = new GameObject("Tutorial_OffscreenIndicator", typeof(RectTransform), typeof(Image));
            _offscreenIndicatorObj.transform.SetParent(targetCanvas.transform, false);

            _offscreenIndicatorRt = _offscreenIndicatorObj.GetComponent<RectTransform>();
            _offscreenIndicatorRt.sizeDelta = new Vector2(150f, 36f);
            _offscreenIndicatorRt.pivot = new Vector2(0.5f, 0.5f);

            var img = _offscreenIndicatorObj.GetComponent<Image>();
            img.color = new Color(0.20f, 0.12f, 0.05f, 0.92f);
            if (_panelSprite != null)
            {
                img.sprite = _panelSprite;
                img.type = Image.Type.Sliced;
            }

            var txtObj = new GameObject("Text", typeof(RectTransform), typeof(TextMeshProUGUI));
            txtObj.transform.SetParent(_offscreenIndicatorObj.transform, false);
            var tRt = txtObj.GetComponent<RectTransform>();
            tRt.anchorMin = Vector2.zero;
            tRt.anchorMax = Vector2.one;
            tRt.sizeDelta = Vector2.zero;
            _offscreenIndicatorText = txtObj.GetComponent<TextMeshProUGUI>();
            _offscreenIndicatorText.text = "GO HERE -->";
            _offscreenIndicatorText.fontSize = 13.5f;
            _offscreenIndicatorText.fontStyle = FontStyles.Bold;
            _offscreenIndicatorText.alignment = TextAlignmentOptions.Center;
            _offscreenIndicatorText.color = new Color(1f, 0.85f, 0.35f, 1f);
            _offscreenIndicatorText.raycastTarget = false;

            _offscreenIndicatorObj.SetActive(false);
        }

        private Canvas FindActiveScreenCanvas()
        {
            var inGameCanvas = GameObject.Find("InGameCanvas");
            if (inGameCanvas != null)
            {
                var c = inGameCanvas.GetComponent<Canvas>();
                if (c != null && c.gameObject.activeInHierarchy) return c;
            }

            var battleCanvas = GameObject.Find("BattleCanvas");
            if (battleCanvas != null)
            {
                var c = battleCanvas.GetComponent<Canvas>();
                if (c != null && c.gameObject.activeInHierarchy) return c;
            }

            var canvas = GameObject.Find("Canvas");
            if (canvas != null)
            {
                var c = canvas.GetComponent<Canvas>();
                if (c != null && c.gameObject.activeInHierarchy) return c;
            }

            var all = FindObjectsByType<Canvas>(FindObjectsSortMode.None);
            foreach (var c in all)
            {
                if (c.renderMode != RenderMode.WorldSpace && c.gameObject.activeInHierarchy)
                    return c;
            }

            return null;
        }

        private void SetVisualsVisible(bool isVisible)
        {
            if (_worldArrowContainer != null) _worldArrowContainer.SetActive(isVisible);
            if (_skipContainerObj != null) _skipContainerObj.SetActive(isVisible);
            if (_offscreenIndicatorObj != null)
            {
                if (!isVisible) _offscreenIndicatorObj.SetActive(false);
            }
        }

        // ─── Update Loop & Smooth Animations ───────────────────────────────────
        private void Update()
        {
            if (GameProgression.HasCompletedTutorial)
            {
                DismissAndDestroy();
                return;
            }

            if (!IsTutorialScene(_currentSceneName))
            {
                return;
            }

            if (!_hasTarget || _worldArrowContainer == null) return;

            // Animate floating arrow
            float bob = Mathf.Sin(Time.time * 4.5f) * 0.16f;
            float pulse = 1f + Mathf.Sin(Time.time * 4.5f) * 0.04f;

            Vector3 arrowPos = _currentTargetPos + new Vector3(0f, _baseArrowHeight + bob, 0f);
            _worldArrowContainer.transform.position = _currentTargetPos;
            if (_arrowTransform != null)
            {
                _arrowTransform.localPosition = new Vector3(0f, _baseArrowHeight + bob, 0f);
                _arrowTransform.localRotation = Quaternion.Euler(0f, 0f, _arrowAngle);
                _arrowTransform.localScale = new Vector3(0.18f * pulse, 0.18f * pulse, 1f);
            }
            if (_badgeRect != null)
            {
                _badgeRect.sizeDelta = _badgeSizeDelta;
                _badgeRect.localPosition = new Vector3(_badgeLocalOffset.x, _baseArrowHeight + bob + _badgeLocalOffset.y, 0f);
            }

            // Ground halo pulse
            if (_groundHaloTransform != null && _groundHaloRenderer != null)
            {
                _groundHaloTransform.localPosition = new Vector3(_haloLocalOffset.x, _haloLocalOffset.y, 0f);
                float haloAlpha = 0.30f + Mathf.Sin(Time.time * 4f) * 0.20f;
                _groundHaloRenderer.color = new Color(1f, 0.82f, 0.25f, haloAlpha);
            }

            // Update Offscreen Indicator
            UpdateOffscreenIndicator(arrowPos);
        }

        private void UpdateOffscreenIndicator(Vector3 targetWorldPos)
        {
            var cam = Camera.main;
            if (cam == null || _offscreenIndicatorObj == null) return;

            Vector3 screenPos = cam.WorldToScreenPoint(targetWorldPos);
            bool isBehind = screenPos.z < 0;

            float margin = 80f;
            bool isOffscreen = isBehind || screenPos.x < margin || screenPos.x > Screen.width - margin ||
                               screenPos.y < margin || screenPos.y > Screen.height - margin;

            if (isOffscreen)
            {
                _offscreenIndicatorObj.SetActive(true);

                if (isBehind) screenPos *= -1;

                Vector2 center = new Vector2(Screen.width * 0.5f, Screen.height * 0.5f);
                Vector2 fromCenter = (Vector2)screenPos - center;
                fromCenter.Normalize();

                float edgeX = Screen.width * 0.5f - margin;
                float edgeY = Screen.height * 0.5f - margin;

                float scaleX = Mathf.Abs(edgeX / (fromCenter.x != 0 ? fromCenter.x : 0.001f));
                float scaleY = Mathf.Abs(edgeY / (fromCenter.y != 0 ? fromCenter.y : 0.001f));
                float scale = Mathf.Min(scaleX, scaleY);

                Vector2 clampedPos = center + fromCenter * scale;

                // Position in Canvas space
                Canvas canvas = FindActiveScreenCanvas();
                if (canvas != null)
                {
                    var canvasRt = canvas.GetComponent<RectTransform>();
                    Vector2 localPoint;
                    RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRt, clampedPos, canvas.worldCamera, out localPoint);
                    _offscreenIndicatorRt.anchoredPosition = localPoint;
                }

                if (_offscreenIndicatorText != null)
                {
                    _offscreenIndicatorText.text = fromCenter.x >= 0 ? $"{_targetLabel} -->" : $"<-- {_targetLabel}";
                }
            }
            else
            {
                _offscreenIndicatorObj.SetActive(false);
            }
        }

        // ─── Completion & Skip ────────────────────────────────────────────────
        public void SkipTutorial()
        {
            Debug.Log("[GameTutorialGuide] Player chose to skip tutorial.");
            CompleteTutorial();
        }

        public void CompleteTutorial()
        {
            GameProgression.HasCompletedTutorial = true;
            _currentPhase = TutorialPhase.Completed;
            PlayerPrefs.SetInt(KEY_TUTORIAL_PHASE, (int)TutorialPhase.Completed);
            PlayerPrefs.Save();

            // When skipped or completed, ensure Field Guide is unlocked so the player can explore freely
            GameProgression.UnlockBook();

            NotificationBannerUI.Show(
                "TUTORIAL COMPLETED",
                "You can now explore the museum freely!",
                5.0f,
                new Color(0.24f, 0.14f, 0.07f, 1f),
                "★"
            );

            DismissAndDestroy();
        }

        public void DismissAndDestroy()
        {
            PurgeAllTutorialObjects();
        }

        public static void PurgeAllTutorialObjects()
        {
            var allGo = FindObjectsByType<GameObject>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            for (int i = 0; i < allGo.Length; i++)
            {
                var go = allGo[i];
                if (go == null) continue;
                string name = go.name;
                if (name == "Tutorial_WorldArrow" ||
                    name == "Tutorial_SkipPrompt" ||
                    name == "Tutorial_OffscreenIndicator" ||
                    name == "Tutorial_ObjectiveBar" ||
                    name == "ActionBadgeCanvas" ||
                    name == "GameTutorialGuide")
                {
                    if (Application.isPlaying)
                    {
                        go.SetActive(false);
                        Destroy(go);
                    }
                    else
                    {
                        DestroyImmediate(go);
                    }
                }
            }

            Instance = null;
        }
    }
}
