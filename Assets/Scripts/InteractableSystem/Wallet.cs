using System.Threading.Tasks;
using Life;
using TMPro;
using UnityEngine;
using Utilities;

namespace InteractableSystem
{
    public class Wallet : Interactable
    {
        private string textToShow = $"Press E to Interact \n Time= -5 \n money= +500";
        [SerializeField] private TMP_Text popUpText;
        public override void Start()
        {
            base.Start();
            IsInteractable = true;
            popUpText.text = textToShow;
            timeToFinishTask = 3;

            energyChange = 0;
            moneyChange = 500;
            timeChange = 5;
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if(!other.gameObject.CompareTag("Player")) return;
            if(!IsInteractable) return;
            ShowInteractPopUp();
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
            IsInteractable = false;
            player.DisableMovement();
            await FillSliderOverTimeAsync(timeToFinishTask);
            OnInteractionComplete();
            await Task.Delay(500);
            player.EnableMovement();
        }

        public override void OnInteractionComplete()
        {
            if(energyChange > 0)
                Actions.OnIncreaseEnergyAction?.Invoke(energyChange);
            else if(energyChange < 0)
                Actions.OnDecreaseEnergyAction?.Invoke(energyChange);

            if (moneyChange > 0)
            {
                Actions.OnIncreaseMoneyAction?.Invoke(moneyChange);
            }
            else if(moneyChange < 0)
                Actions.OnDecreaseMoneyAction?.Invoke(moneyChange);
            
            if(timeChange > 0)
                Actions.OnIncreaseTimeAction?.Invoke(timeChange);
            else if(timeChange < 0)
                Actions.OnDecreaseTimeAction?.Invoke(timeChange);
            gameObject.SetActive(false);
        }
    }
}