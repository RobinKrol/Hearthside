using System;
using System.Collections.Generic;

/// <summary>
/// Сериализуемый класс данных игрока.
/// Хранит весь прогресс: уровни, валюту, героев, апгрейды кафе.
/// Доступ: SaveManager.Instance.Data
/// </summary>
[Serializable]
public class PlayerData
{
    // ─── Прогресс ───────────────────────────────────────────
    public int currentLevel = 1;             // Текущий уровень (номер)
    public int highestLevelReached = 1;      // Максимальный достигнутый уровень
    public int totalOrdersFulfilled = 0;     // Всего выполнено заказов за всё время

    // ─── Валюта ─────────────────────────────────────────────
    public int coins = 0;                    // Основная валюта (зарабатывается в игре)
    public int gems = 0;                     // Премиум валюта (на будущее)

    // ─── Герои ──────────────────────────────────────────────
    // ID строкой = "hero_red", "hero_green" и т.д.
    public List<string> unlockedHeroIds = new List<string> { "hero_red" };
    // Уровни героев (ID -> Level)
    public SerializableDictionary<string, int> heroLevels = new SerializableDictionary<string, int>();

    // ─── Ресурсы уровней (Материалы) ────────────────────────
    // Ключ: название цвета (например, "Red", "Green"), Значение: количество
    public SerializableDictionary<string, int> collectedDrinks = new SerializableDictionary<string, int>();

    // ─── Кафе / Магазины ────────────────────────────────────
    public SerializableDictionary<string, int> cafeUpgrades = new SerializableDictionary<string, int>();
    public SerializableDictionary<string, ShopSaveData> shopStates = new SerializableDictionary<string, ShopSaveData>();

    // ─── Вспомогательные методы ─────────────────────────────

    /// <summary>Проверяет, разблокирован ли герой с данным ID.</summary>
    public bool IsHeroUnlocked(string heroId)
    {
        return unlockedHeroIds.Contains(heroId);
    }

    /// <summary>Разблокирует героя (если ещё не разблокирован).</summary>
    public void UnlockHero(string heroId)
    {
        if (!unlockedHeroIds.Contains(heroId))
            unlockedHeroIds.Add(heroId);
    }

    /// <summary>Возвращает уровень апгрейда кафе. 0 = не куплен.</summary>
    public int GetCafeUpgradeLevel(string upgradeId)
    {
        return cafeUpgrades.ContainsKey(upgradeId) ? cafeUpgrades[upgradeId] : 0;
    }

    /// <summary>Повышает уровень апгрейда кафе на 1.</summary>
    public void UpgradeCafe(string upgradeId)
    {
        if (cafeUpgrades.ContainsKey(upgradeId))
            cafeUpgrades[upgradeId]++;
        else
            cafeUpgrades[upgradeId] = 1;
    }

    /// <summary>Добавляет монеты (безопасно, не уходит в минус).</summary>
    public void AddCoins(int amount)
    {
        coins = Math.Max(0, coins + amount);
    }

    /// <summary>Тратит монеты. Возвращает true если хватило.</summary>
    public bool SpendCoins(int amount)
    {
        if (coins < amount) return false;
        coins -= amount;
        return true;
    }

    /// <summary>Сохраняет собранный напиток (с учетом цвета).</summary>
    public void AddDrink(string colorName, int count)
    {
        if (collectedDrinks.ContainsKey(colorName))
            collectedDrinks[colorName] += count;
        else
            collectedDrinks[colorName] = count;
    }

    /// <summary>Возвращает количество накопленных напитков конкретного цвета.</summary>
    public int GetDrinkCount(string colorName)
    {
        return collectedDrinks.ContainsKey(colorName) ? collectedDrinks[colorName] : 0;
    }

    /// <summary>Списывает напитки (для запуска магазина). Возвращает true, если успешно.</summary>
    public bool SpendDrinks(string colorName, int amount)
    {
        int current = GetDrinkCount(colorName);
        if (current < amount) return false;
        collectedDrinks[colorName] = current - amount;
        return true;
    }
}

/// <summary>
/// Данные о состоянии конкретного магазина/здания.
/// </summary>
[Serializable]
public class ShopSaveData
{
    public int level = 0;             // 0 = не куплен
    public long timerStartTicks = 0;  // Время последнего запуска таймера (или 0, если простаивает)
}
