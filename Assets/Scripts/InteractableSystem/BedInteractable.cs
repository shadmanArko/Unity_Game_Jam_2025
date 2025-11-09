using System.Threading.Tasks;
using Life;
using UnityEngine;
using UnityEngine.UI;
using Utilities;

namespace InteractableSystem
{
    public class BedInteractable : Interactable
    {
        // [SerializeField] private GameObject interactablePopUp;
        // [SerializeField] private bool isInteractable;
        // [SerializeField] private bool isShowingPopUp;

        // [SerializeField] private float timeToFinishTask;
        // [SerializeField] private Slider timeToFinishSlider;
        //
        // [Header("Values to Change")] 
        // [SerializeField] private int energyChange;
        // [SerializeField] private int timeChange;
        // [SerializeField] private int moneyChange;
        
        
        private void OnTriggerEnter2D(Collider2D other)
        {
            Debug.LogError($"Entered Game Object Name: {other.gameObject.name}");
            if(!other.gameObject.CompareTag("Player")) return;
            if(!isInteractable) return;
            Debug.LogError($"Entered collider");
            ShowInteractPopUp();
        }
        
        private void OnTriggerExit2D(Collider2D other)
        {
            Debug.LogError($"Exited Game Object Name: {other.gameObject.name}");
            if(!other.gameObject.CompareTag("Player")) return;
            Debug.LogError($"Exited collider");
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
        
        // public bool IsInteractable { get; set; }
        public override async void Interact(SimplePlayerController player)
        {
            if(!IsInteractable) return;
        
            await FillSliderOverTimeAsync(timeToFinishTask);
            OnInteractionComplete();
        }
        
        // private async Task FillSliderOverTimeAsync(float duration)
        // {
        //     timeToFinishSlider.value = 0f;
        //     timeToFinishSlider.maxValue = 1f;
        //
        //     var startTime = Time.time;
        //     var endTime = startTime + duration;
        //
        //     while (Time.time < endTime)
        //     {
        //         var progress = Mathf.InverseLerp(startTime, endTime, Time.time);
        //         timeToFinishSlider.value = progress;
        //         await Task.Yield();
        //     }
        //     
        //     timeToFinishSlider.value = 1f;
        // }

        public override void OnInteractionComplete()
        {
            if(energyChange > 0)
                Actions.OnIncreaseEnergyAction.Invoke(energyChange);
            else if(energyChange < 0)
                Actions.OnDecreaseEnergyAction.Invoke(energyChange);
            
            if(moneyChange > 0)
                Actions.OnIncreaseMoneyAction.Invoke(energyChange);
            else if(moneyChange < 0)
                Actions.OnDecreaseMoneyAction.Invoke(energyChange);
            
            if(timeChange > 0)
                Actions.OnIncreaseTimeAction.Invoke(energyChange);
            else if(timeChange < 0)
                Actions.OnDecreaseTimeAction.Invoke(energyChange);
        }
    }
}