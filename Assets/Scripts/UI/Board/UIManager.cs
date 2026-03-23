using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UIManager : MonoBehaviour
{
    [Header("Timer Bar Visuals")]
    public Image timerBarFull;           // Спрайт полоски (Настроить: Image Type = Filled, Fill Method = Horizontal)
    public RectTransform timerStar;      // Спрайт звёздочки-ползунка
    
    [Header("Позиция звёздочки (по оси X)")]
    public float starPositionFull = 100f;  // Позиция X звёздочки, когда время 100%
    public float starPositionEmpty = -100f;// Позиция X звёздочки, когда время 0%

    [Header("Текст ходов")]
    public TextMeshProUGUI turnText; // Ссылка на Текст (TextMeshPro) для счетчика ходов

    /// <summary>
    /// Обновляет текстовое отображение текущего хода
    /// </summary>
    public void UpdateTurnUI(int currentTurn)
    {
        if (turnText != null)
        {
            // Фиксированно 7 ходов. Ограничиваем, чтобы не ушло в 8 при game over
            int maxTurns = 7;
            int displayTurn = Mathf.Min(currentTurn, maxTurns);

            // Выводим просто число текущего хода, без " / 7"
            turnText.text = displayTurn.ToString();
        }
    }

    /// <summary>
    /// Обновляет визуальное отображение часов (timer_bar_full) и позицию звёздочки
    /// </summary>
    public void UpdateTimerUI(float currentTimer, float maxTurnDuration)
    {
        if (timerBarFull == null || timerStar == null) return;

        // Вычисляем процент оставшегося времени (от 0 до 1)
        float timePercent = Mathf.Clamp01(currentTimer / maxTurnDuration);

        // 1. Уменьшаем полоску таймера
        timerBarFull.fillAmount = timePercent;

        // 2. Плавно двигаем звёздочку от пустой позиции к полной в зависимости от %
        Vector2 starPos = timerStar.anchoredPosition;
        starPos.x = Mathf.Lerp(starPositionEmpty, starPositionFull, timePercent);
        timerStar.anchoredPosition = starPos;
    }
}
