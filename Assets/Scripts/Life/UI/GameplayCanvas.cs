using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Utilities;

public class GameplayCanvas : MonoBehaviour
{
   [SerializeField] private int baseUberCarFair = 300;
   [SerializeField] private int baseUberBikeFair = 100;
   private int currentChunkIndex = 0;
   public Image energyBarFill;
   public TextMeshProUGUI moneyText;
   public Button smallMobileButton;
   public Image bigMobileScreen;
   public Button callCarButton;
   public Button callBikeButton;
   private bool isMobileScreenActive = false;
   private int currentMoney = 0;
   private void OnEnable()
   {
      Actions.OnPlayerEnteredRoadChunkIndex += OnPlayerEnteredRoadChunkIndex;
      Actions.OnEnergyChangedTo += UpdateEnergyBar;
      Actions.OnMoneyChangedTo += UpdateMoneyText;
      smallMobileButton.onClick.AddListener(ToggleMobileScreen);
      callBikeButton.onClick.AddListener(OnBikeCallButtonClicked);
      callCarButton.onClick.AddListener(OnCarCallButtonClicked);
      Actions.OnPlayerDroppedOff += OnPlayerDroppedOff;
   }

   private void OnPlayerDroppedOff(bool isBike)
   {
      Actions.OnDecreaseMoneyAction?.Invoke(isBike? (int)baseUberBikeFair : (int)baseUberCarFair);
   }

   private void OnPlayerEnteredRoadChunkIndex(int obj)
   {
      currentChunkIndex = obj;
   }

   private void OnBikeCallButtonClicked()
   {
      Actions.OnBikeCalledAction?.Invoke();
      ToggleMobileScreen();
   }

   private void OnCarCallButtonClicked()
   {
      Actions.OnCarCalledAction?.Invoke();
      ToggleMobileScreen();
   }

   private void ToggleMobileScreen()
   {
      isMobileScreenActive = !isMobileScreenActive;
      bigMobileScreen.gameObject.SetActive(isMobileScreenActive);
   }

   private void UpdateMoneyText(int obj)
   {
      currentMoney = obj;
      UpdateUberButtons();
      moneyText.text = obj.ToString()+"$";
   }

   private void UpdateUberButtons()
   {
      if (currentMoney >= baseUberCarFair)
      {
         callCarButton.interactable = true;
      }
      else
      {
         callCarButton.interactable = false;
      }
      
   }

   private void UpdateEnergyBar(int amount)
   {
      energyBarFill.fillAmount = amount / 100f;
   }

   private void OnDisable()
   {
      Actions.OnEnergyChangedTo -= UpdateEnergyBar;
      Actions.OnMoneyChangedTo -= UpdateMoneyText;
      Actions.OnPlayerEnteredRoadChunkIndex -= OnPlayerEnteredRoadChunkIndex;
      Actions.OnPlayerDroppedOff += OnPlayerDroppedOff;

   }
}
