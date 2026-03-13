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
    // Стартовый герой разблокирован по умолчанию
    public List<string> unlockedHeroIds = new List<string> { "hero_red" };

    // ─── Кафе ───────────────────────────────────────────────
    // Словарь: название апгрейда → текущий уровень (0 = не куплен)
    // Пример: { "tables": 1, "kitchen": 2 }
    public SerializableDictionary<string, int> cafeUpgrades = new SerializableDictionary<string, int>();

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
}
