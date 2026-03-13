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
    public Transform fruitTargetPoint; // Точка, куда будут лететь фрукты

    [Header("Frame_outline (Пульс при готовой ульте)")]
    public Image frameOutlineImage;     // Дочерний объект Frame_outline
    public float pulseSpeed = 2.5f;     // Скорость пульсации
    public float pulseMinAlpha = 0.6f;  // Минимальная прозрачность пульса
    public float pulseMaxAlpha = 1.0f;  // Максимальная прозрачность пульса

    // Состояние предыдущего кадра
    private bool wasReadyLastFrame = false;
    private Color originalOutlineColor; // Оригинальный цвет рамки из префаба (черная тень)

    private void Awake()
    {
        heroData = GetComponent<Hero>();
    }

    private void Start()
    {
        // Запоминаем оригинальный цвет рамки из префаба (черная тень)
        if (frameOutlineImage != null)
            originalOutlineColor = frameOutlineImage.color;

        UpdateUI();
        // SetOutlineIdle() не вызываем здесь, чтобы не сбрасывать цвет черной тени из префаба
    }

    private void Update()
    {
        if (heroData == null || frameOutlineImage == null) return;

        bool isReady = heroData.currentEnergy >= heroData.maxEnergy;

        if (isReady)
        {
            // Пульсируем alpha белого цвета между pulseMinAlpha и pulseMaxAlpha
            float alpha = Mathf.Lerp(pulseMinAlpha, pulseMaxAlpha,
                (Mathf.Sin(Time.time * pulseSpeed) + 1f) / 2f);

            frameOutlineImage.color = new Color(1f, 1f, 1f, alpha); // Белый с пульсацией
        }
        else if (wasReadyLastFrame)
        {
            // Только что перестал быть готов (после использования ульты) — сбрасываем
            SetOutlineIdle();
        }

        wasReadyLastFrame = isReady;
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

    /// <summary>
    /// Состояние "не готов" — восстанавливаем оригинальный цвет тени из префаба
    /// </summary>
    private void SetOutlineIdle()
    {
        if (frameOutlineImage == null) return;
        // Восстанавливаем цвет черной тени из префаба
        frameOutlineImage.color = originalOutlineColor;
    }
}
