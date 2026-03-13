using System.Collections;
using System;
using UnityEngine;
using UnityEngine.UI;

public class Customer : MonoBehaviour
{
    [Header("UI Ссылки")]
    public Image portraitImage;      // Картинка самого посетителя
    public Image drinkImage;         // Картинка напитка на столе перед ним (изначально выключена)

    [Header("Настройки")]
    public Sprite defaultSprite;     // Обычное лицо
    public Sprite happySprite;       // Довольное лицо (после получения напитка)
    public Gem.GemColor requestedColor; // Цвет напитка, который он ждет

    private Action onOrderCompleted;
    private CanvasGroup canvasGroup;

    /// <summary>
    /// True, если посетителя уже обслуживают (напиток подан и он уходит).
    /// </summary>
    public bool IsBeingServed { get; private set; } = false;

    public void Setup(Gem.GemColor color, Sprite baseSprite, Sprite joyfulSprite, Action completionCallback)
    {
        requestedColor = color;
        defaultSprite = baseSprite;
        happySprite = joyfulSprite;
        onOrderCompleted = completionCallback;

        // Получаем или добавляем CanvasGroup для управления прозрачностью
        canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null)
            canvasGroup = gameObject.AddComponent<CanvasGroup>();
        canvasGroup.alpha = 1f;

        if (portraitImage != null)
        {
            portraitImage.sprite = defaultSprite;
        }

        if (drinkImage != null)
        {
            drinkImage.gameObject.SetActive(false);
        }

        // Запоминаем оригинальный размер префаба
        Vector3 finalScale = transform.localScale;

        // Анимация появления (как пузырь)
        transform.localScale = Vector3.zero;
        LeanTween.scale(gameObject, finalScale, 0.4f).setEaseOutCirc();
    }

    /// <summary>
    /// Запускает анимацию получения напитка и ухода.
    /// </summary>
    public void ServeDrink(Sprite drinkSprite)
    {
        if (IsBeingServed) return; // Защита от повторного вызова
        IsBeingServed = true;
        StartCoroutine(ServeDrinkCoroutine(drinkSprite));
    }

    private IEnumerator ServeDrinkCoroutine(Sprite drinkSprite)
    {
        // 1. Появляется напиток
        if (drinkImage != null && drinkSprite != null)
        {
            drinkImage.sprite = drinkSprite;

            // Запоминаем оригинальный масштаб из префаба ДО того, как его обнулим
            Vector3 drinkOriginalScale = drinkImage.transform.localScale;

            drinkImage.gameObject.SetActive(true);

            // Анимация появления: прыжок от нуля до оригинального масштаба из префаба
            drinkImage.transform.localScale = Vector3.zero;
            LeanTween.scale(drinkImage.gameObject, drinkOriginalScale, 0.4f).setEaseOutBounce();
        }

        // Ждем, пока игрок полюбуется напитком
        yield return new WaitForSeconds(0.6f);

        // 2. Лицо меняется на довольное
        if (portraitImage != null && happySprite != null)
        {
            portraitImage.sprite = happySprite;
        }

        // Ждем еще немного
        yield return new WaitForSeconds(1.0f);

        // 3. Анимация исчезновения — "сдувается": уменьшается на 10% и растворяется
        Vector3 currentScale = transform.localScale;
        LeanTween.scale(gameObject, currentScale * 0.9f, 0.15f).setEaseOutQuad();

        yield return new WaitForSeconds(0.15f);

        // После "надувания" — растворяемся
        if (canvasGroup != null)
        {
            LeanTween.alphaCanvas(canvasGroup, 0f, 0.25f).setEaseInQuad();
        }

        yield return new WaitForSeconds(0.3f);

        // 4. Оповещаем менеджера, что место свободно
        onOrderCompleted?.Invoke();

        // Удаляем объект
        Destroy(gameObject);
    }
}
