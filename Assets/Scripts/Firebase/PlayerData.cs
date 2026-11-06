using System;
using System.Collections.Generic;

namespace TopDownGame
{
    [Serializable]
    public class RoomProgressData
    {
        public int roomNumber;
        public bool battleCompleted;
        public bool artifactRetrieved;
        public bool returnedToRealWorld;
        public int retrievedCount;
        public int restoredPedestalsCount;
        public string plaquesList = "";
        public List<string> restoredPedestalArtifacts = new List<string>();
    }

    [Serializable]
    public class SavedInventoryItem
    {
        public string id = "";
        public string name = "";
        public string description = "";
        public string spriteName = "";
    }

    [Serializable]
    public class PlayerData
    {
        public string username = "";
        public string email = "";
        public string selectedCharacter = "";
        public string characterName = "";
        public bool hasSelectedCharacter = false;
        public string currentScene = "Lobby";
        public float positionX = 0f;
        public float positionY = 0f;
        public float currentHealth = 100f;
        public float maxHealth = 100f;
        public int coins = 0;
        public float masterVolume = 1f;
        public float sfxVolume = 1f;
        public float bgmVolume = 1f;
        public bool isPortalUnlocked = false;
        public bool hasCompletedTutorial = false;
        public bool isRoom1BattleCompleted = false;
        public string lastLogin = "";

        public List<RoomProgressData> roomProgress = new List<RoomProgressData>();
        public List<SavedInventoryItem> inventoryItems = new List<SavedInventoryItem>();
        public List<string> inventoryArtifacts = new List<string>();

        public PlayerData()
        {
            lastLogin = DateTime.UtcNow.ToString("o");
        }
    }
}
