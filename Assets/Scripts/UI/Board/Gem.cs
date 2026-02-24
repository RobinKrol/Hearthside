using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class Gem : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    [Header("Данные кристалла")]
    public int gemType;
    
    [Header("Компоненты")]
    private Image image;
    private RectTransform rectTransform;
    private BoardManager boardManager;

    [Header("Состояние перетаскивания")]
    private bool isDragging = false;
    private Vector2 originalPosition;
    private Transform originalParent;

    void Awake()
    {
        image = GetComponent<Image>();
        boardManager = FindAnyObjectByType<BoardManager>();
        rectTransform = GetComponent<RectTransform>();

        if (boardManager == null)
        {
            Debug.LogError("BoardManager не найден на сцене!");
        }
    }

     public void Setup(int type)
    {
        gemType = type;
        // Кристалл НЕ хранит свои координаты!
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (isDragging) return;
        
        // Запоминаем исходное состояние для анимации
        originalParent = transform.parent;
        originalPosition = rectTransform.anchoredPosition;
        
        // Визуальная обратная связь при нажатии
        transform.localScale = Vector3.one * 1.1f;
        
        isDragging = true;
        boardManager?.StartDragging(this);
    }

     public void OnPointerUp(PointerEventData eventData)
    {
        if (!isDragging) return;
        
        // Возвращаем нормальный масштаб
        transform.localScale = Vector3.one;
        
        isDragging = false;
        boardManager?.StopDragging(this);
    }

    // Метод для анимации перетаскивания (будет вызываться из BoardManager)
    public void UpdateDragPosition(Vector2 screenPosition)
    {
        if (!isDragging) return;

        // Конвертируем экранные координаты в локальные координаты канваса
        RectTransform canvasRect = boardManager.GetComponent<RectTransform>();
        
        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(
            canvasRect, 
            screenPosition, 
            null, 
            out Vector2 localPoint))
        {
            rectTransform.localPosition = localPoint;
        }
    }
    // Метод для возврата кристалла на место (если обмен не удался)
    public void ReturnToOriginalPosition()
    {
        transform.SetParent(originalParent);
        rectTransform.anchoredPosition = originalPosition;
        transform.localScale = Vector3.one;
        isDragging = false;
    }
    // Метод для анимированного обмена (будет вызываться из BoardManager)
    public void AnimatedSwap(Gem otherGem, System.Action onComplete)
    {
        // Здесь будет анимация обмена
        // Например, перемещение кристаллов по дуге
        StartCoroutine(SwapAnimation(otherGem, onComplete));
    }

    private System.Collections.IEnumerator SwapAnimation(Gem otherGem, System.Action onComplete)
    {
        Vector3 startPos = rectTransform.position;
        Vector3 otherStartPos = otherGem.rectTransform.position;
        
        float duration = 0.3f;
        float elapsed = 0;
        
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            
            // Плавное перемещение кристаллов
            rectTransform.position = Vector3.Lerp(startPos, otherStartPos, t);
            otherGem.rectTransform.position = Vector3.Lerp(otherStartPos, startPos, t);
            
            yield return null;
        }
        
        // Завершаем анимацию
        rectTransform.position = otherStartPos;
        otherGem.rectTransform.position = startPos;
        
        onComplete?.Invoke();
    }


}
