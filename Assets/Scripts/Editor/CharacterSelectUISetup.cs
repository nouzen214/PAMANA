using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace TopDownGame.Editor
{
    public static class CharacterSelectUISetup
    {
        private const string ScenePath = "Assets/Scenes/CharacterSelect.unity";
        private const string SprInputFramePath = "Assets/UI/Login/Login_Input_Frame.png";

        [MenuItem("Tools/Setup Character Select UI")]
        public static void SetupCharacterSelectScene()
        {
            if (EditorApplication.isPlaying)
            {
                Debug.LogWarning("[CharacterSelectUISetup] Please stop Play Mode in Unity to run setup.");
                return;
            }

            if (!File.Exists(ScenePath))
            {
                Debug.LogError($"[CharacterSelectUISetup] Scene not found: {ScenePath}");
                return;
            }

            var scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
            var canvasObj = GameObject.Find("Canvas");
            if (canvasObj == null)
            {
                Debug.LogError("[CharacterSelectUISetup] Canvas not found in CharacterSelect scene!");
                return;
            }

            var manager = canvasObj.GetComponent<CharacterSelectManager>();
            if (manager == null) manager = canvasObj.AddComponent<CharacterSelectManager>();

            // Setup Wall Background
            var bgObj = canvasObj.transform.Find("Background");
            if (bgObj != null)
            {
                var bgImg = bgObj.GetComponent<Image>();
                if (bgImg != null)
                {
                    var sprWall = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/UI/Wall_Background.png");
                    if (sprWall != null)
                    {
                        bgImg.sprite = sprWall;
                        bgImg.color = Color.white;
                    }
                }
            }

            // Setup DarkOverlay for balanced contrast with the wall torches
            var overlayObj = canvasObj.transform.Find("DarkOverlay");
            if (overlayObj != null)
            {
                var overlayImg = overlayObj.GetComponent<Image>();
                if (overlayImg != null)
                {
                    overlayImg.color = new Color(0.05f, 0.05f, 0.08f, 0.20f);
                }
            }

            // Setup StartButton (anchored to bottom)
            var startBtnObj = canvasObj.transform.Find("StartButton");
            if (startBtnObj != null)
            {
                var startBtnRt = startBtnObj.GetComponent<RectTransform>();
                startBtnRt.anchorMin = new Vector2(0.5f, 0f);
                startBtnRt.anchorMax = new Vector2(0.5f, 0f);
                startBtnRt.pivot = new Vector2(0.5f, 0f);
                startBtnRt.anchoredPosition = new Vector2(0f, 40f);
                startBtnRt.sizeDelta = new Vector2(360f, 75f);
            }

            // Setup NameInputContainer (anchored to bottom, stacked above StartButton)
            var containerObj = GetOrCreateChild(canvasObj.transform, "NameInputContainer");
            var containerRt = containerObj.GetComponent<RectTransform>();
            containerRt.anchorMin = new Vector2(0.5f, 0f);
            containerRt.anchorMax = new Vector2(0.5f, 0f);
            containerRt.pivot = new Vector2(0.5f, 0f);
            containerRt.anchoredPosition = new Vector2(0f, 125f);
            containerRt.sizeDelta = new Vector2(440f, 85f);

            // Setup StatusText (anchored to bottom, stacked above NameInputContainer)
            var statusTextObj = canvasObj.transform.Find("StatusText");
            if (statusTextObj != null)
            {
                var statusRt = statusTextObj.GetComponent<RectTransform>();
                statusRt.anchorMin = new Vector2(0.5f, 0f);
                statusRt.anchorMax = new Vector2(0.5f, 0f);
                statusRt.pivot = new Vector2(0.5f, 0f);
                statusRt.anchoredPosition = new Vector2(0f, 215f);
                statusRt.sizeDelta = new Vector2(600f, 35f);
            }

            // Label: "NAME YOUR CHARACTER:"
            var labelObj = GetOrCreateChild(containerObj.transform, "Label");
            var labelRt = labelObj.GetComponent<RectTransform>();
            labelRt.anchorMin = new Vector2(0f, 1f);
            labelRt.anchorMax = new Vector2(1f, 1f);
            labelRt.pivot = new Vector2(0.5f, 1f);
            labelRt.anchoredPosition = new Vector2(0f, 0f);
            labelRt.sizeDelta = new Vector2(0f, 26f);

            var labelTmp = labelObj.GetComponent<TextMeshProUGUI>();
            if (labelTmp == null) labelTmp = labelObj.AddComponent<TextMeshProUGUI>();
            labelTmp.text = "NAME YOUR CHARACTER:";
            labelTmp.fontSize = 18f;
            labelTmp.fontStyle = FontStyles.Bold;
            labelTmp.alignment = TextAlignmentOptions.Center;
            labelTmp.color = new Color(1f, 0.88f, 0.55f, 1f);
            labelTmp.raycastTarget = false;

            // InputField
            var inputObj = GetOrCreateChild(containerObj.transform, "InputField");
            var inputRt = inputObj.GetComponent<RectTransform>();
            inputRt.anchorMin = new Vector2(0.5f, 0f);
            inputRt.anchorMax = new Vector2(0.5f, 0f);
            inputRt.pivot = new Vector2(0.5f, 0f);
            inputRt.anchoredPosition = new Vector2(0f, 0f);
            inputRt.sizeDelta = new Vector2(360f, 54f);

            var inputImg = inputObj.GetComponent<Image>();
            if (inputImg == null) inputImg = inputObj.AddComponent<Image>();
            var sprFrame = AssetDatabase.LoadAssetAtPath<Sprite>(SprInputFramePath);
            inputImg.sprite = sprFrame;
            inputImg.type = Image.Type.Simple;
            inputImg.color = Color.white;
            inputImg.raycastTarget = true;

            var inputField = inputObj.GetComponent<TMP_InputField>();
            if (inputField == null) inputField = inputObj.AddComponent<TMP_InputField>();
            inputField.targetGraphic = inputImg;
            inputField.characterLimit = 16;

            var textAreaObj = GetOrCreateChild(inputObj.transform, "Text Area");
            var textAreaRt = textAreaObj.GetComponent<RectTransform>();
            textAreaRt.anchorMin = Vector2.zero;
            textAreaRt.anchorMax = Vector2.one;
            textAreaRt.offsetMin = new Vector2(20f, 6f);
            textAreaRt.offsetMax = new Vector2(-20f, -6f);

            var rectMask = textAreaObj.GetComponent<RectMask2D>();
            if (rectMask == null) rectMask = textAreaObj.AddComponent<RectMask2D>();

            var phObj = GetOrCreateChild(textAreaObj.transform, "Placeholder");
            var phRt = phObj.GetComponent<RectTransform>();
            phRt.anchorMin = Vector2.zero;
            phRt.anchorMax = Vector2.one;
            phRt.offsetMin = Vector2.zero;
            phRt.offsetMax = Vector2.zero;

            var phTmp = phObj.GetComponent<TextMeshProUGUI>();
            if (phTmp == null) phTmp = phObj.AddComponent<TextMeshProUGUI>();
            phTmp.text = "Enter character name...";
            phTmp.fontSize = 20f;
            phTmp.fontStyle = FontStyles.Italic;
            phTmp.alignment = TextAlignmentOptions.Center;
            phTmp.color = new Color(0.66f, 0.54f, 0.41f, 0.85f);
            phTmp.raycastTarget = false;

            var textObj = GetOrCreateChild(textAreaObj.transform, "Text");
            var textRt = textObj.GetComponent<RectTransform>();
            textRt.anchorMin = Vector2.zero;
            textRt.anchorMax = Vector2.one;
            textRt.offsetMin = Vector2.zero;
            textRt.offsetMax = Vector2.zero;

            var textTmp = textObj.GetComponent<TextMeshProUGUI>();
            if (textTmp == null) textTmp = textObj.AddComponent<TextMeshProUGUI>();
            textTmp.fontSize = 24f;
            textTmp.fontStyle = FontStyles.Bold;
            textTmp.alignment = TextAlignmentOptions.Center;
            textTmp.color = new Color(0.97f, 0.93f, 0.85f, 1f);
            textTmp.raycastTarget = false;

            inputField.textViewport = textAreaRt;
            inputField.textComponent = textTmp;
            inputField.placeholder = phTmp;

            // Wire serialized property on CharacterSelectManager
            var so = new SerializedObject(manager);
            so.Update();
            var spInput = so.FindProperty("nameInputField");
            if (spInput != null) spInput.objectReferenceValue = inputField;
            so.ApplyModifiedProperties();

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            Debug.Log("[CharacterSelectUISetup] CharacterSelect.unity UI setup completed successfully!");
        }

        private static GameObject GetOrCreateChild(Transform parent, string name)
        {
            var child = parent.Find(name);
            if (child != null) return child.gameObject;

            var newObj = new GameObject(name, typeof(RectTransform));
            newObj.transform.SetParent(parent, false);
            return newObj;
        }
    }
}

