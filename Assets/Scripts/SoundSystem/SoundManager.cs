using System.Collections.Generic;
using FMOD.Studio;
using FMODUnity;
using UnityEngine;

namespace SoundSystem
{
    public class SoundManager : MonoBehaviour
    {
        public static SoundManager Instance { get; private set; }

        [Header("FMOD Banks")] [SerializeField]
        private List<string> bankNames = new() { "Master", "Music", "SFX" };

        [Header("SFX Database")] [SerializeField]
        private SfxDatabase sfxDatabase;

        [Header("Music Settings")] [SerializeField]
        private EventReference currentMusicEvent;

        private EventInstance musicInstance;

        [Header("Audio Settings")] [Range(0f, 1f)]
        public float masterVolume = 1f;

        [Range(0f, 1f)] public float musicVolume = 1f;
        [Range(0f, 1f)] public float sfxVolume = 1f;

        private Bus masterBus;
        private Bus musicBus;
        private Bus sfxBus;

        private Dictionary<string, EventInstance> activeInstances = new Dictionary<string, EventInstance>();

        void Awake()
        {
            // Singleton pattern
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
                InitializeAudio();
            }
            else
            {
                Destroy(gameObject);
            }
        }

        void InitializeAudio()
        {
            // Load banks
            foreach (string bankName in bankNames)
            {
                try
                {
                    RuntimeManager.LoadBank(bankName, true);
                    Debug.Log($"Loaded FMOD bank: {bankName}");
                }
                catch (BankLoadException e)
                {
                    Debug.LogError($"Failed to load bank {bankName}: {e.Message}");
                }
            }

            // Initialize SFX database
            if (sfxDatabase != null)
            {
                sfxDatabase.Initialize();
            }

            // Get buses
            masterBus = RuntimeManager.GetBus("bus:/");
            musicBus = RuntimeManager.GetBus("bus:/Music");
            sfxBus = RuntimeManager.GetBus("bus:/SFX");

            // Apply initial volumes
            UpdateVolumes();
        }

        // ===== MUSIC METHODS =====
        public void PlayMusic(EventReference musicEvent)
        {
            StopMusic();

            if (!musicEvent.IsNull)
            {
                musicInstance = RuntimeManager.CreateInstance(musicEvent);
                musicInstance.start();
                currentMusicEvent = musicEvent;
            }
        }

        public void PlayMusic(string musicEventPath)
        {
            StopMusic();

            if (!string.IsNullOrEmpty(musicEventPath))
            {
                musicInstance = RuntimeManager.CreateInstance(musicEventPath);
                musicInstance.start();
            }
        }

        public void StopMusic(bool fadeOut = true)
        {
            if (musicInstance.isValid())
            {
                if (fadeOut)
                {
                    musicInstance.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
                }
                else
                {
                    musicInstance.stop(FMOD.Studio.STOP_MODE.IMMEDIATE);
                }

                musicInstance.release();
            }
        }

        public void PauseMusic()
        {
            if (musicInstance.isValid())
            {
                musicInstance.setPaused(true);
            }
        }

        public void ResumeMusic()
        {
            if (musicInstance.isValid())
            {
                musicInstance.setPaused(false);
            }
        }

        public void SetMusicParameter(string parameterName, float value)
        {
            if (musicInstance.isValid())
            {
                musicInstance.setParameterByName(parameterName, value);
            }
        }

        // ===== SFX METHODS =====
        public void PlaySFX(string sfxKey, Vector3 position = default)
        {
            if (sfxDatabase == null)
            {
                Debug.LogError("SFX Database is not assigned!");
                return;
            }

            EventReference eventRef = sfxDatabase.GetEventReference(sfxKey);
            if (eventRef.IsNull)
            {
                Debug.LogWarning($"SFX key '{sfxKey}' not found in database!");
                return;
            }

            if (position == default)
            {
                RuntimeManager.PlayOneShot(eventRef);
            }
            else
            {
                RuntimeManager.PlayOneShot(eventRef, position);
            }
        }

        public EventInstance PlaySFXInstance(string sfxKey, Vector3 position = default)
        {
            if (sfxDatabase == null)
            {
                Debug.LogError("SFX Database is not assigned!");
                return default;
            }

            EventReference eventRef = sfxDatabase.GetEventReference(sfxKey);
            if (eventRef.IsNull)
            {
                Debug.LogWarning($"SFX key '{sfxKey}' not found in database!");
                return default;
            }

            EventInstance instance = RuntimeManager.CreateInstance(eventRef);

            if (position != default)
            {
                instance.set3DAttributes(RuntimeUtils.To3DAttributes(position));
            }

            instance.start();
            return instance;
        }

        // Play SFX by direct EventReference (bypass database)
        public void PlaySFXDirect(EventReference eventRef, Vector3 position = default)
        {
            if (position == default)
            {
                RuntimeManager.PlayOneShot(eventRef);
            }
            else
            {
                RuntimeManager.PlayOneShot(eventRef, position);
            }
        }

        // ===== VOLUME CONTROL =====
        public void SetMasterVolume(float volume)
        {
            masterVolume = Mathf.Clamp01(volume);
            UpdateVolumes();
        }

        public void SetMusicVolume(float volume)
        {
            musicVolume = Mathf.Clamp01(volume);
            UpdateVolumes();
        }

        public void SetSFXVolume(float volume)
        {
            sfxVolume = Mathf.Clamp01(volume);
            UpdateVolumes();
        }

        private void UpdateVolumes()
        {
            if (masterBus.isValid())
                masterBus.setVolume(masterVolume);

            if (musicBus.isValid())
                musicBus.setVolume(musicVolume);

            if (sfxBus.isValid())
                sfxBus.setVolume(sfxVolume);
        }

        // ===== CLEANUP =====
        void OnDestroy()
        {
            StopMusic(false);

            // Unload banks
            foreach (string bankName in bankNames)
            {
                try
                {
                    RuntimeManager.UnloadBank(bankName);
                }
                catch (System.Exception e)
                {
                    Debug.LogError($"Failed to unload bank {bankName}: {e.Message}");
                }
            }
        }
    }
}