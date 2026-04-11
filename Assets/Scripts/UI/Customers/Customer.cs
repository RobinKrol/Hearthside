using System.Collections;
using System;
using UnityEngine;
using UnityEngine.UI;
using Enums;

public class Customer : MonoBehaviour
{
    [Header("UI Ссылки — Portrait")]
    public Image portraitImage;      // Картинка лица посетителя (меняется на довольное)

    [Header("UI Ссылки — Заказ (чат-бабл)")]
    public GameObject orderBubbleRoot; // Корневой объект бабла (скрывается целиком)
    public Image orderIconImage;     // Иконка напитка внутри бабла

    [Header("UI Ссылки — Напиток")]
    public Image drinkImage;         // Картинка поданного напитка (появляется при обслуживании)

    [Header("Состояние")]
    public Gem.GemColor requestedColor;   // Цвет напитка, который он ждёт
    public DrinkSize requestedSize;       // Размер напитка
    public bool IsBeingServed { get; private set; } = false;

    // Приватные данные
    private Sprite happySprite;
    private Action onOrderCompleted;
    private CanvasGroup canvasGroup;

    /// <summary>
    /// Настраивает посетителя перед появлением на сцене.
    /// </summary>
    /// <param name="color">Запрашиваемый цвет напитка</param>
    /// <param name="size">Запрашиваемый размер напитка</param>
    /// <param name="defaultFace">Обычное лицо</param>
    /// <param name="happyFace">Довольное лицо (после получения заказа)</param>
    /// <param name="orderIcon">Иконка заказа для чат-бабла</param>
    /// <param name="completionCallback">Вызывается когда посетитель ушёл</param>
    public void Setup(Gem.GemColor color, DrinkSize size, Sprite defaultFace, Sprite happyFace,
                      Sprite orderIcon, Action completionCallback)
    {
        requestedColor = color;
        requestedSize = size;
        happySprite = happyFace;
        onOrderCompleted = completionCallback;

        // CanvasGroup для управления прозрачностью
        canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null)
            canvasGroup = gameObject.AddComponent<CanvasGroup>();
        canvasGroup.alpha = 0f;

        // Лицо посетителя
        if (portraitImage != null)
            portraitImage.sprite = defaultFace;

        // Бабл с иконкой заказа
        if (orderBubbleRoot != null)
            orderBubbleRoot.SetActive(true);
        if (orderIconImage != null)
        {
            orderIconImage.sprite = orderIcon;
            orderIconImage.gameObject.SetActive(orderIcon != null);
        }

        // Напиток скрыт пока не подан
        if (drinkImage != null)
            drinkImage.gameObject.SetActive(false);

        // Плавное появление (fade-in)
        LeanTween.alphaCanvas(canvasGroup, 1f, 0.4f).setEaseOutQuad();
    }

    /// <summary>
    /// Запускает анимацию подачи напитка и ухода посетителя.
    /// </summary>
    public void ServeDrink(Sprite drinkSprite)
    {
        if (IsBeingServed) return;
        IsBeingServed = true;
        StartCoroutine(ServeDrinkCoroutine(drinkSprite));
    }

    private IEnumerator ServeDrinkCoroutine(Sprite drinkSprite)
    {
        // 1. Скрываем весь бабл целиком (заказ выполнен)
        if (orderBubbleRoot != null)
            orderBubbleRoot.SetActive(false);
        else if (orderIconImage != null)
            orderIconImage.gameObject.SetActive(false);

        // 2. Появляется поданный напиток
        if (drinkImage != null && drinkSprite != null)
        {
            drinkImage.sprite = drinkSprite;
            Vector3 drinkOriginalScale = drinkImage.transform.localScale;
            drinkImage.gameObject.SetActive(true);
            drinkImage.transform.localScale = Vector3.zero;
            LeanTween.scale(drinkImage.gameObject, drinkOriginalScale, 0.4f).setEaseOutBounce();
        }

        yield return new WaitForSeconds(0.6f);

        // 3. Лицо меняется на довольное
        if (portraitImage != null && happySprite != null)
            portraitImage.sprite = happySprite;

        yield return new WaitForSeconds(1.0f);

        // Плавно растворяемся (без изменения размера)


        if (canvasGroup != null)
            LeanTween.alphaCanvas(canvasGroup, 0f, 0.25f).setEaseInQuad();

        yield return new WaitForSeconds(0.3f);

        // 5. Сообщаем менеджеру — место свободно
        onOrderCompleted?.Invoke();
        Destroy(gameObject);
    }
}
