using System.Threading.Tasks;
using InteractableSystem;
using UnityEngine;
using UnityEngine.SceneManagement;
using Utilities;

namespace GameManager
{
    public class GameLoopManager : MonoBehaviour
    {
        [SerializeField] private Transform spawnPosition;

        [Header("Extras")] [SerializeField] private BedInteractable bed;

        private void Awake()
        {
            Actions.OnStartTimeAction += PlayerWakeUp;
            Actions.OnStopTimeAction += TimeRunsOut;
        }

        private void PlayerWakeUp()
        {
            var player = GameReference.instance.playerController;
            player.transform.position = spawnPosition.position;
            player.EnableObject();
            player.canMove = true;
            GameReference.instance.cameraController.SetPlayer(player.gameObject.transform);
            bed.spriteRenderer.sprite = bed.wakeUpSprite;
        }

        private async void TimeRunsOut()
        {
            GameReference.instance.playerController.canMove = false;
            FadeManager.Instance.gameObject.SetActive(true);
            StartCoroutine(FadeManager.Instance.FadeIn());
            await Task.Delay(2000);
            GameReference.instance.notificationCanvasController
                .ShowNotification("You're late! \n Do better tomorrow.");
            await Task.Delay(3000);
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }

        private void OnDestroy()
        {
            Actions.OnStartTimeAction -= PlayerWakeUp;
            Actions.OnStopTimeAction -= TimeRunsOut;
        }
    }
}