using UnityEngine;
using UnityEngine.UI;
using Utilities;

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
            GameReference.instance.currencyScriptable.time += 5;
        }

        public void OnClickScreen()
        {
            StartGame();
        }

        private void StartGame()
        {
            
        }
    }
}
