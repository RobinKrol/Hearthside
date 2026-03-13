using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// Синглтон для управления переходами между сценами.
/// Поддерживает плавный fade-in/fade-out переход.
/// Доступ: SceneController.Instance.LoadLevel(...)
/// </summary>
public class SceneController : MonoBehaviour
{
    public static SceneController Instance { get; private set; }

    [Header("Настройки перехода")]
    public float fadeDuration = 0.4f; // Длительность затемнения

    // Имена сцен — менять здесь при переименовании
    public const string SCENE_MAIN_MENU = "MainMenu";
    public const string SCENE_CAFE      = "Cafe";
    public const string SCENE_GAME      = "Game"; // Match-3 сцена

    // Канвас для fade-эффекта (создаётся автоматически)
    private CanvasGroup fadeCanvas;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            CreateFadeCanvas();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // ─── Публичные методы перехода ───────────────────────────

    /// <summary>Загрузить игровую Match-3 сцену для заданного уровня.</summary>
    public void LoadGameLevel(int levelIndex = -1)
    {
        if (levelIndex >= 0 && SaveManager.Instance != null)
            SaveManager.Instance.Data.currentLevel = levelIndex;

        LoadScene(SCENE_GAME);
    }

    /// <summary>Вернуться в сцену кафе.</summary>
    public void LoadCafe() => LoadScene(SCENE_CAFE);

    /// <summary>Перейти в главное меню.</summary>
    public void LoadMainMenu() => LoadScene(SCENE_MAIN_MENU);

    /// <summary>Перезагрузить текущую сцену.</summary>
    public void ReloadCurrentScene() => LoadScene(SceneManager.GetActiveScene().name);

    // ─── Внутренняя логика ───────────────────────────────────

    private void LoadScene(string sceneName)
    {
        StartCoroutine(FadeAndLoad(sceneName));
    }

    private IEnumerator FadeAndLoad(string sceneName)
    {
        // Затемняем экран
        yield return StartCoroutine(Fade(0f, 1f));

        // Загружаем сцену
        yield return SceneManager.LoadSceneAsync(sceneName);

        // Проявляем экран
        yield return StartCoroutine(Fade(1f, 0f));
    }

    private IEnumerator Fade(float from, float to)
    {
        if (fadeCanvas == null) yield break;

        fadeCanvas.gameObject.SetActive(true);
        float elapsed = 0f;

        while (elapsed < fadeDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            fadeCanvas.alpha = Mathf.Lerp(from, to, elapsed / fadeDuration);
            yield return null;
        }

        fadeCanvas.alpha = to;

        // Скрываем холст когда прозрачен
        if (to <= 0f) fadeCanvas.gameObject.SetActive(false);
    }

    /// <summary>
    /// Создаёт чёрный CanvasGroup поверх всего для fade-эффекта.
    /// Вызывается один раз при создании объекта.
    /// </summary>
    private void CreateFadeCanvas()
    {
        // Создаём Canvas
        GameObject canvasObj = new GameObject("FadeCanvas");
        canvasObj.transform.SetParent(transform);
        Canvas canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 999; // Поверх всего

        canvasObj.AddComponent<CanvasScaler>();
        canvasObj.AddComponent<GraphicRaycaster>();

        // Чёрный Image на весь экран
        GameObject imageObj = new GameObject("FadeImage");
        imageObj.transform.SetParent(canvasObj.transform, false);
        Image image = imageObj.AddComponent<Image>();
        image.color = Color.black;

        RectTransform rt = imageObj.GetComponent<RectTransform>();
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.sizeDelta = Vector2.zero;

        // CanvasGroup для управления прозрачностью
        fadeCanvas = canvasObj.AddComponent<CanvasGroup>();
        fadeCanvas.alpha = 0f;
        fadeCanvas.blocksRaycasts = true;
        fadeCanvas.interactable = false;
        canvasObj.SetActive(false);
    }
}
