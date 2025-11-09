using System.Collections.Generic;
using FMODUnity;
using UnityEngine;
using UnityEngine.Serialization;

namespace SoundSystem
{
    [CreateAssetMenu(fileName = "SFXDatabase", menuName = "Audio/SFX Database")]
    public class SfxDatabase : ScriptableObject
    {
        [System.Serializable]
        public class SFXEntry
        {
            public string key;
            [FormerlySerializedAs("eventPath")] public EventReference eventReference;
        }

        public List<SFXEntry> sfxList = new List<SFXEntry>();
    
        private Dictionary<string, EventReference> sfxDictionary;

        public void Initialize()
        {
            sfxDictionary = new Dictionary<string, EventReference>();
            foreach (var entry in sfxList)
            {
                if (!string.IsNullOrEmpty(entry.key) && !entry.eventReference.IsNull)
                {
                    sfxDictionary[entry.key] = entry.eventReference;
                }
            }
        }

        public EventReference GetEventReference(string key)
        {
            if (sfxDictionary == null) Initialize();
            return sfxDictionary.ContainsKey(key) ? sfxDictionary[key] : default;
        }
    }
}