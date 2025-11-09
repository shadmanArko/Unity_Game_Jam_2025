using System.Threading.Tasks;
using InteractableSystem;
using Life;
using UnityEngine;
using Utilities;

public class Tong : Interactable
{

    public override void Start()
    {
        base.Start();
        IsInteractable = true;
        timeToFinishTask = 3f;

        moneyChange = 50;
        energyChange = 30;
        timeChange = -15;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.gameObject.CompareTag("Player")) return;
        if (!isInteractable) return;
        ShowInteractPopUp();
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (!other.gameObject.CompareTag("Player")) return;
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
        if (!IsInteractable) return;

        player.DisableMovement();
        await FillSliderOverTimeAsync(timeToFinishTask);
        OnInteractionComplete();
        await Task.Delay(500);
        player.EnableMovement();
        // food.SetActive(false);
        HideInteractPopUp();
    }

    public override void OnInteractionComplete()
    {
        if (energyChange > 0)
            Actions.OnIncreaseEnergyAction?.Invoke(energyChange);
        else if (energyChange < 0)
            Actions.OnDecreaseEnergyAction?.Invoke(energyChange);

       
        Actions.OnDecreaseMoneyAction?.Invoke(moneyChange);

        if (timeChange < 0)
            Actions.OnIncreaseTimeAction?.Invoke(timeChange);
        else if (timeChange > 0)
            Actions.OnDecreaseTimeAction?.Invoke(timeChange);
    }
}


