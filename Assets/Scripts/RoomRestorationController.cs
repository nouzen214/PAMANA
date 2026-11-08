using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace TopDownGame
{
    /// <summary>
    /// Controls dynamic room restoration visuals and transition animations
    /// based on the number of artifacts retrieved or restored.
    /// Stage 0: Ruined (0 artifacts)
    /// Stage 1: Partially Restored (1-4 artifacts)
    /// Stage 2: Fully Restored / Pristine (5 artifacts)
    /// </summary>
    public class RoomRestorationController : MonoBehaviour
    {
        [Header("Room Identity")]
        [SerializeField] private int roomNumber = 1;

        [Header("Restoration Stage Sprites")]
        [SerializeField] private Sprite stage0RuinedSprite;
        [SerializeField] private Sprite stage1PartialSprite;
        [SerializeField] private Sprite stage2RestoredSprite;

        [Header("Scene References")]
        [SerializeField] private SpriteRenderer roomBackgroundRenderer;
        [SerializeField] private Light2D globalLight;

        [Header("Audio")]
        [SerializeField] private AudioSource audioSource;

        private int _currentStage = -1;
        private bool _isTransitioning = false;
        private AudioClip _cachedChimeClip;

        // Stage Lighting profiles
        private readonly Color LightColorStage0 = new Color(0.70f, 0.65f, 0.55f, 1.0f);
        private const float LightIntensityStage0 = 0.82f;

        private readonly Color LightColorStage1 = new Color(0.88f, 0.82f, 0.70f, 1.0f);
        private const float LightIntensityStage1 = 0.95f;

        private readonly Color LightColorStage2 = new Color(1.00f, 0.94f, 0.80f, 1.0f);
        private const float LightIntensityStage2 = 1.10f;

        private void Awake()
        {
            EnsureReferences();
        }

        private void OnEnable()
        {
            GameProgression.OnPedestalRestored += HandlePedestalRestored;
            GameProgression.OnRoomRestorationStageChanged += HandleStageChanged;
        }

        private void OnDisable()
        {
            GameProgression.OnPedestalRestored -= HandlePedestalRestored;
            GameProgression.OnRoomRestorationStageChanged -= HandleStageChanged;
        }

        public void Initialize(int rNumber)
        {
            roomNumber = rNumber;
            EnsureReferences();
            CheckAndApplyRestoration(true);
        }

        public void EnsureReferences()
        {
            if (stage0RuinedSprite == null)
                stage0RuinedSprite = Resources.Load<Sprite>("RoomStages/Room_Area_Ruined");
            if (stage1PartialSprite == null)
                stage1PartialSprite = Resources.Load<Sprite>("RoomStages/Room_Area_PartiallyRestored");
            if (stage2RestoredSprite == null)
                stage2RestoredSprite = Resources.Load<Sprite>("RoomStages/Room_Area_Restored");

            if (roomBackgroundRenderer == null)
            {
                var bgObj = GameObject.Find("RoomBackground");
                if (bgObj != null)
                {
                    roomBackgroundRenderer = bgObj.GetComponent<SpriteRenderer>();
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
                    audioSource.spatialBlend = 0f; // 2D UI / environmental chime
                }
            }
        }

        public Sprite GetSpriteForStage(int stage)
        {
            EnsureReferences();
            switch (stage)
            {
                case 1:
                    return stage1PartialSprite != null ? stage1PartialSprite : stage0RuinedSprite;
                case 2:
                    return stage2RestoredSprite != null ? stage2RestoredSprite : stage1PartialSprite;
                default:
                    return stage0RuinedSprite;
            }
        }

        public void ApplyStageVisual(int stage)
        {
            EnsureReferences();
            _currentStage = Mathf.Clamp(stage, 0, 2);

            if (roomBackgroundRenderer != null)
            {
                var spr = GetSpriteForStage(_currentStage);
                if (spr != null)
                {
                    roomBackgroundRenderer.sprite = spr;
                }
            }

            if (globalLight != null)
            {
                switch (_currentStage)
                {
                    case 0:
                        globalLight.color = LightColorStage0;
                        globalLight.intensity = LightIntensityStage0;
                        break;
                    case 1:
                        globalLight.color = LightColorStage1;
                        globalLight.intensity = LightIntensityStage1;
                        break;
                    case 2:
                        globalLight.color = LightColorStage2;
                        globalLight.intensity = LightIntensityStage2;
                        break;
                }
            }
        }

        public void CheckAndApplyRestoration(bool isEntrance = false)
        {
            if (roomNumber <= 0) return;

            int targetStage = GameProgression.GetRoomRestorationStage(roomNumber);
            int seenStage = GameProgression.GetRoomSeenRestorationStage(roomNumber);

            if (isEntrance)
            {
                if (targetStage > seenStage)
                {
                    // Start at previously seen stage, then play the glorious entrance transformation!
                    ApplyStageVisual(seenStage);
                    StartCoroutine(DelayedEntranceTransformation(seenStage, targetStage));
                }
                else
                {
                    ApplyStageVisual(targetStage);
                }
            }
            else
            {
                if (targetStage > _currentStage)
                {
                    StartCoroutine(RestorationSequence(_currentStage, targetStage, null, false));
                }
                else
                {
                    ApplyStageVisual(targetStage);
                }
            }
        }

        private IEnumerator DelayedEntranceTransformation(int fromStage, int toStage)
        {
            // Give the room banner and camera 0.6s to initialize before transformation begins
            yield return new WaitForSeconds(0.6f);
            yield return RestorationSequence(fromStage, toStage, Vector3.zero, true);
        }

        private void HandlePedestalRestored(int rNum, string artifactName)
        {
            if (rNum != roomNumber) return;

            int targetStage = GameProgression.GetRoomRestorationStage(roomNumber);
            if (targetStage > _currentStage && !_isTransitioning)
            {
                Vector3 origin = Vector3.zero;
                var pedestals = FindObjectsByType<ArtifactInteractable>(FindObjectsSortMode.None);
                foreach (var p in pedestals)
                {
                    if (p != null && p.ArtifactName == artifactName)
                    {
                        origin = p.transform.position;
                        break;
                    }
                }
                StartCoroutine(RestorationSequence(_currentStage, targetStage, origin, false));
            }
        }

        private void HandleStageChanged(int rNum, int newStage)
        {
            if (rNum != roomNumber) return;
            if (newStage > _currentStage && !_isTransitioning)
            {
                StartCoroutine(RestorationSequence(_currentStage, newStage, Vector3.zero, false));
            }
        }

        public IEnumerator RestorationSequence(int fromStage, int toStage, Vector3? burstOrigin, bool isEntrance)
        {
            if (_isTransitioning) yield break;
            _isTransitioning = true;

            EnsureReferences();
            var targetSprite = GetSpriteForStage(toStage);
            if (roomBackgroundRenderer == null || targetSprite == null)
            {
                ApplyStageVisual(toStage);
                GameProgression.SetRoomSeenRestorationStage(roomNumber, toStage);
                _isTransitioning = false;
                yield break;
            }

            Vector3 origin = burstOrigin ?? Vector3.zero;

            // 1. Play synthesized celestial chime
            PlayRestorationChime();

            // 2. Camera pulse & gentle rumble
            StartCoroutine(CameraImpulseRoutine());

            // 3. Spawn Pedestal Halo Pulses
            SpawnPedestalAuras();

            // 4. Create Cross-Fade Overlay
            var overlayObj = new GameObject("RoomBackground_TransitionOverlay");
            overlayObj.transform.position = roomBackgroundRenderer.transform.position;
            overlayObj.transform.rotation = roomBackgroundRenderer.transform.rotation;
            overlayObj.transform.localScale = roomBackgroundRenderer.transform.localScale;

            var overlaySr = overlayObj.AddComponent<SpriteRenderer>();
            overlaySr.sprite = targetSprite;
            overlaySr.sortingOrder = roomBackgroundRenderer.sortingOrder + 1;
            overlaySr.color = new Color(1f, 1f, 1f, 0f);

            // 5. Spawn Expanding Radiant Shockwave Ring
            var ringObj = CreateShockwaveRing(origin);

            // 6. Spawn Ascending Golden Sparkle Motes
            var sparkles = SpawnSparkleMotes(25);

            // 7. Light2D Profile Setup
            Color startLightColor = globalLight != null ? globalLight.color : LightColorStage0;
            float startLightIntensity = globalLight != null ? globalLight.intensity : LightIntensityStage0;
            Color targetLightColor = toStage == 2 ? LightColorStage2 : LightColorStage1;
            float targetLightIntensity = toStage == 2 ? LightIntensityStage2 : LightIntensityStage1;

            // 8. Cross-Fade Animation Loop (1.4 seconds)
            float elapsed = 0f;
            float duration = 1.4f;

            while (elapsed < duration)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = Mathf.Clamp01(elapsed / duration);
                float smoothT = Mathf.SmoothStep(0f, 1f, t);

                // Blend overlay background
                if (overlaySr != null)
                {
                    overlaySr.color = new Color(1f, 1f, 1f, smoothT);
                }

                // Expand and fade shockwave ring
                if (ringObj != null)
                {
                    float ringScale = Mathf.Lerp(1f, 16f, t);
                    ringObj.transform.localScale = new Vector3(ringScale, ringScale, 1f);
                    var ringSr = ringObj.GetComponent<SpriteRenderer>();
                    if (ringSr != null)
                    {
                        float alpha = Mathf.Sin(t * Mathf.PI) * 0.75f;
                        ringSr.color = new Color(1f, 0.92f, 0.5f, alpha);
                    }
                }

                // Interpolate Global Light 2D
                if (globalLight != null)
                {
                    globalLight.color = Color.Lerp(startLightColor, targetLightColor, smoothT);
                    globalLight.intensity = Mathf.Lerp(startLightIntensity, targetLightIntensity, smoothT);
                }

                yield return null;
            }

            // 9. Complete Background Transition
            roomBackgroundRenderer.sprite = targetSprite;
            _currentStage = toStage;
            GameProgression.SetRoomSeenRestorationStage(roomNumber, toStage);

            if (overlayObj != null) Destroy(overlayObj);
            if (ringObj != null) Destroy(ringObj);

            // Clean up sparkle motes
            foreach (var sp in sparkles)
            {
                if (sp != null) Destroy(sp);
            }

            // 10. Display Celebratory Banner
            if (toStage == 2)
            {
                NotificationBannerUI.Show(
                    $"ROOM {roomNumber} FULLY PURIFIED!",
                    "All artifacts restored! The ancient chamber shines with eternal golden light!",
                    5.5f,
                    new Color(0.2f, 0.95f, 0.45f, 1f),
                    "★"
                );
            }
            else if (toStage == 1)
            {
                NotificationBannerUI.Show(
                    $"ROOM {roomNumber} RESTORATION (STAGE 1)",
                    "Ancient curse weakened! Debris and cobwebs have dissolved from the chamber.",
                    5.0f,
                    new Color(1f, 0.88f, 0.35f, 1f),
                    "✦"
                );
            }

            _isTransitioning = false;
        }

        private GameObject CreateShockwaveRing(Vector3 position)
        {
            var ring = new GameObject("RestorationShockwave");
            ring.transform.position = new Vector3(position.x, position.y, 0f);
            ring.transform.localScale = Vector3.one;

            var sr = ring.AddComponent<SpriteRenderer>();
            sr.sprite = Resources.Load<Sprite>("PortalGlow");
            if (sr.sprite == null) sr.sprite = Resources.Load<Sprite>("SilhouetteGlow");
            sr.sortingOrder = (roomBackgroundRenderer != null ? roomBackgroundRenderer.sortingOrder : -10) + 2;
            sr.color = new Color(1f, 0.92f, 0.5f, 0.8f);

            return ring;
        }

        private List<GameObject> SpawnSparkleMotes(int count)
        {
            var list = new List<GameObject>();
            var spr = Resources.Load<Sprite>("SilhouetteGlow");
            if (spr == null) spr = Resources.Load<Sprite>("CircleBadge");

            for (int i = 0; i < count; i++)
            {
                var mote = new GameObject($"SparkleMote_{i}");
                float rx = Random.Range(-5.5f, 5.5f);
                float ry = Random.Range(-3.5f, 2.5f);
                mote.transform.position = new Vector3(rx, ry, -0.1f);
                float s = Random.Range(0.25f, 0.6f);
                mote.transform.localScale = new Vector3(s, s, 1f);

                var sr = mote.AddComponent<SpriteRenderer>();
                sr.sprite = spr;
                sr.sortingOrder = 5;
                sr.color = new Color(1f, Random.Range(0.85f, 1f), Random.Range(0.3f, 0.6f), Random.Range(0.6f, 0.9f));

                StartCoroutine(SparkleFloatRoutine(mote, sr));
                list.Add(mote);
            }

            return list;
        }

        private IEnumerator SparkleFloatRoutine(GameObject mote, SpriteRenderer sr)
        {
            if (mote == null || sr == null) yield break;
            float duration = Random.Range(1.0f, 1.6f);
            float elapsed = 0f;
            Vector3 startPos = mote.transform.position;
            float driftX = Random.Range(-0.4f, 0.4f);
            float driftY = Random.Range(0.8f, 1.8f);
            Color startColor = sr.color;

            while (elapsed < duration && mote != null)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = elapsed / duration;
                mote.transform.position = startPos + new Vector3(driftX * t, driftY * t, 0f);
                float alpha = Mathf.Sin(t * Mathf.PI) * startColor.a;
                sr.color = new Color(startColor.r, startColor.g, startColor.b, alpha);
                yield return null;
            }

            if (mote != null) Destroy(mote);
        }

        private void SpawnPedestalAuras()
        {
            var pedestals = FindObjectsByType<ArtifactInteractable>(FindObjectsSortMode.None);
            var glowSpr = Resources.Load<Sprite>("PortalGlow");
            if (glowSpr == null) glowSpr = Resources.Load<Sprite>("SilhouetteGlow");

            foreach (var p in pedestals)
            {
                if (p == null) continue;
                var aura = new GameObject("PedestalAura");
                aura.transform.position = new Vector3(p.transform.position.x, p.transform.position.y + 0.3f, 0f);
                aura.transform.localScale = new Vector3(0.5f, 0.5f, 1f);

                var sr = aura.AddComponent<SpriteRenderer>();
                sr.sprite = glowSpr;
                sr.sortingOrder = 4;
                sr.color = new Color(1f, 0.9f, 0.4f, 0.85f);

                StartCoroutine(AuraPulseRoutine(aura, sr));
            }
        }

        private IEnumerator AuraPulseRoutine(GameObject aura, SpriteRenderer sr)
        {
            if (aura == null || sr == null) yield break;
            float duration = 1.2f;
            float elapsed = 0f;
            while (elapsed < duration && aura != null)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = elapsed / duration;
                float scale = Mathf.Lerp(0.5f, 2.2f, t);
                aura.transform.localScale = new Vector3(scale, scale, 1f);
                float alpha = Mathf.Sin(t * Mathf.PI) * 0.8f;
                sr.color = new Color(1f, 0.9f, 0.4f, alpha);
                yield return null;
            }
            if (aura != null) Destroy(aura);
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

                float shakeX = Random.Range(-0.035f, 0.035f) * (1f - t);
                float shakeY = Random.Range(-0.035f, 0.035f) * (1f - t);
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
                audioSource.PlayOneShot(_cachedChimeClip, 0.85f);
            }
        }

        private AudioClip GenerateChimeAudioClip()
        {
            int sampleRate = 44100;
            float duration = 1.6f;
            int sampleCount = (int)(sampleRate * duration);
            float[] samples = new float[sampleCount];

            float[] freqs = new float[] { 523.25f, 659.25f, 783.99f, 1046.50f };
            float[] startTimes = new float[] { 0.0f, 0.15f, 0.30f, 0.45f };

            for (int note = 0; note < freqs.Length; note++)
            {
                float f = freqs[note];
                int startIdx = (int)(startTimes[note] * sampleRate);
                for (int i = startIdx; i < sampleCount; i++)
                {
                    float t = (i - startIdx) / (float)sampleRate;
                    float env = Mathf.Exp(-3.5f * t);
                    float wave = Mathf.Sin(2f * Mathf.PI * f * t) * 0.6f
                               + Mathf.Sin(4f * Mathf.PI * f * t) * 0.25f
                               + Mathf.Sin(6f * Mathf.PI * f * t) * 0.15f;
                    samples[i] += wave * env * 0.22f;
                }
            }

            var clip = AudioClip.Create("RestorationChime", sampleCount, 1, sampleRate, false);
            clip.SetData(samples, 0);
            return clip;
        }

        [ContextMenu("Test Transform to Stage 1")]
        public void TestTransformToStage1()
        {
            StartCoroutine(RestorationSequence(_currentStage, 1, Vector3.zero, false));
        }

        [ContextMenu("Test Transform to Stage 2")]
        public void TestTransformToStage2()
        {
            StartCoroutine(RestorationSequence(_currentStage, 2, Vector3.zero, false));
        }

        [ContextMenu("Reset to Stage 0")]
        public void TestResetToStage0()
        {
            ApplyStageVisual(0);
        }
    }
}
