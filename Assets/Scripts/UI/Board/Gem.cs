using System.Collections;
using UnityEngine;

public class Gem : MonoBehaviour
{
    public enum GemColor
    {
        Green,
        Red,
        Violet,
        White,
        Yellow
    }

    [Header("Gem Data")]
    public GemColor color;
    public int xIndex;
    public int yIndex;
    public bool isMatched = false;

    [Header("Match Animation Settings")]
    public float shakeAmount = 0.1f;    // Насколько сильно дрожит (амплитуда в координатах)
    public float shakeDuration = 0.05f; // Скорость одного "рывка" в сторону

    // Ссылки
    private SpriteRenderer spriteRenderer;
    private BoardManager board;

    // Свайп логика
    private Vector2 firstTouchPosition;
    private Vector2 finalTouchPosition;
    public float swipeResist = 0.5f; // Минимальная длина свайпа

    public void Setup(GemColor newColor, Sprite newSprite, int x, int y, BoardManager boardManager)
    {
        color = newColor;
        xIndex = x;
        yIndex = y;
        board = boardManager;

        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer != null)
        {
            spriteRenderer.sprite = newSprite;
        }
    }

    private void OnMouseDown()
    {
        if (isMatched) return;

        // Выводим данные о кристалле в консоль для проверки клика
        Debug.Log($"Клик по кристаллу: x = {xIndex}, y = {yIndex}, цвет = {color}");

        // Запоминаем позицию начала свайпа
        firstTouchPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
    }

    private void OnMouseUp()
    {
        if (isMatched) return;

        // Запоминаем позицию конца свайпа
        finalTouchPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        CalculateAngle();
    }

    void CalculateAngle()
    {
        // Проверяем, был ли свайп достаточно длинным (защита от случайных кликов)
        if (Mathf.Abs(finalTouchPosition.y - firstTouchPosition.y) > swipeResist || Mathf.Abs(finalTouchPosition.x - firstTouchPosition.x) > swipeResist)
        {
            float swipeAngle = Mathf.Atan2(finalTouchPosition.y - firstTouchPosition.y, finalTouchPosition.x - firstTouchPosition.x) * 180 / Mathf.PI;
            DetermineMoveDirection(swipeAngle);
        }
    }

    void DetermineMoveDirection(float angle)
    {
        // Вправо
        if (angle > -45 && angle <= 45)
        {
            board.SwapGems(this, Vector2.right);
        }
        // Вверх
        else if (angle > 45 && angle <= 135)
        {
            board.SwapGems(this, Vector2.up);
        }
        // Влево
        else if (angle > 135 || angle <= -135)
        {
            board.SwapGems(this, Vector2.left);
        }
        // Вниз
        else if (angle < -45 && angle >= -135)
        {
            board.SwapGems(this, Vector2.down);
        }
    }

    private Vector3 matchPosition;

    public void SetMatched()
    {
        if (isMatched) return;
        isMatched = true;

        // Отменяем любые другие анимации (например, если кристалл еще падал)
        LeanTween.cancel(gameObject);

        // Запоминаем центральную позицию для дрожания
        matchPosition = transform.localPosition;
    }

    private void Update()
    {
        if (isMatched)
        {
            // Жесткая математическая синхронизация через единое время Time.time.
            // Все кристаллы будут трястись абсолютно одинаково.
            float shakePhase = Time.time * (Mathf.PI / shakeDuration);
            transform.localPosition = new Vector3(
                matchPosition.x + Mathf.Sin(shakePhase) * shakeAmount,
                matchPosition.y,
                matchPosition.z
            );
        }
    }

    private void OnDestroy()
    {
        LeanTween.cancel(gameObject);
    }
}
