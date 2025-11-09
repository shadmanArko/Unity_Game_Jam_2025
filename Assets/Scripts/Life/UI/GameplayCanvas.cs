using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Utilities;

public class GameplayCanvas : MonoBehaviour
{
   public Image energyBarFill;
   public TextMeshProUGUI moneyText;
   public Button smallMobileButton;
   public Image bigMobileScreen;
   public Button callCarButton;
   public Button callBikeButton;
   private bool isMobileScreenActive = false;
   private void OnEnable()
   {
      Actions.OnEnergyChangedTo += UpdateEnergyBar;
      Actions.OnMoneyChangedTo += UpdateMoneyText;
      smallMobileButton.onClick.AddListener(ToggleMobileScreen);
      callBikeButton.onClick.AddListener(OnBikeCallButtonClicked);
      callCarButton.onClick.AddListener(OnCarCallButtonClicked);
   }

   private void OnBikeCallButtonClicked()
   {
      Actions.OnBikeCalledAction?.Invoke();
   }

   private void OnCarCallButtonClicked()
   {
      Actions.OnCarCalledAction?.Invoke();
   }

   private void ToggleMobileScreen()
   {
      isMobileScreenActive = !isMobileScreenActive;
      bigMobileScreen.gameObject.SetActive(isMobileScreenActive);
   }

   private void UpdateMoneyText(int obj)
   {
      moneyText.text = obj.ToString()+"$";
   }

   private void UpdateEnergyBar(int amount)
   {
      energyBarFill.fillAmount = amount / 100f;
   }

   private void OnDisable()
   {
      Actions.OnEnergyChangedTo -= UpdateEnergyBar;
      Actions.OnMoneyChangedTo -= UpdateMoneyText;
   }
}
