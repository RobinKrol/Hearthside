using UnityEngine;
using System.Collections.Generic;
using Enums;

public enum HeroRarity
{
    Common,
    Rare,
    Epic,
    Legendary
}

[System.Serializable]
public class HeroLevelStats
{
    [Tooltip("Стоимость (в монетах) повышения героя ДО этого уровня. Для 1 уровня = 0.")]
    public int upgradeCostCoins = 0;
    
    [Tooltip("Запас энергии для активации ульты")]
    public int maxEnergy = 100;
    
    [Tooltip("Скорость (множитель энергии). Например, 1.2 = на 20% больше энергии за каждый кристалл")]
    public float speedMultiplier = 1.0f;
    
    [Tooltip("Шанс начисления дополнительных чаевых. 0.15 = 15%")]
    public float tipChance = 0.0f;
    
    [Tooltip("Описание уникальной пассивки для отображения в UI")]
    public string passiveDescription;
}

[System.Serializable]
public class HeroDefinition
{
    public string heroId = "hero_red";
    public string heroName = "Лисенок";
    public HeroRarity rarity = HeroRarity.Common;
    
    [Header("Игровые настройки (Для Match-3)")]
    public Gem.GemColor heroColor = Gem.GemColor.Red;
    public DrinkSize drinkSize = DrinkSize.Small;
    public Sprite ultimateDrinkSprite;
    
    [Header("Визуал для Хаба")]
    public Sprite heroIcon; // Иконка для отображения в списке
    
    [Header("Прокачка по уровням (Индекс 0 = 1 уровень)")]
    public List<HeroLevelStats> levels;
}

[CreateAssetMenu(fileName = "HeroDatabase", menuName = "Hearthside/Hero Database")]
public class HeroDatabase : ScriptableObject
{
    public List<HeroDefinition> heroes;

    public HeroDefinition GetHero(string id)
    {
        return heroes.Find(h => h.heroId == id);
    }
}
