using UnityEngine;

public static class HeroUpgradeManager
{
    /// <summary>
    /// Возвращает текущий уровень героя у игрока (начинается с 1).
    /// </summary>
    public static int GetCurrentLevel(string heroId)
    {
        var data = SaveManager.Instance.Data;
        if (data.heroLevels.ContainsKey(heroId))
        {
            return data.heroLevels[heroId];
        }
        return 1; // Уровень по умолчанию — 1
    }

    /// <summary>
    /// Пытается повысить уровень героя, списывая монеты.
    /// Возвращает true в случае успеха.
    /// </summary>
    public static bool TryUpgradeHero(string heroId, HeroDatabase db)
    {
        var heroDef = db.GetHero(heroId);
        if (heroDef == null) return false;

        int currentLevel = GetCurrentLevel(heroId);
        
        // Проверяем, не достигнут ли максимальный уровень
        if (currentLevel >= heroDef.levels.Count)
        {
            return false; // Уже максимальный уровень
        }

        // Берем стоимость следующего уровня. 
        // Так как уровни начинаются с 1, а индексы в списке с 0:
        // currentLevel = 1 означает что мы переходим к индексу 1 (то есть 2 уровню).
        int nextLevelIndex = currentLevel; 
        int cost = heroDef.levels[nextLevelIndex].upgradeCostCoins;

        if (SaveManager.Instance.Data.SpendCoins(cost))
        {
            // Повышаем уровень в профиле
            SaveManager.Instance.Data.heroLevels[heroId] = currentLevel + 1;
            SaveManager.Instance.Save();
            return true;
        }

        return false; // Не хватило золота
    }

    /// <summary>
    /// Возвращает текущую статистику героя на основе его реального уровня.
    /// </summary>
    public static HeroLevelStats GetCurrentHeroStats(string heroId, HeroDatabase db)
    {
        var heroDef = db.GetHero(heroId);
        if (heroDef == null) return null;

        int currentLevel = GetCurrentLevel(heroId);
        int index = Mathf.Clamp(currentLevel - 1, 0, heroDef.levels.Count - 1);
        
        return heroDef.levels[index];
    }
}
