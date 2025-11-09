using System.Collections.Generic;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    // public static AudioManager Instance;
    //
    // [Header("Audio Data")]
    // public AudioClipScriptableObject audioData;
    //
    // [Header("Audio Sources")]
    // private AudioSource musicSource;
    // private AudioSource sfxSource;
    //
    // [Header("Settings")]
    // [Range(0f, 1f)] public float musicVolume = 0.7f;
    // [Range(0f, 1f)] public float sfxVolume = 1f;
    //
    // void Awake()
    // {
    //     // Singleton pattern
    //     if (Instance == null)
    //     {
    //         Instance = this;
    //         DontDestroyOnLoad(gameObject);
    //         SetupAudioSources();
    //     }
    //     else
    //     {
    //         Destroy(gameObject);
    //     }
    // }
    //
    // void SetupAudioSources()
    // {
    //     // Create music source
    //     musicSource = gameObject.AddComponent<AudioSource>();
    //     musicSource.loop = true;
    //     musicSource.volume = musicVolume;
    //     
    //     // Create sfx source
    //     sfxSource = gameObject.AddComponent<AudioSource>();
    //     sfxSource.volume = sfxVolume;
    // }
    //
    // // Play background music
    // public void PlayMusic(string key)
    // {
    //     AudioClip clip = audioData.GetAudioClip(key);
    //     if (clip != null)
    //     {
    //         musicSource.clip = clip;
    //         musicSource.Play();
    //     }
    // }
    //
    // // Stop background music
    // public void StopMusic()
    // {
    //     musicSource.Stop();
    // }
    //
    // // Pause background music
    // public void PauseMusic()
    // {
    //     musicSource.Pause();
    // }
    //
    // // Resume background music
    // public void ResumeMusic()
    // {
    //     musicSource.UnPause();
    // }
    //
    // // Play sound effect
    // public void PlaySFX(string key)
    // {
    //     AudioClip clip = audioData.GetAudioClip(key);
    //     if (clip != null)
    //     {
    //         sfxSource.PlayOneShot(clip);
    //     }
    // }
    //
    // // Set music volume
    // public void SetMusicVolume(float volume)
    // {
    //     musicVolume = Mathf.Clamp01(volume);
    //     musicSource.volume = musicVolume;
    // }
    //
    // // Set SFX volume
    // public void SetSFXVolume(float volume)
    // {
    //     sfxVolume = Mathf.Clamp01(volume);
    //     sfxSource.volume = sfxVolume;
    // }

    public static AudioManager Instance;

    [Header("Audio Data")] public AudioClipScriptableObject audioData;

    [Header("Audio Sources")] private AudioSource musicSource;
    private AudioSource sfxSource;

    [Header("SFX Pool")] [SerializeField] private int sfxPoolSize = 5;
    private List<AudioSource> sfxPool = new List<AudioSource>();

    [Header("Settings")] [Range(0f, 1f)] public float musicVolume = 0.7f;
    [Range(0f, 1f)] public float sfxVolume = 1f;

    void Awake()
    {
        // Singleton pattern
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            SetupAudioSources();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void SetupAudioSources()
    {
        // Create music source
        musicSource = gameObject.AddComponent<AudioSource>();
        musicSource.loop = true;
        musicSource.volume = musicVolume;

        // Create sfx source (for simple one-shots)
        sfxSource = gameObject.AddComponent<AudioSource>();
        sfxSource.volume = sfxVolume;

        // Create SFX pool for multiple simultaneous sounds
        for (int i = 0; i < sfxPoolSize; i++)
        {
            AudioSource source = gameObject.AddComponent<AudioSource>();
            source.volume = sfxVolume;
            sfxPool.Add(source);
        }
    }

    // Play background music
    public void PlayMusic(string key)
    {
        AudioClip clip = audioData.GetAudioClip(key);
        if (clip != null)
        {
            musicSource.clip = clip;
            musicSource.Play();
        }
    }

    // Stop background music
    public void StopMusic()
    {
        musicSource.Stop();
    }

    // Pause background music
    public void PauseMusic()
    {
        musicSource.Pause();
    }

    // Resume background music
    public void ResumeMusic()
    {
        musicSource.UnPause();
    }

    // Play sound effect (simple one-shot)
    public void PlaySFX(string key)
    {
        AudioClip clip = audioData.GetAudioClip(key);
        if (clip != null)
        {
            sfxSource.PlayOneShot(clip);
        }
    }

    // Play SFX with control (returns AudioSource for manual control)
    public AudioSource PlaySFXWithControl(string key)
    {
        AudioClip clip = audioData.GetAudioClip(key);
        if (clip != null)
        {
            AudioSource availableSource = GetAvailableSFXSource();
            if (availableSource != null)
            {
                availableSource.clip = clip;
                availableSource.Play();
                return availableSource;
            }
        }

        return null;
    }

    // Get available SFX source from pool
    private AudioSource GetAvailableSFXSource()
    {
        foreach (var source in sfxPool)
        {
            if (!source.isPlaying)
            {
                return source;
            }
        }

        // If all busy, use the first one (will interrupt)
        return sfxPool[0];
    }

    // Stop specific SFX by key (stops all instances of that sound)
    public void StopSFX(string key)
    {
        AudioClip clip = audioData.GetAudioClip(key);
        if (clip != null)
        {
            foreach (var source in sfxPool)
            {
                if (source.clip == clip && source.isPlaying)
                {
                    source.Stop();
                }
            }
        }
    }

    // Stop specific AudioSource
    public void StopSFXSource(AudioSource source)
    {
        if (source != null)
        {
            source.Stop();
        }
    }

    // Stop ALL SFX
    public void StopAllSFX()
    {
        sfxSource.Stop();
        foreach (var source in sfxPool)
        {
            source.Stop();
        }
    }

    // Set music volume
    public void SetMusicVolume(float volume)
    {
        musicVolume = Mathf.Clamp01(volume);
        musicSource.volume = musicVolume;
    }

    // Set SFX volume
    public void SetSFXVolume(float volume)
    {
        sfxVolume = Mathf.Clamp01(volume);
        sfxSource.volume = sfxVolume;

        foreach (var source in sfxPool)
        {
            source.volume = sfxVolume;
        }
    }
}



// ===== HOW TO USE =====
//
// 1. SETUP:
//    - Create empty GameObject, name it "AudioManager"
//    - Add AudioManager script to it
//    - Drag your AudioClipScriptableObject into "Audio Data" field
//
// 2. PLAY FROM ANY SCRIPT:
//
//    void Start()
//    {
//        // Play background music
//        AudioManager.Instance.PlayMusic("background");
//    }
//
//    void OnJump()
//    {
//        // Play sound effect
//        AudioManager.Instance.PlaySFX("jump");
//    }
//
//    void OnButtonClick()
//    {
//        AudioManager.Instance.PlaySFX("click");
//    }
//
// 3. CONTROL MUSIC:
//
//    AudioManager.Instance.StopMusic();
//    AudioManager.Instance.PauseMusic();
//    AudioManager.Instance.ResumeMusic();
//
// 4. CONTROL VOLUME:
//
//    AudioManager.Instance.SetMusicVolume(0.5f);
//    AudioManager.Instance.SetSFXVolume(0.8f);