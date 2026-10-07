using System.Collections;
using UnityEngine;
using UnityEngine.Events;

namespace TopDownGame
{
    /// <summary>
    /// Attach to any door GameObject.
    /// The door can be opened/closed by a nearby DoorInteractor (on the player).
    /// Supports two animation modes:
    ///   • Slide  – door moves along a local axis (e.g. up or sideways)
    ///   • Pivot  – door rotates around its origin (classic hinged swing)
    /// </summary>
    public class DoorController : MonoBehaviour
    {
        public enum DoorAnimMode { Slide, Pivot }

        // ─── Inspector ────────────────────────────────────────────────────────
        [Header("Animation")]
        [SerializeField] private DoorAnimMode animMode = DoorAnimMode.Slide;

        [Tooltip("Slide mode: local-space direction & distance the door travels when opening.")]
        [SerializeField] private Vector2 slideOffset = new Vector2(0f, 1.2f);

        [Tooltip("Pivot mode: angle (degrees) the door rotates when opening (+ = counter-clockwise).")]
        [SerializeField] private float pivotAngle = 90f;

        [SerializeField] private float openDuration  = 0.35f;
        [SerializeField] private float closeDuration = 0.45f;
        [SerializeField] private AnimationCurve openCurve  = AnimationCurve.EaseInOut(0, 0, 1, 1);
        [SerializeField] private AnimationCurve closeCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

        [Header("Auto-Close")]
        [Tooltip("Automatically close the door after this many seconds. 0 = never.")]
        [SerializeField] private float autoCloseDelay = 0f;

        [Header("Collider")]
        [Tooltip("Collider to disable while the door is open so the player can walk through.")]
        [SerializeField] private Collider2D doorCollider;

        [Header("Audio")]
        [SerializeField] private AudioSource audioSource;
        [SerializeField] private AudioClip openSound;
        [SerializeField] private AudioClip closeSound;

        [Header("Events")]
        public UnityEvent onDoorOpen;
        public UnityEvent onDoorClose;

        // ─── State ────────────────────────────────────────────────────────────
        public bool IsOpen { get; private set; }

        private Vector3   _closedLocalPos;
        private Quaternion _closedLocalRot;
        private Vector3   _openLocalPos;
        private Quaternion _openLocalRot;

        private Coroutine _animCoroutine;
        private Coroutine _autoCloseCoroutine;

        // ─────────────────────────────────────────────────────────────────────
        private void Awake()
        {
            // Record "closed" transform
            _closedLocalPos = transform.localPosition;
            _closedLocalRot = transform.localRotation;

            // Pre-compute "open" transform
            if (animMode == DoorAnimMode.Slide)
            {
                _openLocalPos = _closedLocalPos + (Vector3)slideOffset;
                _openLocalRot = _closedLocalRot;
            }
            else // Pivot
            {
                _openLocalPos = _closedLocalPos;
                _openLocalRot = _closedLocalRot * Quaternion.Euler(0f, 0f, pivotAngle);
            }

            if (doorCollider == null)
                doorCollider = GetComponent<Collider2D>();
        }

        // ─────────────────────────────────────────────────────────────────────
        /// <summary>Called by DoorInteractor when the player presses Interact.</summary>
        public virtual void Interact()
        {
            if (IsOpen)
                Close();
            else
                Open();
        }

        public virtual string GetPromptText(string openText, string closeText)
        {
            return IsOpen ? closeText : openText;
        }

        public void Open()
        {
            if (IsOpen) return;
            IsOpen = true;

            PlaySound(openSound);
            onDoorOpen?.Invoke();

            if (doorCollider != null) doorCollider.enabled = false;

            RunAnim(_closedLocalPos, _closedLocalRot, _openLocalPos, _openLocalRot, openDuration, openCurve);

            if (autoCloseDelay > 0f)
            {
                if (_autoCloseCoroutine != null) StopCoroutine(_autoCloseCoroutine);
                _autoCloseCoroutine = StartCoroutine(AutoCloseRoutine());
            }
        }

        public void Close()
        {
            if (!IsOpen) return;
            IsOpen = false;

            PlaySound(closeSound);
            onDoorClose?.Invoke();

            RunAnim(_openLocalPos, _openLocalRot, _closedLocalPos, _closedLocalRot, closeDuration, closeCurve,
                    onComplete: () =>
                    {
                        if (doorCollider != null) doorCollider.enabled = true;
                    });
        }

        // ─── Internal ─────────────────────────────────────────────────────────
        private void RunAnim(Vector3 fromPos, Quaternion fromRot,
                             Vector3 toPos,   Quaternion toRot,
                             float duration,  AnimationCurve curve,
                             System.Action onComplete = null)
        {
            if (_animCoroutine != null) StopCoroutine(_animCoroutine);
            _animCoroutine = StartCoroutine(AnimRoutine(fromPos, fromRot, toPos, toRot, duration, curve, onComplete));
        }

        private IEnumerator AnimRoutine(Vector3 fromPos, Quaternion fromRot,
                                        Vector3 toPos,   Quaternion toRot,
                                        float duration,  AnimationCurve curve,
                                        System.Action onComplete)
        {
            float elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = curve.Evaluate(Mathf.Clamp01(elapsed / duration));
                transform.localPosition = Vector3.Lerp(fromPos, toPos, t);
                transform.localRotation = Quaternion.Slerp(fromRot, toRot, t);
                yield return null;
            }
            transform.localPosition = toPos;
            transform.localRotation = toRot;
            onComplete?.Invoke();
        }

        private IEnumerator AutoCloseRoutine()
        {
            yield return new WaitForSeconds(autoCloseDelay);
            Close();
        }

        private void PlaySound(AudioClip clip)
        {
            if (audioSource != null && clip != null)
                audioSource.PlayOneShot(clip);
        }

        // ─── Gizmo ────────────────────────────────────────────────────────────
        private void OnDrawGizmosSelected()
        {
            if (!Application.isPlaying)
            {
                Gizmos.color = new Color(0f, 1f, 0.5f, 0.5f);
                if (animMode == DoorAnimMode.Slide)
                {
                    Vector3 openPos = transform.position + transform.TransformDirection((Vector3)slideOffset);
                    Gizmos.DrawLine(transform.position, openPos);
                    Gizmos.DrawWireCube(openPos, transform.lossyScale);
                }
            }
        }
    }
}
