using System;
using UnityEngine;
using UnityEngine.UI;
using Utilities;

public class CurrencyManager : MonoBehaviour
{
    public int maxEnergy = 100;
    public int startingEnergy = 60;
    public int startingMoney = 500;
    public int currentEnergy;
    public int currentMoney;
    private void Start()
    {
        currentEnergy = startingEnergy;
        currentMoney = startingMoney;
        OnEnergyChanged();
        OnMoneyChanged();
        Actions.OnIncreaseEnergyAction += OnIncreaseEnergyAction;
        Actions.OnDecreaseEnergyAction += OnDecreaseEnergyAction;
        Actions.OnIncreaseMoneyAction += OnIncreaseMoneyAction;
        Actions.OnDecreaseMoneyAction += OnDecreaseMoneyAction;
    }

    private void OnDecreaseMoneyAction(int amount)
    {
        currentMoney -= amount;
        if (currentMoney <= 0)
        {
            currentMoney = 0;
        }
        OnMoneyChanged();
    }

    private void OnIncreaseMoneyAction(int amount)
    {
        currentMoney += amount;
        OnMoneyChanged();
    }

    private void OnDecreaseEnergyAction(int amount)
    {
        currentEnergy -= amount;
        if (currentEnergy <= 0)
        {
            currentEnergy = 0;
        }
        OnEnergyChanged();
    }

    private void OnIncreaseEnergyAction(int amount)
    {
        currentEnergy += amount;
        OnEnergyChanged();
    }

    private void OnMoneyChanged()
    {
        Actions.OnMoneyChangedTo?.Invoke(currentMoney);
    }

    private void OnEnergyChanged()
    {
        Actions.OnEnergyChangedTo?.Invoke(currentEnergy);
    }

    private void OnDisable()
    {
        Actions.OnIncreaseEnergyAction -= OnIncreaseEnergyAction;
        Actions.OnDecreaseEnergyAction -= OnDecreaseEnergyAction;
        Actions.OnIncreaseMoneyAction -= OnIncreaseMoneyAction;
        Actions.OnDecreaseMoneyAction -= OnDecreaseMoneyAction;
    }
}
