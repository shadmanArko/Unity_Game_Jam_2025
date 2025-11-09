using System.Threading.Tasks;
using Life;
using UnityEngine;
using Utilities;

namespace InteractableSystem
{
    public class BedInteractable : Interactable
    {
        public Sprite sleepSprite;
        public Sprite wakeUpSprite;
        public SpriteRenderer spriteRenderer;

        public override void Start()
        {
            base.Start();
            IsInteractable = false;

            energyChange = 10;
            moneyChange = 0;
            timeChange = 30;
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if(!other.gameObject.CompareTag("Player")) return;
            if(!IsInteractable) return;
            ShowInteractPopUp();
            IsInteractable = true;
        }
        
        private void OnTriggerExit2D(Collider2D other)
        {
            if(!other.gameObject.CompareTag("Player")) return;
            HideInteractPopUp();
            IsInteractable = true;
        }
        
        public override void ShowInteractPopUp()
        {
            interactablePopUp.SetActive(true);
        }

        public override void HideInteractPopUp()
        {
            interactablePopUp.SetActive(false);
        }
        
        public override async void Interact(SimplePlayerController player)
        {
            if(!IsInteractable) return;
            
            player.DisableMovement();
            player.DisableObject();
            spriteRenderer.sprite = sleepSprite;
            await FillSliderOverTimeAsync(timeToFinishTask);
            OnInteractionComplete();
            player.EnableMovement();
            await Task.Delay(500);
            spriteRenderer.sprite = wakeUpSprite;
            player.EnableMovement();
            player.EnableObject();
        }

        public override void OnInteractionComplete()
        {
            if(energyChange > 0)
                Actions.OnIncreaseEnergyAction?.Invoke(energyChange);
            else if(energyChange < 0)
                Actions.OnDecreaseEnergyAction?.Invoke(energyChange);
            
            if(moneyChange > 0)
                Actions.OnIncreaseMoneyAction?.Invoke(energyChange);
            else if(moneyChange < 0)
                Actions.OnDecreaseMoneyAction?.Invoke(energyChange);
            
            if(timeChange > 0)
                Actions.OnIncreaseTimeAction?.Invoke(energyChange);
            else if(timeChange < 0)
                Actions.OnDecreaseTimeAction?.Invoke(energyChange);
        }
    }
}