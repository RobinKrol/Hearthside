using UnityEngine;
using System.Collections;

[System.Serializable]
public class EnvironmentStage
{
    public string stageName;        // Название (Утро, День, и т.д.)
    public Sprite backgroundSprite; // Картинка окружения
    public int turnsDuration;       // Сколько ходов длится
}

public class EnvironmentManager : MonoBehaviour
{
    [Header("Настройки Окружения")]
    public SpriteRenderer mainBackgroundRenderer; // Ссылка на SpriteRenderer текущего фона (со Scale 945 и Position 0, 557, 1)

    [Header("Стадии Игры")]
    public EnvironmentStage[] stages; // Массив из 4-х стадий (утро, день, вечер, ночь)

    private int currentStageIndex = 0;
    private int turnsInCurrentStage = 0;

    [Header("Настройки переходов")]
    public float fadeDuration = 2f; // Длительность плавного перекрестного растворения

    public bool isGameOver { get; private set; } = false;

    private void Start()
    {
        // Подтягиваем длительности из LevelConfig, если он установлен в GameManager
        if (GameManager.Instance != null && GameManager.Instance.currentLevel != null && stages != null && stages.Length >= 3)
        {
            var config = GameManager.Instance.currentLevel;
            stages[0].turnsDuration = config.turnsMorning;
            stages[1].turnsDuration = config.turnsDay;
            stages[2].turnsDuration = config.turnsEvening;
            // Ночь (stages[3] и тд) по сути не ограничена, игра уже заканчивается
        }

        // Инициализируем стартовый фон (Утро), если стадии настроены
        if (stages != null && stages.Length > 0 && mainBackgroundRenderer != null)
        {
            mainBackgroundRenderer.sprite = stages[0].backgroundSprite;
        }
    }

    /// <summary>
    /// Вызывается из BoardManager при каждом завершении хода (истечении таймера 15 сек).
    /// </summary>
    public void OnTurnCompleted()
    {
        if (currentStageIndex >= stages.Length) return; // Игра уже завершена

        turnsInCurrentStage++;

        // Проверяем, достигли ли мы конца текущей стадии
        if (turnsInCurrentStage >= stages[currentStageIndex].turnsDuration)
        {
            // Переходим к следующей стадии
            currentStageIndex++;
            turnsInCurrentStage = 0; // Сбрасываем счетчик для новой стадии

            // Если стадии закончились — Конец игры
            if (currentStageIndex >= stages.Length)
            {
                TriggerGameOver();
            }
            else
            {
                // Запускаем плавный переход на спрайт нового времени суток
                StartCoroutine(CrossfadeBackground(stages[currentStageIndex].backgroundSprite));

                // Если мы переключились на последнюю стадию (Ночь), то играть больше нельзя
                if (currentStageIndex == stages.Length - 1)
                {
                    TriggerGameOver();
                }
            }
        }
    }

    /// <summary>
    /// Создает временный дубликат фона для красивого эффекта затухания (Crossfade)
    /// </summary>
    private IEnumerator CrossfadeBackground(Sprite newSprite)
    {
        if (mainBackgroundRenderer == null) yield break;

        // Создаем временный GameObject, который будет хранить старый спрайт
        GameObject tempBgObj = new GameObject("TempBackgroundFader");
        tempBgObj.transform.SetParent(mainBackgroundRenderer.transform.parent); // Кладем в ту же иерархию
        tempBgObj.transform.position = mainBackgroundRenderer.transform.position;
        tempBgObj.transform.localScale = mainBackgroundRenderer.transform.localScale;
        tempBgObj.transform.rotation = mainBackgroundRenderer.transform.rotation;

        SpriteRenderer tempRenderer = tempBgObj.AddComponent<SpriteRenderer>();
        tempRenderer.sprite = mainBackgroundRenderer.sprite;
        tempRenderer.color = mainBackgroundRenderer.color;
        tempRenderer.sortingLayerID = mainBackgroundRenderer.sortingLayerID;

        // Временный спрайт должен быть позади основного
        tempRenderer.sortingOrder = mainBackgroundRenderer.sortingOrder - 1;

        // На основной фон сразу ставим НОВЫЙ спрайт, но делаем его невидимым (Альфа = 0)
        mainBackgroundRenderer.sprite = newSprite;
        Color color = mainBackgroundRenderer.color;
        color.a = 0f;
        mainBackgroundRenderer.color = color;

        // Теперь плавно делаем новый спрайт видимым (Fade In через LeanTween)
        LeanTween.alpha(mainBackgroundRenderer.gameObject, 1f, fadeDuration).setEaseInOutSine();

        // Ждем, пока анимация закончится
        yield return new WaitForSeconds(fadeDuration);

        // Уничтожаем временный старый фон под ним
        Destroy(tempBgObj);
    }

    private void TriggerGameOver()
    {
        isGameOver = true;

        Debug.Log("======================================");
        Debug.Log("НАСТУПИЛА ИГРОВАЯ НОЧЬ! ХОДЫ ЗАКОНЧИЛИСЬ!");
        Debug.Log("======================================");

        // Здесь мы позже можем добавить вызов UI окна конца игры,
        // Остановку скриптов или переход на сцену результатов.
    }
}
