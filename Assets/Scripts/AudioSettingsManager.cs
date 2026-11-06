using UnityEngine;

namespace TopDownGame
{
    public static class AudioSettingsManager
    {
        private const string PrefMaster = "Settings_MasterVolume";
        private const string PrefSFX = "Settings_SFXVolume";
        private const string PrefBGM = "Settings_BGMVolume";

        public static float MasterVolume { get; private set; } = 1f;
        public static float SFXVolume { get; private set; } = 1f;
        public static float BGMVolume { get; private set; } = 1f;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        public static void Initialize()
        {
            Load();
            Apply();
        }

        public static void Load()
        {
            MasterVolume = PlayerPrefs.GetFloat(PrefMaster, 1f);
            SFXVolume = PlayerPrefs.GetFloat(PrefSFX, 1f);
            BGMVolume = PlayerPrefs.GetFloat(PrefBGM, 1f);
        }

        public static void SetMaster(float val)
        {
            MasterVolume = Mathf.Clamp01(val);
            Apply();
        }

        public static void SetSFX(float val)
        {
            SFXVolume = Mathf.Clamp01(val);
        }

        public static void SetBGM(float val)
        {
            BGMVolume = Mathf.Clamp01(val);
        }

        public static void Save()
        {
            PlayerPrefs.SetFloat(PrefMaster, MasterVolume);
            PlayerPrefs.SetFloat(PrefSFX, SFXVolume);
            PlayerPrefs.SetFloat(PrefBGM, BGMVolume);
            PlayerPrefs.Save();
        }

        public static void ResetDefaults()
        {
            SetMaster(1f);
            SetSFX(1f);
            SetBGM(1f);
            Save();
        }

        public static void Apply()
        {
            AudioListener.volume = MasterVolume;
        }
    }
}

