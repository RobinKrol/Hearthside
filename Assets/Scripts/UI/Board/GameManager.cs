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
        if (uiManager != null)
        {
            uiManager.UpdateTurnUI(turnCount);
        }
    }

    private void Update()
    {
        if (isTimerRunning)
        {
            currentTimer -= Time.deltaTime;

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
            // Если таймер уже идет, добавляем бонусную 1 секунду за комбо!
            currentTimer += 1f;
            // Не даем таймеру превысить изначальный максимум
            currentTimer = Mathf.Clamp(currentTimer, 0f, turnDuration);
            Debug.Log($"Комбо! +1 секунда. Текущее время: {currentTimer}");
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
    }
}
