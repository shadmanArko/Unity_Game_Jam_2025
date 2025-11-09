using System.Threading.Tasks;
using Life;
using UnityEngine;
using Utilities;

namespace InteractableSystem
{
    public class FridgeInteractable : Interactable
    {
        [SerializeField] private SpriteRenderer spriteRend;
        [SerializeField] private Sprite fridgeOpenSprite;
        [SerializeField] private Sprite fridgeCloseSprite;
        
        private void OnTriggerEnter2D(Collider2D other)
        {
            if(!other.gameObject.CompareTag("Player")) return;
            if(!isInteractable) return;
            ShowInteractPopUp();
        }
        
        private void OnTriggerExit2D(Collider2D other)
        {
            if(!other.gameObject.CompareTag("Player")) return;
            HideInteractPopUp();
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
            spriteRend.sprite = fridgeOpenSprite;
            player.DisableMovement();
            await FillSliderOverTimeAsync(timeToFinishTask);
            OnInteractionComplete();
            player.EnableMovement();
            await Task.Delay(500);
            spriteRend.sprite = fridgeCloseSprite;
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