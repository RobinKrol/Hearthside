using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoardManager : MonoBehaviour
{
    [Header("Board Dimensions")]
    public int width = 5;  // столбцы
    public int height = 7; // ряды

    [Header("Tile Settings")]
    public float tileWidth = 1.0f;  // расстояние между центрами по X
    public float tileHeight = 1.0f; // расстояние между центрами по Y
    public float swapDuration = 0.25f; // Длительность анимации обмена

    [Header("References")]
    public GameObject gemPrefab;

    [Header("Gem Graphics (5 Colors)")]
    public Sprite[] gemSprites;    // Перетащите сюда 5 спрайтов кристаллов

    private Gem[,] allGems;

    void Start()
    {
        GenerateBoard();
    }

    public void ClearBoard()
    {
        // Удаляем все объекты-кристаллы (дети этого объекта)
        foreach (Transform child in transform)
        {
            Destroy(child.gameObject);
        }

        // Очищаем массив ссылок
        if (allGems != null)
        {
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    allGems[x, y] = null;
                }
            }
        }
    }

    public void GenerateBoard()
    {
        ClearBoard();

        // Инициализируем массив, если он еще не создан
        if (allGems == null)
        {
            allGems = new Gem[width, height];
        }

        // Проверяем, что все ссылки назначены
        if (gemPrefab == null || gemSprites.Length == 0)
        {
            Debug.LogError("BoardManager: Не все ссылки назначены в инспекторе!");
            return;
        }

        // Вычисляем смещение, чтобы центрировать поле относительно позиции BoardManager
        Vector3 offset = new Vector3(
            (width - 1) * tileWidth / 2f,
            (height - 1) * tileHeight / 2f,
            0
        );

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                // Вычисляем позицию в мире (относительно этого BoardManager)
                Vector3 spawnPosition = transform.position + new Vector3(x * tileWidth, y * tileHeight, 0) - offset;

                // Создаем кристалл
                GameObject gemObject = Instantiate(gemPrefab, spawnPosition, Quaternion.identity);
                // Делаем BoardManager родителем для порядка в иерархии
                gemObject.transform.SetParent(this.transform);
                gemObject.name = $"Gem_{x}_{y}";

                Gem gem = gemObject.GetComponent<Gem>();
                if (gem != null)
                {
                    // Выбираем случайный цвет, избегая совпадений 3 в ряд
                    int randomColorIndex = GetValidColorIndex(x, y);
                    Gem.GemColor randomColor = (Gem.GemColor)randomColorIndex;
                    Sprite randomSprite = gemSprites[randomColorIndex];

                    // Настраиваем кристалл (передаем координаты x, y и ссылку на BoardManager)
                    gem.Setup(randomColor, randomSprite, x, y, this);

                    allGems[x, y] = gem;
                }
                else
                {
                    Debug.LogError("На префабе кристалла нет компонента Gem!");
                }
            }
        }
    }

    private int GetValidColorIndex(int x, int y)
    {
        List<int> availableColors = new List<int>();
        for (int i = 0; i < gemSprites.Length; i++)
        {
            availableColors.Add(i);
        }

        // Проверяем горизонталь (влево)
        if (x >= 2)
        {
            Gem gem1 = allGems[x - 1, y];
            Gem gem2 = allGems[x - 2, y];
            if (gem1 != null && gem2 != null && gem1.color == gem2.color)
            {
                availableColors.Remove((int)gem1.color);
            }
        }

        // Проверяем вертикаль (вниз)
        if (y >= 2)
        {
            Gem gem1 = allGems[x, y - 1];
            Gem gem2 = allGems[x, y - 2];
            if (gem1 != null && gem2 != null && gem1.color == gem2.color)
            {
                availableColors.Remove((int)gem1.color);
            }
        }

        return availableColors[Random.Range(0, availableColors.Count)];
    }

    public void SwapGems(Gem currentGem, Vector2 direction)
    {
        int targetX = currentGem.xIndex + (int)direction.x;
        int targetY = currentGem.yIndex + (int)direction.y;

        // Проверяем, не выходит ли свайп за границы экрана
        if (targetX < 0 || targetX >= width || targetY < 0 || targetY >= height)
        {
            return; // Игнорируем неверный свайп
        }

        Gem targetGem = allGems[targetX, targetY];

        // 1. Логический обмен в массиве
        allGems[currentGem.xIndex, currentGem.yIndex] = targetGem;
        allGems[targetX, targetY] = currentGem;

        // Обновляем индексы внутри самих кристаллов
        int tempX = currentGem.xIndex;
        int tempY = currentGem.yIndex;

        currentGem.xIndex = targetGem.xIndex;
        currentGem.yIndex = targetGem.yIndex;

        targetGem.xIndex = tempX;
        targetGem.yIndex = tempY;

        // 2. Визуальный обмен (анимация через корутину)
        StartCoroutine(MoveGemVisual(currentGem, targetGem.transform.position));
        StartCoroutine(MoveGemVisual(targetGem, currentGem.transform.position));
    }

    // Анимация обмена

    private IEnumerator MoveGemVisual(Gem gem, Vector3 targetPosition)
    {
        float elapsedTime = 0f;
        Vector3 startingPosition = gem.transform.position;

        while (elapsedTime < swapDuration)
        {
            // Плавная интерполяция
            gem.transform.position = Vector3.Lerp(startingPosition, targetPosition, elapsedTime / swapDuration);
            elapsedTime += Time.deltaTime;
            yield return null; // Ждем следующий кадр
        }

        // Фиксируем конечную позицию точно
        gem.transform.position = targetPosition;
    }
}
