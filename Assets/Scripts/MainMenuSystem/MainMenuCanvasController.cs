using System;
using UnityEngine;
using UnityEngine.UI;

namespace MainMenuSystem
{
    public class MainMenuCanvasController : MonoBehaviour
    {
        private Button snoozeButton;

        private void Start()
        {
            snoozeButton.onClick.AddListener(OnPressSnoozeButton);
        }

        public void OnPressSnoozeButton()
        {
            
        }
    }
}
