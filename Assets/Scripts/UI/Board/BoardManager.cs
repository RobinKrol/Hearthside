using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;  // нужно для Mouse.current
using System.Collections;
using System.Collections.Generic;


public class BoardManager : MonoBehaviour
{
    public static BoardManager Instance;
    
    [Header("Размеры сетки")]
    public int columns = 7;
    public int rows = 5;

    [Header("Настройки свайпа")]
    private float dragThreshold = 30f;
    public bool enableAnimations = true;


    private Gem[,] grid;
    private Gem draggedGem;             // кристалл который тащим
    private bool isDragging = false;    // тащим ли сейчас
    private Vector2 dragStartPos;       // позиция начала таскания
    


    void Awake()
    {
        Instance = this;
        grid = new Gem[rows, columns];
    }

    public void RegisterGem(Gem gem, int row, int col)
    {
        if (row >= 0 && row < rows && col >= 0 && col < columns)
        {
            grid[row, col] = gem;
            Debug.Log($"[REGISTER] Кристалл типа {gem.gemType} помещен в сетку [{row}, {col}]");
        }
    }



     // Находим позицию кристалла в grid
    (int row, int col) FindGemPosition(Gem gem)
    {
        for (int r = 0; r < rows; r++)
        {
            for (int c = 0; c < columns; c++)
            {
                if (grid[r, c] == gem)
                    return (r, c);
            }
        }
        return (-1, -1);
    }

    // ===== ПЕРЕТАСКИВАНИЕ =====
     public void StartDragging(Gem gem)
    {
        if (isDragging) return;
        
        var (row, col) = FindGemPosition(gem);
        if (row == -1)
        {
            Debug.LogError("StartDragging: кристалл не найден в сетке!");
            return;
        }
        
        draggedGem = gem;
        isDragging = true;
        dragStartPos = Mouse.current.position.ReadValue();
        
        Debug.Log($"[DRAG] Выбран кристалл из клетки [{row}, {col}] (тип: {gem.gemType})");
    }
    
    // Конец перетаскивания - вызывается из Gem
     public void StopDragging(Gem gem)
    {
        if (!isDragging || draggedGem != gem) return;
        
        // Обновляем позицию кристалла за мышкой
        gem.UpdateDragPosition(Mouse.current.position.ReadValue());
        
        Vector2 dragEndPos = Mouse.current.position.ReadValue();
        Vector2 dragDelta = dragEndPos - dragStartPos;
        
        if (dragDelta.magnitude < dragThreshold)
        {
            Debug.Log("[SWIPE] Слишком короткий свайп");
            gem.ReturnToOriginalPosition();
            ResetDrag();
            return;
        }

        Direction swipeDir = GetSwipeDirection(dragDelta);
        var (gemRow, gemCol) = FindGemPosition(gem);
        
        if (gemRow == -1)
        {
            gem.ReturnToOriginalPosition();
            ResetDrag();
            return;
        }

        var (targetRow, targetCol) = GetTargetPosition(gemRow, gemCol, swipeDir);

        if (targetRow >= 0 && targetRow < rows && targetCol >= 0 && targetCol < columns)
        {
            Gem neighbor = grid[targetRow, targetCol];
            if (neighbor != null)
            {
                Debug.Log($"[SWAP] Меняем кристаллы: [{gemRow}, {gemCol}] <-> [{targetRow}, {targetCol}]");
                
                if (enableAnimations)
                {
                    StartCoroutine(AnimatedSwap(gem, neighbor, gemRow, gemCol, targetRow, targetCol));
                }
                else
                {
                    SwapGems(gemRow, gemCol, targetRow, targetCol);
                    gem.ReturnToOriginalPosition();
                }
            }
            else
            {
                gem.ReturnToOriginalPosition();
            }
        }
        else
        {
            gem.ReturnToOriginalPosition();
        }
        
        ResetDrag();
    }

     (int, int) GetTargetPosition(int row, int col, Direction dir)
    {
        switch (dir)
        {
            case Direction.Up:    return (row - 1, col);
            case Direction.Down:  return (row + 1, col);
            case Direction.Left:  return (row, col - 1);
            case Direction.Right: return (row, col + 1);
            default: return (row, col);
        }
    }

     Direction GetSwipeDirection(Vector2 delta)
    {
        Vector2 normalizedDelta = delta.normalized;
        float angle = Mathf.Atan2(normalizedDelta.y, normalizedDelta.x) * Mathf.Rad2Deg;
        if (angle < 0) angle += 360;
        
        if (angle >= 315 || angle < 45) return Direction.Right;
        if (angle >= 45 && angle < 135) return Direction.Up;
        if (angle >= 135 && angle < 225) return Direction.Left;
        return Direction.Down;
    }

    // ===== ОБМЕН КРИСТАЛЛОВ =====

    void SwapGems(int row1, int col1, int row2, int col2)
    {
        Gem gem1 = grid[row1, col1];
        Gem gem2 = grid[row2, col2];
        
        if (gem1 == null || gem2 == null)
        {
            Debug.LogError($"[SWAP] Один из кристаллов null: [{row1}, {col1}] = {gem1}, [{row2}, {col2}] = {gem2}");
            return;
        }
        
        // Меняем спрайты
        Sprite tempSprite = gem1.GetComponent<Image>().sprite;
        gem1.GetComponent<Image>().sprite = gem2.GetComponent<Image>().sprite;
        gem2.GetComponent<Image>().sprite = tempSprite;
        
        // Меняем типы
        int tempType = gem1.gemType;
        gem1.gemType = gem2.gemType;
        gem2.gemType = tempType;
        
        // Меняем местами в grid
        grid[row1, col1] = gem2;
        grid[row2, col2] = gem1;
        
        Debug.Log($"[SWAP] После обмена: в [{row1}, {col1}] теперь кристалл типа {gem2.gemType}, в [{row2}, {col2}] теперь кристалл типа {gem1.gemType}");
    }

    IEnumerator AnimatedSwap(Gem gem1, Gem gem2, int row1, int col1, int row2, int col2)
    {
        // Запускаем анимацию обмена
        bool animationComplete = false;
        
        gem1.AnimatedSwap(gem2, () => animationComplete = true);
        
        yield return new WaitUntil(() => animationComplete);
        
        // После анимации выполняем сам обмен
        SwapGems(row1, col1, row2, col2);
        
        // Возвращаем кристаллы в их клетки
        gem1.ReturnToOriginalPosition();
        gem2.ReturnToOriginalPosition();
        
        // Обновляем позиции в сетке после обмена
        UpdateGridPositions();
    }
    
    void UpdateGridPositions()
    {
        // Проходим по всем кристаллам и обновляем их позиции в сетке
        for (int r = 0; r < rows; r++)
        {
            for (int c = 0; c < columns; c++)
            {
                if (grid[r, c] != null)
                {
                    // Обновляем позицию кристалла в соответствии с его местом в сетке
                    RectTransform rectTransform = grid[r, c].GetComponent<RectTransform>();
                    if (rectTransform != null)
                    {
                        // Устанавливаем позицию в соответствии с сеткой
                        // Используем размеры родительского канваса для корректного позиционирования
                        if (rectTransform.parent != null)
                        {
                            RectTransform parentRect = rectTransform.parent.GetComponent<RectTransform>();
                            if (parentRect != null)
                            {
                                float cellWidth = parentRect.rect.width / columns;
                                float cellHeight = parentRect.rect.height / rows;
                                
                                float x = (c - (columns - 1) / 2f) * cellWidth;
                                float y = (-(r - (rows - 1) / 2f)) * cellHeight;
                                
                                rectTransform.anchoredPosition = new Vector2(x, y);
                                Debug.Log($"[UPDATE] Кристалл [{r}, {c}] обновлен: row={r}, col={c}");
                            }
                            else
                            {
                                Debug.LogError($"[UPDATE] Не удалось найти RectTransform родителя для кристалла [{r}, {c}]");
                            }
                        }
                        else
                        {
                            Debug.LogError($"[UPDATE] У кристалла [{r}, {c}] нет родителя");
                        }
                    }
                    else
                    {
                        Debug.LogError($"[UPDATE] Не удалось найти RectTransform для кристалла [{r}, {c}]");
                    }
                }
            }
        }
        Debug.Log("[UPDATE] Обновление позиций завершено");
    }
    
    void ResetDrag()
    {
        draggedGem = null;
        isDragging = false;
    }
 
    // ===== ОТЛАДКА =====

     [ContextMenu("Показать сетку")]
    public void ShowGrid()
    {
        string gridView = "\n";
        for (int r = 0; r < rows; r++)
        {
            for (int c = 0; c < columns; c++)
            {
                if (grid[r, c] != null)
                    gridView += $"[{grid[r, c].gemType}] ";
                else
                    gridView += "[ ] ";
            }
            gridView += "\n";
        }
        Debug.Log(gridView);
    }

    [ContextMenu("Проверить синхронизацию")]
    public void CheckSync()
    {
        bool isSynced = true;
        
        for (int r = 0; r < rows; r++)
        {
            for (int c = 0; c < columns; c++)
            {
                if (grid[r, c] != null)
                {
                    var (foundR, foundC) = FindGemPosition(grid[r, c]);
                    if (foundR != r || foundC != c)
                    {
                        Debug.LogError($"Несоответствие: кристалл в grid[{r},{c}] найден в [{foundR},{foundC}]");
                        isSynced = false;
                    }
                }
            }
        }
        
        Debug.Log(isSynced ? "Синхронизация в порядке" : "Есть проблемы с синхронизацией");
    }
    
    [ContextMenu("Принудительное обновление позиций")]
    public void ForceUpdatePositions()
    {
        Debug.Log("[FORCE] Принудительное обновление позиций всех кристаллов...");
        UpdateGridPositions();
    }

    enum Direction { Up, Down, Left, Right }
 
}
