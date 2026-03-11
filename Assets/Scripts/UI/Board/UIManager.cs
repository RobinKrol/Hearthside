using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UIManager : MonoBehaviour
{
    [Header("UI & Timer Visuals")]
    public Image timerImage; // Ссылка на Image компонент часов
    public Sprite[] timerSprites;  // Массив спрайтов часов (от полных до пустых)
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
    /// Обновляет визуальное отображение часов в зависимости от оставшегося времени
    /// </summary>
    public void UpdateTimerUI(float currentTimer, float maxTurnDuration)
    {
        if (timerImage == null || timerSprites == null || timerSprites.Length == 0) return;

        // Если время вышло окончательно, возвращаемся к первому спрайту (полным часам)
        if (currentTimer <= 0)
        {
            timerImage.sprite = timerSprites[0];
            return;
        }

        // Вычисляем процент оставшегося времени
        float timePercent = currentTimer / maxTurnDuration;

        // Распределяем все доступные кадры равномерно на протяжении 15 секунд.
        // Массив: 0 (полные) ... N-1 (пустые). 
        // 1.0f - timePercent даст значение от 0 до почти 1.
        int spriteIndex = Mathf.Clamp(Mathf.FloorToInt((1f - timePercent) * timerSprites.Length), 0, timerSprites.Length - 1);

        timerImage.sprite = timerSprites[spriteIndex];
    }
}
