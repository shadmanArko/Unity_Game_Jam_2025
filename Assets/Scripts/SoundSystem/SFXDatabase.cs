using System.Collections.Generic;
using FMODUnity;
using UnityEngine;

namespace SoundSystem
{
    [CreateAssetMenu(fileName = "SFXDatabase", menuName = "Audio/SFX Database")]
    public class SfxDatabase : ScriptableObject
    {
        [System.Serializable]
        public class SFXEntry
        {
            public string key;
            public EventReference eventPath;
        }

        public List<SFXEntry> sfxList = new List<SFXEntry>();
    
        private Dictionary<string, EventReference> sfxDictionary;

        public void Initialize()
        {
            sfxDictionary = new Dictionary<string, EventReference>();
            foreach (var entry in sfxList)
            {
                if (!string.IsNullOrEmpty(entry.key) && !entry.eventPath.IsNull)
                {
                    sfxDictionary[entry.key] = entry.eventPath;
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