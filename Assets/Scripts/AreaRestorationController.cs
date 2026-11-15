using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace TopDownGame
{
    /// <summary>
    /// Manages visual restoration for the museum wings (LeftHallway, RightHallway)
    /// and the Grand Lobby:
    /// - Left Wing (LeftHallway): Restored when Rooms 1–6 are all 100% complete.
    /// - Right Wing (RightHallway): Restored when Rooms 7–12 are all 100% complete.
    /// - Grand Lobby (Lobby): Restored when all 12 rooms across both wings are 100% complete.
    /// </summary>
    public class AreaRestorationController : MonoBehaviour
    {
        public enum AreaType
        {
            AutoDetect,
            LeftWing,
            RightWing,
            Lobby
        }

        [Header("Area Configuration")]
        [SerializeField] private AreaType areaType = AreaType.AutoDetect;

        [Header("Scene References")]
        [SerializeField] private SpriteRenderer backgroundRenderer;
        [SerializeField] private Light2D globalLight;

        [Header("Sprites")]
        [SerializeField] private Sprite ruinedSprite;
        [SerializeField] private Sprite restoredSprite;

        [Header("Lighting Settings")]
        [SerializeField] private Color ruinedLightColor = Color.white;
        [SerializeField] private float ruinedLightIntensity = 1.0f;
        [SerializeField] private Color restoredLightColor = new Color(1.00f, 0.96f, 0.88f, 1.0f);
        [SerializeField] private float restoredLightIntensity = 1.15f;

        [Header("Audio")]
        [SerializeField] private AudioSource audioSource;

        private bool _isTransitioning = false;
        private AudioClip _cachedChimeClip;

        private void Awake()
        {
            EnsureReferences();
        }

        private void Start()
        {
            ApplyRestorationState(animate: false);
        }

        private void OnEnable()
        {
            GameProgression.OnPedestalRestored += HandlePedestalRestored;
            GameProgression.OnLeftWingRestored += HandleWingRestored;
            GameProgression.OnRightWingRestored += HandleWingRestored;
            GameProgression.OnLobbyRestored += HandleWingRestored;
        }

        private void OnDisable()
        {
            GameProgression.OnPedestalRestored -= HandlePedestalRestored;
            GameProgression.OnLeftWingRestored -= HandleWingRestored;
            GameProgression.OnRightWingRestored -= HandleWingRestored;
            GameProgression.OnLobbyRestored -= HandleWingRestored;
        }

        private void HandlePedestalRestored(int roomNumber, string artifactName)
        {
            CheckAndApplyRestoration(animate: true);
        }

        private void HandleWingRestored()
        {
            CheckAndApplyRestoration(animate: true);
        }

        public AreaType ResolveAreaType()
        {
            if (areaType != AreaType.AutoDetect) return areaType;

            string sceneName = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
            if (sceneName.Contains("LeftHallway")) return AreaType.LeftWing;
            if (sceneName.Contains("RightHallway")) return AreaType.RightWing;
            if (sceneName.Contains("Lobby")) return AreaType.Lobby;

            return AreaType.LeftWing;
        }

        public bool IsAreaRestored()
        {
            var resolved = ResolveAreaType();
            switch (resolved)
            {
                case AreaType.LeftWing:
                    return GameProgression.IsLeftWingRestored();
                case AreaType.RightWing:
                    return GameProgression.IsRightWingRestored();
                case AreaType.Lobby:
                    return GameProgression.IsLobbyRestored();
                default:
                    return false;
            }
        }

        public void EnsureReferences()
        {
            var resolved = ResolveAreaType();

            if (backgroundRenderer == null)
            {
                var bgObj = GameObject.Find("HallwayBackground") ?? GameObject.Find("LobbyBackground") ?? GameObject.Find("Background");
                if (bgObj != null)
                {
                    backgroundRenderer = bgObj.GetComponent<SpriteRenderer>();
                }
            }

            if (globalLight == null)
            {
                var lightObj = GameObject.Find("Global Light 2D");
                if (lightObj != null)
                {
                    globalLight = lightObj.GetComponent<Light2D>();
                }
            }

            if (audioSource == null)
            {
                audioSource = GetComponent<AudioSource>();
                if (audioSource == null)
                {
                    audioSource = gameObject.AddComponent<AudioSource>();
                    audioSource.playOnAwake = false;
                    audioSource.spatialBlend = 0f;
                }
            }

            // Load sprites if not assigned
            if (ruinedSprite == null)
            {
                switch (resolved)
                {
                    case AreaType.LeftWing:
                        ruinedSprite = Resources.Load<Sprite>("LeftHallway");
#if UNITY_EDITOR
                        if (ruinedSprite == null) ruinedSprite = UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/LeftHallway.png");
#endif
                        break;
                    case AreaType.RightWing:
                        ruinedSprite = Resources.Load<Sprite>("RightHallway");
#if UNITY_EDITOR
                        if (ruinedSprite == null) ruinedSprite = UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/RightHallway.png");
#endif
                        break;
                    case AreaType.Lobby:
                        ruinedSprite = Resources.Load<Sprite>("LobbyBackground");
#if UNITY_EDITOR
                        if (ruinedSprite == null) ruinedSprite = UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/LobbyBackground.png");
#endif
                        break;
                }
            }

            if (restoredSprite == null)
            {
                switch (resolved)
                {
                    case AreaType.LeftWing:
                        restoredSprite = Resources.Load<Sprite>("HallwayStages/LeftHallway_Restored");
#if UNITY_EDITOR
                        if (restoredSprite == null) restoredSprite = UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/LeftHallway_Restored.png");
#endif
                        break;
                    case AreaType.RightWing:
                        restoredSprite = Resources.Load<Sprite>("HallwayStages/RightHallway_Restored");
#if UNITY_EDITOR
                        if (restoredSprite == null) restoredSprite = UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/RightHallway_Restored.png");
#endif
                        break;
                    case AreaType.Lobby:
                        restoredSprite = Resources.Load<Sprite>("HallwayStages/Lobby_Restored");
#if UNITY_EDITOR
                        if (restoredSprite == null) restoredSprite = UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/Lobby_Restored.png");
#endif
                        break;
                }
            }
        }

        public void CheckAndApplyRestoration(bool animate = false)
        {
            bool isRestored = IsAreaRestored();
            if (isRestored && animate && !_isTransitioning)
            {
                StartCoroutine(RestorationSequenceRoutine());
            }
            else
            {
                ApplyRestorationState(animate: false);
            }
        }

        public void ApplyRestorationState(bool animate = false)
        {
            EnsureReferences();
            bool isRestored = IsAreaRestored();

            if (backgroundRenderer != null)
            {
                Sprite targetSprite = isRestored && restoredSprite != null ? restoredSprite : ruinedSprite;
                if (targetSprite != null)
                {
                    backgroundRenderer.sprite = targetSprite;
                }
                backgroundRenderer.color = Color.white;
            }

            if (globalLight != null)
            {
                globalLight.color = isRestored ? restoredLightColor : ruinedLightColor;
                globalLight.intensity = isRestored ? restoredLightIntensity : ruinedLightIntensity;
            }

            // Check if celebratory notification should be shown
            if (isRestored)
            {
                TriggerCelebratoryBannerIfNeeded();
            }
        }

        private void TriggerCelebratoryBannerIfNeeded()
        {
            var resolved = ResolveAreaType();
            string key = "";
            string title = "";
            string message = "";

            switch (resolved)
            {
                case AreaType.LeftWing:
                    key = "GameProgression_LeftWing_Celebrated";
                    title = "WEST WING 100% RESTORED!";
                    message = "All 6 exhibition halls (Rooms 1–6) are complete!\nThe West Wing has been restored to its golden splendor!";
                    break;
                case AreaType.RightWing:
                    key = "GameProgression_RightWing_Celebrated";
                    title = "EAST WING 100% RESTORED!";
                    message = "All 6 exhibition halls (Rooms 7–12) are complete!\nThe East Wing has been restored to its golden splendor!";
                    break;
                case AreaType.Lobby:
                    key = "GameProgression_Lobby_Celebrated";
                    title = "PAMANA MUSEUM 100% RESTORED!";
                    message = "All 12 exhibition halls across both wings are complete!\nThe Grand Lobby has been restored to its golden splendor!";
                    break;
            }

            if (!string.IsNullOrEmpty(key) && PlayerPrefs.GetInt(key, 0) == 0)
            {
                PlayerPrefs.SetInt(key, 1);
                PlayerPrefs.Save();

                NotificationBannerUI.Show(
                    title,
                    message,
                    6.0f,
                    new Color(0.95f, 0.85f, 0.35f, 1f),
                    "★"
                );
            }
        }

        private IEnumerator RestorationSequenceRoutine()
        {
            if (_isTransitioning) yield break;
            _isTransitioning = true;

            EnsureReferences();

            // 1. Play celestial crystal chime
            PlayRestorationChime();

            // 2. Camera impulse
            StartCoroutine(CameraImpulseRoutine());

            // 3. Temporary cross-fade overlay if restoredSprite exists
            if (backgroundRenderer != null && restoredSprite != null)
            {
                var overlayObj = new GameObject("Area_TransitionFX");
                overlayObj.transform.position = backgroundRenderer.transform.position;
                overlayObj.transform.rotation = backgroundRenderer.transform.rotation;
                overlayObj.transform.localScale = backgroundRenderer.transform.localScale;

                var overlaySr = overlayObj.AddComponent<SpriteRenderer>();
                overlaySr.sprite = restoredSprite;
                overlaySr.sortingOrder = backgroundRenderer.sortingOrder + 2;
                overlaySr.color = new Color(1f, 1f, 1f, 0f);

                float elapsed = 0f;
                float duration = 0.65f;
                float startLightIntensity = globalLight != null ? globalLight.intensity : ruinedLightIntensity;
                Color startLightColor = globalLight != null ? globalLight.color : ruinedLightColor;

                while (elapsed < duration)
                {
                    elapsed += Time.unscaledDeltaTime;
                    float t = Mathf.Clamp01(elapsed / duration);
                    float smoothT = Mathf.SmoothStep(0f, 1f, t);

                    overlaySr.color = new Color(1f, 1f, 1f, smoothT);

                    if (globalLight != null)
                    {
                        globalLight.intensity = Mathf.Lerp(startLightIntensity, restoredLightIntensity, smoothT);
                        globalLight.color = Color.Lerp(startLightColor, restoredLightColor, smoothT);
                    }

                    yield return null;
                }

                backgroundRenderer.sprite = restoredSprite;
                Destroy(overlayObj);
            }

            ApplyRestorationState(animate: false);
            _isTransitioning = false;
        }

        private IEnumerator CameraImpulseRoutine()
        {
            var cam = Camera.main;
            if (cam == null) cam = FindAnyObjectByType<Camera>();
            if (cam == null) yield break;

            float baseOrtho = cam.orthographicSize;
            Vector3 baseCamPos = cam.transform.position;

            float elapsed = 0f;
            float duration = 0.5f;

            while (elapsed < duration)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = elapsed / duration;
                float punch = Mathf.Sin(t * Mathf.PI) * 0.14f;
                cam.orthographicSize = baseOrtho - punch;

                float shakeX = Random.Range(-0.03f, 0.03f) * (1f - t);
                float shakeY = Random.Range(-0.03f, 0.03f) * (1f - t);
                cam.transform.position = baseCamPos + new Vector3(shakeX, shakeY, 0f);

                yield return null;
            }

            cam.orthographicSize = baseOrtho;
            cam.transform.position = baseCamPos;
        }

        private void PlayRestorationChime()
        {
            if (audioSource == null) return;

            if (_cachedChimeClip == null)
            {
                _cachedChimeClip = GenerateChimeAudioClip();
            }

            if (_cachedChimeClip != null)
            {
                audioSource.PlayOneShot(_cachedChimeClip, 0.9f);
            }
        }

        private AudioClip GenerateChimeAudioClip()
        {
            const int sampleRate = 44100;
            const float duration = 2.4f;
            int totalSamples = (int)(sampleRate * duration);
            float[] samples = new float[totalSamples];

            float[] chordFrequencies = { 523.25f, 659.25f, 783.99f, 1046.50f, 1318.51f };

            for (int i = 0; i < totalSamples; i++)
            {
                float time = (float)i / sampleRate;
                float sampleVal = 0f;

                for (int c = 0; c < chordFrequencies.Length; c++)
                {
                    float noteDelay = c * 0.09f;
                    if (time < noteDelay) continue;

                    float noteTime = time - noteDelay;
                    float freq = chordFrequencies[c];
                    float env = Mathf.Exp(-noteTime * 2.5f);

                    float osc = Mathf.Sin(2f * Mathf.PI * freq * noteTime) * 0.6f +
                                Mathf.Sin(4f * Mathf.PI * freq * noteTime) * 0.25f +
                                Mathf.Sin(6f * Mathf.PI * freq * noteTime) * 0.15f;

                    sampleVal += osc * env * 0.2f;
                }

                samples[i] = Mathf.Clamp(sampleVal, -1f, 1f);
            }

            var clip = AudioClip.Create("WingRestorationChime", totalSamples, 1, sampleRate, false);
            clip.SetData(samples, 0);
            return clip;
        }
    }
}

