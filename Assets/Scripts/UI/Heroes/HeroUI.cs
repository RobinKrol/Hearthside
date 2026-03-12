using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

[RequireComponent(typeof(Hero))]
public class HeroUI : MonoBehaviour, IPointerClickHandler
{
    private Hero heroData;

    [Header("Настройки Рамки Энергии")]
    public Image frameImage;      // Ссылка на Image компонент UI
    public Sprite[] frameSprites; // Массив из 5 спрайтов
    public Transform fruitTargetPoint; // Точка, куда будут лететь фрукты (например, gem_red_0)

    private void Awake()
    {
        heroData = GetComponent<Hero>();
    }

    private void Start()
    {
        UpdateUI();
    }

    /// <summary>
    /// Переключает кадры рамки героя в зависимости от % накопленной энергии
    /// </summary>
    public void UpdateUI()
    {
        if (heroData == null || frameImage == null || frameSprites == null || frameSprites.Length == 0) return;

        // Вычисляем текущий процент (от 0 до 1)
        float percent = (float)heroData.currentEnergy / heroData.maxEnergy;
        percent = Mathf.Clamp01(percent);

        int maxIndex = frameSprites.Length - 1;

        // Расчет индекса рамки математически
        int index = Mathf.Clamp(Mathf.FloorToInt(percent * frameSprites.Length), 0, maxIndex);

        // Жёсткие условия, чтобы максимальный спрайт горел только при 100% готовности
        if (percent >= 1f)
        {
            index = maxIndex;
        }
        else if (index == maxIndex)
        {
            // Если игрок почти накопил энергию (например 98%), показываем все равно предпоследний кадр
            index = maxIndex - 1;
        }

        frameImage.sprite = frameSprites[index];
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (heroData == null) return;

        // Пытаемся потратить энергию (сработает только если 100%)
        if (heroData.TryConsumeEnergy())
        {
            Debug.Log($"[HeroUI] Герой {heroData.heroColor} приготовил напиток!");
            
            // Обновляем шкалу, так как энергия скинулась в ноль
            UpdateUI();

            // Передаем сигнал в систему заказов
            OrderManager.Instance?.TryFulfillOrder(heroData.heroColor, heroData.ultimateDrinkSprite);
        }
        else
        {
            Debug.Log($"[HeroUI] Герой {heroData.heroColor} еще не готов! Энергия: {heroData.currentEnergy}/{heroData.maxEnergy}");
        }
    }
}
