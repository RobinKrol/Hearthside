using System.Collections.Generic;
using UnityEngine;

public class HeroManager : MonoBehaviour
{
    [Header("Список героев на сцене")]
    public List<Hero> activeHeroes = new List<Hero>();

    [Header("Настройки Баланса")]
    public int baseEnergyPerGem = 5; // Сколько энергии дает 1 кристалл подходящего цвета

    /// <summary>
    /// Начисляет энергию всем активным героям указанного цвета.
    /// </summary>
    public void AddEnergyToColor(Gem.GemColor color, int gemCount, float comboMultiplier = 1f)
    {
        // Вычисляем итоговый прирост энергии по формуле баланса
        int totalEnergy = Mathf.RoundToInt(gemCount * baseEnergyPerGem * comboMultiplier);

        foreach (Hero hero in activeHeroes)
        {
            if (hero.heroColor == color)
            {
                hero.AddEnergy(totalEnergy);
                
                // Обновляем визуал интерфейса у героя
                HeroUI ui = hero.GetComponent<HeroUI>();
                if (ui != null)
                {
                    ui.UpdateUI();
                }

                Debug.Log($"[HeroManager] Герой стихии {color} получил {totalEnergy} энергии! Теперь: {hero.currentEnergy} / {hero.maxEnergy}");
            }
        }
    }
}
