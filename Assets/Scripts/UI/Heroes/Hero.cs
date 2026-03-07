using UnityEngine;

public class Hero : MonoBehaviour
{
    [Header("Настройки Героя")]
    public Gem.GemColor heroColor; // Цвет стихии героя, с которым он связан
    public int maxEnergy = 60;     // Максимальный запас энергии для полного бара

    [Header("Текущее состояние")]
    public int currentEnergy = 0;  // Текущее количество накопленной энергии

    /// <summary>
    /// Добавляет энергию герою и ограничивает её до максимального значения.
    /// Возвращает true, если герой заряжен на 100%.
    /// </summary>
    public bool AddEnergy(int amount)
    {
        currentEnergy += amount;
        
        if (currentEnergy > maxEnergy)
        {
            currentEnergy = maxEnergy;
        }

        return currentEnergy >= maxEnergy;
    }
}
