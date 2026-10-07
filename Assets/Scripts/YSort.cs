using UnityEngine;

namespace TopDownGame
{
    /// <summary>
    /// Updates SpriteRenderer sortingOrder dynamically based on Y position
    /// for natural 2D top-down perspective depth sorting.
    /// </summary>
    [RequireComponent(typeof(SpriteRenderer))]
    public class YSort : MonoBehaviour
    {
        [SerializeField] private int baseOrder = 5000;
        [SerializeField] private float sortingOffset = 0f;
        [SerializeField] private bool isStatic = false;

        private SpriteRenderer _sr;

        private void Awake()
        {
            _sr = GetComponent<SpriteRenderer>();
            UpdateSort();
        }

        private void LateUpdate()
        {
            if (!isStatic)
            {
                UpdateSort();
            }
        }

        public void UpdateSort()
        {
            if (_sr != null)
            {
                _sr.sortingOrder = baseOrder - Mathf.RoundToInt((transform.position.y + sortingOffset) * 100f);
            }
        }
    }
}
