using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Rendering.Universal;
using TMPro;

namespace TopDownGame.Editor
{
    public static class BattleSceneGenerator
    {
        [MenuItem("Tools/Setup/Generate Battle Scene and Redirect Alterworld Rooms")]
        public static void GenerateAndRedirectAll()
        {
            GenerateBattleScene();
            RedirectAlterworldRooms();
            UpdateBuildSettings();
            Debug.Log("[BattleSceneGenerator] Successfully generated Quiz Battle Scene and redirected all Alter World rooms to it!");
        }

        public static void GenerateBattleScene()
        {
            string scenePath = "Assets/Scenes/BattleScene.unity";

            // 1. Load required sprites
            var bgSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/BattleBackground.png");
            var enemySprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/Enemy_Room1.png");
            var penSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/Artifacts/Artifact_PenNib.png");
            var stampSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/Artifacts/Artifact_Stamp.png");
            var bookSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/Artifacts/Artifact_Book.png");
            var heartFull = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/Heart_Full.png");
            var heartEmpty = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/Heart_Empty.png");
            var darrelAvatar = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/UI/ThoughtBubbleAvatar_Darrel.png");
            var darlaAvatar = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/UI/ThoughtBubbleAvatar_Darla.png");
            var avatarFrame = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/UI/AvatarFrame.png");
            var parchmentBg = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/UI/ParchmentDialogueBox.png");
            var backpackIcon = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/UI/Icon_Backpack.png");
            var settingsIcon = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/UI/Icon_Settings.png");

            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            // 2. Camera
            var camObj = new GameObject("Main Camera", typeof(Camera), typeof(AudioListener));
            camObj.tag = "MainCamera";
            camObj.transform.position = new Vector3(0f, 0f, -10f);
            var cam = camObj.GetComponent<Camera>();
            cam.orthographic = true;
            cam.orthographicSize = 5f;
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = new Color(0.04f, 0.03f, 0.03f, 1f);
            camObj.AddComponent<UniversalAdditionalCameraData>();

            // 3. Canvas (Screen Space Overlay)
            var canvasObj = new GameObject("BattleCanvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            var canvas = canvasObj.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            var scaler = canvasObj.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            scaler.matchWidthOrHeight = 0.5f;

            // 4. Background Image
            var bgObj = CreateUIElement("Background", canvasObj.transform);
            StretchFull(bgObj);
            var bgImg = bgObj.AddComponent<Image>();
            bgImg.sprite = bgSprite;
            bgImg.color = Color.white;

            // Vignette overlay
            var vigObj = CreateUIElement("VignetteOverlay", canvasObj.transform);
            StretchFull(vigObj);
            var vigImg = vigObj.AddComponent<Image>();
            vigImg.color = new Color(0.02f, 0f, 0.05f, 0.35f);
            vigImg.raycastTarget = false;

            // 5. Top HUD
            var topHudObj = CreateUIElement("TopHUD", canvasObj.transform);
            StretchTop(topHudObj, 100f);

            // Artifact Counter (Top-Left)
            var counterObj = CreateUIElement("ArtifactCounter", topHudObj.transform);
            SetAnchor(counterObj, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(60f, -35f), new Vector2(300f, 50f));
            var counterTmp = counterObj.AddComponent<TextMeshProUGUI>();
            counterTmp.text = "Artifacts: 0/3";
            counterTmp.fontSize = 32f;
            counterTmp.fontStyle = FontStyles.Bold;
            counterTmp.color = Color.white;

            // Settings Button (Top-Right)
            var settingsBtnObj = CreateUIElement("SettingsButton", topHudObj.transform);
            SetAnchor(settingsBtnObj, new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(-55f, -30f), new Vector2(70f, 70f));
            var settingsBtnImg = settingsBtnObj.AddComponent<Image>();
            settingsBtnImg.sprite = settingsIcon;
            settingsBtnImg.preserveAspect = true;
            var settingsBtn = settingsBtnObj.AddComponent<Button>();


            // 6. Center - Monster Container (Enlarged)
            var monsterObj = CreateUIElement("MonsterContainer", canvasObj.transform);
            SetAnchor(monsterObj, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(-3f, -235f), new Vector2(720f, 780f));

            var monsterImgObj = CreateUIElement("MonsterImage", monsterObj.transform);
            SetAnchor(monsterImgObj, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(780f, 780f));
            var monsterImg = monsterImgObj.AddComponent<Image>();
            monsterImg.sprite = enemySprite;
            monsterImg.preserveAspect = true;

            // Monster Hearts
            var monsterHeartsObj = CreateUIElement("MonsterHearts", monsterObj.transform);
            SetAnchor(monsterHeartsObj, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0.5f, 0.5f), new Vector2(0f, 15f), new Vector2(340f, 48f));
            var mHlg = monsterHeartsObj.AddComponent<HorizontalLayoutGroup>();
            mHlg.spacing = 10f;
            mHlg.childAlignment = TextAnchor.MiddleCenter;
            mHlg.childForceExpandWidth = false;
            mHlg.childForceExpandHeight = false;

            List<Image> monsterHeartsList = new List<Image>();
            for (int h = 0; h < 6; h++)
            {
                var hObj = CreateUIElement($"Heart_{h}", monsterHeartsObj.transform);
                var hRt = hObj.GetComponent<RectTransform>();
                hRt.sizeDelta = new Vector2(44f, 44f);
                var hImg = hObj.AddComponent<Image>();
                hImg.sprite = heartFull;
                hImg.preserveAspect = true;
                monsterHeartsList.Add(hImg);
            }

            // Floating Damage Text
            var floatTextObj = CreateUIElement("FloatingDamageText", monsterObj.transform);
            SetAnchor(floatTextObj, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, 175f), new Vector2(300f, 70f));
            var floatTmp = floatTextObj.AddComponent<TextMeshProUGUI>();
            floatTmp.text = "-2 HP!";
            floatTmp.fontSize = 50f;
            floatTmp.fontStyle = FontStyles.Bold;
            floatTmp.color = new Color(1f, 0.25f, 0.25f, 1f);
            floatTmp.alignment = TextAlignmentOptions.Center;
            floatTextObj.SetActive(false);

            var monsterCtrl = monsterObj.AddComponent<MonsterBattleController>();
            SetPrivateField(monsterCtrl, "monsterImage", monsterImg);
            SetPrivateField(monsterCtrl, "monsterSprite", enemySprite);
            SetPrivateField(monsterCtrl, "heartImages", monsterHeartsList);
            SetPrivateField(monsterCtrl, "heartFullSprite", heartFull);
            SetPrivateField(monsterCtrl, "heartEmptySprite", heartEmpty);
            SetPrivateField(monsterCtrl, "floatingDamageText", floatTmp);

            // 7. Artifact Slots (Enlarged)
            var artifactSlotsContainer = CreateUIElement("ArtifactSlotsContainer", canvasObj.transform);
            SetAnchor(artifactSlotsContainer, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(-6f, -185f), new Vector2(1200f, 260f));

            List<Button> slotButtons = new List<Button>();
            List<Image> slotImages = new List<Image>();
            List<GameObject> slotBorders = new List<GameObject>();
            List<GameObject> slotChecks = new List<GameObject>();

            Vector2[] slotPositions = new Vector2[]
            {
                new Vector2(-390f, 15f),
                new Vector2(0f, -5f),
                new Vector2(390f, 15f)
            };
            Sprite[] artifactSprites = new Sprite[] { penSprite, stampSprite, bookSprite };

            for (int s = 0; s < 3; s++)
            {
                var sObj = CreateUIElement($"ArtifactSlot_{s}", artifactSlotsContainer.transform);
                SetAnchor(sObj, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), slotPositions[s], new Vector2(250f, 250f));
                var sBtn = sObj.AddComponent<Button>();
                slotButtons.Add(sBtn);


                // Hollow Selection Border Outline
                var sBorder = CreateUIElement("SelectionBorder", sObj.transform);
                SetAnchor(sBorder, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(265f, 265f));
                var borderImg = sBorder.AddComponent<Image>();
                borderImg.color = Color.clear;
                borderImg.raycastTarget = false;

                Color edgeColor = new Color(0.88f, 0.55f, 1.0f, 0.95f);
                float thickness = 4f;

                var topBorder = CreateUIElement("Border_Top", sBorder.transform);
                SetAnchor(topBorder, new Vector2(0, 1), new Vector2(1, 1), new Vector2(0.5f, 1), Vector2.zero, new Vector2(0, thickness));
                topBorder.AddComponent<Image>().color = edgeColor;
                topBorder.GetComponent<Image>().raycastTarget = false;

                var btmBorder = CreateUIElement("Border_Bottom", sBorder.transform);
                SetAnchor(btmBorder, new Vector2(0, 0), new Vector2(1, 0), new Vector2(0.5f, 0), Vector2.zero, new Vector2(0, thickness));
                btmBorder.AddComponent<Image>().color = edgeColor;
                btmBorder.GetComponent<Image>().raycastTarget = false;

                var leftBorder = CreateUIElement("Border_Left", sBorder.transform);
                SetAnchor(leftBorder, new Vector2(0, 0), new Vector2(0, 1), new Vector2(0, 0.5f), Vector2.zero, new Vector2(thickness, 0));
                leftBorder.AddComponent<Image>().color = edgeColor;
                leftBorder.GetComponent<Image>().raycastTarget = false;

                var rightBorder = CreateUIElement("Border_Right", sBorder.transform);
                SetAnchor(rightBorder, new Vector2(1, 0), new Vector2(1, 1), new Vector2(1, 0.5f), Vector2.zero, new Vector2(thickness, 0));
                rightBorder.AddComponent<Image>().color = edgeColor;
                rightBorder.GetComponent<Image>().raycastTarget = false;

                sBorder.SetActive(false);
                slotBorders.Add(sBorder);

                // Artifact Icon (Enlarged)
                var sIcon = CreateUIElement("ArtifactIcon", sObj.transform);
                SetAnchor(sIcon, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(240f, 240f));
                var iconImg = sIcon.AddComponent<Image>();
                iconImg.sprite = artifactSprites[s];
                iconImg.preserveAspect = true;
                slotImages.Add(iconImg);

                // Checkmark / Retrieved Badge
                var checkObj = CreateUIElement("CheckBadge", sObj.transform);
                SetAnchor(checkObj, new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(0.5f, 0.5f), new Vector2(-12f, -12f), new Vector2(56f, 56f));
                var checkImg = checkObj.AddComponent<Image>();
                checkImg.color = new Color(0.2f, 0.85f, 0.35f, 1f);
                var checkTextObj = CreateUIElement("CheckText", checkObj.transform);
                StretchFull(checkTextObj);
                var checkTmp = checkTextObj.AddComponent<TextMeshProUGUI>();
                checkTmp.text = "✓";
                checkTmp.fontSize = 38f;
                checkTmp.fontStyle = FontStyles.Bold;
                checkTmp.alignment = TextAlignmentOptions.Center;
                checkTmp.color = Color.white;
                checkObj.SetActive(false);
                slotChecks.Add(checkObj);
            }

            // 8. Player Portrait (Bottom-Left)
            var playerPortraitObj = CreateUIElement("PlayerPortraitContainer", canvasObj.transform);
            SetAnchor(playerPortraitObj, new Vector2(0f, 0f), new Vector2(0f, 0f), new Vector2(0f, 0f), new Vector2(50f, 35f), new Vector2(190f, 225f));

            var frameObj = CreateUIElement("Frame", playerPortraitObj.transform);
            SetAnchor(frameObj, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -5f), new Vector2(175f, 175f));
            var frameImg = frameObj.AddComponent<Image>();
            frameImg.sprite = avatarFrame;
            frameImg.preserveAspect = true;

            var avatarImgObj = CreateUIElement("Avatar", frameObj.transform);
            SetAnchor(avatarImgObj, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, -2f), new Vector2(130f, 130f));
            var avatarImg = avatarImgObj.AddComponent<Image>();
            avatarImg.sprite = darrelAvatar;
            avatarImg.preserveAspect = true;

            // Player Hearts (below avatar)
            var playerHeartsObj = CreateUIElement("PlayerHearts", playerPortraitObj.transform);
            SetAnchor(playerHeartsObj, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0f, 6f), new Vector2(160f, 38f));
            var pHlg = playerHeartsObj.AddComponent<HorizontalLayoutGroup>();
            pHlg.spacing = 10f;
            pHlg.childAlignment = TextAnchor.MiddleCenter;
            pHlg.childForceExpandWidth = false;
            pHlg.childForceExpandHeight = false;

            List<Image> playerHeartsList = new List<Image>();
            for (int ph = 0; ph < 3; ph++)
            {
                var phObj = CreateUIElement($"PlayerHeart_{ph}", playerHeartsObj.transform);
                var phRt = phObj.GetComponent<RectTransform>();
                phRt.sizeDelta = new Vector2(38f, 38f);
                var phImg = phObj.AddComponent<Image>();
                phImg.sprite = heartFull;
                phImg.preserveAspect = true;
                playerHeartsList.Add(phImg);
            }

            // 9. Dialogue & Quiz Box (Bottom, right of player portrait, Enlarged & English)
            var dialogueBoxObj = CreateUIElement("DialogueBox", canvasObj.transform);
            SetAnchor(dialogueBoxObj, new Vector2(0f, 0f), new Vector2(1f, 0f), new Vector2(0f, 0f), new Vector2(265f, 35f), new Vector2(-320f, 225f));

            var dialogueBgImg = dialogueBoxObj.AddComponent<Image>();
            dialogueBgImg.sprite = parchmentBg;
            dialogueBgImg.type = Image.Type.Sliced;
            dialogueBgImg.color = new Color(1f, 0.96f, 0.88f, 1f);

            // Speaker Name (Bigger bold font)
            var speakerObj = CreateUIElement("SpeakerName", dialogueBoxObj.transform);
            SetAnchor(speakerObj, new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(0f, 1f), new Vector2(40f, -18f), new Vector2(-80f, 46f));
            var speakerTmp = speakerObj.AddComponent<TextMeshProUGUI>();
            speakerTmp.text = "Darrel";
            speakerTmp.fontSize = 42f;
            speakerTmp.fontStyle = FontStyles.Bold;
            speakerTmp.color = new Color(0.15f, 0.08f, 0.03f, 1f);

            // Dialogue Body Text (Much bigger font & English)
            var bodyObj = CreateUIElement("DialogueBody", dialogueBoxObj.transform);
            SetAnchor(bodyObj, new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(0f, 1f), new Vector2(40f, -68f), new Vector2(-80f, 85f));
            var bodyTmp = bodyObj.AddComponent<TextMeshProUGUI>();
            bodyTmp.text = "Which artifact should I retrieve first?";
            bodyTmp.fontSize = 36f;
            bodyTmp.lineSpacing = 8f;
            bodyTmp.enableAutoSizing = true;
            bodyTmp.fontSizeMin = 28f;
            bodyTmp.fontSizeMax = 36f;
            bodyTmp.fontStyle = FontStyles.Bold;
            bodyTmp.color = new Color(0.14f, 0.08f, 0.03f, 1f);

            // Choices Container (2x2 grid inside dialogue box)
            var choicesObj = CreateUIElement("ChoicesContainer", dialogueBoxObj.transform);
            SetAnchor(choicesObj, new Vector2(0f, 0f), new Vector2(1f, 0f), new Vector2(0f, 0f), new Vector2(30f, 12f), new Vector2(-60f, 96f));

            var cGlg = choicesObj.AddComponent<GridLayoutGroup>();
            cGlg.cellSize = new Vector2(740f, 46f);
            cGlg.spacing = new Vector2(20f, 8f);
            cGlg.childAlignment = TextAnchor.MiddleCenter;

            List<Button> choiceBtns = new List<Button>();
            List<TMP_Text> choiceTexts = new List<TMP_Text>();
            string[] defaultChoices = new string[] { "A. Choice 1", "B. Choice 2", "C. Choice 3", "D. Choice 4" };

            for (int c = 0; c < 4; c++)
            {
                var cBtnObj = CreateUIElement($"ChoiceBtn_{c}", choicesObj.transform);
                var cBtnImg = cBtnObj.AddComponent<Image>();
                cBtnImg.color = new Color(0.92f, 0.86f, 0.74f, 1f);
                var btn = cBtnObj.AddComponent<Button>();
                choiceBtns.Add(btn);

                var cTextObj = CreateUIElement("Text", cBtnObj.transform);
                StretchFull(cTextObj);
                var cTmp = cTextObj.AddComponent<TextMeshProUGUI>();
                cTmp.text = defaultChoices[c];
                cTmp.fontSize = 23f;
                cTmp.fontStyle = FontStyles.Bold;
                cTmp.enableAutoSizing = true;
                cTmp.fontSizeMin = 19f;
                cTmp.fontSizeMax = 23f;
                cTmp.color = new Color(0.18f, 0.12f, 0.08f, 1f);
                cTmp.alignment = TextAlignmentOptions.MidlineLeft;
                cTmp.margin = new Vector4(16f, 0f, 16f, 0f);
                choiceTexts.Add(cTmp);
            }
            choicesObj.SetActive(false);


            // 11. Victory Modal (English)
            var vicModalObj = CreateUIElement("VictoryModal", canvasObj.transform);
            StretchFull(vicModalObj);
            var vicBg = vicModalObj.AddComponent<Image>();
            vicBg.color = new Color(0f, 0f, 0f, 0.82f);

            var vicWindow = CreateUIElement("Window", vicModalObj.transform);
            SetAnchor(vicWindow, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(680f, 380f));
            var vwImg = vicWindow.AddComponent<Image>();
            vwImg.color = new Color(0.12f, 0.1f, 0.08f, 0.98f);

            var vtObj = CreateUIElement("Title", vicWindow.transform);
            SetAnchor(vtObj, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -40f), new Vector2(500f, 50f));
            var vtTmp = vtObj.AddComponent<TextMeshProUGUI>();
            vtTmp.text = "VICTORY!";
            vtTmp.fontSize = 44f;
            vtTmp.fontStyle = FontStyles.Bold;
            vtTmp.alignment = TextAlignmentOptions.Center;
            vtTmp.color = new Color(0.96f, 0.82f, 0.35f, 1f);

            var vbObj = CreateUIElement("Body", vicWindow.transform);
            SetAnchor(vbObj, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, 15f), new Vector2(580f, 100f));
            var vbTmp = vbObj.AddComponent<TextMeshProUGUI>();
            vbTmp.text = "You defeated the shadow monster and recovered the artifacts in Room 1!\nTake the portal back and bring them inside Room 1 in the Real World to unlock Alterworld Room 2!";
            vbTmp.fontSize = 22f;
            vbTmp.alignment = TextAlignmentOptions.Center;
            vbTmp.color = new Color(0.9f, 0.9f, 0.9f, 1f);

            var vrBtnObj = CreateUIElement("ReturnButton", vicWindow.transform);
            SetAnchor(vrBtnObj, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0f, 40f), new Vector2(280f, 60f));
            var vrImg = vrBtnObj.AddComponent<Image>();
            vrImg.color = new Color(0.85f, 0.65f, 0.2f, 1f);
            var vrBtn = vrBtnObj.AddComponent<Button>();
            var vrTextObj = CreateUIElement("Text", vrBtnObj.transform);
            StretchFull(vrTextObj);
            var vrTmp = vrTextObj.AddComponent<TextMeshProUGUI>();
            vrTmp.text = "Return to Hallway";
            vrTmp.fontSize = 22f;
            vrTmp.fontStyle = FontStyles.Bold;
            vrTmp.alignment = TextAlignmentOptions.Center;
            vrTmp.color = new Color(0.12f, 0.08f, 0.04f, 1f);
            vicModalObj.SetActive(false);

            // 12. Defeat Modal (English)
            var defModalObj = CreateUIElement("DefeatModal", canvasObj.transform);
            StretchFull(defModalObj);
            var defBg = defModalObj.AddComponent<Image>();
            defBg.color = new Color(0f, 0f, 0f, 0.85f);

            var defWindow = CreateUIElement("Window", defModalObj.transform);
            SetAnchor(defWindow, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(650f, 360f));
            var dwImg = defWindow.AddComponent<Image>();
            dwImg.color = new Color(0.16f, 0.08f, 0.08f, 0.98f);

            var dtObj = CreateUIElement("Title", defWindow.transform);
            SetAnchor(dtObj, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -40f), new Vector2(500f, 50f));
            var dtTmp = dtObj.AddComponent<TextMeshProUGUI>();
            dtTmp.text = "DEFEATED!";
            dtTmp.fontSize = 44f;
            dtTmp.fontStyle = FontStyles.Bold;
            dtTmp.alignment = TextAlignmentOptions.Center;
            dtTmp.color = new Color(1f, 0.3f, 0.3f, 1f);

            var dbObj = CreateUIElement("Body", defWindow.transform);
            SetAnchor(dbObj, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, 20f), new Vector2(550f, 80f));
            var dbTmp = dbObj.AddComponent<TextMeshProUGUI>();
            dbTmp.text = "Your health was depleted by the monster...";
            dbTmp.fontSize = 22f;
            dbTmp.alignment = TextAlignmentOptions.Center;
            dbTmp.color = new Color(0.85f, 0.85f, 0.85f, 1f);

            var defRetryBtnObj = CreateUIElement("RetryButton", defWindow.transform);
            SetAnchor(defRetryBtnObj, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(-120f, 40f), new Vector2(200f, 55f));
            var drbImg = defRetryBtnObj.AddComponent<Image>();
            drbImg.color = new Color(0.8f, 0.3f, 0.25f, 1f);
            var defRetryBtn = defRetryBtnObj.AddComponent<Button>();
            var drbTxt = CreateUIElement("Text", defRetryBtnObj.transform);
            StretchFull(drbTxt);
            var drbTmp = drbTxt.AddComponent<TextMeshProUGUI>();
            drbTmp.text = "Try Again";
            drbTmp.fontSize = 20f;
            drbTmp.fontStyle = FontStyles.Bold;
            drbTmp.alignment = TextAlignmentOptions.Center;
            drbTmp.color = Color.white;

            var defExitBtnObj = CreateUIElement("ExitButton", defWindow.transform);
            SetAnchor(defExitBtnObj, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(120f, 40f), new Vector2(200f, 55f));
            var debImg = defExitBtnObj.AddComponent<Image>();
            debImg.color = new Color(0.45f, 0.42f, 0.42f, 1f);
            var defExitBtn = defExitBtnObj.AddComponent<Button>();
            var debTxt = CreateUIElement("Text", defExitBtnObj.transform);
            StretchFull(debTxt);
            var debTmp = debTxt.AddComponent<TextMeshProUGUI>();
            debTmp.text = "Retreat";
            debTmp.fontSize = 20f;
            debTmp.fontStyle = FontStyles.Bold;
            debTmp.alignment = TextAlignmentOptions.Center;
            debTmp.color = Color.white;
            defModalObj.SetActive(false);

            // 13. Settings Modal (English)
            var setModalObj = CreateUIElement("SettingsModal", canvasObj.transform);
            StretchFull(setModalObj);
            var setBg = setModalObj.AddComponent<Image>();
            setBg.color = new Color(0f, 0f, 0f, 0.75f);

            var setWindow = CreateUIElement("Window", setModalObj.transform);
            SetAnchor(setWindow, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(500f, 320f));
            var sewImg = setWindow.AddComponent<Image>();
            sewImg.color = new Color(0.14f, 0.12f, 0.12f, 0.98f);

            var stObj = CreateUIElement("Title", setWindow.transform);
            SetAnchor(stObj, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -30f), new Vector2(400f, 40f));
            var setTmp = stObj.AddComponent<TextMeshProUGUI>();
            setTmp.text = "PAUSE / MENU";
            setTmp.fontSize = 26f;
            setTmp.fontStyle = FontStyles.Bold;
            setTmp.alignment = TextAlignmentOptions.Center;
            setTmp.color = new Color(0.96f, 0.82f, 0.35f, 1f);

            var setResumeBtnObj = CreateUIElement("ResumeButton", setWindow.transform);
            SetAnchor(setResumeBtnObj, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, 20f), new Vector2(260f, 50f));
            var srbImg = setResumeBtnObj.AddComponent<Image>();
            srbImg.color = new Color(0.25f, 0.65f, 0.35f, 1f);
            var setResumeBtn = setResumeBtnObj.AddComponent<Button>();
            var srbTxt = CreateUIElement("Text", setResumeBtnObj.transform);
            StretchFull(srbTxt);
            var srbTmp = srbTxt.AddComponent<TextMeshProUGUI>();
            srbTmp.text = "Resume";
            srbTmp.fontSize = 20f;
            srbTmp.fontStyle = FontStyles.Bold;
            srbTmp.alignment = TextAlignmentOptions.Center;
            srbTmp.color = Color.white;

            var setExitBtnObj = CreateUIElement("ExitButton", setWindow.transform);
            SetAnchor(setExitBtnObj, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, -45f), new Vector2(260f, 50f));
            var sebImg = setExitBtnObj.AddComponent<Image>();
            sebImg.color = new Color(0.7f, 0.28f, 0.25f, 1f);
            var setExitBtn = setExitBtnObj.AddComponent<Button>();
            var sebTxt = CreateUIElement("Text", setExitBtnObj.transform);
            StretchFull(sebTxt);
            var sebTmp = sebTxt.AddComponent<TextMeshProUGUI>();
            sebTmp.text = "Retreat to Hallway";
            sebTmp.fontSize = 20f;
            sebTmp.fontStyle = FontStyles.Bold;
            sebTmp.alignment = TextAlignmentOptions.Center;
            sebTmp.color = Color.white;
            setModalObj.SetActive(false);

            // 14. QuizBattleManager on Canvas
            var qbm = canvasObj.AddComponent<QuizBattleManager>();
            SetPrivateField(qbm, "monsterController", monsterCtrl);

            // Artifact Slots Data
            var slotDataList = new List<ArtifactBattleSlot>();

            var slot0 = new ArtifactBattleSlot();
            slot0.artifactId = "Room1_DipPenNib";
            slot0.artifactName = "Dip Pen Nib";
            slot0.artifactSprite = penSprite;
            slot0.artifactDescription = "A fine calligraphy dip pen nib used during José Rizal's early education and reform writings.";
            slot0.questionText = "Who was Rizal's first formal school teacher during his early education in the town of Biñan?";
            slot0.choices = new string[] {
                "A. Maestro Justiniano Aquino Cruz",
                "B. Father Jose Bech",
                "C. Maestro Celestino",
                "D. Father Francisco Sanchez"
            };
            slot0.correctChoiceIndex = 0;
            slotDataList.Add(slot0);

            var slot1 = new ArtifactBattleSlot();
            slot1.artifactId = "Room1_UniversityWaxSeal";
            slot1.artifactName = "University Wax Seal Stamp";
            slot1.artifactSprite = stampSprite;
            slot1.artifactDescription = "An official university wax seal stamp commemorating Rizal's higher education in Spain.";
            slot1.questionText = "At which prestigious university in Spain did Rizal finish his licentiate in Medicine and Philosophy?";
            slot1.choices = new string[] {
                "A. Universidad Central de Madrid",
                "B. Universidad de Barcelona",
                "C. Universidad de Salamanca",
                "D. Universidad de Sevilla"
            };
            slot1.correctChoiceIndex = 0;
            slotDataList.Add(slot1);

            var slot2 = new ArtifactBattleSlot();
            slot2.artifactId = "Room1_TravelNotebook";
            slot2.artifactName = "Travel Notebook";
            slot2.artifactSprite = bookSprite;
            slot2.artifactDescription = "A leather-bound travel notebook containing Rizal's European diary entries, sketches, and literary translations.";
            slot2.questionText = "Which European language did Rizal learn and use to translate Friedrich Schiller's drama \"William Tell\" into Tagalog?";
            slot2.choices = new string[] {
                "A. German language",
                "B. French language",
                "C. English language",
                "D. Italian language"
            };
            slot2.correctChoiceIndex = 0;
            slotDataList.Add(slot2);

            SetPrivateField(qbm, "artifactSlots", slotDataList);
            SetPrivateField(qbm, "artifactSlotButtons", slotButtons);
            SetPrivateField(qbm, "artifactSlotImages", slotImages);
            SetPrivateField(qbm, "artifactSelectionBorders", slotBorders);
            SetPrivateField(qbm, "artifactCheckmarkIcons", slotChecks);

            // Top HUD Wiring
            SetPrivateField(qbm, "artifactCounterText", counterTmp);
            SetPrivateField(qbm, "settingsButton", settingsBtn);

            // Player Status Wiring
            SetPrivateField(qbm, "playerAvatarImage", avatarImg);
            SetPrivateField(qbm, "boyAvatarSprite", darrelAvatar);
            SetPrivateField(qbm, "girlAvatarSprite", darlaAvatar);
            SetPrivateField(qbm, "playerNameText", speakerTmp);
            SetPrivateField(qbm, "playerHeartImages", playerHeartsList);
            SetPrivateField(qbm, "heartFullSprite", heartFull);
            SetPrivateField(qbm, "heartEmptySprite", heartEmpty);
            SetPrivateField(qbm, "playerPortraitTransform", playerPortraitObj.transform);

            // Dialogue & Choices Wiring
            SetPrivateField(qbm, "dialogueSpeakerText", speakerTmp);
            SetPrivateField(qbm, "dialogueBodyText", bodyTmp);
            SetPrivateField(qbm, "choicesContainer", choicesObj);
            SetPrivateField(qbm, "choiceButtons", choiceBtns);
            SetPrivateField(qbm, "choiceButtonTexts", choiceTexts);

            // Modals Wiring
            SetPrivateField(qbm, "victoryModal", vicModalObj);
            SetPrivateField(qbm, "victoryReturnButton", vrBtn);
            SetPrivateField(qbm, "defeatModal", defModalObj);
            SetPrivateField(qbm, "defeatRetryButton", defRetryBtn);
            SetPrivateField(qbm, "defeatExitButton", defExitBtn);
            SetPrivateField(qbm, "settingsModal", setModalObj);
            SetPrivateField(qbm, "settingsCloseButton", setResumeBtn);
            SetPrivateField(qbm, "settingsExitHallwayButton", setExitBtn);

            // 15. EventSystem
            var esObj = new GameObject("EventSystem", typeof(UnityEngine.EventSystems.EventSystem), typeof(UnityEngine.InputSystem.UI.InputSystemUIInputModule));

            // Save Scene
            EditorSceneManager.SaveScene(scene, scenePath);
            Debug.Log($"[BattleSceneGenerator] Quiz BattleScene saved to {scenePath}");
        }

        public static void RedirectAlterworldRooms()
        {
            // 1. Redirect AltLeftHallway doors (Rooms 1..6)
            if (File.Exists("Assets/Scenes/AltLeftHallway.unity"))
            {
                EditorSceneManager.OpenScene("Assets/Scenes/AltLeftHallway.unity", OpenSceneMode.Single);
                var doors = Object.FindObjectsByType<RoomDoor>(FindObjectsSortMode.None);
                foreach (var d in doors)
                {
                    var offsetField = typeof(RoomDoor).GetField("returnOffset", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                    Vector2 currentOffset = offsetField != null ? (Vector2)offsetField.GetValue(d) : new Vector2(0f, 1.8f);
                    d.SetConfig("BattleScene", d.RoomDisplayName, currentOffset);
                    EditorUtility.SetDirty(d);
                }
                EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene(), "Assets/Scenes/AltLeftHallway.unity");
                Debug.Log($"[BattleSceneGenerator] Redirected {doors.Length} doors in AltLeftHallway to BattleScene!");
            }

            // 2. Redirect AltRightHallway doors (Rooms 7..12)
            if (File.Exists("Assets/Scenes/AltRightHallway.unity"))
            {
                EditorSceneManager.OpenScene("Assets/Scenes/AltRightHallway.unity", OpenSceneMode.Single);
                var doors = Object.FindObjectsByType<RoomDoor>(FindObjectsSortMode.None);
                foreach (var d in doors)
                {
                    var offsetField = typeof(RoomDoor).GetField("returnOffset", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                    Vector2 currentOffset = offsetField != null ? (Vector2)offsetField.GetValue(d) : new Vector2(0f, 1.8f);
                    d.SetConfig("BattleScene", d.RoomDisplayName, currentOffset);
                    EditorUtility.SetDirty(d);
                }
                EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene(), "Assets/Scenes/AltRightHallway.unity");
                Debug.Log($"[BattleSceneGenerator] Redirected {doors.Length} doors in AltRightHallway to BattleScene!");
            }
        }

        public static void UpdateBuildSettings()
        {
            List<EditorBuildSettingsScene> buildScenes = new List<EditorBuildSettingsScene>(EditorBuildSettings.scenes);
            string battlePath = "Assets/Scenes/BattleScene.unity";

            if (File.Exists(battlePath) && !buildScenes.Exists(s => s.path == battlePath))
            {
                buildScenes.Add(new EditorBuildSettingsScene(battlePath, true));
                EditorBuildSettings.scenes = buildScenes.ToArray();
                AssetDatabase.SaveAssets();
                Debug.Log("[BattleSceneGenerator] Registered Assets/Scenes/BattleScene.unity in Build Settings!");
            }
        }

        private static GameObject CreateUIElement(string name, Transform parent)
        {
            var go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            return go;
        }

        private static void StretchFull(GameObject go)
        {
            var rt = go.GetComponent<RectTransform>();
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.anchoredPosition = Vector2.zero;
            rt.sizeDelta = Vector2.zero;
        }

        private static void StretchTop(GameObject go, float height)
        {
            var rt = go.GetComponent<RectTransform>();
            rt.anchorMin = new Vector2(0f, 1f);
            rt.anchorMax = new Vector2(1f, 1f);
            rt.pivot = new Vector2(0.5f, 1f);
            rt.anchoredPosition = Vector2.zero;
            rt.sizeDelta = new Vector2(0f, height);
        }

        private static void SetAnchor(GameObject go, Vector2 aMin, Vector2 aMax, Vector2 pivot, Vector2 pos, Vector2 size)
        {
            var rt = go.GetComponent<RectTransform>();
            rt.anchorMin = aMin;
            rt.anchorMax = aMax;
            rt.pivot = pivot;
            rt.anchoredPosition = pos;
            rt.sizeDelta = size;
        }

        private static void SetPrivateField(object target, string fieldName, object value)
        {
            var f = target.GetType().GetField(fieldName, System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (f != null)
            {
                f.SetValue(target, value);
            }
            else
            {
                Debug.LogWarning($"[BattleSceneGenerator] Field '{fieldName}' not found on {target.GetType().Name}!");
            }
        }
    }
}
