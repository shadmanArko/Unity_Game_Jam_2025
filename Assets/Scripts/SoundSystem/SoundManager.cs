using System.Collections;
using System.Collections.Generic;
using System.Linq;
using FMOD.Studio;
using FMODUnity;
using UnityEngine;
using STOP_MODE = FMOD.Studio.STOP_MODE;

namespace SoundSystem
{
    public class SoundManager : MonoBehaviour
    {
        // public static SoundManager Instance { get; private set; }
        //
        // [Header("FMOD Banks")] [SerializeField]
        // private List<string> bankNames = new() { "Master", "Music", "SFX" };
        //
        // [Header("SFX Database")] [SerializeField]
        // private SfxDatabase sfxDatabase;
        //
        // [SerializeField] private SfxDatabase musicDatabase;
        //
        // [Header("Music Settings")] [SerializeField]
        // private EventReference currentMusicEvent;
        //
        // private EventInstance musicInstance;
        //
        // [Header("Audio Settings")] [Range(0f, 1f)]
        // public float masterVolume = 1f;
        //
        // [Range(0f, 1f)] public float musicVolume = 1f;
        // [Range(0f, 1f)] public float sfxVolume = 1f;
        //
        // private Bus masterBus;
        // private Bus musicBus;
        // private Bus sfxBus;
        //
        // private Dictionary<string, EventInstance> activeInstances = new Dictionary<string, EventInstance>();
        //
        // void Awake()
        // {
        //     // Singleton pattern
        //     if (Instance == null)
        //     {
        //         Instance = this;
        //         DontDestroyOnLoad(gameObject);
        //         //InitializeAudio();
        //     }
        //     else
        //     {
        //         Destroy(gameObject);
        //     }
        // }
        //
        // #region New Code
        //
        // public void PlayOneShot(string eventName, Vector2 worldPos)
        // {
        //     RuntimeManager.PlayOneShot(FetchSfxEventReference(eventName), worldPos);
        // }
        //
        // private EventReference FetchSfxEventReference(string eventName)
        // {
        //     var sfxData = sfxDatabase.sfxList.FirstOrDefault(
        //         sfxData => sfxData.key == eventName);
        //     if (sfxData != null) return sfxData.eventReference;
        //     
        //     Debug.LogError($"Fatal Error: Sfx event {eventName} not found");
        //     return new EventReference();
        // }
        //
        // #endregion
        //
        // private EventInstance _musicInstance;
        // public void PlaySoundTrack(string eventName)
        // {
        //     var eventReference = FetchSoundTrackEventReference(eventName);
        //     _musicInstance = RuntimeManager.CreateInstance(eventReference);
        //     _musicInstance.start();
        // }
        //
        // public void StopSoundTrack(string eventName)
        // {
        //     _musicInstance.stop(STOP_MODE.ALLOWFADEOUT);
        //     Debug.Log($"Sound track stopped: {eventName})");
        //     _musicInstance.release();
        // }
        //
        // private EventReference FetchSoundTrackEventReference(string eventName)
        // {
        //     var sfxData = musicDatabase.sfxList.FirstOrDefault(
        //         sfxData => sfxData.key == eventName);
        //     if (sfxData != null) return sfxData.eventReference;
        //     
        //     Debug.LogError($"Fatal Error: Sfx event {eventName} not found");
        //     return new EventReference();
        // }
        
        public static SoundManager Instance { get; private set; }

    [Header("FMOD Banks")] [SerializeField]
    private List<string> bankNames = new() { "Master", "Music", "SFX" };

    [Header("SFX Database")] [SerializeField]
    private SfxDatabase sfxDatabase;

    [SerializeField] private SfxDatabase musicDatabase;

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

    // Track active SFX instances
    private Dictionary<string, List<EventInstance>> activeSfxInstances = new Dictionary<string, List<EventInstance>>();

    void Awake()
    {
        // Singleton pattern
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            //InitializeAudio();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    #region SFX Methods

    // Play one-shot SFX (fire and forget, can't be stopped)
    public void PlayOneShot(string eventName, Vector2 worldPos)
    {
        RuntimeManager.PlayOneShot(FetchSfxEventReference(eventName), worldPos);
    }
    
    // Play SFX with control (can be stopped later)
    public EventInstance PlaySfx(string eventName, Vector2 worldPos)
    {
        var eventReference = FetchSfxEventReference(eventName);
        EventInstance instance = RuntimeManager.CreateInstance(eventReference);
        
        instance.set3DAttributes(RuntimeUtils.To3DAttributes(worldPos));
        instance.start();
        
        // Track this instance
        if (!activeSfxInstances.ContainsKey(eventName))
        {
            activeSfxInstances[eventName] = new List<EventInstance>();
        }
        activeSfxInstances[eventName].Add(instance);
        
        return instance;
    }
    
    // Stop specific SFX instance
    public void StopSfx(EventInstance instance, bool allowFadeout = true)
    {
        if (!instance.isValid()) return;
        
        instance.stop(allowFadeout ? STOP_MODE.ALLOWFADEOUT : STOP_MODE.IMMEDIATE);
        instance.release();
        
        // Remove from tracking
        RemoveInstanceFromTracking(instance);
    }
    
    // Stop all instances of a specific SFX by name
    public void StopSfxByName(string eventName, bool allowFadeout = true)
    {
        if (!activeSfxInstances.ContainsKey(eventName)) return;
        
        var instances = activeSfxInstances[eventName];
        foreach (var instance in instances.ToList())
        {
            if (instance.isValid())
            {
                instance.stop(allowFadeout ? STOP_MODE.ALLOWFADEOUT : STOP_MODE.IMMEDIATE);
                instance.release();
            }
        }
        
        activeSfxInstances[eventName].Clear();
    }
    
    // Stop ALL SFX
    public void StopAllSfx(bool allowFadeout = true)
    {
        foreach (var kvp in activeSfxInstances.ToList())
        {
            foreach (var instance in kvp.Value.ToList())
            {
                if (instance.isValid())
                {
                    instance.stop(allowFadeout ? STOP_MODE.ALLOWFADEOUT : STOP_MODE.IMMEDIATE);
                    instance.release();
                }
            }
        }
        
        activeSfxInstances.Clear();
    }
    
    // Pause specific SFX instance
    public void PauseSfx(EventInstance instance)
    {
        if (instance.isValid())
        {
            instance.setPaused(true);
        }
    }
    
    // Resume specific SFX instance
    public void ResumeSfx(EventInstance instance)
    {
        if (instance.isValid())
        {
            instance.setPaused(false);
        }
    }
    
    // Check if SFX is still playing
    public bool IsSfxPlaying(EventInstance instance)
    {
        if (!instance.isValid()) return false;
        
        instance.getPlaybackState(out PLAYBACK_STATE state);
        return state != PLAYBACK_STATE.STOPPED;
    }
    
    private void RemoveInstanceFromTracking(EventInstance instance)
    {
        foreach (var kvp in activeSfxInstances.ToList())
        {
            kvp.Value.Remove(instance);
            if (kvp.Value.Count == 0)
            {
                activeSfxInstances.Remove(kvp.Key);
            }
        }
    }
    
    private EventReference FetchSfxEventReference(string eventName)
    {
        var sfxData = sfxDatabase.sfxList.FirstOrDefault(
            sfxData => sfxData.key == eventName);
        if (sfxData != null) return sfxData.eventReference;
        
        Debug.LogError($"Fatal Error: Sfx event {eventName} not found");
        return new EventReference();
    }

    #endregion
    
    #region Music Methods
    
    private EventInstance _musicInstance;
    
    public void PlaySoundTrack(string eventName)
    {
        var eventReference = FetchSoundTrackEventReference(eventName);
        _musicInstance = RuntimeManager.CreateInstance(eventReference);
        _musicInstance.start();
    }

    public void StopSoundTrack(string eventName, bool allowFadeout = true)
    {
        _musicInstance.stop(allowFadeout ? STOP_MODE.ALLOWFADEOUT : STOP_MODE.IMMEDIATE);
        Debug.Log($"Sound track stopped: {eventName}");
        _musicInstance.release();
    }

    private EventReference FetchSoundTrackEventReference(string eventName)
    {
        var sfxData = musicDatabase.sfxList.FirstOrDefault(
            sfxData => sfxData.key == eventName);
        if (sfxData != null) return sfxData.eventReference;
        
        Debug.LogError($"Fatal Error: Music event {eventName} not found");
        return new EventReference();
    }
    
    #endregion
    
    #region Cleanup
    
    void OnDestroy()
    {
        // Stop and release all SFX
        StopAllSfx(false);
        
        // Stop and release music
        if (_musicInstance.isValid())
        {
            _musicInstance.stop(STOP_MODE.IMMEDIATE);
            _musicInstance.release();
        }
    }
    
    #endregion
    }
}