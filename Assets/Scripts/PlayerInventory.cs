using System;
using System.Collections.Generic;
using UnityEngine;

namespace TopDownGame
{
    [Serializable]
    public class ArtifactItemData
    {
        public string id;
        public string name;
        public Sprite icon;
        [TextArea(2, 4)] public string description;

        public ArtifactItemData(string id, string name, Sprite icon, string description)
        {
            this.id = id;
            this.name = name;
            this.icon = icon;
            this.description = description;
        }
    }

    /// <summary>
    /// Persistent player inventory holding collected artifacts.
    /// Preserved across scene transitions.
    /// </summary>
    public static class PlayerInventory
    {
        private static readonly List<ArtifactItemData> _items = new List<ArtifactItemData>();

        public static IReadOnlyList<ArtifactItemData> Items => _items.AsReadOnly();
        public static int Count => _items.Count;

        public static event Action OnInventoryChanged;

        public static bool AddArtifact(string id, string name, Sprite icon, string description)
        {
            if (HasArtifact(id)) return false;

            var item = new ArtifactItemData(id, name, icon, description);
            _items.Add(item);
            Debug.Log($"[PlayerInventory] Added artifact: '{name}' (Total: {_items.Count})");
            OnInventoryChanged?.Invoke();
            return true;
        }

        public static bool HasArtifact(string idOrName)
        {
            if (string.IsNullOrEmpty(idOrName)) return false;
            return _items.Exists(item => 
                string.Equals(item.id, idOrName, StringComparison.OrdinalIgnoreCase) ||
                string.Equals(item.name, idOrName, StringComparison.OrdinalIgnoreCase));
        }

        public static ArtifactItemData GetArtifact(string idOrName)
        {
            if (string.IsNullOrEmpty(idOrName)) return null;
            return _items.Find(item => 
                string.Equals(item.id, idOrName, StringComparison.OrdinalIgnoreCase) ||
                string.Equals(item.name, idOrName, StringComparison.OrdinalIgnoreCase));
        }

        public static bool RemoveArtifact(string idOrName)
        {
            if (string.IsNullOrEmpty(idOrName)) return false;
            int index = _items.FindIndex(item => 
                string.Equals(item.id, idOrName, StringComparison.OrdinalIgnoreCase) ||
                string.Equals(item.name, idOrName, StringComparison.OrdinalIgnoreCase));

            if (index >= 0)
            {
                var removed = _items[index];
                _items.RemoveAt(index);
                Debug.Log($"[PlayerInventory] Removed artifact: '{removed.name}' (Remaining: {_items.Count})");
                OnInventoryChanged?.Invoke();
                return true;
            }
            return false;
        }

        public static void Clear()
        {
            _items.Clear();
            OnInventoryChanged?.Invoke();
        }
    }
}
