using FMOD.Studio;
using SoundSystem;
using UnityEngine;
using UnityEngine.UI;
using Utilities;

namespace MainMenuSystem
{
    public class MainMenuCanvasController : MonoBehaviour
    {
        [SerializeField] private Button snoozeButton;

        private EventInstance snoozeSfx;
        private void Start()
        {
            snoozeButton.onClick.AddListener(OnPressSnoozeButton);
            snoozeSfx = SoundManager.Instance.PlaySfx($"alarm", transform.position);
        }

        private void OnPressSnoozeButton()
        {
            Actions.OnDecreaseTimeAction.Invoke(5);
        }
        

        public void OnClickScreen()
        {
            StartGame();
        }

        private void StartGame()
        {
            gameObject.SetActive(false);
            Actions.OnStartTimeAction.Invoke();
            SoundManager.Instance.StopSfxByName("alarm");
            SoundManager.Instance.PlaySfx($"Music", transform.position);
        }
    }
}
