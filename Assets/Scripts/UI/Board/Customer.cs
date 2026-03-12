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

    public void Setup(Gem.GemColor color, Sprite baseSprite, Sprite joyfulSprite, Action completionCallback)
    {
        requestedColor = color;
        defaultSprite = baseSprite;
        happySprite = joyfulSprite;
        onOrderCompleted = completionCallback;

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

        // 3. Анимация исчезновения (уменьшение и прозрачность)
        LeanTween.scale(gameObject, Vector3.zero, 0.4f).setEaseInBack();
        
        yield return new WaitForSeconds(0.4f);

        // 4. Оповещаем менеджера, что место свободно
        onOrderCompleted?.Invoke();

        // Удаляем объект
        Destroy(gameObject);
    }
}
