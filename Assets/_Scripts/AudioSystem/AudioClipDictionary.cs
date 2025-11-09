using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class AudioClipDictionary
{
    [SerializeField]
    private List<string> keys = new List<string>();
    
    [SerializeField]
    private List<AudioClip> values = new List<AudioClip>();
    
    private Dictionary<string, AudioClip> dictionary;
    
    public Dictionary<string, AudioClip> GetDictionary()
    {
        if (dictionary == null)
        {
            dictionary = new Dictionary<string, AudioClip>();
            for (int i = 0; i < Mathf.Min(keys.Count, values.Count); i++)
            {
                if (!string.IsNullOrEmpty(keys[i]))
                {
                    dictionary[keys[i]] = values[i];
                }
            }
        }
        return dictionary;
    }
    
    public AudioClip GetValue(string key)
    {
        return GetDictionary().ContainsKey(key) ? GetDictionary()[key] : null;
    }
    
    public void OnBeforeSerialize()
    {
        // Optional: sync dictionary back to lists if modified at runtime
    }
    
    public void OnAfterDeserialize()
    {
        dictionary = null; // Clear cache
    }
    
    // Editor helper methods
    public List<string> GetKeys() => keys;
    public List<AudioClip> GetValues() => values;
    
    public void AddEntry(string key, AudioClip value)
    {
        keys.Add(key);
        values.Add(value);
        dictionary = null;
    }
    
    public void RemoveEntry(int index)
    {
        if (index >= 0 && index < keys.Count)
        {
            keys.RemoveAt(index);
            values.RemoveAt(index);
            dictionary = null;
        }
    }
    
    public void UpdateEntry(int index, string key, AudioClip value)
    {
        if (index >= 0 && index < keys.Count)
        {
            keys[index] = key;
            values[index] = value;
            dictionary = null;
        }
    }
}