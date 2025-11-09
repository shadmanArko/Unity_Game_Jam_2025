using UnityEngine;

[CreateAssetMenu(fileName = "AudioClipData", menuName = "ScriptableObjects/AudioClipData")]
public class AudioClipScriptableObject : ScriptableObject
{
    public AudioClipDictionary audioClips = new AudioClipDictionary();
    
    public AudioClip GetAudioClip(string key)
    {
        return audioClips.GetValue(key);
    }
}