using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class HubUIManager : MonoBehaviour
{
    public static HubUIManager Instance { get; private set; }

    [Header("Основные окна (Panels)")]
    public GameObject heroPanel;
    public GameObject cafePanel;
    public GameObject levelPanel;

    [Header("Кнопки нижней навигации (Bottom Nav)")]
    public Button btnHero;
    public Button btnCafe;
    public Button btnLevel;

    [Header("Верхний бар (Шапка)")]
    public TextMeshProUGUI coinsText;
    // public TextMeshProUGUI gemsText; // На будущее

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private void Start()
    {
        // Подключаем слушатели к кнопкам:
        if (btnHero != null) btnHero.onClick.AddListener(ShowHeroPanel);
        if (btnCafe != null) btnCafe.onClick.AddListener(ShowCafePanel);
        if (btnLevel != null) btnLevel.onClick.AddListener(ShowLevelPanel);
        
        UpdateTopBar();

        // По умолчанию открываем панель уровней (Карту)
        ShowLevelPanel();
    }

    /// <summary>
    /// Обновляет счетчики баланса золота (и других ресурсов) вверху экрана
    /// </summary>
    public void UpdateTopBar()
    {
        var data = SaveManager.Instance?.Data;
        if (data != null)
        {
            if (coinsText != null) coinsText.text = data.coins.ToString();
        }
    }

    public void ShowHeroPanel()
    {
        HideAllPanels();
        if (heroPanel != null) heroPanel.SetActive(true);
        UpdateTopBar(); // Обновляем баланс при входе
    }

    public void ShowCafePanel()
    {
        HideAllPanels();
        if (cafePanel != null) cafePanel.SetActive(true);
        UpdateTopBar();
    }

    public void ShowLevelPanel()
    {
        HideAllPanels();
        if (levelPanel != null) levelPanel.SetActive(true);
        UpdateTopBar();
    }

    private void HideAllPanels()
    {
        if (heroPanel != null) heroPanel.SetActive(false);
        if (cafePanel != null) cafePanel.SetActive(false);
        if (levelPanel != null) levelPanel.SetActive(false);
    }
}
