using System.Collections.Generic;
using System.IO;
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

        // Settings UI Assets
        private const string SprScrollCleanTracksPath = "Assets/UI/Settings/Settings_Scroll_Clean_Tracks.png";
        private const string SprKnobPath = "Assets/UI/Settings/Settings_Slider_Knob.png";
        private const string SprCloseCleanPath = "Assets/UI/Settings/Settings_Btn_Close_Clean.png";
        private const string SprSettingsIconGoogleNormalPath = "Assets/UI/Settings/Settings_Icon_Google_Normal.png";
        private const string SprSettingsIconGoogleHoverPath = "Assets/UI/Settings/Settings_Icon_Google_Hover.png";

        // Login UI Assets
        private const string SprLoginScreenBasePath = "Assets/UI/Login/Login_Screen_Base.png";
        private const string SprLoginWallPath = "Assets/UI/Login/Login_Wall_Background.png";
        private const string SprLoginScrollPath = "Assets/UI/Login/Login_Scroll_Frame.png";
        private const string SprLoginInputPath = "Assets/UI/Login/Login_Input_Frame.png";
        private const string SprLoginOrConnectPath = "Assets/UI/Login/Login_OrConnectWith.png";
        private const string SprLoginBtnLoginNormalPath = "Assets/UI/Login/Login_Btn_Login_Normal.png";
        private const string SprLoginBtnLoginHoverPath = "Assets/UI/Login/Login_Btn_Login_Hover.png";
        private const string SprLoginBtnRegisterNormalPath = "Assets/UI/Login/Login_Btn_Register_Normal.png";
        private const string SprLoginBtnRegisterHoverPath = "Assets/UI/Login/Login_Btn_Register_Hover.png";
        private const string SprLoginBtnGooglePath = "Assets/UI/Login/Login_Btn_Google.png";
        private const string SprLoginBtnGoogleNormalPath = "Assets/UI/Login/Login_Btn_Google_Normal.png";
        private const string SprLoginBtnGoogleHoverPath = "Assets/UI/Login/Login_Btn_Google_Hover.png";
        private const string SprLoginBtnGuestNormalPath = "Assets/UI/Login/Login_Btn_Guest_Normal.png";
        private const string SprLoginBtnGuestHoverPath = "Assets/UI/Login/Login_Btn_Guest_Hover.png";
        private const string SprLoginBtnTransparentPath = "Assets/UI/Login/Login_Btn_Transparent.png";

        // Book UI Assets
        private const string SprBookFramePath = "Assets/UI/Book/Book_Open_Frame.png";
        private const string SprBookArrowLeftPath = "Assets/UI/Book/Book_Arrow_Left.png";
        private const string SprBookArrowRightPath = "Assets/UI/Book/Book_Arrow_Right.png";

        [MenuItem("Tools/Setup In-Game HUD (Settings, Bag, Book)")]
        public static void SetupAll()
        {
            if (EditorApplication.isPlaying)
            {
                Debug.LogWarning("[InGameUISetup] Please stop Play Mode in Unity to run setup.");
                return;
            }
            EnsureSpriteImport(SprSettingsIconGoogleNormalPath);
            EnsureSpriteImport(SprSettingsIconGoogleHoverPath);
            SetupPrefab();
            SyncGameplayScenes();
            SetupMainMenuScene();
            CharacterSelectUISetup.SetupCharacterSelectScene();
            Debug.Log("[InGameUISetup] Settings, Login, and CharacterSelect UI successfully set up!");
        }

        public static void SetupPrefab()
        {
            var prefabRoot = PrefabUtility.LoadPrefabContents(PrefabPath);
            try
            {
                var menuCtrl = prefabRoot.GetComponent<InGameMenuController>();
                if (menuCtrl == null) menuCtrl = prefabRoot.AddComponent<InGameMenuController>();

                var oldHome = prefabRoot.transform.Find("HomeButton");
                if (oldHome != null) oldHome.gameObject.SetActive(false);

                var heartCont = prefabRoot.transform.Find("HeartContainer");
                if (heartCont != null) heartCont.gameObject.SetActive(false);

                var sprSettings = AssetDatabase.LoadAssetAtPath<Sprite>(SprSettingsPath);
                var sprBackpack = AssetDatabase.LoadAssetAtPath<Sprite>(SprBackpackPath);
                var sprBook = AssetDatabase.LoadAssetAtPath<Sprite>(SprBookPath);
                var sprParchment = AssetDatabase.LoadAssetAtPath<Sprite>(SprParchmentPath);
                var sprBtnFrame = AssetDatabase.LoadAssetAtPath<Sprite>(SprButtonFramePath);

                var btnSettings = GetOrCreateChild(prefabRoot.transform, "SettingsButton");
                ConfigureIconButton(btnSettings, sprSettings, new Vector2(1f, 1f), new Vector2(0.5f, 0.5f), new Vector2(-95f, -95f), new Vector2(110f, 110f));
                var btnSettingsComp = btnSettings.GetComponent<Button>();
                btnSettingsComp.onClick = new Button.ButtonClickedEvent();
                UnityEditor.Events.UnityEventTools.AddPersistentListener(btnSettingsComp.onClick, menuCtrl.OnSettingsClicked);

                var btnBag = GetOrCreateChild(prefabRoot.transform, "BagButton");
                ConfigureIconButton(btnBag, sprBackpack, new Vector2(1f, 1f), new Vector2(0.5f, 0.5f), new Vector2(-95f, -225f), new Vector2(120f, 120f));
                var btnBagComp = btnBag.GetComponent<Button>();
                btnBagComp.onClick = new Button.ButtonClickedEvent();
                UnityEditor.Events.UnityEventTools.AddPersistentListener(btnBagComp.onClick, menuCtrl.OnBagClicked);

                var btnBook = GetOrCreateChild(prefabRoot.transform, "BookButton");
                ConfigureIconButton(btnBook, sprBook, new Vector2(1f, 0f), new Vector2(0.5f, 0.5f), new Vector2(-120f, 120f), new Vector2(150f, 150f));
                var btnBookComp = btnBook.GetComponent<Button>();
                btnBookComp.onClick = new Button.ButtonClickedEvent();
                UnityEditor.Events.UnityEventTools.AddPersistentListener(btnBookComp.onClick, menuCtrl.OnBookClicked);

                var settingsPanel = BuildSettingsModal(prefabRoot.transform, menuCtrl, isGameplay: true);
                var bagModalObj = BuildBagModal(prefabRoot.transform, sprParchment, sprBtnFrame);
                var bagModalComp = bagModalObj.GetComponent<BagModalUI>();
                var bookModalObj = BuildBookModal(prefabRoot.transform);

                WireInGameMenuController(menuCtrl, settingsPanel, bagModalComp);

                PrefabUtility.SaveAsPrefabAsset(prefabRoot, PrefabPath);
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

        private static GameObject BuildSettingsModal(Transform parent, InGameMenuController menuCtrl, bool isGameplay)
        {
            var panelObj = GetOrCreateChild(parent, "SettingsPanel");
            panelObj.SetActive(false);

            var panelRt = panelObj.GetComponent<RectTransform>();
            panelRt.anchorMin = Vector2.zero;
            panelRt.anchorMax = Vector2.one;
            panelRt.offsetMin = Vector2.zero;
            panelRt.offsetMax = Vector2.zero;

            // Dark Dimmer
            var dimmerObj = GetOrCreateChild(panelObj.transform, "Dimmer");
            var dimmerRt = dimmerObj.GetComponent<RectTransform>();
            dimmerRt.anchorMin = Vector2.zero;
            dimmerRt.anchorMax = Vector2.one;
            dimmerRt.offsetMin = Vector2.zero;
            dimmerRt.offsetMax = Vector2.zero;

            var dimmerImg = dimmerObj.GetComponent<Image>();
            if (dimmerImg == null) dimmerImg = dimmerObj.AddComponent<Image>();
            dimmerImg.color = new Color(0f, 0f, 0f, 0.85f);
            dimmerImg.raycastTarget = true;

            var dimmerBtn = dimmerObj.GetComponent<Button>();
            if (dimmerBtn == null) dimmerBtn = dimmerObj.AddComponent<Button>();
            dimmerBtn.transition = Selectable.Transition.None;
            dimmerBtn.onClick = new Button.ButtonClickedEvent();
            if (menuCtrl != null)
            {
                UnityEditor.Events.UnityEventTools.AddPersistentListener(dimmerBtn.onClick, menuCtrl.OnCloseSettingsClicked);
            }

            // Modal Dialog Scroll Frame (1024 x 738)
            var dialogObj = GetOrCreateChild(panelObj.transform, "DialogBox");
            var dialogRt = dialogObj.GetComponent<RectTransform>();
            dialogRt.anchorMin = new Vector2(0.5f, 0.5f);
            dialogRt.anchorMax = new Vector2(0.5f, 0.5f);
            dialogRt.pivot = new Vector2(0.5f, 0.5f);
            dialogRt.anchoredPosition = Vector2.zero;
            dialogRt.sizeDelta = new Vector2(1024f, 738f);

            for (int i = dialogObj.transform.childCount - 1; i >= 0; i--)
            {
                Object.DestroyImmediate(dialogObj.transform.GetChild(i).gameObject);
            }

            var dialogImg = dialogObj.GetComponent<Image>();
            if (dialogImg == null) dialogImg = dialogObj.AddComponent<Image>();
            var sprScroll = AssetDatabase.LoadAssetAtPath<Sprite>(SprScrollCleanTracksPath);
            dialogImg.sprite = sprScroll;
            dialogImg.type = Image.Type.Simple;
            dialogImg.preserveAspect = true;
            dialogImg.color = Color.white;

            // Close Button [X] (top-right red box)
            var closeObj = GetOrCreateChild(dialogObj.transform, "CloseButton");
            var closeRt = closeObj.GetComponent<RectTransform>();
            closeRt.anchorMin = new Vector2(0.5f, 1f);
            closeRt.anchorMax = new Vector2(0.5f, 1f);
            closeRt.pivot = new Vector2(0.5f, 0.5f);
            closeRt.anchoredPosition = new Vector2(364f, -154f);
            closeRt.sizeDelta = new Vector2(44f, 44f);

            var closeImg = closeObj.GetComponent<Image>();
            if (closeImg == null) closeImg = closeObj.AddComponent<Image>();
            closeImg.color = new Color(1f, 1f, 1f, 0f);
            closeImg.raycastTarget = true;

            var closeBtn = closeObj.GetComponent<Button>();
            if (closeBtn == null) closeBtn = closeObj.AddComponent<Button>();
            closeBtn.targetGraphic = closeImg;
            var closeColors = closeBtn.colors;
            closeColors.normalColor = new Color(1f, 1f, 1f, 0f);
            closeColors.highlightedColor = new Color(1f, 1f, 1f, 0.25f);
            closeColors.pressedColor = new Color(0f, 0f, 0f, 0.35f);
            closeBtn.colors = closeColors;
            closeBtn.onClick = new Button.ButtonClickedEvent();
            if (menuCtrl != null)
            {
                UnityEditor.Events.UnityEventTools.AddPersistentListener(closeBtn.onClick, menuCtrl.OnCloseSettingsClicked);
            }

            // Sliders with pixel-art knob
            var sprKnob = AssetDatabase.LoadAssetAtPath<Sprite>(SprKnobPath);

            var masterSlider = CreateInteractiveSlider("Slider_MasterVolume", new Vector2(149f, -243.5f), dialogObj.transform, sprKnob);
            if (menuCtrl != null) UnityEditor.Events.UnityEventTools.AddPersistentListener(masterSlider.onValueChanged, menuCtrl.OnMasterVolumeChanged);

            var sfxSlider = CreateInteractiveSlider("Slider_SoundEffects", new Vector2(149f, -327.5f), dialogObj.transform, sprKnob);
            if (menuCtrl != null) UnityEditor.Events.UnityEventTools.AddPersistentListener(sfxSlider.onValueChanged, menuCtrl.OnSFXVolumeChanged);

            var bgmSlider = CreateInteractiveSlider("Slider_BackgroundMusic", new Vector2(149f, -410.5f), dialogObj.transform, sprKnob);
            if (menuCtrl != null) UnityEditor.Events.UnityEventTools.AddPersistentListener(bgmSlider.onValueChanged, menuCtrl.OnBGMVolumeChanged);

            // Overlay Buttons for SAVE and DEFAULT
            var btnSaveObj = CreateOverlayButton("Button_Save", new Vector2(-123.5f, -493f), new Vector2(152f, 57f), dialogObj.transform);
            var btnSave = btnSaveObj.GetComponent<Button>();
            if (menuCtrl != null) UnityEditor.Events.UnityEventTools.AddPersistentListener(btnSave.onClick, menuCtrl.OnSaveSettingsClicked);

            var btnDefaultObj = CreateOverlayButton("Button_Default", new Vector2(120.5f, -493f), new Vector2(156f, 57f), dialogObj.transform);
            var btnDefault = btnDefaultObj.GetComponent<Button>();
            if (menuCtrl != null) UnityEditor.Events.UnityEventTools.AddPersistentListener(btnDefault.onClick, menuCtrl.OnDefaultSettingsClicked);

            // Bottom Buttons: RETURN TO MAIN MENU and CONNECT TO GOOGLE
            var btnMenuObj = CreateOverlayButton("Button_ExitToMenu", new Vector2(-177f, -574.5f), new Vector2(309f, 54f), dialogObj.transform);
            var btnMenu = btnMenuObj.GetComponent<Button>();
            if (isGameplay && menuCtrl != null)
            {
                UnityEditor.Events.UnityEventTools.AddPersistentListener(btnMenu.onClick, menuCtrl.GoToMainMenu);
            }
            else if (menuCtrl != null)
            {
                UnityEditor.Events.UnityEventTools.AddPersistentListener(btnMenu.onClick, menuCtrl.OnCloseSettingsClicked);
            }

            var btnGoogleObj = CreateGoogleConnectOverlayButton("Button_ConnectToGoogle", new Vector2(176f, -574.5f), new Vector2(309f, 54f), dialogObj.transform);
            var btnGoogle = btnGoogleObj.GetComponent<Button>();
            if (menuCtrl != null)
            {
                UnityEditor.Events.UnityEventTools.AddPersistentListener(btnGoogle.onClick, menuCtrl.OnConnectToGoogleClicked);
            }

            return panelObj;
        }

        private static Slider CreateInteractiveSlider(string name, Vector2 pos, Transform parent, Sprite sprKnob)
        {
            var sliderObj = GetOrCreateChild(parent, name);
            var sliderRt = sliderObj.GetComponent<RectTransform>();
            sliderRt.anchorMin = new Vector2(0.5f, 1f);
            sliderRt.anchorMax = new Vector2(0.5f, 1f);
            sliderRt.pivot = new Vector2(0.5f, 0.5f);
            sliderRt.anchoredPosition = pos;
            sliderRt.sizeDelta = new Vector2(371f, 60f);

            var slider = sliderObj.GetComponent<Slider>();
            if (slider == null) slider = sliderObj.AddComponent<Slider>();
            slider.direction = Slider.Direction.LeftToRight;
            slider.minValue = 0f;
            slider.maxValue = 1f;
            slider.value = 1f;

            var bgObj = GetOrCreateChild(sliderObj.transform, "Background");
            var bgRt = bgObj.GetComponent<RectTransform>();
            bgRt.anchorMin = Vector2.zero;
            bgRt.anchorMax = Vector2.one;
            bgRt.offsetMin = Vector2.zero;
            bgRt.offsetMax = Vector2.zero;
            var bgImg = bgObj.GetComponent<Image>();
            if (bgImg == null) bgImg = bgObj.AddComponent<Image>();
            bgImg.color = new Color(1f, 1f, 1f, 0f);
            bgImg.raycastTarget = true;

            var slideArea = GetOrCreateChild(sliderObj.transform, "Handle Slide Area");
            var slideAreaRt = slideArea.GetComponent<RectTransform>();
            slideAreaRt.anchorMin = Vector2.zero;
            slideAreaRt.anchorMax = Vector2.one;
            slideAreaRt.offsetMin = new Vector2(23f, 0f);
            slideAreaRt.offsetMax = new Vector2(-23f, 0f);

            var handleObj = GetOrCreateChild(slideArea.transform, "Handle");
            var handleRt = handleObj.GetComponent<RectTransform>();
            handleRt.anchorMin = new Vector2(0.5f, 0.5f);
            handleRt.anchorMax = new Vector2(0.5f, 0.5f);
            handleRt.pivot = new Vector2(0.5f, 0.5f);
            handleRt.sizeDelta = new Vector2(42f, 48f);
            var handleImg = handleObj.GetComponent<Image>();
            if (handleImg == null) handleImg = handleObj.AddComponent<Image>();
            handleImg.sprite = sprKnob;
            handleImg.preserveAspect = true;
            handleImg.raycastTarget = true;

            slider.targetGraphic = handleImg;
            slider.handleRect = handleRt;

            return slider;
        }

        private static GameObject CreateOverlayButton(string name, Vector2 pos, Vector2 size, Transform parent)
        {
            var obj = GetOrCreateChild(parent, name);
            var rt = obj.GetComponent<RectTransform>();
            rt.anchorMin = new Vector2(0.5f, 1f);
            rt.anchorMax = new Vector2(0.5f, 1f);
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.anchoredPosition = pos;
            rt.sizeDelta = size;

            var img = obj.GetComponent<Image>();
            if (img == null) img = obj.AddComponent<Image>();
            img.color = new Color(1f, 1f, 1f, 0f);
            img.raycastTarget = true;

            var btn = obj.GetComponent<Button>();
            if (btn == null) btn = obj.AddComponent<Button>();
            btn.targetGraphic = img;

            var colors = btn.colors;
            colors.normalColor = new Color(1f, 1f, 1f, 0f);
            colors.highlightedColor = new Color(1f, 1f, 1f, 0.2f);
            colors.pressedColor = new Color(0f, 0f, 0f, 0.3f);
            colors.selectedColor = new Color(1f, 1f, 1f, 0f);
            btn.colors = colors;
            btn.onClick = new Button.ButtonClickedEvent();

            return obj;
        }

        private static GameObject CreateGoogleConnectOverlayButton(string name, Vector2 pos, Vector2 size, Transform parent)
        {
            var btnObj = CreateOverlayButton(name, pos, size, parent);

            var txtObj = GetOrCreateChild(btnObj.transform, "Text");
            var txtRt = txtObj.GetComponent<RectTransform>();
            txtRt.anchorMin = Vector2.zero;
            txtRt.anchorMax = Vector2.one;
            txtRt.offsetMin = Vector2.zero;
            txtRt.offsetMax = Vector2.zero;

            var txtTmp = txtObj.GetComponent<TextMeshProUGUI>();
            if (txtTmp == null) txtTmp = txtObj.AddComponent<TextMeshProUGUI>();
            txtTmp.text = ""; // Empty by default so baked pixel art "CONNECT TO GOOGLE" is visible
            txtTmp.fontSize = 15f;
            txtTmp.enableAutoSizing = true;
            txtTmp.fontSizeMin = 10f;
            txtTmp.fontSizeMax = 15f;
            txtTmp.fontStyle = FontStyles.Bold;
            txtTmp.alignment = TextAlignmentOptions.Center;
            txtTmp.color = new Color(0.95f, 0.92f, 0.85f, 1f);
            txtTmp.raycastTarget = false;

            return btnObj;
        }

        [MenuItem("Tools/Setup/Generate Login Base Image")]
        public static void GenerateLoginBaseTexture()
        {
            string sourceJpgPath = "Assets/UI/Login/Login_Scroll_Source.jpg";
            string wallPath = SprLoginWallPath;
            string outBasePath = SprLoginScreenBasePath;
            string outScrollPath = SprLoginScrollPath;

            if (!File.Exists(sourceJpgPath))
            {
                Debug.LogWarning("[InGameUISetup] Login_Scroll_Source.jpg not found!");
                return;
            }

            byte[] sourceBytes = File.ReadAllBytes(sourceJpgPath);
            Texture2D sourceTex = new Texture2D(2, 2);
            sourceTex.LoadImage(sourceBytes);

            byte[] wallBytes = File.Exists(wallPath) ? File.ReadAllBytes(wallPath) : null;
            Texture2D wallTex = null;
            if (wallBytes != null)
            {
                wallTex = new Texture2D(2, 2);
                wallTex.LoadImage(wallBytes);
            }

            int w = sourceTex.width;
            int h = sourceTex.height;

            // Flood fill from borders to identify exterior background
            bool[,] isBg = new bool[w, h];
            Queue<Vector2Int> q = new Queue<Vector2Int>();

            void CheckAndEnqueue(int x, int y)
            {
                if (!isBg[x, y])
                {
                    Color c = sourceTex.GetPixel(x, y);
                    if (c.r > 0.90f && c.g > 0.90f && c.b > 0.90f)
                    {
                        isBg[x, y] = true;
                        q.Enqueue(new Vector2Int(x, y));
                    }
                }
            }

            for (int x = 0; x < w; x++)
            {
                CheckAndEnqueue(x, 0);
                CheckAndEnqueue(x, h - 1);
            }
            for (int y = 0; y < h; y++)
            {
                CheckAndEnqueue(0, y);
                CheckAndEnqueue(w - 1, y);
            }

            int[] dx = { 1, -1, 0, 0 };
            int[] dy = { 0, 0, 1, -1 };

            while (q.Count > 0)
            {
                Vector2Int p = q.Dequeue();
                for (int i = 0; i < 4; i++)
                {
                    int nx = p.x + dx[i];
                    int ny = p.y + dy[i];
                    if (nx >= 0 && nx < w && ny >= 0 && ny < h && !isBg[nx, ny])
                    {
                        Color c = sourceTex.GetPixel(nx, ny);
                        if (c.r > 0.90f && c.g > 0.90f && c.b > 0.90f)
                        {
                            isBg[nx, ny] = true;
                            q.Enqueue(new Vector2Int(nx, ny));
                        }
                    }
                }
            }

            Texture2D compositeTex = new Texture2D(w, h, TextureFormat.RGBA32, false);
            Texture2D scrollOnlyTex = new Texture2D(w, h, TextureFormat.RGBA32, false);

            for (int y = 0; y < h; y++)
            {
                for (int x = 0; x < w; x++)
                {
                    if (isBg[x, y])
                    {
                        Color wc = (wallTex != null) ? wallTex.GetPixel(x, y) : new Color(0.12f, 0.08f, 0.05f, 1f);
                        compositeTex.SetPixel(x, y, wc);
                        scrollOnlyTex.SetPixel(x, y, new Color(0f, 0f, 0f, 0f));
                    }
                    else
                    {
                        Color sc = sourceTex.GetPixel(x, y);
                        compositeTex.SetPixel(x, y, sc);
                        scrollOnlyTex.SetPixel(x, y, sc);
                    }
                }
            }

            compositeTex.Apply();
            scrollOnlyTex.Apply();

            File.WriteAllBytes(outBasePath, compositeTex.EncodeToPNG());
            File.WriteAllBytes(outScrollPath, scrollOnlyTex.EncodeToPNG());

            Object.DestroyImmediate(sourceTex);
            if (wallTex != null) Object.DestroyImmediate(wallTex);
            Object.DestroyImmediate(compositeTex);
            Object.DestroyImmediate(scrollOnlyTex);

            AssetDatabase.ImportAsset(outBasePath, ImportAssetOptions.ForceUpdate);
            AssetDatabase.ImportAsset(outScrollPath, ImportAssetOptions.ForceUpdate);
            Debug.Log("[InGameUISetup] Successfully generated composite Login_Screen_Base.png and Login_Scroll_Frame.png!");

            ProcessHoverButtonImage("Assets/UI/Login/Login_Btn_Google_Normal_Raw.jpg", SprLoginBtnGoogleNormalPath);
            ProcessHoverButtonImage("Assets/UI/Login/Login_Btn_Google_Hover_Raw.jpg", SprLoginBtnGoogleHoverPath);
            ProcessHoverButtonImage("Assets/UI/Login/Login_Btn_Guest_Normal_Raw.jpg", SprLoginBtnGuestNormalPath);
            ProcessHoverButtonImage("Assets/UI/Login/Login_Btn_Guest_Hover_Raw.jpg", SprLoginBtnGuestHoverPath);
            EnsureTransparentSprite();
        }

        public static void EnsureSpriteImport(string path)
        {
            var importer = AssetImporter.GetAtPath(path) as TextureImporter;
            if (importer != null)
            {
                bool dirty = false;
                if (importer.textureType != TextureImporterType.Sprite)
                {
                    importer.textureType = TextureImporterType.Sprite;
                    dirty = true;
                }
                if (!importer.alphaIsTransparency)
                {
                    importer.alphaIsTransparency = true;
                    dirty = true;
                }
                if (dirty)
                {
                    importer.SaveAndReimport();
                }
            }
        }

        private static void EnsureTransparentSprite()
        {
            if (!File.Exists(SprLoginBtnTransparentPath))
            {
                var tex = new Texture2D(32, 32, TextureFormat.RGBA32, false);
                for (int y = 0; y < 32; y++)
                {
                    for (int x = 0; x < 32; x++)
                    {
                        tex.SetPixel(x, y, new Color(0f, 0f, 0f, 0f));
                    }
                }
                tex.Apply();
                File.WriteAllBytes(SprLoginBtnTransparentPath, tex.EncodeToPNG());
                Object.DestroyImmediate(tex);
                AssetDatabase.ImportAsset(SprLoginBtnTransparentPath, ImportAssetOptions.ForceUpdate);
            }
            EnsureSpriteImport(SprLoginBtnTransparentPath);
        }

        public static void ProcessHoverButtonImage(string rawJpgPath, string outPngPath)
        {
            if (!File.Exists(rawJpgPath))
            {
                Debug.LogWarning($"[InGameUISetup] Raw hover image not found: {rawJpgPath}");
                return;
            }

            byte[] bytes = File.ReadAllBytes(rawJpgPath);
            Texture2D tex = new Texture2D(2, 2);
            tex.LoadImage(bytes);

            int w = tex.width;
            int h = tex.height;

            bool[,] isBg = new bool[w, h];
            Queue<Vector2Int> q = new Queue<Vector2Int>();

            void EnqueueIfWhite(int x, int y)
            {
                if (!isBg[x, y])
                {
                    Color c = tex.GetPixel(x, y);
                    if (c.r > 0.90f && c.g > 0.90f && c.b > 0.90f)
                    {
                        isBg[x, y] = true;
                        q.Enqueue(new Vector2Int(x, y));
                    }
                }
            }

            for (int x = 0; x < w; x++)
            {
                EnqueueIfWhite(x, 0);
                EnqueueIfWhite(x, h - 1);
            }
            for (int y = 0; y < h; y++)
            {
                EnqueueIfWhite(0, y);
                EnqueueIfWhite(w - 1, y);
            }

            int[] dx = { 1, -1, 0, 0 };
            int[] dy = { 0, 0, 1, -1 };

            while (q.Count > 0)
            {
                Vector2Int p = q.Dequeue();
                for (int i = 0; i < 4; i++)
                {
                    int nx = p.x + dx[i];
                    int ny = p.y + dy[i];
                    if (nx >= 0 && nx < w && ny >= 0 && ny < h && !isBg[nx, ny])
                    {
                        Color c = tex.GetPixel(nx, ny);
                        if (c.r > 0.90f && c.g > 0.90f && c.b > 0.90f)
                        {
                            isBg[nx, ny] = true;
                            q.Enqueue(new Vector2Int(nx, ny));
                        }
                    }
                }
            }

            Texture2D outTex = new Texture2D(w, h, TextureFormat.RGBA32, false);
            for (int y = 0; y < h; y++)
            {
                for (int x = 0; x < w; x++)
                {
                    if (isBg[x, y])
                    {
                        outTex.SetPixel(x, y, new Color(0f, 0f, 0f, 0f));
                    }
                    else
                    {
                        outTex.SetPixel(x, y, tex.GetPixel(x, y));
                    }
                }
            }
            outTex.Apply();

            File.WriteAllBytes(outPngPath, outTex.EncodeToPNG());
            Object.DestroyImmediate(tex);
            Object.DestroyImmediate(outTex);

            AssetDatabase.ImportAsset(outPngPath, ImportAssetOptions.ForceUpdate);
            EnsureSpriteImport(outPngPath);
            Debug.Log($"[InGameUISetup] Successfully generated hover sprite: {outPngPath}");
        }

        public static GameObject BuildLoginPanel(Transform parent, MainMenuController menuCtrl)
        {
            var panelObj = GetOrCreateChild(parent, "LoginPanel");
            panelObj.SetActive(false);

            var panelRt = panelObj.GetComponent<RectTransform>();
            panelRt.anchorMin = Vector2.zero;
            panelRt.anchorMax = Vector2.one;
            panelRt.offsetMin = Vector2.zero;
            panelRt.offsetMax = Vector2.zero;
            panelRt.pivot = new Vector2(0.5f, 0.5f);

            var panelImg = panelObj.GetComponent<Image>();
            if (panelImg == null) panelImg = panelObj.AddComponent<Image>();
            panelImg.color = new Color(0.08f, 0.06f, 0.05f, 1f);
            panelImg.raycastTarget = true;

            // Destroy any previous/nested children (WallBackground, ScrollDialog, etc.)
            for (int i = panelObj.transform.childCount - 1; i >= 0; i--)
            {
                Object.DestroyImmediate(panelObj.transform.GetChild(i).gameObject);
            }

            // Single unified ContentBox matching 16:9 1920x1080 canvas
            var contentObj = GetOrCreateChild(panelObj.transform, "ContentBox");
            var contentRt = contentObj.GetComponent<RectTransform>();
            contentRt.anchorMin = new Vector2(0.5f, 0.5f);
            contentRt.anchorMax = new Vector2(0.5f, 0.5f);
            contentRt.pivot = new Vector2(0.5f, 0.5f);
            contentRt.anchoredPosition = Vector2.zero;
            contentRt.sizeDelta = new Vector2(1920f, 1080f);

            var contentImg = contentObj.GetComponent<Image>();
            if (contentImg == null) contentImg = contentObj.AddComponent<Image>();
            var sprBase = AssetDatabase.LoadAssetAtPath<Sprite>(SprLoginScreenBasePath);
            contentImg.sprite = sprBase;
            contentImg.type = Image.Type.Simple;
            contentImg.preserveAspect = true;
            contentImg.color = Color.white;
            contentImg.raycastTarget = true;

            // 1. Close Button [X] at top right of the parchment
            var sprClose = AssetDatabase.LoadAssetAtPath<Sprite>(SprCloseCleanPath);
            var closeObj = GetOrCreateChild(contentObj.transform, "CloseButton");
            var closeRt = closeObj.GetComponent<RectTransform>();
            closeRt.anchorMin = new Vector2(0.5f, 0.5f);
            closeRt.anchorMax = new Vector2(0.5f, 0.5f);
            closeRt.pivot = new Vector2(0.5f, 0.5f);
            closeRt.anchoredPosition = new Vector2(581f, 356f);
            closeRt.sizeDelta = new Vector2(75f, 75f);

            var closeImg = closeObj.GetComponent<Image>();
            if (closeImg == null) closeImg = closeObj.AddComponent<Image>();
            closeImg.sprite = sprClose;
            closeImg.preserveAspect = true;
            closeImg.raycastTarget = true;

            var closeBtn = closeObj.GetComponent<Button>();
            if (closeBtn == null) closeBtn = closeObj.AddComponent<Button>();
            closeBtn.targetGraphic = closeImg;
            var cColors = closeBtn.colors;
            cColors.normalColor = Color.white;
            cColors.highlightedColor = new Color(1f, 0.85f, 0.85f, 1f);
            cColors.pressedColor = new Color(0.7f, 0.7f, 0.7f, 1f);
            closeBtn.colors = cColors;
            closeBtn.onClick = new Button.ButtonClickedEvent();
            if (menuCtrl != null) UnityEditor.Events.UnityEventTools.AddPersistentListener(closeBtn.onClick, menuCtrl.OnCloseLoginClicked);

            // 2. SIGN IN WITH GOOGLE Button (with user-provided Normal & Hover Sprites)
            var sprGoogleNormal = AssetDatabase.LoadAssetAtPath<Sprite>(SprLoginBtnGoogleNormalPath);
            var sprGoogleHover = AssetDatabase.LoadAssetAtPath<Sprite>(SprLoginBtnGoogleHoverPath);
            var googleBtnObj = CreateHoverButton("Button_GoogleSignIn", sprGoogleNormal, sprGoogleHover, new Vector2(0f, 170f), new Vector2(994f, 290f), contentObj.transform);
            var googleBtn = googleBtnObj.GetComponent<Button>();
            if (menuCtrl != null) UnityEditor.Events.UnityEventTools.AddPersistentListener(googleBtn.onClick, menuCtrl.OnGoogleLoginClicked);

            // 3. PLAY AS GUEST Button (with user-provided Normal & Hover Sprites)
            var sprGuestNormal = AssetDatabase.LoadAssetAtPath<Sprite>(SprLoginBtnGuestNormalPath);
            var sprGuestHover = AssetDatabase.LoadAssetAtPath<Sprite>(SprLoginBtnGuestHoverPath);
            var guestBtnObj = CreateHoverButton("Button_PlayAsGuest", sprGuestNormal, sprGuestHover, new Vector2(0f, -167f), new Vector2(994f, 290f), contentObj.transform);
            var guestBtn = guestBtnObj.GetComponent<Button>();
            if (menuCtrl != null) UnityEditor.Events.UnityEventTools.AddPersistentListener(guestBtn.onClick, menuCtrl.OnGuestLoginClicked);

            // 4. Login Status / Feedback Text
            var statusObj = GetOrCreateChild(contentObj.transform, "LoginStatusText");
            var statusRt = statusObj.GetComponent<RectTransform>();
            statusRt.anchorMin = new Vector2(0.5f, 0.5f);
            statusRt.anchorMax = new Vector2(0.5f, 0.5f);
            statusRt.pivot = new Vector2(0.5f, 0.5f);
            statusRt.anchoredPosition = new Vector2(0f, -345f);
            statusRt.sizeDelta = new Vector2(900f, 45f);

            var statusTmp = statusObj.GetComponent<TextMeshProUGUI>();
            if (statusTmp == null) statusTmp = statusObj.AddComponent<TextMeshProUGUI>();
            statusTmp.text = "";
            statusTmp.fontSize = 24f;
            statusTmp.fontStyle = FontStyles.Bold;
            statusTmp.alignment = TextAlignmentOptions.Center;
            statusTmp.color = new Color(0.85f, 0.15f, 0.15f, 1f);
            statusTmp.raycastTarget = false;

            // Connect references on MainMenuController
            if (menuCtrl != null)
            {
                var so = new SerializedObject(menuCtrl);
                so.Update();
                var spLogin = so.FindProperty("loginPanel");
                if (spLogin != null) spLogin.objectReferenceValue = panelObj;

                var spUser = so.FindProperty("usernameInput");
                if (spUser != null) spUser.objectReferenceValue = null;

                var spPass = so.FindProperty("passwordInput");
                if (spPass != null) spPass.objectReferenceValue = null;

                var spStatus = so.FindProperty("loginStatusText");
                if (spStatus != null) spStatus.objectReferenceValue = statusTmp;

                so.ApplyModifiedProperties();
            }

            return panelObj;
        }

        private static TMP_InputField CreateInputField(string name, string placeholderText, bool isPassword, Vector2 pos, Vector2 size, Transform parent, Sprite sprFrame)
        {
            var go = GetOrCreateChild(parent, name);
            var rt = go.GetComponent<RectTransform>();
            rt.anchorMin = new Vector2(0.5f, 0.5f);
            rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.anchoredPosition = pos;
            rt.sizeDelta = size;

            var img = go.GetComponent<Image>();
            if (img == null) img = go.AddComponent<Image>();
            if (sprFrame != null)
            {
                img.sprite = sprFrame;
                img.type = Image.Type.Simple;
                img.color = Color.white;
            }
            else
            {
                img.sprite = null;
                img.color = new Color(0f, 0f, 0f, 0f);
            }
            img.raycastTarget = true;

            var inputField = go.GetComponent<TMP_InputField>();
            if (inputField == null) inputField = go.AddComponent<TMP_InputField>();
            inputField.targetGraphic = img;

            var textArea = GetOrCreateChild(go.transform, "Text Area");
            var textAreaRt = textArea.GetComponent<RectTransform>();
            textAreaRt.anchorMin = Vector2.zero;
            textAreaRt.anchorMax = Vector2.one;
            textAreaRt.offsetMin = new Vector2(40f, 10f);
            textAreaRt.offsetMax = new Vector2(-40f, -10f);

            var rectMask = textArea.GetComponent<RectMask2D>();
            if (rectMask == null) rectMask = textArea.AddComponent<RectMask2D>();

            var phObj = GetOrCreateChild(textArea.transform, "Placeholder");
            var phRt = phObj.GetComponent<RectTransform>();
            phRt.anchorMin = Vector2.zero;
            phRt.anchorMax = Vector2.one;
            phRt.offsetMin = Vector2.zero;
            phRt.offsetMax = Vector2.zero;

            var phTmp = phObj.GetComponent<TextMeshProUGUI>();
            if (phTmp == null) phTmp = phObj.AddComponent<TextMeshProUGUI>();
            phTmp.text = placeholderText;
            phTmp.fontSize = 32f;
            phTmp.fontStyle = FontStyles.Bold;
            phTmp.alignment = TextAlignmentOptions.Center;
            phTmp.color = new Color(0.70f, 0.52f, 0.35f, 0.85f);

            var txtObj = GetOrCreateChild(textArea.transform, "Text");
            var txtRt = txtObj.GetComponent<RectTransform>();
            txtRt.anchorMin = Vector2.zero;
            txtRt.anchorMax = Vector2.one;
            txtRt.offsetMin = Vector2.zero;
            txtRt.offsetMax = Vector2.zero;

            var txtTmp = txtObj.GetComponent<TextMeshProUGUI>();
            if (txtTmp == null) txtTmp = txtObj.AddComponent<TextMeshProUGUI>();
            txtTmp.fontSize = 32f;
            txtTmp.fontStyle = FontStyles.Bold;
            txtTmp.alignment = TextAlignmentOptions.Center;
            txtTmp.color = new Color(0.96f, 0.90f, 0.80f, 1f);

            inputField.textViewport = textAreaRt;
            inputField.textComponent = txtTmp;
            inputField.placeholder = phTmp;
            inputField.caretColor = new Color(1f, 0.95f, 0.85f, 1f);
            inputField.customCaretColor = true;
            inputField.caretWidth = 3;
            inputField.lineType = TMP_InputField.LineType.SingleLine;

            if (isPassword)
            {
                inputField.contentType = TMP_InputField.ContentType.Password;
                inputField.inputType = TMP_InputField.InputType.Password;
            }
            else
            {
                inputField.contentType = TMP_InputField.ContentType.Standard;
                inputField.inputType = TMP_InputField.InputType.Standard;
            }

            return inputField;
        }

        private static GameObject CreateHoverButton(string name, Sprite sprNormal, Sprite sprHover, Vector2 pos, Vector2 size, Transform parent)
        {
            var obj = GetOrCreateChild(parent, name);
            var rt = obj.GetComponent<RectTransform>();
            rt.anchorMin = new Vector2(0.5f, 0.5f);
            rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.anchoredPosition = pos;
            rt.sizeDelta = size;

            var img = obj.GetComponent<Image>();
            if (img == null) img = obj.AddComponent<Image>();
            img.sprite = sprNormal;
            img.color = Color.white;
            img.preserveAspect = false;
            img.raycastTarget = true;

            var btn = obj.GetComponent<Button>();
            if (btn == null) btn = obj.AddComponent<Button>();
            btn.targetGraphic = img;
            btn.transition = Selectable.Transition.SpriteSwap;

            var ss = btn.spriteState;
            ss.highlightedSprite = sprHover;
            ss.pressedSprite = sprHover;
            ss.selectedSprite = sprHover;
            btn.spriteState = ss;

            btn.onClick = new Button.ButtonClickedEvent();
            return obj;
        }

        private static GameObject BuildBagModal(Transform parent, Sprite sprParchment, Sprite sprBtnFrame)
        {
            var modalObj = GetOrCreateChild(parent, "BagModal");
            modalObj.SetActive(false);

            var modalRt = modalObj.GetComponent<RectTransform>();
            modalRt.anchorMin = Vector2.zero;
            modalRt.anchorMax = Vector2.one;
            modalRt.offsetMin = Vector2.zero;
            modalRt.offsetMax = Vector2.zero;

            var dimmerObj = GetOrCreateChild(modalObj.transform, "Dimmer");
            var dimmerRt = dimmerObj.GetComponent<RectTransform>();
            dimmerRt.anchorMin = Vector2.zero;
            dimmerRt.anchorMax = Vector2.one;
            dimmerRt.offsetMin = Vector2.zero;
            dimmerRt.offsetMax = Vector2.zero;

            var dimmerImg = dimmerObj.GetComponent<Image>();
            if (dimmerImg == null) dimmerImg = dimmerObj.AddComponent<Image>();
            dimmerImg.color = new Color(0f, 0f, 0f, 0.75f);
            dimmerImg.raycastTarget = true;

            var dimmerBtn = dimmerObj.GetComponent<Button>();
            if (dimmerBtn == null) dimmerBtn = dimmerObj.AddComponent<Button>();
            dimmerBtn.transition = Selectable.Transition.None;

            var dialogObj = GetOrCreateChild(modalObj.transform, "DialogBox");
            var dialogRt = dialogObj.GetComponent<RectTransform>();
            dialogRt.anchorMin = new Vector2(0.5f, 0.5f);
            dialogRt.anchorMax = new Vector2(0.5f, 0.5f);
            dialogRt.pivot = new Vector2(0.5f, 0.5f);
            dialogRt.anchoredPosition = Vector2.zero;
            dialogRt.sizeDelta = new Vector2(899f, 492f);

            var dialogImg = dialogObj.GetComponent<Image>();
            if (dialogImg == null) dialogImg = dialogObj.AddComponent<Image>();
            var sprBag = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/UI/BagInventory_Frame.png");
            if (sprBag != null)
            {
                dialogImg.sprite = sprBag;
                dialogImg.type = Image.Type.Simple;
                dialogImg.color = Color.white;
            }
            else if (sprParchment != null)
            {
                dialogImg.sprite = sprParchment;
                dialogImg.type = Image.Type.Sliced;
                dialogImg.color = Color.white;
            }
            else
            {
                dialogImg.color = new Color(0.97f, 0.94f, 0.88f, 1f);
            }

            var titleObj = GetOrCreateChild(dialogObj.transform, "TitleText");
            titleObj.SetActive(false); // Baked into flap art

            var countObj = GetOrCreateChild(dialogObj.transform, "CountText");
            var countRt = countObj.GetComponent<RectTransform>();
            countRt.anchorMin = new Vector2(0.5f, 0.5f);
            countRt.anchorMax = new Vector2(0.5f, 0.5f);
            countRt.pivot = new Vector2(0f, 0.5f);
            countRt.anchoredPosition = new Vector2(82f, 169f);
            countRt.sizeDelta = new Vector2(120f, 30f);

            var countTmp = countObj.GetComponent<TextMeshProUGUI>();
            if (countTmp == null) countTmp = countObj.AddComponent<TextMeshProUGUI>();
            countTmp.text = "0";
            countTmp.fontSize = 22f;
            countTmp.fontStyle = FontStyles.Bold;
            countTmp.alignment = TextAlignmentOptions.MidlineLeft;
            countTmp.color = new Color(0.24f, 0.16f, 0.08f, 1f);

            var scrollObj = GetOrCreateChild(dialogObj.transform, "ArtifactScrollView");
            var scrollRt = scrollObj.GetComponent<RectTransform>();
            scrollRt.anchorMin = new Vector2(0.5f, 0.5f);
            scrollRt.anchorMax = new Vector2(0.5f, 0.5f);
            scrollRt.pivot = new Vector2(0.5f, 0.5f);
            scrollRt.anchoredPosition = new Vector2(0f, -36f);
            scrollRt.sizeDelta = new Vector2(790f, 325f);

            var scrollRect = scrollObj.GetComponent<ScrollRect>();
            if (scrollRect == null) scrollRect = scrollObj.AddComponent<ScrollRect>();
            scrollRect.horizontal = false;
            scrollRect.vertical = true;

            var viewportObj = GetOrCreateChild(scrollObj.transform, "Viewport");
            var viewportRt = viewportObj.GetComponent<RectTransform>();
            viewportRt.anchorMin = Vector2.zero;
            viewportRt.anchorMax = Vector2.one;
            viewportRt.offsetMin = Vector2.zero;
            viewportRt.offsetMax = Vector2.zero;

            var viewportMask = viewportObj.GetComponent<Mask>();
            if (viewportMask == null) viewportMask = viewportObj.AddComponent<Mask>();
            viewportMask.showMaskGraphic = false;

            var viewportImg = viewportObj.GetComponent<Image>();
            if (viewportImg == null) viewportImg = viewportObj.AddComponent<Image>();
            viewportImg.color = Color.white;

            var gridObj = GetOrCreateChild(viewportObj.transform, "GridContent");
            var gridRt = gridObj.GetComponent<RectTransform>();
            gridRt.anchorMin = new Vector2(0f, 1f);
            gridRt.anchorMax = new Vector2(1f, 1f);
            gridRt.pivot = new Vector2(0.5f, 1f);
            gridRt.anchoredPosition = Vector2.zero;
            gridRt.sizeDelta = new Vector2(0f, 325f);

            var glg = gridObj.GetComponent<GridLayoutGroup>();
            if (glg == null) glg = gridObj.AddComponent<GridLayoutGroup>();
            glg.cellSize = new Vector2(382f, 95f);
            glg.spacing = new Vector2(16f, 12f);
            glg.padding = new RectOffset(0, 0, 0, 0);
            glg.startCorner = GridLayoutGroup.Corner.UpperLeft;
            glg.startAxis = GridLayoutGroup.Axis.Horizontal;
            glg.childAlignment = TextAnchor.UpperLeft;
            glg.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            glg.constraintCount = 2;

            var csf = gridObj.GetComponent<ContentSizeFitter>();
            if (csf == null) csf = gridObj.AddComponent<ContentSizeFitter>();
            csf.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            scrollRect.content = gridRt;
            scrollRect.viewport = viewportRt;

            var emptyNoticeObj = GetOrCreateChild(dialogObj.transform, "EmptyNoticeText");
            var enRt = emptyNoticeObj.GetComponent<RectTransform>();
            enRt.anchorMin = new Vector2(0.5f, 0.5f);
            enRt.anchorMax = new Vector2(0.5f, 0.5f);
            enRt.pivot = new Vector2(0.5f, 0.5f);
            enRt.anchoredPosition = new Vector2(0f, -36f);
            enRt.sizeDelta = new Vector2(650f, 100f);

            var enTmp = emptyNoticeObj.GetComponent<TextMeshProUGUI>();
            if (enTmp == null) enTmp = emptyNoticeObj.AddComponent<TextMeshProUGUI>();
            enTmp.text = "No artifacts in your bag yet.\nAnswer quizzes correctly in Alterworld trials to recover artifacts!";
            enTmp.fontSize = 20f;
            enTmp.alignment = TextAlignmentOptions.Center;
            enTmp.color = new Color(0.85f, 0.80f, 0.70f, 0.95f);

            var closeBtnObj = GetOrCreateChild(dialogObj.transform, "CloseButton");
            var closeRt = closeBtnObj.GetComponent<RectTransform>();
            closeRt.anchorMin = new Vector2(1f, 1f);
            closeRt.anchorMax = new Vector2(1f, 1f);
            closeRt.pivot = new Vector2(1f, 1f);
            closeRt.anchoredPosition = new Vector2(-16f, -16f);
            closeRt.sizeDelta = new Vector2(34f, 34f);

            var closeImg = closeBtnObj.GetComponent<Image>();
            if (closeImg == null) closeImg = closeBtnObj.AddComponent<Image>();
            closeImg.color = new Color(0.72f, 0.20f, 0.18f, 0.95f);

            var closeOutline = closeBtnObj.GetComponent<Outline>();
            if (closeOutline == null) closeOutline = closeBtnObj.AddComponent<Outline>();
            closeOutline.effectColor = new Color(0.35f, 0.08f, 0.06f, 0.9f);
            closeOutline.effectDistance = new Vector2(1f, -1f);

            var closeBtn = closeBtnObj.GetComponent<Button>();
            if (closeBtn == null) closeBtn = closeBtnObj.AddComponent<Button>();

            var closeTxtObj = GetOrCreateChild(closeBtnObj.transform, "Text");
            var closeTxtRt = closeTxtObj.GetComponent<RectTransform>();
            closeTxtRt.anchorMin = Vector2.zero;
            closeTxtRt.anchorMax = Vector2.one;
            closeTxtRt.offsetMin = Vector2.zero;
            closeTxtRt.offsetMax = Vector2.zero;

            var closeTmp = closeTxtObj.GetComponent<TextMeshProUGUI>();
            if (closeTmp == null) closeTmp = closeTxtObj.AddComponent<TextMeshProUGUI>();
            closeTmp.text = "X";
            closeTmp.fontSize = 20f;
            closeTmp.fontStyle = FontStyles.Bold;
            closeTmp.alignment = TextAlignmentOptions.Center;
            closeTmp.color = Color.white;

            var bagModalComp = modalObj.GetComponent<BagModalUI>();
            if (bagModalComp == null) bagModalComp = modalObj.AddComponent<BagModalUI>();

            var soModal = new SerializedObject(bagModalComp);
            soModal.Update();
            var spModalRoot = soModal.FindProperty("modalRoot");
            if (spModalRoot != null) spModalRoot.objectReferenceValue = modalObj;

            var spGrid = soModal.FindProperty("itemsGridContainer");
            if (spGrid != null) spGrid.objectReferenceValue = gridObj.transform;

            var spTitle = soModal.FindProperty("titleText");
            if (spTitle != null) spTitle.objectReferenceValue = null;

            var spCount = soModal.FindProperty("countText");
            if (spCount != null) spCount.objectReferenceValue = countTmp;

            var spEmpty = soModal.FindProperty("emptyNoticeText");
            if (spEmpty != null) spEmpty.objectReferenceValue = enTmp;

            var spClose = soModal.FindProperty("closeButton");
            if (spClose != null) spClose.objectReferenceValue = closeBtn;

            var spDimmer = soModal.FindProperty("dimmerButton");
            if (spDimmer != null) spDimmer.objectReferenceValue = dimmerBtn;

            soModal.ApplyModifiedProperties();

            closeBtn.onClick = new Button.ButtonClickedEvent();
            UnityEditor.Events.UnityEventTools.AddPersistentListener(closeBtn.onClick, bagModalComp.CloseBag);

            dimmerBtn.onClick = new Button.ButtonClickedEvent();
            UnityEditor.Events.UnityEventTools.AddPersistentListener(dimmerBtn.onClick, bagModalComp.CloseBag);

            modalObj.SetActive(false);
            return modalObj;
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
            txtTmp.fontSize = 20f;
            txtTmp.fontStyle = FontStyles.Bold;
            txtTmp.alignment = TextAlignmentOptions.Center;
            txtTmp.color = new Color(0.95f, 0.92f, 0.85f, 1f);

            return btnObj;
        }

        private static GameObject CreateGoogleConnectButton(string name, Vector2 pos, Vector2 size, Transform parent, Sprite sprNormal, Sprite sprHover, Sprite sprFrame)
        {
            var btnObj = GetOrCreateChild(parent, name);
            var btnRt = btnObj.GetComponent<RectTransform>();
            btnRt.anchorMin = new Vector2(0.5f, 1f);
            btnRt.anchorMax = new Vector2(0.5f, 1f);
            btnRt.pivot = new Vector2(0.5f, 1f);
            btnRt.anchoredPosition = pos;
            btnRt.sizeDelta = size;

            var btnImg = btnObj.GetComponent<Image>();
            if (btnImg == null) btnImg = btnObj.AddComponent<Image>();
            btnImg.sprite = sprFrame;
            btnImg.type = Image.Type.Sliced;
            btnImg.color = Color.white;
            btnImg.raycastTarget = true;

            // Google Icon (child) with normal and hover states
            var iconObj = GetOrCreateChild(btnObj.transform, "GoogleIcon");
            var iconRt = iconObj.GetComponent<RectTransform>();
            iconRt.anchorMin = new Vector2(0f, 0.5f);
            iconRt.anchorMax = new Vector2(0f, 0.5f);
            iconRt.pivot = new Vector2(0.5f, 0.5f);
            iconRt.anchoredPosition = new Vector2(22f, 0f);
            iconRt.sizeDelta = new Vector2(28f, 28f);

            var iconImg = iconObj.GetComponent<Image>();
            if (iconImg == null) iconImg = iconObj.AddComponent<Image>();
            iconImg.sprite = sprNormal;
            iconImg.preserveAspect = true;
            iconImg.raycastTarget = false;

            // Text
            var txtObj = GetOrCreateChild(btnObj.transform, "Text");
            var txtRt = txtObj.GetComponent<RectTransform>();
            txtRt.anchorMin = Vector2.zero;
            txtRt.anchorMax = Vector2.one;
            txtRt.offsetMin = new Vector2(40f, 0f);
            txtRt.offsetMax = new Vector2(-8f, 0f);

            var txtTmp = txtObj.GetComponent<TextMeshProUGUI>();
            if (txtTmp == null) txtTmp = txtObj.AddComponent<TextMeshProUGUI>();
            txtTmp.text = "CONNECT TO GOOGLE";
            txtTmp.fontSize = 15f;
            txtTmp.enableAutoSizing = true;
            txtTmp.fontSizeMin = 10f;
            txtTmp.fontSizeMax = 15f;
            txtTmp.fontStyle = FontStyles.Bold;
            txtTmp.alignment = TextAlignmentOptions.Center;
            txtTmp.color = new Color(0.95f, 0.92f, 0.85f, 1f);
            txtTmp.raycastTarget = false;

            // Button with SpriteSwap targeting GoogleIcon
            var btnComp = btnObj.GetComponent<Button>();
            if (btnComp == null) btnComp = btnObj.AddComponent<Button>();
            btnComp.targetGraphic = iconImg;
            btnComp.transition = Selectable.Transition.SpriteSwap;

            var ss = btnComp.spriteState;
            ss.highlightedSprite = sprHover;
            ss.pressedSprite = sprHover;
            ss.selectedSprite = sprHover;
            btnComp.spriteState = ss;

            return btnObj;
        }

        private static GameObject BuildBookModal(Transform parent)
        {
            var modalObj = GetOrCreateChild(parent, "BookModal");
            modalObj.SetActive(false);

            var modalRt = modalObj.GetComponent<RectTransform>();
            modalRt.anchorMin = Vector2.zero;
            modalRt.anchorMax = Vector2.one;
            modalRt.offsetMin = Vector2.zero;
            modalRt.offsetMax = Vector2.zero;

            var modalComp = modalObj.GetComponent<BookModalUI>();
            if (modalComp == null) modalComp = modalObj.AddComponent<BookModalUI>();

            // Dimmer
            var dimmerObj = GetOrCreateChild(modalObj.transform, "Dimmer");
            var dimmerRt = dimmerObj.GetComponent<RectTransform>();
            dimmerRt.anchorMin = Vector2.zero;
            dimmerRt.anchorMax = Vector2.one;
            dimmerRt.offsetMin = Vector2.zero;
            dimmerRt.offsetMax = Vector2.zero;

            var dimmerImg = dimmerObj.GetComponent<Image>();
            if (dimmerImg == null) dimmerImg = dimmerObj.AddComponent<Image>();
            dimmerImg.color = new Color(0f, 0f, 0f, 0.75f);
            dimmerImg.raycastTarget = true;

            var dimmerBtn = dimmerObj.GetComponent<Button>();
            if (dimmerBtn == null) dimmerBtn = dimmerObj.AddComponent<Button>();
            dimmerBtn.transition = Selectable.Transition.None;
            dimmerBtn.onClick = new Button.ButtonClickedEvent();
            UnityEditor.Events.UnityEventTools.AddPersistentListener(dimmerBtn.onClick, modalComp.CloseBook);

            // Book Container (size 840 x 702 maintaining 670:560 ratio)
            var bookContObj = GetOrCreateChild(modalObj.transform, "BookContainer");
            var bookContRt = bookContObj.GetComponent<RectTransform>();
            bookContRt.anchorMin = new Vector2(0.5f, 0.5f);
            bookContRt.anchorMax = new Vector2(0.5f, 0.5f);
            bookContRt.pivot = new Vector2(0.5f, 0.5f);
            bookContRt.anchoredPosition = Vector2.zero;
            bookContRt.sizeDelta = new Vector2(840f, 702f);

            // Open Book Frame
            var bookFrameObj = GetOrCreateChild(bookContObj.transform, "BookFrame");
            var bookFrameRt = bookFrameObj.GetComponent<RectTransform>();
            bookFrameRt.anchorMin = Vector2.zero;
            bookFrameRt.anchorMax = Vector2.one;
            bookFrameRt.offsetMin = Vector2.zero;
            bookFrameRt.offsetMax = Vector2.zero;

            var bookFrameImg = bookFrameObj.GetComponent<Image>();
            if (bookFrameImg == null) bookFrameImg = bookFrameObj.AddComponent<Image>();
            var sprFrame = AssetDatabase.LoadAssetAtPath<Sprite>(SprBookFramePath);
            if (sprFrame != null) bookFrameImg.sprite = sprFrame;
            bookFrameImg.type = Image.Type.Simple;
            bookFrameImg.preserveAspect = true;
            bookFrameImg.raycastTarget = true;

            // Close Button [X] (top right corner of book)
            var closeObj = GetOrCreateChild(bookContObj.transform, "CloseButton");
            var closeRt = closeObj.GetComponent<RectTransform>();
            closeRt.anchorMin = new Vector2(1f, 1f);
            closeRt.anchorMax = new Vector2(1f, 1f);
            closeRt.pivot = new Vector2(1f, 1f);
            closeRt.anchoredPosition = new Vector2(-12f, -12f);
            closeRt.sizeDelta = new Vector2(40f, 40f);

            var closeImg = closeObj.GetComponent<Image>();
            if (closeImg == null) closeImg = closeObj.AddComponent<Image>();
            var sprClose = AssetDatabase.LoadAssetAtPath<Sprite>(SprCloseCleanPath);
            if (sprClose != null) closeImg.sprite = sprClose;
            closeImg.preserveAspect = true;
            closeImg.raycastTarget = true;

            var closeBtn = closeObj.GetComponent<Button>();
            if (closeBtn == null) closeBtn = closeObj.AddComponent<Button>();
            closeBtn.onClick = new Button.ButtonClickedEvent();
            UnityEditor.Events.UnityEventTools.AddPersistentListener(closeBtn.onClick, modalComp.CloseBook);

            // Left Arrow Button (Prev Page)
            var prevObj = GetOrCreateChild(bookContObj.transform, "Button_PrevPage");
            var prevRt = prevObj.GetComponent<RectTransform>();
            prevRt.anchorMin = new Vector2(0.5f, 0.5f);
            prevRt.anchorMax = new Vector2(0.5f, 0.5f);
            prevRt.pivot = new Vector2(0.5f, 0.5f);
            prevRt.anchoredPosition = new Vector2(-485f, -10f);
            prevRt.sizeDelta = new Vector2(96f, 120f);

            var prevImg = prevObj.GetComponent<Image>();
            if (prevImg == null) prevImg = prevObj.AddComponent<Image>();
            var sprLeft = AssetDatabase.LoadAssetAtPath<Sprite>(SprBookArrowLeftPath);
            if (sprLeft != null) prevImg.sprite = sprLeft;
            prevImg.preserveAspect = true;
            prevImg.raycastTarget = true;

            var prevBtn = prevObj.GetComponent<Button>();
            if (prevBtn == null) prevBtn = prevObj.AddComponent<Button>();
            var pColors = prevBtn.colors;
            pColors.normalColor = Color.white;
            pColors.highlightedColor = new Color(1f, 0.95f, 0.8f, 1f);
            pColors.pressedColor = new Color(0.85f, 0.75f, 0.6f, 1f);
            prevBtn.colors = pColors;
            prevBtn.onClick = new Button.ButtonClickedEvent();
            UnityEditor.Events.UnityEventTools.AddPersistentListener(prevBtn.onClick, modalComp.OnPrevPageClicked);

            // Right Arrow Button (Next Page)
            var nextObj = GetOrCreateChild(bookContObj.transform, "Button_NextPage");
            var nextRt = nextObj.GetComponent<RectTransform>();
            nextRt.anchorMin = new Vector2(0.5f, 0.5f);
            nextRt.anchorMax = new Vector2(0.5f, 0.5f);
            nextRt.pivot = new Vector2(0.5f, 0.5f);
            nextRt.anchoredPosition = new Vector2(485f, -10f);
            nextRt.sizeDelta = new Vector2(96f, 120f);

            var nextImg = nextObj.GetComponent<Image>();
            if (nextImg == null) nextImg = nextObj.AddComponent<Image>();
            var sprRight = AssetDatabase.LoadAssetAtPath<Sprite>(SprBookArrowRightPath);
            if (sprRight != null) nextImg.sprite = sprRight;
            nextImg.preserveAspect = true;
            nextImg.raycastTarget = true;

            var nextBtn = nextObj.GetComponent<Button>();
            if (nextBtn == null) nextBtn = nextObj.AddComponent<Button>();
            nextBtn.colors = pColors;
            nextBtn.onClick = new Button.ButtonClickedEvent();
            UnityEditor.Events.UnityEventTools.AddPersistentListener(nextBtn.onClick, modalComp.OnNextPageClicked);

            // Left Page Container
            var leftContObj = GetOrCreateChild(bookContObj.transform, "LeftPageContainer");
            var leftContRt = leftContObj.GetComponent<RectTransform>();
            leftContRt.anchorMin = new Vector2(0.06f, 0.10f);
            leftContRt.anchorMax = new Vector2(0.48f, 0.88f);
            leftContRt.offsetMin = Vector2.zero;
            leftContRt.offsetMax = Vector2.zero;

            // Right Page Container
            var rightContObj = GetOrCreateChild(bookContObj.transform, "RightPageContainer");
            var rightContRt = rightContObj.GetComponent<RectTransform>();
            rightContRt.anchorMin = new Vector2(0.52f, 0.10f);
            rightContRt.anchorMax = new Vector2(0.94f, 0.88f);
            rightContRt.offsetMin = Vector2.zero;
            rightContRt.offsetMax = Vector2.zero;

            // Wire SerializedObject
            var so = new SerializedObject(modalComp);
            so.Update();
            var spModalRoot = so.FindProperty("modalRoot");
            if (spModalRoot != null) spModalRoot.objectReferenceValue = modalObj;
            var spBookCont = so.FindProperty("bookContainer");
            if (spBookCont != null) spBookCont.objectReferenceValue = bookContRt;
            var spBookFrame = so.FindProperty("bookFrameImage");
            if (spBookFrame != null) spBookFrame.objectReferenceValue = bookFrameImg;
            var spPrevBtn = so.FindProperty("prevPageButton");
            if (spPrevBtn != null) spPrevBtn.objectReferenceValue = prevBtn;
            var spNextBtn = so.FindProperty("nextPageButton");
            if (spNextBtn != null) spNextBtn.objectReferenceValue = nextBtn;
            var spCloseBtn = so.FindProperty("closeButton");
            if (spCloseBtn != null) spCloseBtn.objectReferenceValue = closeBtn;
            var spDimmerBtn = so.FindProperty("dimmerButton");
            if (spDimmerBtn != null) spDimmerBtn.objectReferenceValue = dimmerBtn;
            var spLeftCont = so.FindProperty("leftPageContainer");
            if (spLeftCont != null) spLeftCont.objectReferenceValue = leftContRt;
            var spRightCont = so.FindProperty("rightPageContainer");
            if (spRightCont != null) spRightCont.objectReferenceValue = rightContRt;
            so.ApplyModifiedProperties();

            return modalObj;
        }

        private static void WireInGameMenuController(InGameMenuController menuCtrl, GameObject settingsPanel, BagModalUI bagModalComp)
        {
            var so = new SerializedObject(menuCtrl);
            so.Update();

            var spSettingsPanel = so.FindProperty("settingsPanel");
            if (spSettingsPanel != null) spSettingsPanel.objectReferenceValue = settingsPanel;

            var spBagModal = so.FindProperty("bagModal");
            if (spBagModal != null) spBagModal.objectReferenceValue = bagModalComp;

            var spBookBtn = so.FindProperty("bookButton");
            if (spBookBtn != null)
            {
                var btnBook = menuCtrl.transform.Find("BookButton")?.GetComponent<Button>();
                if (btnBook != null) spBookBtn.objectReferenceValue = btnBook;
            }

            var dialog = settingsPanel.transform.Find("DialogBox");
            if (dialog != null)
            {
                var sMaster = dialog.Find("Slider_MasterVolume")?.GetComponent<Slider>();
                var sSFX = dialog.Find("Slider_SoundEffects")?.GetComponent<Slider>();
                var sBGM = dialog.Find("Slider_BackgroundMusic")?.GetComponent<Slider>();

                var spMaster = so.FindProperty("masterVolumeSlider");
                if (spMaster != null) spMaster.objectReferenceValue = sMaster;

                var spSFX = so.FindProperty("sfxVolumeSlider");
                if (spSFX != null) spSFX.objectReferenceValue = sSFX;

                var spBGM = so.FindProperty("bgmVolumeSlider");
                if (spBGM != null) spBGM.objectReferenceValue = sBGM;

                var btnGoogle = dialog.Find("Button_ConnectToGoogle")?.GetComponent<Button>();
                var spGoogleBtn = so.FindProperty("connectGoogleButton");
                if (spGoogleBtn != null) spGoogleBtn.objectReferenceValue = btnGoogle;

                var txtGoogle = dialog.Find("Button_ConnectToGoogle/Text")?.GetComponent<TextMeshProUGUI>();
                var spGoogleTxt = so.FindProperty("connectGoogleText");
                if (spGoogleTxt != null) spGoogleTxt.objectReferenceValue = txtGoogle;
            }

            so.ApplyModifiedProperties();
        }

        public static void SyncGameplayScenes()
        {
            string[] standaloneScenes = new string[] {
                "Assets/Scenes/Lobby.unity",
                "Assets/Scenes/LeftHallway.unity",
                "Assets/Scenes/RightHallway.unity",
                "Assets/Scenes/AltLobby.unity",
                "Assets/Scenes/AltLeftHallway.unity",
                "Assets/Scenes/AltRightHallway.unity"
            };

            var sprSettings = AssetDatabase.LoadAssetAtPath<Sprite>(SprSettingsPath);
            var sprBackpack = AssetDatabase.LoadAssetAtPath<Sprite>(SprBackpackPath);
            var sprBook = AssetDatabase.LoadAssetAtPath<Sprite>(SprBookPath);
            var sprParchment = AssetDatabase.LoadAssetAtPath<Sprite>(SprParchmentPath);
            var sprBtnFrame = AssetDatabase.LoadAssetAtPath<Sprite>(SprButtonFramePath);

            foreach (var scenePath in standaloneScenes)
            {
                if (!System.IO.File.Exists(scenePath)) continue;

                var scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);
                var existingCanvas = GameObject.Find("Canvas");
                if (existingCanvas != null)
                {
                    var menuCtrl = existingCanvas.GetComponent<InGameMenuController>();
                    if (menuCtrl == null) menuCtrl = existingCanvas.AddComponent<InGameMenuController>();

                    var oldHome = existingCanvas.transform.Find("HomeButton");
                    if (oldHome != null) oldHome.gameObject.SetActive(false);

                    var heartCont = existingCanvas.transform.Find("HeartContainer");
                    if (heartCont != null) heartCont.gameObject.SetActive(false);

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
                    btnBook.SetActive(false);
                    var btnBookComp = btnBook.GetComponent<Button>();
                    btnBookComp.onClick = new Button.ButtonClickedEvent();
                    UnityEditor.Events.UnityEventTools.AddPersistentListener(btnBookComp.onClick, menuCtrl.OnBookClicked);

                    var settingsPanel = BuildSettingsModal(existingCanvas.transform, menuCtrl, isGameplay: true);
                    var bagModalObj = BuildBagModal(existingCanvas.transform, sprParchment, sprBtnFrame);
                    var bagModalComp = bagModalObj.GetComponent<BagModalUI>();
                    var bookModalObj = BuildBookModal(existingCanvas.transform);

                    WireInGameMenuController(menuCtrl, settingsPanel, bagModalComp);

                    EditorSceneManager.MarkSceneDirty(scene);
                    EditorSceneManager.SaveScene(scene);
                }
            }
        }

        public static void SetupMainMenuScene()
        {
            string scenePath = "Assets/Scenes/MainMenu.unity";
            if (!System.IO.File.Exists(scenePath)) return;

            var scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);
            var canvas = GameObject.Find("Canvas");
            if (canvas == null) return;

            var menuCtrl = canvas.GetComponent<MainMenuController>();
            if (menuCtrl == null) menuCtrl = canvas.AddComponent<MainMenuController>();

            // Setup SettingsPanel
            var panelObj = GetOrCreateChild(canvas.transform, "SettingsPanel");
            panelObj.SetActive(false);

            var panelRt = panelObj.GetComponent<RectTransform>();
            panelRt.anchorMin = Vector2.zero;
            panelRt.anchorMax = Vector2.one;
            panelRt.offsetMin = Vector2.zero;
            panelRt.offsetMax = Vector2.zero;

            var dimmerObj = GetOrCreateChild(panelObj.transform, "Dimmer");
            var dimmerRt = dimmerObj.GetComponent<RectTransform>();
            dimmerRt.anchorMin = Vector2.zero;
            dimmerRt.anchorMax = Vector2.one;
            dimmerRt.offsetMin = Vector2.zero;
            dimmerRt.offsetMax = Vector2.zero;

            var dimmerImg = dimmerObj.GetComponent<Image>();
            if (dimmerImg == null) dimmerImg = dimmerObj.AddComponent<Image>();
            dimmerImg.color = new Color(0f, 0f, 0f, 0.85f);
            dimmerImg.raycastTarget = true;

            var dimmerBtn = dimmerObj.GetComponent<Button>();
            if (dimmerBtn == null) dimmerBtn = dimmerObj.AddComponent<Button>();
            dimmerBtn.transition = Selectable.Transition.None;
            dimmerBtn.onClick = new Button.ButtonClickedEvent();
            UnityEditor.Events.UnityEventTools.AddPersistentListener(dimmerBtn.onClick, menuCtrl.OnCloseSettingsClicked);

            var dialogObj = GetOrCreateChild(panelObj.transform, "DialogBox");
            var dialogRt = dialogObj.GetComponent<RectTransform>();
            dialogRt.anchorMin = new Vector2(0.5f, 0.5f);
            dialogRt.anchorMax = new Vector2(0.5f, 0.5f);
            dialogRt.pivot = new Vector2(0.5f, 0.5f);
            dialogRt.anchoredPosition = Vector2.zero;
            dialogRt.sizeDelta = new Vector2(1024f, 738f);

            for (int i = dialogObj.transform.childCount - 1; i >= 0; i--)
            {
                Object.DestroyImmediate(dialogObj.transform.GetChild(i).gameObject);
            }

            var dialogImg = dialogObj.GetComponent<Image>();
            if (dialogImg == null) dialogImg = dialogObj.AddComponent<Image>();
            var sprScroll = AssetDatabase.LoadAssetAtPath<Sprite>(SprScrollCleanTracksPath);
            dialogImg.sprite = sprScroll;
            dialogImg.type = Image.Type.Simple;
            dialogImg.preserveAspect = true;
            dialogImg.color = Color.white;

            var closeObj = GetOrCreateChild(dialogObj.transform, "CloseButton");
            var closeRt = closeObj.GetComponent<RectTransform>();
            closeRt.anchorMin = new Vector2(0.5f, 1f);
            closeRt.anchorMax = new Vector2(0.5f, 1f);
            closeRt.pivot = new Vector2(0.5f, 0.5f);
            closeRt.anchoredPosition = new Vector2(364f, -154f);
            closeRt.sizeDelta = new Vector2(44f, 44f);

            var closeImg = closeObj.GetComponent<Image>();
            if (closeImg == null) closeImg = closeObj.AddComponent<Image>();
            closeImg.color = new Color(1f, 1f, 1f, 0f);
            closeImg.raycastTarget = true;

            var closeBtn = closeObj.GetComponent<Button>();
            if (closeBtn == null) closeBtn = closeObj.AddComponent<Button>();
            closeBtn.targetGraphic = closeImg;
            var closeColors = closeBtn.colors;
            closeColors.normalColor = new Color(1f, 1f, 1f, 0f);
            closeColors.highlightedColor = new Color(1f, 1f, 1f, 0.25f);
            closeColors.pressedColor = new Color(0f, 0f, 0f, 0.35f);
            closeBtn.colors = closeColors;
            closeBtn.onClick = new Button.ButtonClickedEvent();
            UnityEditor.Events.UnityEventTools.AddPersistentListener(closeBtn.onClick, menuCtrl.OnCloseSettingsClicked);

            var sprKnob = AssetDatabase.LoadAssetAtPath<Sprite>(SprKnobPath);

            var masterSlider = CreateInteractiveSlider("Slider_MasterVolume", new Vector2(149f, -243.5f), dialogObj.transform, sprKnob);
            UnityEditor.Events.UnityEventTools.AddPersistentListener(masterSlider.onValueChanged, menuCtrl.OnMasterVolumeChanged);

            var sfxSlider = CreateInteractiveSlider("Slider_SoundEffects", new Vector2(149f, -327.5f), dialogObj.transform, sprKnob);
            UnityEditor.Events.UnityEventTools.AddPersistentListener(sfxSlider.onValueChanged, menuCtrl.OnSFXVolumeChanged);

            var bgmSlider = CreateInteractiveSlider("Slider_BackgroundMusic", new Vector2(149f, -410.5f), dialogObj.transform, sprKnob);
            UnityEditor.Events.UnityEventTools.AddPersistentListener(bgmSlider.onValueChanged, menuCtrl.OnBGMVolumeChanged);

            var btnSaveObj = CreateOverlayButton("Button_Save", new Vector2(-123.5f, -493f), new Vector2(152f, 57f), dialogObj.transform);
            var btnSave = btnSaveObj.GetComponent<Button>();
            UnityEditor.Events.UnityEventTools.AddPersistentListener(btnSave.onClick, menuCtrl.OnSaveSettingsClicked);

            var btnDefaultObj = CreateOverlayButton("Button_Default", new Vector2(120.5f, -493f), new Vector2(156f, 57f), dialogObj.transform);
            var btnDefault = btnDefaultObj.GetComponent<Button>();
            UnityEditor.Events.UnityEventTools.AddPersistentListener(btnDefault.onClick, menuCtrl.OnDefaultSettingsClicked);

            var btnMenuObj = CreateOverlayButton("Button_ExitToMenu", new Vector2(-177f, -574.5f), new Vector2(309f, 54f), dialogObj.transform);
            var btnMenu = btnMenuObj.GetComponent<Button>();
            UnityEditor.Events.UnityEventTools.AddPersistentListener(btnMenu.onClick, menuCtrl.OnCloseSettingsClicked);

            var btnGoogleObj = CreateGoogleConnectOverlayButton("Button_ConnectToGoogle", new Vector2(176f, -574.5f), new Vector2(309f, 54f), dialogObj.transform);
            var btnGoogle = btnGoogleObj.GetComponent<Button>();
            UnityEditor.Events.UnityEventTools.AddPersistentListener(btnGoogle.onClick, menuCtrl.OnConnectToGoogleClicked);
            var txtGoogle = btnGoogleObj.transform.Find("Text")?.GetComponent<TextMeshProUGUI>();

            // Setup LoginPanel
            GenerateLoginBaseTexture();
            var loginPanel = BuildLoginPanel(canvas.transform, menuCtrl);

            // Connect StartGameButton to menuCtrl.OnStartGameClicked
            var startBtnObj = canvas.transform.Find("ButtonsContainer/StartGameButton");
            if (startBtnObj != null)
            {
                var startBtn = startBtnObj.GetComponent<Button>();
                if (startBtn != null)
                {
                    startBtn.onClick = new Button.ButtonClickedEvent();
                    UnityEditor.Events.UnityEventTools.AddPersistentListener(startBtn.onClick, menuCtrl.OnStartGameClicked);
                }
            }

            // Connect SettingsButton to menuCtrl.OnSettingsClicked
            var settingsBtnObj = canvas.transform.Find("ButtonsContainer/SettingsButton");
            if (settingsBtnObj != null)
            {
                var sBtn = settingsBtnObj.GetComponent<Button>();
                if (sBtn != null)
                {
                    sBtn.onClick = new Button.ButtonClickedEvent();
                    UnityEditor.Events.UnityEventTools.AddPersistentListener(sBtn.onClick, menuCtrl.OnSettingsClicked);
                }
            }

            // Connect references on MainMenuController
            var so = new SerializedObject(menuCtrl);
            so.Update();
            var spSettings = so.FindProperty("settingsPanel");
            if (spSettings != null) spSettings.objectReferenceValue = panelObj;

            var spLogin = so.FindProperty("loginPanel");
            if (spLogin != null) spLogin.objectReferenceValue = loginPanel;

            var spMaster = so.FindProperty("masterVolumeSlider");
            if (spMaster != null) spMaster.objectReferenceValue = masterSlider;

            var spSFX = so.FindProperty("sfxVolumeSlider");
            if (spSFX != null) spSFX.objectReferenceValue = sfxSlider;

            var spBGM = so.FindProperty("bgmVolumeSlider");
            if (spBGM != null) spBGM.objectReferenceValue = bgmSlider;

            var spGoogleBtn = so.FindProperty("connectGoogleButton");
            if (spGoogleBtn != null) spGoogleBtn.objectReferenceValue = btnGoogle;

            var spGoogleTxt = so.FindProperty("connectGoogleText");
            if (spGoogleTxt != null) spGoogleTxt.objectReferenceValue = txtGoogle;

            so.ApplyModifiedProperties();

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            Debug.Log("[InGameUISetup] MainMenu.unity Settings and Login UI updated successfully!");
        }
    }
}
