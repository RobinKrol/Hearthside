using UnityEngine;

public class GameManager : MonoBehaviour
{
    [Header("Turn & Timer Logic")]
    public float turnDuration = 15f;
    public int turnCount = 1;

    [Header("References")]
    public UIManager uiManager;
    public BoardManager boardManager;
   

    private float currentTimer;
    private bool isTimerRunning = false;
    private bool isTurnActive = true;

    private void Start()
    {
        currentTimer = turnDuration;
        if (uiManager != null)
        {
            uiManager.UpdateTurnUI(turnCount);
            uiManager.UpdateTimerUI(currentTimer, turnDuration); // Сразу показываем полный таймер
        }
    }

    private void Update()
    {
        if (isTimerRunning)
        {
            currentTimer -= Time.deltaTime;
            Debug.Log(currentTimer);
            if (uiManager != null)
            {
                uiManager.UpdateTimerUI(currentTimer, turnDuration);
            }

            if (currentTimer <= 0)
            {
                OnTimeUp();
            }

            
        }
    }

    public bool IsTurnActive()
    {
        return isTurnActive;
    }

    public bool IsTimerRunning()
    {
        return isTimerRunning;
    }

    public float GetCurrentTimer()
    {
        return currentTimer;
    }

    public void OnFirstComboMatch()
    {
        if (!isTimerRunning && isTurnActive)
        {
            isTimerRunning = true;
            currentTimer = turnDuration;
            Debug.Log($"Первое комбо! Таймер запущен на {turnDuration} секунд!");
        }
        else if (isTimerRunning && isTurnActive)
        {
            // За комбо добавляем 50% от оставшегося времени
            float bonus = currentTimer * 0.5f;
            currentTimer += bonus;
            // Не даем таймеру превысить изначальный максимум
            currentTimer = Mathf.Clamp(currentTimer, 0f, turnDuration);
            Debug.Log($"Комбо! +{bonus:F1}с (50% от остатка). Текущее время: {currentTimer:F1}");
        }
    }

    private void OnTimeUp()
    {
        isTimerRunning = false;
        isTurnActive = false; // Жестко блокируем новые свайпы
        
        if (boardManager != null)
        {
            boardManager.OnTurnTimeUp();
        }
    }

    public void CompleteTurn()
    {
        turnCount++;
        
        if (uiManager != null)
        {
            uiManager.UpdateTurnUI(turnCount);
        }
    }

    public void UnlockTurn()
    {
        isTurnActive = true;
        currentTimer = turnDuration; // Готовим таймер для следующего хода
        
        if (uiManager != null)
        {
            uiManager.UpdateTimerUI(currentTimer, turnDuration); // Визуально возвращаем таймер на 100%
        }
    }
}
