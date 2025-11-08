using UnityEngine;
using UnityEngine.UI;
using Utilities;

namespace MainMenuSystem
{
    public class MainMenuCanvasController : MonoBehaviour
    {
        [SerializeField] private Button snoozeButton;

        private void Start()
        {
            snoozeButton.onClick.AddListener(OnPressSnoozeButton);
        }

        private void OnPressSnoozeButton()
        {
            Actions.OnIncreaseTimeAction.Invoke(5);
        }

        public void OnClickScreen()
        {
            StartGame();
        }

        private void StartGame()
        {
            gameObject.SetActive(false);
            Actions.OnStartTimeAction.Invoke();
        }
    }
}
