using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace TopDownGame.Editor
{
    public static class InGameUISetup
    {
        private const string PrefabPath = "Assets/Prefabs/InGameCanvas.prefab";
        private const string SprSettingsPath = "Assets/UI/Icon_Settings.png";
        private const string SprBackpackPath = "Assets/UI/Icon_Backpack.png";
        private const string SprBookPath = "Assets/UI/Icon_Book.png";
        private const string SprParchmentPath = "Assets/UI/ParchmentDialogueBox.png";
        private const string SprButtonFramePath = "Assets/UI/CharacterButton_Frame.png";

        [MenuItem("Tools/Setup In-Game HUD (Settings, Bag, Book)")]
        public static void SetupAll()
        {
            SetupPrefab();
            SyncGameplayScenes();
        }

        public static void SetupPrefab()
        {
            var prefabRoot = PrefabUtility.LoadPrefabContents(PrefabPath);
            try
            {
                var menuCtrl = prefabRoot.GetComponent<InGameMenuController>();
                if (menuCtrl == null)
                {
                    menuCtrl = prefabRoot.AddComponent<InGameMenuController>();
                }

                // Deactivate old HomeButton if present
                var oldHome = prefabRoot.transform.Find("HomeButton");
                if (oldHome != null)
                {
                    oldHome.gameObject.SetActive(false);
                }

                var sprSettings = AssetDatabase.LoadAssetAtPath<Sprite>(SprSettingsPath);
                var sprBackpack = AssetDatabase.LoadAssetAtPath<Sprite>(SprBackpackPath);
                var sprBook = AssetDatabase.LoadAssetAtPath<Sprite>(SprBookPath);
                var sprParchment = AssetDatabase.LoadAssetAtPath<Sprite>(SprParchmentPath);
                var sprBtnFrame = AssetDatabase.LoadAssetAtPath<Sprite>(SprButtonFramePath);

                // 1. Settings Button (Top-Right)
                var btnSettings = GetOrCreateChild(prefabRoot.transform, "SettingsButton");
                ConfigureIconButton(btnSettings, sprSettings, new Vector2(1f, 1f), new Vector2(0.5f, 0.5f), new Vector2(-95f, -95f), new Vector2(110f, 110f));
                var btnSettingsComp = btnSettings.GetComponent<Button>();
                btnSettingsComp.onClick = new Button.ButtonClickedEvent();
                UnityEditor.Events.UnityEventTools.AddPersistentListener(btnSettingsComp.onClick, menuCtrl.OnSettingsClicked);

                // 2. Bag Button (Below Settings on Top-Right)
                var btnBag = GetOrCreateChild(prefabRoot.transform, "BagButton");
                ConfigureIconButton(btnBag, sprBackpack, new Vector2(1f, 1f), new Vector2(0.5f, 0.5f), new Vector2(-95f, -225f), new Vector2(120f, 120f));
                var btnBagComp = btnBag.GetComponent<Button>();
                btnBagComp.onClick = new Button.ButtonClickedEvent();
                UnityEditor.Events.UnityEventTools.AddPersistentListener(btnBagComp.onClick, menuCtrl.OnBagClicked);

                // 3. Book Button (Bottom-Right)
                var btnBook = GetOrCreateChild(prefabRoot.transform, "BookButton");
                ConfigureIconButton(btnBook, sprBook, new Vector2(1f, 0f), new Vector2(0.5f, 0.5f), new Vector2(-120f, 120f), new Vector2(150f, 150f));
                var btnBookComp = btnBook.GetComponent<Button>();
                btnBookComp.onClick = new Button.ButtonClickedEvent();
                UnityEditor.Events.UnityEventTools.AddPersistentListener(btnBookComp.onClick, menuCtrl.OnBookClicked);

                // 4. Settings Modal Panel
                var settingsPanel = BuildSettingsModal(prefabRoot.transform, menuCtrl, sprParchment, sprBtnFrame);

                // Connect serialized references on InGameMenuController
                var so = new SerializedObject(menuCtrl);
                so.Update();
                var spSettingsPanel = so.FindProperty("settingsPanel");
                if (spSettingsPanel != null) spSettingsPanel.objectReferenceValue = settingsPanel;

                var sliderComp = settingsPanel.GetComponentInChildren<Slider>(true);
                var spVolumeSlider = so.FindProperty("volumeSlider");
                if (spVolumeSlider != null) spVolumeSlider.objectReferenceValue = sliderComp;

                so.ApplyModifiedProperties();

                PrefabUtility.SaveAsPrefabAsset(prefabRoot, PrefabPath);
                Debug.Log("[InGameUISetup] InGameCanvas.prefab updated successfully!");
            }
            finally
            {
                PrefabUtility.UnloadPrefabContents(prefabRoot);
            }
        }

        private static GameObject GetOrCreateChild(Transform parent, string name)
        {
            var t = parent.Find(name);
            if (t != null) return t.gameObject;
            var go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            return go;
        }

        private static void ConfigureIconButton(GameObject go, Sprite spr, Vector2 anchor, Vector2 pivot, Vector2 pos, Vector2 size)
        {
            var rt = go.GetComponent<RectTransform>();
            rt.anchorMin = anchor;
            rt.anchorMax = anchor;
            rt.pivot = pivot;
            rt.anchoredPosition = pos;
            rt.sizeDelta = size;

            var img = go.GetComponent<Image>();
            if (img == null) img = go.AddComponent<Image>();
            img.sprite = spr;
            img.preserveAspect = true;
            img.color = Color.white;
            img.raycastTarget = true;

            var btn = go.GetComponent<Button>();
            if (btn == null) btn = go.AddComponent<Button>();
            btn.targetGraphic = img;

            var colors = btn.colors;
            colors.normalColor = Color.white;
            colors.highlightedColor = new Color(0.92f, 0.92f, 0.92f, 1f);
            colors.pressedColor = new Color(0.75f, 0.75f, 0.75f, 1f);
            colors.selectedColor = Color.white;
            colors.colorMultiplier = 1f;
            btn.colors = colors;
        }

        private static GameObject BuildSettingsModal(Transform parent, InGameMenuController menuCtrl, Sprite sprParchment, Sprite sprBtnFrame)
        {
            var panelObj = GetOrCreateChild(parent, "SettingsPanel");
            panelObj.SetActive(false);

            var panelRt = panelObj.GetComponent<RectTransform>();
            panelRt.anchorMin = Vector2.zero;
            panelRt.anchorMax = Vector2.one;
            panelRt.offsetMin = Vector2.zero;
            panelRt.offsetMax = Vector2.zero;

            // Translucent Dimmer Background
            var dimmerObj = GetOrCreateChild(panelObj.transform, "Dimmer");
            var dimmerRt = dimmerObj.GetComponent<RectTransform>();
            dimmerRt.anchorMin = Vector2.zero;
            dimmerRt.anchorMax = Vector2.one;
            dimmerRt.offsetMin = Vector2.zero;
            dimmerRt.offsetMax = Vector2.zero;

            var dimmerImg = dimmerObj.GetComponent<Image>();
            if (dimmerImg == null) dimmerImg = dimmerObj.AddComponent<Image>();
            dimmerImg.color = new Color(0f, 0f, 0f, 0.65f);
            dimmerImg.raycastTarget = true;

            // Modal Dialog Frame
            var dialogObj = GetOrCreateChild(panelObj.transform, "DialogBox");
            var dialogRt = dialogObj.GetComponent<RectTransform>();
            dialogRt.anchorMin = new Vector2(0.5f, 0.5f);
            dialogRt.anchorMax = new Vector2(0.5f, 0.5f);
            dialogRt.pivot = new Vector2(0.5f, 0.5f);
            dialogRt.anchoredPosition = Vector2.zero;
            dialogRt.sizeDelta = new Vector2(560f, 520f);

            var dialogImg = dialogObj.GetComponent<Image>();
            if (dialogImg == null) dialogImg = dialogObj.AddComponent<Image>();
            dialogImg.sprite = sprParchment;
            dialogImg.type = Image.Type.Sliced;
            dialogImg.color = Color.white;

            // Header Title
            var titleObj = GetOrCreateChild(dialogObj.transform, "TitleText");
            var titleRt = titleObj.GetComponent<RectTransform>();
            titleRt.anchorMin = new Vector2(0.5f, 1f);
            titleRt.anchorMax = new Vector2(0.5f, 1f);
            titleRt.pivot = new Vector2(0.5f, 1f);
            titleRt.anchoredPosition = new Vector2(0f, -40f);
            titleRt.sizeDelta = new Vector2(400f, 50f);

            var titleTmp = titleObj.GetComponent<TextMeshProUGUI>();
            if (titleTmp == null) titleTmp = titleObj.AddComponent<TextMeshProUGUI>();
            titleTmp.text = "PAUSED";
            titleTmp.fontSize = 36f;
            titleTmp.fontStyle = FontStyles.Bold;
            titleTmp.alignment = TextAlignmentOptions.Center;
            titleTmp.color = new Color(0.24f, 0.16f, 0.08f, 1f);

            // Volume Label & Slider
            var volLabelObj = GetOrCreateChild(dialogObj.transform, "VolumeLabel");
            var volLabelRt = volLabelObj.GetComponent<RectTransform>();
            volLabelRt.anchorMin = new Vector2(0.5f, 1f);
            volLabelRt.anchorMax = new Vector2(0.5f, 1f);
            volLabelRt.pivot = new Vector2(0.5f, 1f);
            volLabelRt.anchoredPosition = new Vector2(0f, -115f);
            volLabelRt.sizeDelta = new Vector2(380f, 32f);

            var volLabelTmp = volLabelObj.GetComponent<TextMeshProUGUI>();
            if (volLabelTmp == null) volLabelTmp = volLabelObj.AddComponent<TextMeshProUGUI>();
            volLabelTmp.text = "MASTER VOLUME";
            volLabelTmp.fontSize = 20f;
            volLabelTmp.fontStyle = FontStyles.Bold;
            volLabelTmp.alignment = TextAlignmentOptions.Center;
            volLabelTmp.color = new Color(0.32f, 0.22f, 0.12f, 1f);

            var sliderObj = GetOrCreateChild(dialogObj.transform, "VolumeSlider");
            var sliderRt = sliderObj.GetComponent<RectTransform>();
            sliderRt.anchorMin = new Vector2(0.5f, 1f);
            sliderRt.anchorMax = new Vector2(0.5f, 1f);
            sliderRt.pivot = new Vector2(0.5f, 1f);
            sliderRt.anchoredPosition = new Vector2(0f, -155f);
            sliderRt.sizeDelta = new Vector2(360f, 30f);

            var slider = sliderObj.GetComponent<Slider>();
            if (slider == null) slider = sliderObj.AddComponent<Slider>();
            slider.minValue = 0f;
            slider.maxValue = 1f;
            slider.value = 1f;

            // Slider Background
            var sBgObj = GetOrCreateChild(sliderObj.transform, "Background");
            var sBgRt = sBgObj.GetComponent<RectTransform>();
            sBgRt.anchorMin = new Vector2(0f, 0.25f);
            sBgRt.anchorMax = new Vector2(1f, 0.75f);
            sBgRt.offsetMin = Vector2.zero;
            sBgRt.offsetMax = Vector2.zero;
            var sBgImg = sBgObj.GetComponent<Image>();
            if (sBgImg == null) sBgImg = sBgObj.AddComponent<Image>();
            sBgImg.color = new Color(0.18f, 0.14f, 0.1f, 0.6f);

            // Slider Fill Area & Fill
            var sFillArea = GetOrCreateChild(sliderObj.transform, "Fill Area");
            var sFillAreaRt = sFillArea.GetComponent<RectTransform>();
            sFillAreaRt.anchorMin = new Vector2(0f, 0.25f);
            sFillAreaRt.anchorMax = new Vector2(1f, 0.75f);
            sFillAreaRt.offsetMin = new Vector2(5f, 0f);
            sFillAreaRt.offsetMax = new Vector2(-5f, 0f);

            var sFillObj = GetOrCreateChild(sFillArea.transform, "Fill");
            var sFillRt = sFillObj.GetComponent<RectTransform>();
            sFillRt.anchorMin = Vector2.zero;
            sFillRt.anchorMax = Vector2.one;
            sFillRt.offsetMin = Vector2.zero;
            sFillRt.offsetMax = Vector2.zero;
            var sFillImg = sFillObj.GetComponent<Image>();
            if (sFillImg == null) sFillImg = sFillObj.AddComponent<Image>();
            sFillImg.color = new Color(0.78f, 0.58f, 0.24f, 1f);

            slider.fillRect = sFillRt;

            // Action Buttons Container
            float startY = -230f;
            float stepY = 70f;

            // Button 1: RESUME
            var btnResume = CreateModalButton("ResumeButton", "RESUME", new Vector2(0f, startY), dialogObj.transform, sprBtnFrame);
            var btnResumeComp = btnResume.GetComponent<Button>();
            btnResumeComp.onClick = new Button.ButtonClickedEvent();
            UnityEditor.Events.UnityEventTools.AddPersistentListener(btnResumeComp.onClick, menuCtrl.OnCloseSettingsClicked);

            // Button 2: RESTART ROOM
            var btnRestart = CreateModalButton("RestartButton", "RESTART ROOM", new Vector2(0f, startY - stepY), dialogObj.transform, sprBtnFrame);
            var btnRestartComp = btnRestart.GetComponent<Button>();
            btnRestartComp.onClick = new Button.ButtonClickedEvent();
            UnityEditor.Events.UnityEventTools.AddPersistentListener(btnRestartComp.onClick, menuCtrl.RestartCurrentScene);

            // Button 3: MAIN MENU
            var btnMainMenu = CreateModalButton("MainMenuButton", "MAIN MENU", new Vector2(0f, startY - (stepY * 2f)), dialogObj.transform, sprBtnFrame);
            var btnMainMenuComp = btnMainMenu.GetComponent<Button>();
            btnMainMenuComp.onClick = new Button.ButtonClickedEvent();
            UnityEditor.Events.UnityEventTools.AddPersistentListener(btnMainMenuComp.onClick, menuCtrl.GoToMainMenu);

            // Dimmer click closes settings
            var dimmerBtn = dimmerObj.GetComponent<Button>();
            if (dimmerBtn == null) dimmerBtn = dimmerObj.AddComponent<Button>();
            dimmerBtn.transition = Selectable.Transition.None;
            dimmerBtn.onClick = new Button.ButtonClickedEvent();
            UnityEditor.Events.UnityEventTools.AddPersistentListener(dimmerBtn.onClick, menuCtrl.OnCloseSettingsClicked);

            return panelObj;
        }

        private static GameObject CreateModalButton(string name, string text, Vector2 pos, Transform parent, Sprite sprBtnFrame)
        {
            var btnObj = GetOrCreateChild(parent, name);
            var btnRt = btnObj.GetComponent<RectTransform>();
            btnRt.anchorMin = new Vector2(0.5f, 1f);
            btnRt.anchorMax = new Vector2(0.5f, 1f);
            btnRt.pivot = new Vector2(0.5f, 1f);
            btnRt.anchoredPosition = pos;
            btnRt.sizeDelta = new Vector2(340f, 56f);

            var btnImg = btnObj.GetComponent<Image>();
            if (btnImg == null) btnImg = btnObj.AddComponent<Image>();
            btnImg.sprite = sprBtnFrame;
            btnImg.type = Image.Type.Sliced;
            btnImg.color = Color.white;

            var btnComp = btnObj.GetComponent<Button>();
            if (btnComp == null) btnComp = btnObj.AddComponent<Button>();
            btnComp.targetGraphic = btnImg;

            var txtObj = GetOrCreateChild(btnObj.transform, "Text");
            var txtRt = txtObj.GetComponent<RectTransform>();
            txtRt.anchorMin = Vector2.zero;
            txtRt.anchorMax = Vector2.one;
            txtRt.offsetMin = Vector2.zero;
            txtRt.offsetMax = Vector2.zero;

            var txtTmp = txtObj.GetComponent<TextMeshProUGUI>();
            if (txtTmp == null) txtTmp = txtObj.AddComponent<TextMeshProUGUI>();
            txtTmp.text = text;
            txtTmp.fontSize = 22f;
            txtTmp.fontStyle = FontStyles.Bold;
            txtTmp.alignment = TextAlignmentOptions.Center;
            txtTmp.color = new Color(0.95f, 0.92f, 0.85f, 1f);

            return btnObj;
        }

        public static void SyncGameplayScenes()
        {
            string[] standaloneScenes = new string[] {
                "Assets/Scenes/Lobby.unity",
                "Assets/Scenes/LeftHallway.unity",
                "Assets/Scenes/RightHallway.unity"
            };

            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(PrefabPath);
            if (prefab == null) return;

            foreach (var scenePath in standaloneScenes)
            {
                var scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);
                var existingCanvas = GameObject.Find("Canvas");
                if (existingCanvas != null)
                {
                    // Copy buttons or instantiate prefab
                    // If existing Canvas is not a prefab, let's wire up the buttons on it
                    var menuCtrl = existingCanvas.GetComponent<InGameMenuController>();
                    if (menuCtrl == null) menuCtrl = existingCanvas.AddComponent<InGameMenuController>();

                    var oldHome = existingCanvas.transform.Find("HomeButton");
                    if (oldHome != null) oldHome.gameObject.SetActive(false);

                    var sprSettings = AssetDatabase.LoadAssetAtPath<Sprite>(SprSettingsPath);
                    var sprBackpack = AssetDatabase.LoadAssetAtPath<Sprite>(SprBackpackPath);
                    var sprBook = AssetDatabase.LoadAssetAtPath<Sprite>(SprBookPath);
                    var sprParchment = AssetDatabase.LoadAssetAtPath<Sprite>(SprParchmentPath);
                    var sprBtnFrame = AssetDatabase.LoadAssetAtPath<Sprite>(SprButtonFramePath);

                    var btnSettings = GetOrCreateChild(existingCanvas.transform, "SettingsButton");
                    ConfigureIconButton(btnSettings, sprSettings, new Vector2(1f, 1f), new Vector2(0.5f, 0.5f), new Vector2(-95f, -95f), new Vector2(110f, 110f));
                    var btnSettingsComp = btnSettings.GetComponent<Button>();
                    btnSettingsComp.onClick = new Button.ButtonClickedEvent();
                    UnityEditor.Events.UnityEventTools.AddPersistentListener(btnSettingsComp.onClick, menuCtrl.OnSettingsClicked);

                    var btnBag = GetOrCreateChild(existingCanvas.transform, "BagButton");
                    ConfigureIconButton(btnBag, sprBackpack, new Vector2(1f, 1f), new Vector2(0.5f, 0.5f), new Vector2(-95f, -225f), new Vector2(120f, 120f));
                    var btnBagComp = btnBag.GetComponent<Button>();
                    btnBagComp.onClick = new Button.ButtonClickedEvent();
                    UnityEditor.Events.UnityEventTools.AddPersistentListener(btnBagComp.onClick, menuCtrl.OnBagClicked);

                    var btnBook = GetOrCreateChild(existingCanvas.transform, "BookButton");
                    ConfigureIconButton(btnBook, sprBook, new Vector2(1f, 0f), new Vector2(0.5f, 0.5f), new Vector2(-120f, 120f), new Vector2(150f, 150f));
                    var btnBookComp = btnBook.GetComponent<Button>();
                    btnBookComp.onClick = new Button.ButtonClickedEvent();
                    UnityEditor.Events.UnityEventTools.AddPersistentListener(btnBookComp.onClick, menuCtrl.OnBookClicked);

                    var settingsPanel = BuildSettingsModal(existingCanvas.transform, menuCtrl, sprParchment, sprBtnFrame);

                    var so = new SerializedObject(menuCtrl);
                    so.Update();
                    var spSettingsPanel = so.FindProperty("settingsPanel");
                    if (spSettingsPanel != null) spSettingsPanel.objectReferenceValue = settingsPanel;

                    var sliderComp = settingsPanel.GetComponentInChildren<Slider>(true);
                    var spVolumeSlider = so.FindProperty("volumeSlider");
                    if (spVolumeSlider != null) spVolumeSlider.objectReferenceValue = sliderComp;
                    so.ApplyModifiedProperties();

                    EditorSceneManager.MarkSceneDirty(scene);
                    EditorSceneManager.SaveScene(scene);
                    Debug.Log($"[InGameUISetup] Synchronized canvas in scene {scenePath}");
                }
            }
        }
    }
}
