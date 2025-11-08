using Life;
using UnityEngine;

namespace InteractableSystem
{
    public class BedInteractable : MonoBehaviour, IInteractable
    {
        [SerializeField] private GameObject interactablePopUp;
        [SerializeField] private bool isInteractable;
        [SerializeField] private bool isShowingPopUp;

        private void Start()
        {
            IsInteractable = isInteractable;
        }
        
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
        
        private void ShowInteractPopUp()
        {
            interactablePopUp.SetActive(true);
        }

        private void HideInteractPopUp()
        {
            interactablePopUp.SetActive(false);
        }
        
        public bool IsInteractable { get; set; }
        public void Interact(SimplePlayerController player)
        {
            if(!IsInteractable) return;
            Debug.LogError($"Is Interacting!!!");
        }
    }
}