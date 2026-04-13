using UnityEngine;
using System.Collections.Generic;
using Enums;

[System.Serializable]
public class ShopLevelConfig
{
    [Tooltip("Сколько напитков нужно собрать на уровнях для старта/апгрейда этого уровня")]
    public int drinkCost = 3;
    
    [Tooltip("Время таймера (в секундах). Например, 600 = 10 минут")]
    public float timerSeconds = 600f;
    
    [Tooltip("Сколько золота принесет магазин, когда таймер истечет")]
    public int rewardCoins = 50;
}

[System.Serializable]
public class ShopDefinition
{
    public string shopId = "shop_red";
    public string shopName = "Красный Магазин";
    public Gem.GemColor requiredDrinkColor = Gem.GemColor.Red;
    public Sprite shopIcon;
    
    [Tooltip("Настройки для разных уровней. Индекс 0 = 1 уровень.")]
    public List<ShopLevelConfig> levelConfigs;
}

[CreateAssetMenu(fileName = "ShopDatabase", menuName = "Hearthside/Shop Database")]
public class ShopDatabase : ScriptableObject
{
    public List<ShopDefinition> shops;

    public ShopDefinition GetShop(string id)
    {
        return shops.Find(s => s.shopId == id);
    }
}
