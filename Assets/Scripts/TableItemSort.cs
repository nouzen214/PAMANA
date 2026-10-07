using UnityEngine;

namespace TopDownGame
{
    /// <summary>
    /// For items/artifacts resting on top of a table or pedestal.
    /// Matches the table's SpriteRenderer sortingOrder plus an offset,
    /// ensuring it always renders cleanly on the table surface.
    /// </summary>
    [RequireComponent(typeof(SpriteRenderer))]
    public class TableItemSort : MonoBehaviour
    {
        [Tooltip("The table or pedestal this item sits on.")]
        [SerializeField] private SpriteRenderer parentTableRenderer;

        [Tooltip("Sorting order offset above the table surface (default +2)")]
        [SerializeField] private int orderOffset = 2;

        private SpriteRenderer _sr;

        private void Awake()
        {
            _sr = GetComponent<SpriteRenderer>();
            SyncOrder();
        }

        private void Start()
        {
            SyncOrder();
        }

        private void LateUpdate()
        {
            SyncOrder();
        }

        public void SyncOrder()
        {
            if (_sr == null) _sr = GetComponent<SpriteRenderer>();
            if (parentTableRenderer != null && _sr != null)
            {
                _sr.sortingLayerID = parentTableRenderer.sortingLayerID;
                _sr.sortingOrder = parentTableRenderer.sortingOrder + orderOffset;
            }
        }

        public void SetParentTable(SpriteRenderer tableRenderer)
        {
            parentTableRenderer = tableRenderer;
            SyncOrder();
        }
    }
}
