using System;
using UnityEngine;

namespace TopDownGame
{
    /// <summary>
    /// Tracks global gameplay progression across rooms and scenes.
    /// </summary>
    public static class GameProgression
    {
        private const string KEY_PORTAL_UNLOCKED = "GameProgression_PortalUnlocked";
        private const string KEY_ROOM1_ARTIFACT_INTERACTED = "GameProgression_Room1ArtifactInteracted";
        private const string KEY_TUTORIAL_COMPLETED = "GameProgression_TutorialCompleted";
        private const string KEY_ROOM1_ARTIFACT_RETRIEVED = "GameProgression_Room1ArtifactRetrieved";
        private const string KEY_ROOM1_BATTLE_COMPLETED = "GameProgression_Room1BattleCompleted";
        private const string KEY_ROOM1_ARTIFACT_RETURNED = "GameProgression_Room1ArtifactReturnedToRealWorld";

        public static event Action OnPortalUnlocked;
        public static event Action OnRoom1ArtifactRetrieved;
        public static event Action OnRoom1BattleCompleted;
        public static event Action OnRoom1ArtifactReturned;

        public static bool IsRoom1BattleCompleted
        {
            get => PlayerPrefs.GetInt(KEY_ROOM1_BATTLE_COMPLETED, 0) == 1;
            set
            {
                PlayerPrefs.SetInt(KEY_ROOM1_BATTLE_COMPLETED, value ? 1 : 0);
                PlayerPrefs.Save();
            }
        }

        public static void CompleteRoom1Battle()
        {
            bool wasCompleted = IsRoom1BattleCompleted;
            IsRoom1BattleCompleted = true;
            if (!wasCompleted)
            {
                Debug.Log("[GameProgression] Room 1 Battle completed! Room 1 locked, cannot re-enter.");
                OnRoom1BattleCompleted?.Invoke();
            }
        }

        public static bool IsRoomBattleCompleted(int roomNum)
        {
            if (roomNum == 1) return IsRoom1BattleCompleted;
            return PlayerPrefs.GetInt($"GameProgression_Room{roomNum}BattleCompleted", 0) == 1;
        }

        public static void CompleteRoomBattle(int roomNum)
        {
            if (roomNum == 1)
            {
                CompleteRoom1Battle();
                return;
            }
            string key = $"GameProgression_Room{roomNum}BattleCompleted";
            bool wasCompleted = PlayerPrefs.GetInt(key, 0) == 1;
            PlayerPrefs.SetInt(key, 1);
            PlayerPrefs.Save();
            if (!wasCompleted)
            {
                Debug.Log($"[GameProgression] Room {roomNum} Battle completed! Room {roomNum} locked, cannot re-enter.");
                OnRoom1BattleCompleted?.Invoke();
            }
        }

        public static bool HasCompletedTutorial
        {
            get => PlayerPrefs.GetInt(KEY_TUTORIAL_COMPLETED, 0) == 1;
            set
            {
                PlayerPrefs.SetInt(KEY_TUTORIAL_COMPLETED, value ? 1 : 0);
                PlayerPrefs.Save();
            }
        }

        public static bool IsPortalUnlocked
        {
            get => PlayerPrefs.GetInt(KEY_PORTAL_UNLOCKED, 0) == 1;
            set
            {
                PlayerPrefs.SetInt(KEY_PORTAL_UNLOCKED, value ? 1 : 0);
                PlayerPrefs.Save();
            }
        }

        public static bool HasInteractedWithRoom1Artifact
        {
            get => PlayerPrefs.GetInt(KEY_ROOM1_ARTIFACT_INTERACTED, 0) == 1;
            set
            {
                PlayerPrefs.SetInt(KEY_ROOM1_ARTIFACT_INTERACTED, value ? 1 : 0);
                PlayerPrefs.Save();
            }
        }

        public static bool HasRetrievedRoom1Artifact
        {
            get
            {
                if (PlayerPrefs.GetInt(KEY_ROOM1_ARTIFACT_RETRIEVED, 0) == 1) return true;
                if (PlayerInventory.HasArtifact("Room1_DipPenNib") ||
                    PlayerInventory.HasArtifact("Room1_UniversityWaxSeal") ||
                    PlayerInventory.HasArtifact("Room1_TravelNotebook") ||
                    PlayerInventory.HasArtifact("Room1_WireSpectacles") ||
                    PlayerInventory.HasArtifact("Room1_KeepsakeLocket") ||
                    PlayerInventory.HasArtifact("Room1_GoldLocketPendant") ||
                    PlayerInventory.Count > 0)
                {
                    return true;
                }
                return false;
            }
            set
            {
                PlayerPrefs.SetInt(KEY_ROOM1_ARTIFACT_RETRIEVED, value ? 1 : 0);
                PlayerPrefs.Save();
            }
        }

        public static bool HasReturnedRoom1ArtifactToRealWorld
        {
            get => PlayerPrefs.GetInt(KEY_ROOM1_ARTIFACT_RETURNED, 0) == 1;
            set
            {
                PlayerPrefs.SetInt(KEY_ROOM1_ARTIFACT_RETURNED, value ? 1 : 0);
                PlayerPrefs.Save();
            }
        }

        public static bool RetrieveRoom1Artifact()
        {
            bool wasRetrieved = HasRetrievedRoom1Artifact;
            HasRetrievedRoom1Artifact = true;

            if (!wasRetrieved)
            {
                Debug.Log("[GameProgression] Player retrieved at least 1 artifact from Room 1 monster! Must return it inside Room 1 in Real World to unlock Room 2.");
                OnRoom1ArtifactRetrieved?.Invoke();
                return true;
            }
            return false;
        }

        public static bool ReturnRoom1ArtifactToRealWorld()
        {
            bool wasReturned = HasReturnedRoom1ArtifactToRealWorld;
            HasReturnedRoom1ArtifactToRealWorld = true;

            if (!wasReturned)
            {
                Debug.Log("[GameProgression] Player brought Room 1 artifacts inside Room 1 in the Real World! Alterworld Room 2 is now UNLOCKED.");
                OnRoom1ArtifactReturned?.Invoke();
                return true;
            }
            return false;
        }

        public static bool HasReturnedRoomArtifactsToRealWorld(int roomNumber)
        {
            if (roomNumber == 1) return HasReturnedRoom1ArtifactToRealWorld;
            return PlayerPrefs.GetInt($"GameProgression_Room{roomNumber}ArtifactReturnedToRealWorld", 0) == 1;
        }

        public static bool ReturnRoomArtifactsToRealWorld(int roomNumber)
        {
            if (roomNumber == 1) return ReturnRoom1ArtifactToRealWorld();

            string key = $"GameProgression_Room{roomNumber}ArtifactReturnedToRealWorld";
            bool wasReturned = PlayerPrefs.GetInt(key, 0) == 1;
            PlayerPrefs.SetInt(key, 1);
            PlayerPrefs.Save();

            if (!wasReturned)
            {
                Debug.Log($"[GameProgression] Player brought Room {roomNumber} artifacts inside Room {roomNumber} in the Real World! Alterworld Room {roomNumber + 1} is now UNLOCKED.");
                OnRoom1ArtifactReturned?.Invoke();
                return true;
            }
            return false;
        }

        public static bool HasRetrievedRoomArtifact(int roomNum)
        {
            if (roomNum == 1) return HasRetrievedRoom1Artifact;
            return PlayerPrefs.GetInt($"GameProgression_Room{roomNum}ArtifactRetrieved", 0) == 1;
        }

        public static bool RetrieveRoomArtifact(int roomNum)
        {
            if (roomNum == 1) return RetrieveRoom1Artifact();
            string key = $"GameProgression_Room{roomNum}ArtifactRetrieved";
            bool wasRetrieved = PlayerPrefs.GetInt(key, 0) == 1;
            PlayerPrefs.SetInt(key, 1);
            PlayerPrefs.Save();
            if (!wasRetrieved)
            {
                Debug.Log($"[GameProgression] Player retrieved at least 1 artifact from Room {roomNum} monster! Must return it inside Room {roomNum} in Real World to unlock Room {roomNum + 1}.");
                OnRoom1ArtifactRetrieved?.Invoke();
                return true;
            }
            return false;
        }

        public static bool UnlockPortal()
        {
            bool wasUnlocked = IsPortalUnlocked;
            HasInteractedWithRoom1Artifact = true;
            IsPortalUnlocked = true;

            if (!wasUnlocked)
            {
                Debug.Log("[GameProgression] Portal to Alterworld has been UNLOCKED!");
                OnPortalUnlocked?.Invoke();
                return true;
            }
            return false;
        }

        public static void ResetProgression()
        {
            PlayerPrefs.DeleteKey(KEY_PORTAL_UNLOCKED);
            PlayerPrefs.DeleteKey(KEY_ROOM1_ARTIFACT_INTERACTED);
            PlayerPrefs.DeleteKey("HasAcknowledgedPortalInDialogue");
            PlayerPrefs.DeleteKey(KEY_TUTORIAL_COMPLETED);
            PlayerPrefs.DeleteKey(KEY_ROOM1_ARTIFACT_RETRIEVED);
            PlayerPrefs.DeleteKey(KEY_ROOM1_BATTLE_COMPLETED);
            PlayerPrefs.DeleteKey(KEY_ROOM1_ARTIFACT_RETURNED);
            for (int i = 1; i <= 12; i++)
            {
                PlayerPrefs.DeleteKey($"GameProgression_Room{i}BattleCompleted");
                PlayerPrefs.DeleteKey($"GameProgression_Room{i}ArtifactRetrieved");
                PlayerPrefs.DeleteKey($"GameProgression_Room{i}ArtifactReturnedToRealWorld");
            }
            PlayerPrefs.Save();
            Debug.Log("[GameProgression] Progression reset. All rooms reset.");
        }
    }
}
