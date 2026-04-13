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
            if (hero != null && hero.heroColor == color)
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

    /// <summary>
    /// Возвращает мировые координаты рамки UI нужного героя (для анимации полета фруктов).
    /// Если герой не найден, возвращает нулевой вектор.
    /// </summary>
    public Vector3 GetHeroPosition(Gem.GemColor color)
    {
        foreach (Hero hero in activeHeroes)
        {
            if (hero != null && hero.heroColor == color)
            {
                // Пытаемся получить точную целевую точку из UI компонента
                HeroUI ui = hero.GetComponent<HeroUI>();
                if (ui != null && ui.fruitTargetPoint != null)
                {
                    return ui.fruitTargetPoint.position;
                }

                // Запасной вариант - центр самого героя
                return hero.transform.position;
            }
        }
        return Vector3.zero;
    }

    /// <summary>
    /// Проверяет, есть ли хотя бы один герой с заряженной (100%) ультой.
    /// </summary>
    public bool HasAnyUltimateReady()
    {
        foreach (Hero hero in activeHeroes)
        {
            if (hero != null && hero.currentEnergy >= hero.maxEnergy)
            {
                return true;
            }
        }
        return false;
    }

    public void HideAllHeroes()
    {
        foreach (Hero hero in activeHeroes)
        {
            if (hero != null)
            {
                hero.gameObject.SetActive(false);
            }
        }
    }
}
