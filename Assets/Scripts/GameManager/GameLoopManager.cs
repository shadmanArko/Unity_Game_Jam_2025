using System;
using InteractableSystem;
using UnityEngine;
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

        private void OnDestroy()
        {
            Actions.OnStartTimeAction -= PlayerWakeUp;
        }
    }
}