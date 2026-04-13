using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Level Config & Flow")]
    public LevelConfig currentLevel;     // Настройки текущего уровня
    public AwardUIManager awardUI;       // Ссылка на экран наград
    public HeroManager heroManager;      // Ссылка на менеджер героев
    
    [Header("Turn & Timer Logic")]
    public float turnDuration = 15f;
    public int turnCount = 0;

    [Header("References")]
    public UIManager uiManager;
    public BoardManager boardManager;
   

    private float currentTimer;
    private bool isTimerRunning = false;
    private bool isTurnActive = true;
    private bool isNight = false; // Наступила ли ночь (конец всех ходов)

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private void Start()
    {
        // Подхватываем настройки уровня, если задан
        if (currentLevel != null)
        {
            turnDuration = currentLevel.turnDurationSeconds;
        }

        currentTimer = turnDuration;
        if (uiManager != null)
        {
            int maxTurns = currentLevel != null ? currentLevel.TotalTurns : 7;
            uiManager.UpdateTurnUI(turnCount, maxTurns);
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

    public bool IsGameEnding()
    {
        return isNight || isGameOverTriggered;
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
        
        int maxTurns = currentLevel != null ? currentLevel.TotalTurns : 7;
        
        if (uiManager != null)
        {
            uiManager.UpdateTurnUI(turnCount, maxTurns);
        }

        // Если превышен лимит ходов (игроком совершено maxTurns ходов) — наступает ночь
        if (turnCount >= maxTurns)
        {
            TriggerNightPhase();
        }
    }

    private void TriggerNightPhase()
    {
        Debug.Log("[GameManager] Наступила ночь! Ходы окончены.");
        isNight = true;
        isTurnActive = false; // Блокируем новые обычные свайпы

        CheckEndGameCondition();
    }

    private bool isGameOverTriggered = false;

    /// <summary>
    /// Проверяет, можно ли закончить игру досрочно (все заказы выполнены).
    /// Должна вызываться из OrderManager при успешной подаче напитка.
    /// </summary>
    public void CheckWinConditionEarly()
    {
        if (isGameOverTriggered || currentLevel == null) return;
        
        int fulfilled = OrderManager.Instance != null ? OrderManager.Instance.GetFulfilledCount(currentLevel.targetDrinkColor, currentLevel.targetDrinkSize) : 0;
        if (fulfilled >= currentLevel.targetDrinksCount)
        {
            Debug.Log("[GameManager] Все заказы выполнены! Досрочная победа!");
            isGameOverTriggered = true;
            isTimerRunning = false;
            isTurnActive = false; // Блокируем доску
            
            Invoke(nameof(ShowAwardScreen), 1.5f);
        }
    }

    /// <summary>
    /// Проверяет, можно ли закончить игру из-за наступления ночи.
    /// Должна вызываться при наступлении ночи и после каждой использованной ульты.
    /// </summary>
    public void CheckEndGameCondition()
    {
        if (!isNight || isGameOverTriggered) return;

        bool hasUltimate = heroManager != null && heroManager.HasAnyUltimateReady();

        if (!hasUltimate)
        {
            isGameOverTriggered = true;
            isTimerRunning = false;
            isTurnActive = false; // Блокируем доску
            
            Invoke(nameof(ShowAwardScreen), 1.5f);
        }
        else
        {
            Debug.Log("[GameManager] Ночь наступила, но у героев есть ульта! Ждём...");
        }
    }

    private void ShowAwardScreen()
    {
        if (awardUI == null || currentLevel == null) return;

        // Очищаем и скрываем доску с кристаллами, чтобы они не мешали окну
        if (boardManager != null)
        {
            boardManager.ClearBoard();
            boardManager.gameObject.SetActive(false);
        }

        // Скрываем героев и их спавнер
        if (heroManager != null)
        {
            heroManager.HideAllHeroes();
        }
        
        HeroSpawner spawner = FindAnyObjectByType<HeroSpawner>();
        if (spawner != null)
        {
            spawner.gameObject.SetActive(false);
        }

        // Проверяем победу по заказам
        int fulfilled = OrderManager.Instance != null ? OrderManager.Instance.GetFulfilledCount(currentLevel.targetDrinkColor, currentLevel.targetDrinkSize) : 0;
        bool isWin = fulfilled >= currentLevel.targetDrinksCount;

        Debug.Log($"[GameManager] Конец игры. Цель: {currentLevel.targetDrinksCount}, Сделано: {fulfilled}. Победа: {isWin}");
        awardUI.ShowAward(currentLevel, isWin);
    }

    public void UnlockTurn()
    {
        if (isNight) return; // Во время ночи ходы не разблокируются

        isTurnActive = true;
        currentTimer = turnDuration; // Готовим таймер для следующего хода
        
        if (uiManager != null)
        {
            uiManager.UpdateTimerUI(currentTimer, turnDuration); // Визуально возвращаем таймер на 100%
        }
    }
}
