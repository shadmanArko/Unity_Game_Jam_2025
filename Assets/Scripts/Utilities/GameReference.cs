using _Scripts.InputSystem;
using GameManager;
using Life;
using MainMenuSystem;
using NotificationSystem;
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
        public SimplePlayerController playerController;
        public SideScrollingCameraController cameraController;
        public NotificationCanvasController notificationCanvasController;
        
        // GameObjects
        [Header("GamObjects")]
        public GameObject gameplayCanvas;
        
        
        private void Start()
        {
            if (instance == null)
                instance = this;
        }
    }
}