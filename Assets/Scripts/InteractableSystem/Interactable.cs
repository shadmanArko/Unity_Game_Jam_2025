using System.Threading.Tasks;
using Life;
using UnityEngine;
using UnityEngine.UI;

namespace InteractableSystem
{
    public abstract class Interactable : MonoBehaviour, IInteractable
    {
        public GameObject interactablePopUp;
        public GameObject sliderCanvas;
        public bool isInteractable;
        public bool isShowingPopUp;
        
        public float timeToFinishTask;
        public Slider timeToFinishSlider;
        
        [Header("Values to Change")] 
        public int energyChange;
        public int timeChange;
        public int moneyChange;

        public virtual void Start()
        {
            IsInteractable = isInteractable;
            timeChange = -15;
        }
        
        public virtual void ShowInteractPopUp()
        {
            interactablePopUp.SetActive(true);
        }
        
        public virtual void HideInteractPopUp()
        {
            interactablePopUp.SetActive(false);
        }

        public bool IsInteractable { get; set; }
        public virtual async void Interact(SimplePlayerController player)
        {
            
        }

        public virtual void OnInteractionComplete()
        {
            
        }
        
        protected async Task FillSliderOverTimeAsync(float duration)
        {
            sliderCanvas.SetActive(true);
            timeToFinishSlider.value = 0f;
            timeToFinishSlider.maxValue = 1f;

            var startTime = Time.time;
            var endTime = startTime + duration;

            while (Time.time < endTime)
            {
                var progress = Mathf.InverseLerp(startTime, endTime, Time.time);
                timeToFinishSlider.value = progress;
                await Task.Yield();
            }
            
            timeToFinishSlider.value = 1f;
            sliderCanvas.SetActive(false);
        }

    }
}