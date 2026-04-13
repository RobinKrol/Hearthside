using System;
using UnityEngine;

public static class ShopManager
{
    /// <summary>
    /// Проверяет, накопил ли магазин золото (закончился ли таймер).
    /// </summary>
    public static bool IsShopReadyToCollect(string shopId, ShopDatabase db)
    {
        var data = SaveManager.Instance.Data;
        if (!data.shopStates.ContainsKey(shopId)) return false;
        
        var state = data.shopStates[shopId];
        if (state.level == 0 || state.timerStartTicks == 0) return false;

        var shopDef = db.GetShop(shopId);
        if (shopDef == null) return false;

        // Если уровень в сохранке (1-й = индекс 0)
        int levelIndex = Mathf.Clamp(state.level - 1, 0, shopDef.levelConfigs.Count - 1);
        float requiredSeconds = shopDef.levelConfigs[levelIndex].timerSeconds;

        TimeSpan passed = DateTime.UtcNow - new DateTime(state.timerStartTicks);
        return passed.TotalSeconds >= requiredSeconds;
    }

    /// <summary>
    /// Возвращает оставшееся время до сбора в секундах (или 0, если готово/не запущен)
    /// </summary>
    public static float GetRemainingSeconds(string shopId, ShopDatabase db)
    {
        var data = SaveManager.Instance.Data;
        if (!data.shopStates.ContainsKey(shopId)) return 0f;
        
        var state = data.shopStates[shopId];
        if (state.level == 0 || state.timerStartTicks == 0) return 0f;

        var shopDef = db.GetShop(shopId);
        if (shopDef == null) return 0f;

        int levelIndex = Mathf.Clamp(state.level - 1, 0, shopDef.levelConfigs.Count - 1);
        float requiredSeconds = shopDef.levelConfigs[levelIndex].timerSeconds;

        TimeSpan passed = DateTime.UtcNow - new DateTime(state.timerStartTicks);
        float remaining = requiredSeconds - (float)passed.TotalSeconds;

        return Mathf.Max(0f, remaining);
    }

    /// <summary>
    /// Запускает магазин (повышает левел при открытии/апгрейде и сбрасывает таймер). 
    /// Списывает напитки. Возвращает false если не хватает напитков.
    /// </summary>
    public static bool TryStartShop(string shopId, ShopDatabase db)
    {
        var data = SaveManager.Instance.Data;
        var shopDef = db.GetShop(shopId);
        if (shopDef == null) return false;

        if (!data.shopStates.ContainsKey(shopId))
        {
            data.shopStates[shopId] = new ShopSaveData();
        }

        var state = data.shopStates[shopId];
        
        // Уровень, который пытаемся запустить/прокачать (если level 0, то индекс 0)
        int nextLevelIndex = state.level; 
        
        // Ограничение по макс настроенному левелу
        if (nextLevelIndex >= shopDef.levelConfigs.Count)
        {
             // Если это повторный запуск на максимальном уровне
             nextLevelIndex = shopDef.levelConfigs.Count - 1;
        }

        int cost = shopDef.levelConfigs[nextLevelIndex].drinkCost;
        string colorStr = shopDef.requiredDrinkColor.ToString();

        // Пытаемся забрать заказанные напитки
        if (data.SpendDrinks(colorStr, cost))
        {
            // Увеличиваем уровень, только если это не повторный запуск на макс. левеле
            if (state.level <= nextLevelIndex) 
            {
                state.level++; 
            }

            // Запускаем реальный таймер
            state.timerStartTicks = DateTime.UtcNow.Ticks;
            SaveManager.Instance.Save();
            return true;
        }

        return false;
    }

    /// <summary>
    /// Собирает монетки, если таймер окончен.
    /// </summary>
    public static bool TryCollectReward(string shopId, ShopDatabase db)
    {
        if (!IsShopReadyToCollect(shopId, db)) return false;

        var data = SaveManager.Instance.Data;
        var state = data.shopStates[shopId];
        var shopDef = db.GetShop(shopId);
        
        int levelIndex = Mathf.Clamp(state.level - 1, 0, shopDef.levelConfigs.Count - 1);
        int reward = shopDef.levelConfigs[levelIndex].rewardCoins;

        // Выдаем золото
        data.AddCoins(reward);
        
        // Сбрасываем таймер — магазин простаивает до следующей загрузки напитками
        state.timerStartTicks = 0; 
        
        SaveManager.Instance.Save();
        return true;
    }
}
