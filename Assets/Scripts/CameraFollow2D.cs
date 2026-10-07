using UnityEngine;

namespace TopDownGame
{
    [RequireComponent(typeof(Camera))]
    public class CameraFollow2D : MonoBehaviour
    {
        [Header("Target")]
        [SerializeField] private Transform target;

        [Header("Follow Settings")]
        [SerializeField] private float smoothSpeed = 12f;
        [SerializeField] private Vector2 offset = Vector2.zero;
        [SerializeField] private float cameraZ = -10f;

        [Header("Room World Boundaries")]
        [SerializeField] private bool useBounds = true;
        [SerializeField] private Vector2 mapMin = new Vector2(-27.80f, -5.76f);
        [SerializeField] private Vector2 mapMax = new Vector2(27.80f, 5.76f);
        [SerializeField] private bool lockY = true;
        [SerializeField] private float lockedY = 0f;

        [Header("Appearance")]
        [SerializeField] private Color voidClearColor = new Color(0.08f, 0.07f, 0.06f, 1f);

        private Camera _cam;
        private float _zDistance = -10f;

        private void Awake()
        {
            _cam = GetComponent<Camera>();
            if (_cam != null)
            {
                _cam.clearFlags = CameraClearFlags.SolidColor;
                _cam.backgroundColor = voidClearColor;
            }

            _zDistance = transform.position.z < -0.1f ? transform.position.z : cameraZ;
            float startY = lockY ? lockedY : transform.position.y;
            transform.position = new Vector3(transform.position.x, startY, _zDistance);
        }

        private void Start()
        {
            if (target == null)
            {
                var player = GameObject.FindGameObjectWithTag("Player");
                if (player != null)
                {
                    target = player.transform;
                }
            }

            SnapToTarget();
        }

        private void LateUpdate()
        {
            if (target == null) return;
            if (_cam == null) _cam = GetComponent<Camera>();

            // Calculate base desired position
            float rawTargetX = target.position.x + offset.x;
            float rawTargetY = lockY ? lockedY : (target.position.y + offset.y);

            float clampedTargetX = rawTargetX;
            float clampedTargetY = rawTargetY;

            if (useBounds && _cam != null)
            {
                float vertExtent = _cam.orthographicSize;
                float horzExtent = vertExtent * _cam.aspect;

                float minX = mapMin.x + horzExtent;
                float maxX = mapMax.x - horzExtent;
                float minY = mapMin.y + vertExtent;
                float maxY = mapMax.y - vertExtent;

                if (maxX >= minX)
                {
                    clampedTargetX = Mathf.Clamp(rawTargetX, minX, maxX);
                }
                else
                {
                    // Map is narrower than camera view; center view smoothly
                    clampedTargetX = (mapMin.x + mapMax.x) * 0.5f;
                }

                if (!lockY)
                {
                    if (maxY >= minY)
                    {
                        clampedTargetY = Mathf.Clamp(rawTargetY, minY, maxY);
                    }
                    else
                    {
                        clampedTargetY = (mapMin.y + mapMax.y) * 0.5f;
                    }
                }
            }

            Vector3 destination = new Vector3(clampedTargetX, clampedTargetY, _zDistance);

            // Framerate-independent exponential smoothing: perfectly fluid, never overshoots bounds
            float t = 1f - Mathf.Exp(-smoothSpeed * Time.deltaTime);
            Vector3 smoothed = Vector3.Lerp(transform.position, destination, t);

            // Hard clamp safeguard ensuring camera viewport never reveals outside the map
            if (useBounds && _cam != null)
            {
                float vertExtent = _cam.orthographicSize;
                float horzExtent = vertExtent * _cam.aspect;

                float minX = mapMin.x + horzExtent;
                float maxXBound = mapMax.x - horzExtent;

                if (maxXBound >= minX)
                {
                    smoothed.x = Mathf.Clamp(smoothed.x, minX, maxXBound);
                }
                else
                {
                    smoothed.x = (mapMin.x + mapMax.x) * 0.5f;
                }

                if (lockY)
                {
                    smoothed.y = lockedY;
                }
                else
                {
                    float minY = mapMin.y + vertExtent;
                    float maxYBound = mapMax.y - vertExtent;
                    if (maxYBound >= minY)
                    {
                        smoothed.y = Mathf.Clamp(smoothed.y, minY, maxYBound);
                    }
                    else
                    {
                        smoothed.y = (mapMin.y + mapMax.y) * 0.5f;
                    }
                }
            }

            smoothed.z = _zDistance;
            transform.position = smoothed;
        }

        public void SnapToTarget()
        {
            if (target == null) return;
            if (_cam == null) _cam = GetComponent<Camera>();

            float x = target.position.x + offset.x;
            float y = lockY ? lockedY : (target.position.y + offset.y);

            if (useBounds && _cam != null)
            {
                float vertExtent = _cam.orthographicSize;
                float horzExtent = vertExtent * _cam.aspect;

                float minX = mapMin.x + horzExtent;
                float maxX = mapMax.x - horzExtent;
                float minY = mapMin.y + vertExtent;
                float maxY = mapMax.y - vertExtent;

                x = maxX >= minX ? Mathf.Clamp(x, minX, maxX) : (mapMin.x + mapMax.x) * 0.5f;
                if (!lockY)
                {
                    y = maxY >= minY ? Mathf.Clamp(y, minY, maxY) : (mapMin.y + mapMax.y) * 0.5f;
                }
            }

            transform.position = new Vector3(x, y, _zDistance);
        }

        public void SetTarget(Transform newTarget)
        {
            target = newTarget;
            SnapToTarget();
        }

        public void SetMapBounds(Vector2 min, Vector2 max)
        {
            mapMin = min;
            mapMax = max;
            useBounds = true;
        }
    }
}

