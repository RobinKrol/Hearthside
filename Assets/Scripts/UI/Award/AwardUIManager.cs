using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class AwardUIManager : MonoBehaviour
{
    [Header("UI Элементы")]
    public GameObject awardPanel;    // Родительская панель окна
    public TextMeshProUGUI titleText; // "УРОВЕНЬ ПРОЙДЕН" или "НЕУДАЧА"
    public TextMeshProUGUI goldText;  // Текст с золотом
    public TextMeshProUGUI xpText;    // Текст с опытом
    public TextMeshProUGUI keysText;  // Текст с ключами
    public Button nextButton;         // Кнопка продолжения (В кафе)

    [Header("Item Panels Settings")]
    public AwardItemUI itemPanelPrefab; // Префаб плашки
    public Transform itemsParent;       // Родитель для плашек
    public float itemSpacing = 120f;    // Расстояние между плашками (от центра до центра)
    public float itemsYPosition = 54f;  // Y позиция плашек

    private List<AwardItemUI> instantiatedItems = new List<AwardItemUI>();

    private void Start()
    {
        if (awardPanel != null) awardPanel.SetActive(false);
        if (nextButton != null) nextButton.onClick.AddListener(OnNextClicked);
    }

    /// <summary>
    /// Вызывается из GameManager при завершении игры (Ночь и нет ульты).
    /// </summary>
    public void ShowAward(LevelConfig config, bool isWin)
    {
        if (awardPanel == null) return;
        awardPanel.SetActive(true);

        if (isWin)
        {
            if (titleText != null) titleText.text = "УРОВЕНЬ ПРОЙДЕН!";
            if (goldText != null) goldText.text = $"+{config.rewardGold}";
            if (xpText != null) xpText.text = $"+{config.rewardXP}";
            if (keysText != null) keysText.text = $"+{config.rewardKeys}";
            
            SpawnRewardItems(config.rewardItems);

            // Начисляем награды (за золото, опыт и прочее можно в PlayerData расширить, пока только золото)
            if (SaveManager.Instance != null && SaveManager.Instance.Data != null)
            {
                SaveManager.Instance.Data.coins += config.rewardGold;
                SaveManager.Instance.Data.currentLevel++;
                SaveManager.Instance.Save();
            }
        }
        else
        {
            if (titleText != null) titleText.text = "ВРЕМЯ ВЫШЛО";
            if (goldText != null) goldText.text = "+0";
            if (xpText != null) xpText.text = "+0";
            if (keysText != null) keysText.text = "+0";
            
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
            AwardItemUI newItem = Instantiate(itemPanelPrefab, itemsParent);
            newItem.Setup(rewards[i]);

            RectTransform rt = newItem.GetComponent<RectTransform>();
            if (rt != null)
            {
                float xPos = startX + i * itemSpacing;
                rt.anchoredPosition = new Vector2(xPos, itemsYPosition);
            }

            instantiatedItems.Add(newItem);
        }
    }

    private void ClearRewardItems()
    {
        foreach(var item in instantiatedItems)
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
