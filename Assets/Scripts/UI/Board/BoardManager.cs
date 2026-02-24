using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;  // нужно для Mouse.current
using System.Collections;
using System.Collections.Generic;


public class BoardManager : MonoBehaviour
{
    public static BoardManager Instance;

    public int columns = 7;
    public int rows = 5;

    
    private Gem[,] grid;
    private Gem draggedGem;             // кристалл который тащим
    private bool isDragging = false;    // тащим ли сейчас
    private Vector2 dragStartPos;       // позиция начала таскания
    private float dragThreshold = 30f;  // минимальное расстояние для свайпа (в пикселях)

    private Gem firstSelected;
    private Gem secondSelected;

    void Awake()
    {
        Instance = this;
        grid = new Gem[rows, columns];
    }

       
    public void RegisterGem (Gem gem, int roow, int col)
    {
        grid[roow, col] = gem;
        
        // Обновляем позицию кристалла в соответствии с сеткой
        if (gem != null)
        {
            gem.row = roow;
            gem.column = col;
            Debug.Log($"[REGISTER] Кристалл зарегистрирован на позиции: [{roow}, {col}]");
        }
    }
   
    // Начало перетаскивания - вызывается из Gem
    public void StartDragging (Gem gem)
    {
        if (isDragging) return; // Защита от повторного начала перетаскивания
        
        // Проверка на null
        if (gem == null)
        {
            Debug.LogError("StartDragging: gem is null");
            return;
        }
        
        // Немедленно обновляем позицию кристалла в соответствии с его реальным положением в сетке
        UpdateGemPosition(gem);
        
        draggedGem = gem;
        isDragging = true;
        dragStartPos = Mouse.current.position.ReadValue();
        
        // Дебаг: выводим позицию выбранного кристалла
        Debug.Log($"[DRAG] Выбран кристалл на позиции: [{gem.row}, {gem.column}] (тип: {gem.gemType})");
    }
    
    // Конец перетаскивания - вызывается из Gem
    public void StopDragging (Gem gem)
    {
        if (!isDragging || draggedGem != gem) return;

        // Дополнительная проверка на null
        if (gem == null)
        {
            Debug.LogError("StopDragging: gem is null");
            ResetDrag();
            return;
        }
        
        // Немедленно обновляем позицию кристалла в соответствии с его реальным положением в сетке
        UpdateGemPosition(gem);

        Vector2 dragEndPos = Mouse.current.position.ReadValue();
        Vector2 dragDelta = dragEndPos - dragStartPos;

        if (dragDelta.magnitude < dragThreshold)
        {
            ResetDrag();
            return;
        }

        Direction swipeDir = GetSwipeDirection(dragDelta);
        Gem neighbor = GetNeighborInDirection(gem, swipeDir);

        // Дебаг: выводим информацию о свайпе и целевом кристалле
        string directionName = swipeDir.ToString();
        Debug.Log($"[SWIPE] Свайп в направлении: {directionName} (длина: {dragDelta.magnitude:F1}px)");
        
        if (neighbor != null)
        {
            Debug.Log($"[SWAP] Меняем кристалл [{gem.row}, {gem.column}] (тип: {gem.gemType}) с кристаллом [{neighbor.row}, {neighbor.column}] (тип: {neighbor.gemType})");
            SwapGems(gem, neighbor);
            
            // Дополнительная проверка синхронизации после обновления
            Debug.Log($"[SYNC] После обновления: кристалл [{gem.row}, {gem.column}] (тип: {gem.gemType})");
        }
        else
        {
            Debug.Log($"[SWIPE] Нет соседа в направлении {directionName} для кристалла [{gem.row}, {gem.column}]");
        }
        
        ResetDrag();
    }
    // Определяем направление свайпа
    Direction GetSwipeDirection(Vector2 delta)
    {
        // Нормализуем вектор для более точного определения направления
        Vector2 normalizedDelta = delta.normalized;
        
        // Определяем угол в градусах
        float angle = Mathf.Atan2(normalizedDelta.y, normalizedDelta.x) * Mathf.Rad2Deg;
        
        // Приводим угол к диапазону 0-360
        if (angle < 0) angle += 360;
        
        // Определяем направление по углам
        if (angle >= 315 || angle < 45) return Direction.Right;      // 315-360, 0-45
        if (angle >= 45 && angle < 135) return Direction.Up;        // 45-135
        if (angle >= 135 && angle < 225) return Direction.Left;     // 135-225
        return Direction.Down;                                      // 225-315
    }
    // Получаем соседа в направлении
    Gem GetNeighborInDirection(Gem gem, Direction dir)
    {
        // Проверяем, что gem не null
        if (gem == null)
        {
            Debug.LogError("GetNeighborInDirection: gem is null");
            return null;
        }

        int targetRow = gem.row;
        int targetCol = gem.column;

        switch (dir)
        {
            case Direction.Up:    targetRow--; break;
            case Direction.Down:  targetRow++; break;
            case Direction.Left:  targetCol--; break;
            case Direction.Right: targetCol++; break;
        }

        // Проверяем границы
        if (targetRow >= 0 && targetRow < rows && targetCol >= 0 && targetCol < columns)
        {
            return grid[targetRow, targetCol];
        }

        return null;
    }

    void ResetDrag()
    {
        draggedGem = null;
        isDragging = false;
    }

    // Метод для обновления позиции одного кристалла в соответствии с сеткой
    void UpdateGemPosition(Gem gem)
    {
        if (gem == null) return;
        
        // Находим реальное положение кристалла в сетке
        for (int r = 0; r < rows; r++)
        {
            for (int c = 0; c < columns; c++)
            {
                if (grid[r, c] == gem)
                {
                    // Обновляем координаты кристалла в соответствии с его реальным положением в сетке
                    gem.row = r;
                    gem.column = c;
                    
                    // Дебаг: выводим обновленные координаты
                    Debug.Log($"[UPDATE] Кристалл [{r}, {c}] обновлен: row={gem.row}, col={gem.column}");
                    return;
                }
            }
        }
        
        // Если кристалл не найден в сетке
        Debug.LogWarning($"[UPDATE] Кристалл не найден в сетке: row={gem.row}, col={gem.column}");
        
        // Дополнительная проверка: может быть кристалл в сетке, но с другими координатами?
        for (int r = 0; r < rows; r++)
        {
            for (int c = 0; c < columns; c++)
            {
                if (grid[r, c] != null && grid[r, c].row == gem.row && grid[r, c].column == gem.column)
                {
                    Debug.LogWarning($"[UPDATE] Найден кристалл с такими же координатами в [{r}, {c}]: row={grid[r, c].row}, col={grid[r, c].column}");
                }
            }
        }
    }

    // Метод для обновления позиций всех кристаллов в соответствии с сеткой
    public void UpdateGemPositions()
    {
        Debug.Log("[UPDATE] Начинаем обновление позиций всех кристаллов...");
        
        for (int r = 0; r < rows; r++)
        {
            for (int c = 0; c < columns; c++)
            {
                if (grid[r, c] != null)
                {
                    // Обновляем координаты кристалла в соответствии с его позицией в сетке
                    grid[r, c].row = r;
                    grid[r, c].column = c;
                    
                    // Дебаг: выводим обновленные координаты
                    Debug.Log($"[UPDATE] Кристалл [{r}, {c}] обновлен: row={grid[r, c].row}, col={grid[r, c].column}");
                }
            }
        }
        
        Debug.Log("[UPDATE] Обновление позиций завершено");
    }

    void SwapGems(Gem a, Gem b)
    {
        // Проверяем, что кристаллы действительно соседи
        if (!AreNeighbors(a, b)) return;

        // Дебаг: выводим информацию о позициях до обмена
        Debug.Log($"[SWAP] Обмен: [{a.row}, {a.column}] <-> [{b.row}, {b.column}]");

        // Сохраняем ссылки на компоненты для оптимизации
        Image imgA = a.GetComponent<Image>();
        Image imgB = b.GetComponent<Image>();

        // Меняем спрайты
        Sprite tempSprite = imgA.sprite;
        imgA.sprite = imgB.sprite;
        imgB.sprite = tempSprite;

        // Меняем типы
        int tempType = a.gemType;
        a.gemType = b.gemType;
        b.gemType = tempType;

        // Сохраняем текущие позиции
        int aRow = a.row, aCol = a.column;
        int bRow = b.row, bCol = b.column;

        // Обновляем массив
        grid[aRow, aCol] = b;
        grid[bRow, bCol] = a;

        // Немедленно обновляем позиции обоих кристаллов в соответствии с их новым положением в сетке
        UpdateGemPosition(a);
        UpdateGemPosition(b);

        // Дебаг: выводим информацию о позициях после обмена
        Debug.Log($"[SWAP] После обмена: [{a.row}, {a.column}] (тип: {a.gemType}) <-> [{b.row}, {b.column}] (тип: {b.gemType})");
    }

    bool AreNeighbors(Gem a, Gem b)
    {
        int rowDiff = Mathf.Abs(a.row - b.row);
        int colDiff = Mathf.Abs(a.column - b.column);
        
        // Соседи только если разница в 1 по одной координате и 0 по другой
        return (rowDiff == 1 && colDiff == 0) || (rowDiff == 0 && colDiff == 1);
    }

    // Для отладки - перечисление направлений
    enum Direction { Up, Down, Left, Right }

    // Метод для ручного вызова обновления позиций (для тестирования)
    public void ForceUpdatePositions()
    {
        Debug.Log("[FORCE UPDATE] Принудительное обновление позиций всех кристаллов");
        
        // Сначала обновляем позиции всех кристаллов
        UpdateGemPositions();
        
        // Затем дополнительно обновляем позицию каждого кристалла в массиве
        for (int r = 0; r < rows; r++)
        {
            for (int c = 0; c < columns; c++)
            {
                if (grid[r, c] != null)
                {
                    UpdateGemPosition(grid[r, c]);
                }
            }
        }
        
        Debug.Log("[FORCE UPDATE] Принудительное обновление завершено");
    }

    // Метод для проверки синхронизации позиций (для тестирования)
    public void CheckPositionSync()
    {
        Debug.Log("[CHECK] Проверка синхронизации позиций кристаллов...");
        bool isSynced = true;
        
        for (int r = 0; r < rows; r++)
        {
            for (int c = 0; c < columns; c++)
            {
                if (grid[r, c] != null)
                {
                    if (grid[r, c].row != r || grid[r, c].column != c)
                    {
                        Debug.LogError($"[CHECK] Несоответствие позиций: кристалл в сетке [{r}, {c}] имеет координаты [{grid[r, c].row}, {grid[r, c].column}]");
                        isSynced = false;
                    }
                }
            }
        }
        
        if (isSynced)
        {
            Debug.Log("[CHECK] Все позиции синхронизированы корректно!");
        }
        else
        {
            Debug.LogError("[CHECK] Обнаружены несоответствия позиций!");
        }
    }

    // Метод для отображения текущих позиций всех кристаллов (для тестирования)
    public void ShowCurrentPositions()
    {
        Debug.Log("[SHOW] Текущие позиции всех кристаллов:");
        
        for (int r = 0; r < rows; r++)
        {
            string rowInfo = $"[ROW {r}] ";
            for (int c = 0; c < columns; c++)
            {
                if (grid[r, c] != null)
                {
                    rowInfo += $"[{c}:{grid[r, c].gemType}({grid[r, c].row},{grid[r, c].column})] ";
                }
                else
                {
                    rowInfo += $"[  -  ] ";
                }
            }
            Debug.Log(rowInfo);
        }
    }

    // Метод для отображения детальной информации о позициях (для тестирования)
    public void ShowDetailedPositionInfo()
    {
        Debug.Log("[DEBUG] === ДЕТАЛЬНАЯ ИНФОРМАЦИЯ О ПОЗИЦИЯХ ===");
        
        for (int r = 0; r < rows; r++)
        {
            for (int c = 0; c < columns; c++)
            {
                if (grid[r, c] != null)
                {
                    Gem gem = grid[r, c];
                    Debug.Log($"[DEBUG] Сетка [{r}, {c}]: кристалл {gem.gemType}, координаты кристалла ({gem.row}, {gem.column}), соответствие: {(gem.row == r && gem.column == c ? "ДА" : "НЕТ")}");
                }
            }
        }
        
        Debug.Log("[DEBUG] === ПРОВЕРКА ДУБЛИКАТОВ ===");
        for (int r1 = 0; r1 < rows; r1++)
        {
            for (int c1 = 0; c1 < columns; c1++)
            {
                if (grid[r1, c1] != null)
                {
                    for (int r2 = 0; r2 < rows; r2++)
                    {
                        for (int c2 = 0; c2 < columns; c2++)
                        {
                            if (r1 != r2 || c1 != c2)
                            {
                                if (grid[r2, c2] != null && grid[r1, c1] == grid[r2, c2])
                                {
                                    Debug.LogError($"[DEBUG] ДУБЛИКАТ: кристалл {grid[r1, c1].gemType} найден в [{r1}, {c1}] и [{r2}, {c2}]");
                                }
                            }
                        }
                    }
                }
            }
        }
        
        Debug.Log("[DEBUG] === ПРОВЕРКА ОТСУТСТВУЮЩИХ КРИСТАЛЛОВ ===");
        for (int r = 0; r < rows; r++)
        {
            for (int c = 0; c < columns; c++)
            {
                if (grid[r, c] != null)
                {
                    bool found = false;
                    for (int r2 = 0; r2 < rows; r2++)
                    {
                        for (int c2 = 0; c2 < columns; c2++)
                        {
                            if (grid[r2, c2] != null && grid[r2, c2].row == r && grid[r2, c2].column == c)
                            {
                                found = true;
                                break;
                            }
                        }
                        if (found) break;
                    }
                    if (!found)
                    {
                        Debug.LogWarning($"[DEBUG] Кристалл с координатами ({r}, {c}) не найден в сетке");
                    }
                }
            }
        }
        
        Debug.Log("[DEBUG] === ДЕТАЛЬНАЯ ИНФОРМАЦИЯ ЗАВЕРШЕНА ===");
    }
}
