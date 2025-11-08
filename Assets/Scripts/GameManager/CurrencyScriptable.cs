using UnityEngine;

namespace GameManager
{
    [CreateAssetMenu(fileName = "CurrencyScriptable", menuName = "Scriptable/CurrencyScriptable")]
    public class CurrencyScriptable : ScriptableObject
    {
        public int time; // 120 seconds that is later converted into time convention
        public int money;
        public int energy;

        public void Initialize(int newMoney, int newEnergy, int newTime = 120)
        {
            time = newTime;
            money = newMoney;
            energy = newEnergy;
        }
        
    }
}