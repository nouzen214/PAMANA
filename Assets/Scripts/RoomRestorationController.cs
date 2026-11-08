using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace TopDownGame
{
    /// <summary>
    /// Controls dynamic room restoration visuals and transition animations
    /// based on strict artifact placement thresholds:
    /// - 0 or 1 artifact: 0% restored (Ruined) -> 1st artifact does NOT restore the room.
    /// - 2/5 artifacts: 33% room restored -> triggers 33% transformation.
    /// - 3-4/5 artifacts: 67% room restored (Clean Stone) -> triggers 67% transformation.
    /// - 5/5 artifacts: 100% room restored (Golden Sanctuary) -> triggers 100% transformation.
    /// </summary>
    public class RoomRestorationController : MonoBehaviour
    {
        [Header("Room Identity")]
        [SerializeField] private int roomNumber = 1;

        [Header("Restoration Stage Sprites")]
        [SerializeField] private Sprite stage0RuinedSprite;    // 0% (0-1 artifacts)
        [SerializeField] private Sprite stage1PartialSprite;   // 33% (2 artifacts)
        [SerializeField] private Sprite stage2AdvancedSprite;  // 67% (3-4 artifacts)
        [SerializeField] private Sprite stage3PristineSprite;  // 100% (5 artifacts)

        [Header("Scene References")]
        [SerializeField] private SpriteRenderer roomBackgroundRenderer;
        [SerializeField] private Light2D globalLight;

        [Header("Audio")]
        [SerializeField] private AudioSource audioSource;

        private int _currentRestoredCount = 0;
        private bool _isTransitioning = false;
        private AudioClip _cachedChimeClip;

        // Stage Lighting profiles
        private readonly Color LightColorStage0 = new Color(0.70f, 0.65f, 0.55f, 1.0f);
        private const float LightIntensityStage0 = 0.78f;

        private readonly Color LightColorStage1 = new Color(0.80f, 0.75f, 0.65f, 1.0f);
        private const float LightIntensityStage1 = 0.88f;

        private readonly Color LightColorStage2 = new Color(0.90f, 0.84f, 0.72f, 1.0f);
        private const float LightIntensityStage2 = 0.98f;

        private readonly Color LightColorStage3 = new Color(1.00f, 0.94f, 0.80f, 1.0f);
        private const float LightIntensityStage3 = 1.15f;

        private void Awake()
        {
            EnsureReferences();
        }

        private void OnEnable()
        {
            GameProgression.OnPedestalRestored += HandlePedestalRestored;
        }

        private void OnDisable()
        {
            GameProgression.OnPedestalRestored -= HandlePedestalRestored;
        }

        public void Initialize(int rNumber)
        {
            roomNumber = rNumber;
            EnsureReferences();

            // Strictly apply current placed pedestal count without any animation on room enter
            _currentRestoredCount = GameProgression.GetPedestalsRestoredCount(roomNumber);
            int tier = GetRestorationTier(_currentRestoredCount);
            ApplyRestorationTier(tier);
        }

        public static int GetRestorationTier(int restoredCount)
        {
            if (restoredCount >= 5) return 3; // 100% restored (5/5)
            if (restoredCount >= 3) return 2; // 67% restored (3-4/5)
            if (restoredCount >= 2) return 1; // 33% restored (2/5)
            return 0;                         // 0% restored (0-1/5)
        }

        public static int GetRestorationPercentage(int restoredCount)
        {
            if (restoredCount >= 5) return 100;
            if (restoredCount >= 3) return 67;
            if (restoredCount >= 2) return 33;
            return 0;
        }

        public void EnsureReferences()
        {
            if (stage0RuinedSprite == null)
                stage0RuinedSprite = Resources.Load<Sprite>("RoomStages/Room_Area_Ruined");
            if (stage1PartialSprite == null)
                stage1PartialSprite = Resources.Load<Sprite>("RoomStages/Room_Area_33Percent");
            if (stage2AdvancedSprite == null)
                stage2AdvancedSprite = Resources.Load<Sprite>("RoomStages/Room_Area_PartiallyRestored");
            if (stage3PristineSprite == null)
                stage3PristineSprite = Resources.Load<Sprite>("RoomStages/Room_Area_Restored");

            if (roomBackgroundRenderer == null)
            {
                var bgObj = GameObject.Find("RoomBackground");
                if (bgObj != null)
                {
                    roomBackgroundRenderer = bgObj.GetComponent<SpriteRenderer>();
                }
            }

            // Remove any old blend overlay if present to keep hierarchy clean
            if (roomBackgroundRenderer != null)
            {
                var oldBlend = roomBackgroundRenderer.transform.Find("RoomBackground_BlendOverlay");
                if (oldBlend != null)
                {
                    if (Application.isPlaying) Destroy(oldBlend.gameObject);
                    else DestroyImmediate(oldBlend.gameObject);
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
        }

        public Sprite GetSpriteForTier(int tier)
        {
            EnsureReferences();
            switch (tier)
            {
                case 1:
                    return stage1PartialSprite != null ? stage1PartialSprite : stage0RuinedSprite;
                case 2:
                    return stage2AdvancedSprite != null ? stage2AdvancedSprite : stage1PartialSprite;
                case 3:
                    return stage3PristineSprite != null ? stage3PristineSprite : stage2AdvancedSprite;
                default:
                    return stage0RuinedSprite;
            }
        }

        /// <summary>
        /// Immediately sets the room's visual appearance based on restoration tier:
        /// Tier 0: 0 or 1 placed -> 0% (Ruined)
        /// Tier 1: 2 placed -> 33% (Partially Restored)
        /// Tier 2: 3-4 placed -> 67% (Clean Stone Room)
        /// Tier 3: 5 placed -> 100% (Pristine Golden Sanctuary)
        /// </summary>
        public void ApplyRestorationTier(int tier)
        {
            EnsureReferences();
            if (roomBackgroundRenderer == null) return;

            roomBackgroundRenderer.sprite = GetSpriteForTier(tier);
            roomBackgroundRenderer.color = Color.white;

            if (globalLight != null)
            {
                switch (tier)
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
                    case 3:
                    default:
                        globalLight.color = LightColorStage3;
                        globalLight.intensity = LightIntensityStage3;
                        break;
                }
            }
        }

        private void HandlePedestalRestored(int rNum, string artifactName)
        {
            if (rNum != roomNumber) return;

            int newRestored = GameProgression.GetPedestalsRestoredCount(roomNumber);
            int oldTier = GetRestorationTier(_currentRestoredCount);
            int newTier = GetRestorationTier(newRestored);

            _currentRestoredCount = newRestored;

            // ONLY trigger room transformation when the tier actually upgrades!
            // Placing 1st artifact: oldTier=0, newTier=0 -> NO transformation! (0% stays ruined)
            // Placing 2nd artifact: oldTier=0, newTier=1 -> TRANSFORMS to 33%!
            // Placing 3rd artifact: oldTier=1, newTier=2 -> TRANSFORMS to 67%!
            // Placing 4th artifact: oldTier=2, newTier=2 -> NO transformation (stays 67%)!
            // Placing 5th artifact: oldTier=2, newTier=3 -> TRANSFORMS to 100%!
            if (newTier > oldTier && !_isTransitioning)
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

                StartCoroutine(RestorationSequence(oldTier, newTier, newRestored, origin, artifactName));
            }
            else
            {
                // Tier didn't change (e.g. 1st artifact or 4th artifact), just apply state cleanly
                ApplyRestorationTier(newTier);
            }
        }

        public IEnumerator RestorationSequence(int fromTier, int toTier, int restoredCount, Vector3 burstOrigin, string artifactName)
        {
            if (_isTransitioning) yield break;
            _isTransitioning = true;

            EnsureReferences();

            // 1. Play celestial crystal chime
            PlayRestorationChime();

            // 2. Camera pulse & gentle rumble
            StartCoroutine(CameraImpulseRoutine());

            // 3. Spawn Pedestal Aura Ring on the restored pedestal
            SpawnPedestalAuras(burstOrigin);

            // 4. Spawn Expanding Radiant Shockwave Ring directly from the restored pedestal
            var ringObj = CreateShockwaveRing(burstOrigin);

            // 5. Spawn Ascending Golden Sparkle Motes
            var sparkles = SpawnSparkleMotes(25);

            // 6. Transition Background Visuals over 1.4 seconds
            float startLightIntensity = globalLight != null ? globalLight.intensity : LightIntensityStage0;
            Color startLightColor = globalLight != null ? globalLight.color : LightColorStage0;

            float targetLightIntensity = LightIntensityStage0;
            Color targetLightColor = LightColorStage0;
            Sprite targetOverlaySprite = GetSpriteForTier(toTier);

            switch (toTier)
            {
                case 1:
                    targetLightIntensity = LightIntensityStage1;
                    targetLightColor = LightColorStage1;
                    break;
                case 2:
                    targetLightIntensity = LightIntensityStage2;
                    targetLightColor = LightColorStage2;
                    break;
                case 3:
                default:
                    targetLightIntensity = LightIntensityStage3;
                    targetLightColor = LightColorStage3;
                    break;
            }

            // Create temporary transition cross-fade overlay
            var overlayObj = new GameObject("RoomBackground_TransitionFX");
            overlayObj.transform.position = roomBackgroundRenderer.transform.position;
            overlayObj.transform.rotation = roomBackgroundRenderer.transform.rotation;
            overlayObj.transform.localScale = roomBackgroundRenderer.transform.localScale;

            var overlaySr = overlayObj.AddComponent<SpriteRenderer>();
            overlaySr.sprite = targetOverlaySprite;
            overlaySr.sortingOrder = roomBackgroundRenderer.sortingOrder + 2;
            overlaySr.color = new Color(1f, 1f, 1f, 0f);

            float elapsed = 0f;
            float duration = 1.4f;

            while (elapsed < duration)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = Mathf.Clamp01(elapsed / duration);
                float smoothT = Mathf.SmoothStep(0f, 1f, t);

                if (overlaySr != null)
                {
                    overlaySr.color = new Color(1f, 1f, 1f, smoothT);
                }

                if (ringObj != null)
                {
                    float ringScale = Mathf.Lerp(0.8f, 16f, t);
                    ringObj.transform.localScale = new Vector3(ringScale, ringScale, 1f);
                    var ringSr = ringObj.GetComponent<SpriteRenderer>();
                    if (ringSr != null)
                    {
                        float alpha = Mathf.Sin(t * Mathf.PI) * 0.85f;
                        ringSr.color = new Color(1f, 0.92f, 0.5f, alpha);
                    }
                }

                if (globalLight != null)
                {
                    globalLight.color = Color.Lerp(startLightColor, targetLightColor, smoothT);
                    globalLight.intensity = Mathf.Lerp(startLightIntensity, targetLightIntensity, smoothT);
                }

                yield return null;
            }

            // Apply definitive restoration state for this tier
            ApplyRestorationTier(toTier);

            if (overlayObj != null) Destroy(overlayObj);
            if (ringObj != null) Destroy(ringObj);

            foreach (var sp in sparkles)
            {
                if (sp != null) Destroy(sp);
            }

            // Show celebratory banner for the reached threshold
            ShowRestorationBanner(toTier, restoredCount, artifactName);

            _isTransitioning = false;
        }

        private void ShowRestorationBanner(int tier, int count, string artifactName)
        {
            switch (tier)
            {
                case 1:
                    // 2/5 artifacts -> 33% room restored
                    NotificationBannerUI.Show(
                        $"ROOM {roomNumber} RESTORATION: 33%",
                        $"2/5 artifacts restored to pedestals!\nCobwebs and rubble dissolve from the chamber.",
                        5.0f,
                        new Color(1f, 0.88f, 0.35f, 1f),
                        "✦"
                    );
                    break;

                case 2:
                    // 3-4/5 artifacts -> 67% room restored
                    NotificationBannerUI.Show(
                        $"ROOM {roomNumber} RESTORATION: 67%",
                        $"{count}/5 artifacts restored to pedestals!\nThe chamber is 67% restored with clean ancient stone masonry.",
                        5.0f,
                        new Color(1f, 0.92f, 0.40f, 1f),
                        "✦"
                    );
                    break;

                case 3:
                default:
                    // 5/5 artifacts -> 100% room restored
                    NotificationBannerUI.Show(
                        $"ROOM {roomNumber} 100% PURIFIED!",
                        $"All 5/5 artifacts restored to pedestals!\nThe chamber is fully restored to its eternal golden glory!",
                        6.0f,
                        new Color(0.2f, 0.95f, 0.45f, 1f),
                        "★"
                    );
                    break;
            }
        }

        private GameObject CreateShockwaveRing(Vector3 position)
        {
            var ring = new GameObject("RestorationShockwave");
            ring.transform.position = new Vector3(position.x, position.y, 0f);
            ring.transform.localScale = Vector3.one;

            var sr = ring.AddComponent<SpriteRenderer>();
            sr.sprite = Resources.Load<Sprite>("PortalGlow");
            if (sr.sprite == null) sr.sprite = Resources.Load<Sprite>("SilhouetteGlow");
            sr.sortingOrder = (roomBackgroundRenderer != null ? roomBackgroundRenderer.sortingOrder : -10) + 3;
            sr.color = new Color(1f, 0.92f, 0.5f, 0.85f);

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

        private void SpawnPedestalAuras(Vector3 origin)
        {
            var glowSpr = Resources.Load<Sprite>("PortalGlow");
            if (glowSpr == null) glowSpr = Resources.Load<Sprite>("SilhouetteGlow");

            var aura = new GameObject("PedestalAura");
            aura.transform.position = new Vector3(origin.x, origin.y + 0.3f, 0f);
            aura.transform.localScale = new Vector3(0.5f, 0.5f, 1f);

            var sr = aura.AddComponent<SpriteRenderer>();
            sr.sprite = glowSpr;
            sr.sortingOrder = 4;
            sr.color = new Color(1f, 0.9f, 0.4f, 0.85f);

            StartCoroutine(AuraPulseRoutine(aura, sr));
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
                float scale = Mathf.Lerp(0.5f, 2.5f, t);
                aura.transform.localScale = new Vector3(scale, scale, 1f);
                float alpha = Mathf.Sin(t * Mathf.PI) * 0.85f;
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

        [ContextMenu("Test Place 1 (0% - No Room Transform)")]
        public void TestPlace1()
        {
            _currentRestoredCount = 1;
            ApplyRestorationTier(GetRestorationTier(1));
        }

        [ContextMenu("Test Place 2 (33% Restored)")]
        public void TestPlace2()
        {
            StartCoroutine(RestorationSequence(0, 1, 2, Vector3.zero, "Sample Artifact 2"));
        }

        [ContextMenu("Test Place 3 (67% Restored)")]
        public void TestPlace3()
        {
            StartCoroutine(RestorationSequence(1, 2, 3, Vector3.zero, "Sample Artifact 3"));
        }

        [ContextMenu("Test Place 4 (67% - Same Tier, No Transform)")]
        public void TestPlace4()
        {
            _currentRestoredCount = 4;
            ApplyRestorationTier(GetRestorationTier(4));
        }

        [ContextMenu("Test Place 5 (100% Fully Restored)")]
        public void TestPlace5()
        {
            StartCoroutine(RestorationSequence(2, 3, 5, Vector3.zero, "Sample Artifact 5"));
        }

        [ContextMenu("Reset to 0% (Ruined)")]
        public void TestResetTo0()
        {
            _currentRestoredCount = 0;
            ApplyRestorationTier(0);
        }
    }
}
