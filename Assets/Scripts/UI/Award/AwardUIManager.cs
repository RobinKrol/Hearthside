using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class AwardUIManager : MonoBehaviour
{
    [Header("Базовые UI Элементы")]
    public GameObject awardPanel;    // Родительская панель окна
    public TextMeshProUGUI titleText; // "УРОВЕНЬ ПРОЙДЕН" или "НЕУДАЧА"
    public TextMeshProUGUI levelNumberText; // Строка вида "Уровень X", которую добавили вы
    public Button nextButton;         // Кнопка продолжения (В кафе)

    [Header("Иконки стандартных наград")]
    public Sprite goldSprite;
    public Sprite xpSprite;
    public Sprite keysSprite;
    public Sprite gemsSprite;

    [Header("Item Panels Settings")]
    public AwardItemUI itemPanelPrefab; // Один универсальный шаблон плашки
    public Transform itemsParent;       // Розничная панель для плашек
    public float itemSpacing = 120f;    // Расстояние между плашками (от центра до центра)
    public float itemsYPosition = 54f;  // Y позиция плашек

    private List<AwardItemUI> instantiatedItems = new List<AwardItemUI>();

    private void Start()
    {
        if (awardPanel != null) awardPanel.SetActive(false);
        if (nextButton != null) nextButton.onClick.AddListener(OnNextClicked);
    }

    /// <summary>
    /// Вызывается из GameManager при завершении игры.
    /// </summary>
    public void ShowAward(LevelConfig config, bool isWin)
    {
        if (awardPanel == null) return;

        awardPanel.SetActive(true);

        // Синхронизируем текст уровня из конфига
        if (levelNumberText != null && config != null)
        {
            levelNumberText.text = $"Уровень {config.levelNumber}";
        }

        // Анимация: Панель плавно вырастает из центра с отскоком (Pop-in)
        awardPanel.transform.localScale = Vector3.zero;
        LeanTween.scale(awardPanel, Vector3.one, 0.5f).setEaseOutBack();

        if (isWin)
        {
            if (titleText != null) titleText.text = "УРОВЕНЬ ПРОЙДЕН!";

            // Собираем все награды (и стандартные, и дополнительные) в один список
            List<RewardItemConfig> allRewardsToDisplay = new List<RewardItemConfig>();

            if (config.rewardGold > 0)
                allRewardsToDisplay.Add(new RewardItemConfig { itemName = "Gold", itemSprite = goldSprite, itemCount = config.rewardGold });

            if (config.rewardXP > 0)
                allRewardsToDisplay.Add(new RewardItemConfig { itemName = "EXP", itemSprite = xpSprite, itemCount = config.rewardXP });

            if (config.rewardKeys > 0)
                allRewardsToDisplay.Add(new RewardItemConfig { itemName = "Key", itemSprite = keysSprite, itemCount = config.rewardKeys });

            if (config.rewardGems > 0)
                allRewardsToDisplay.Add(new RewardItemConfig { itemName = "Gem", itemSprite = gemsSprite, itemCount = config.rewardGems });

            // Добавляем те кастомные награды, которые дизайнер добавил в массив LevelConfig руками
            if (config.rewardItems != null)
            {
                allRewardsToDisplay.AddRange(config.rewardItems);
            }

            SpawnRewardItems(allRewardsToDisplay);

            // Начисляем награды
            if (SaveManager.Instance != null && SaveManager.Instance.Data != null)
            {
                SaveManager.Instance.Data.coins += config.rewardGold;
                // SaveManager.Instance.Data.gems += config.rewardGems; // Раскомментируйте, когда добавите gems в PlayerData
                SaveManager.Instance.Data.currentLevel++;
                SaveManager.Instance.Save();
            }
        }
        else
        {
            if (titleText != null) titleText.text = "ВРЕМЯ ВЫШЛО";
            ClearRewardItems();
        }
    }

    private void SpawnRewardItems(List<RewardItemConfig> rewards)
    {
        ClearRewardItems();

        if (rewards == null || rewards.Count == 0 || itemPanelPrefab == null || itemsParent == null)
            return;

        int count = rewards.Count;
        float totalWidth = (count - 1) * itemSpacing;
        float startX = -totalWidth / 2f;

        for (int i = 0; i < count; i++)
        {
            AwardItemUI newItem = Instantiate(itemPanelPrefab, itemsParent, false);
            newItem.gameObject.SetActive(true);
            newItem.Setup(rewards[i]);

            RectTransform rt = newItem.GetComponent<RectTransform>();
            if (rt != null)
            {
                float xPos = startX + i * itemSpacing;
                rt.anchoredPosition3D = new Vector3(xPos, itemsYPosition, 0f); // Важно обнулить Z
            }

            // --- АНИМАЦИЯ ПЕЧАТИ ---
            // 1. Изначально прячем плашку (размер 0)
            newItem.transform.localScale = Vector3.zero;

            // Рассчитываем задержку (справа-налево = инвертируем индекс i)
            int reverseIndex = count - 1 - i;
            float delay = 0.5f + (reverseIndex * 0.35f); // Больше пауза между появлением (0.35с)

            // 2. Через delay делаем масштаб 3x и плавно роняем
            LeanTween.scale(newItem.gameObject, Vector3.one * 2f, 0.01f)
                     .setDelay(delay)
                     .setOnComplete(() =>
                     {
                         // Эффект падающего камня с отскоком (сделали медленнее: 0.6с)
                         LeanTween.scale(newItem.gameObject, Vector3.one, 0.6f).setEaseOutBounce();
                     });

            instantiatedItems.Add(newItem);
        }
    }

    private void ClearRewardItems()
    {
        foreach (var item in instantiatedItems)
        {
            if (item != null) Destroy(item.gameObject);
        }
        instantiatedItems.Clear();
    }

    private void OnNextClicked()
    {
        // Переход в кафе
        if (SceneController.Instance != null)
        {
            SceneController.Instance.LoadCafe();
        }
        else
        {
            Debug.Log("[AwardUIManager] SceneController не найден, загрузка Кафе симулируется.");
        }
    }
}
