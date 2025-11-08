using System;
using GameManager;
using MainMenuSystem;
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
        
        
        
        
        private void Start()
        {
            if (instance == null)
                instance = this;
        }
    }
}