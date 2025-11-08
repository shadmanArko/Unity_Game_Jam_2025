using GameManager;
using MainMenuSystem;
using TimeSystem;
using UnityEngine;

namespace Utilities
{
    public class GameReference : MonoBehaviour
    {
        public static GameReference instance;

        
        [Header("Scriptables")]
        public CurrencyScriptable currencyScriptable;
        
        [Header("Controllers")]
        public MainMenuCanvasController mainMenuCanvasController;
        public TimeController timeController;
        
        
        
        
        private void Start()
        {
            if (instance == null)
                instance = this;
        }
    }
}