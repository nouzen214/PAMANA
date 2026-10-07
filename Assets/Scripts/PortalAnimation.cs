using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace TopDownGame
{
    public class PortalAnimation : MonoBehaviour
    {
        [Header("Swirl Rotation")]
        [SerializeField] private Transform outerRing;
        [SerializeField] private Transform innerCore;
        [SerializeField] private float outerRotationSpeed = -90f;
        [SerializeField] private float innerRotationSpeed = 150f;

        [Header("Pulsing Scale")]
        [SerializeField] private float pulseSpeed = 2.5f;
        [SerializeField] private float pulseMagnitude = 0.12f;

        [Header("Light Breathing (Optional)")]
        [SerializeField] private Light2D portalLight;
        [SerializeField] private float minLightIntensity = 1.2f;
        [SerializeField] private float maxLightIntensity = 2.2f;

        [Header("Teal & Gold Color Shift")]
        [Tooltip("The SpriteRenderer on the vortex/swirl layer that will be color-tinted.")]
        [SerializeField] private SpriteRenderer vortexRenderer;
        [SerializeField] private Color tealColor  = new Color(0.10f, 0.85f, 0.80f, 1f);   // cyan-teal
        [SerializeField] private Color goldColor  = new Color(1.00f, 0.78f, 0.15f, 1f);   // warm gold
        [SerializeField] private float colorCycleSpeed = 0.8f;

        [Header("Outer Glow Tint")]
        [Tooltip("Optional second SpriteRenderer (glow layer) – kept in teal.")]
        [SerializeField] private SpriteRenderer glowRenderer;
        [SerializeField] private Color glowColor = new Color(0.10f, 0.90f, 0.95f, 0.55f);

        private Vector3 _initialScale;

        private void Awake()
        {
            _initialScale = transform.localScale;

            if (portalLight == null)
                portalLight = GetComponentInChildren<Light2D>();

            // Auto-find renderers if not assigned
            if (vortexRenderer == null && innerCore != null)
                vortexRenderer = innerCore.GetComponent<SpriteRenderer>();

            if (glowRenderer == null && outerRing != null)
                glowRenderer = outerRing.GetComponent<SpriteRenderer>();

            // Apply static glow tint
            if (glowRenderer != null)
                glowRenderer.color = glowColor;

            // Apply portal light color to teal-cyan
            if (portalLight != null)
                portalLight.color = new Color(0.15f, 0.90f, 0.90f, 1f);
        }

        private void Update()
        {
            // ── Rotation ──────────────────────────────────────────────────────
            if (outerRing != null)
                outerRing.Rotate(0f, 0f, outerRotationSpeed * Time.deltaTime);

            if (innerCore != null)
                innerCore.Rotate(0f, 0f, innerRotationSpeed * Time.deltaTime);

            // ── Pulse scale ───────────────────────────────────────────────────
            float scaleMultiplier = 1f + Mathf.Sin(Time.time * pulseSpeed) * pulseMagnitude;
            transform.localScale = _initialScale * scaleMultiplier;

            // ── Color shift: teal → gold → teal ───────────────────────────────
            if (vortexRenderer != null)
            {
                float t = (Mathf.Sin(Time.time * colorCycleSpeed) + 1f) * 0.5f;
                vortexRenderer.color = Color.Lerp(tealColor, goldColor, t);
            }

            // ── Light2D breathing ─────────────────────────────────────────────
            if (portalLight != null)
            {
                float t = (Mathf.Sin(Time.time * pulseSpeed * 1.5f) + 1f) * 0.5f;
                portalLight.intensity = Mathf.Lerp(minLightIntensity, maxLightIntensity, t);
            }
        }
    }
}
