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

    [Header("Turn & Timer Logic")]
    public float turnDuration = 15f;
    public int turnCount = 0;
    private float currentTimer;
    private bool isTimerRunning = false;
    private bool isTurnActive = true;

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

        // Проверяем все возможные пересечения для избежания случайных совпадений при падении
        for (int i = availableColors.Count - 1; i >= 0; i--)
        {
            if (CreatesMatch(x, y, (Gem.GemColor)availableColors[i]))
            {
                availableColors.RemoveAt(i);
            }
        }

        if (availableColors.Count == 0)
        {
            return Random.Range(0, gemSprites.Length);
        }

        return availableColors[Random.Range(0, availableColors.Count)];
    }

    private bool CreatesMatch(int x, int y, Gem.GemColor color)
    {
        // Горизонталь
        if (x >= 2 && allGems[x - 1, y] != null && allGems[x - 1, y].color == color &&
            allGems[x - 2, y] != null && allGems[x - 2, y].color == color) return true;

        if (x <= width - 3 && allGems[x + 1, y] != null && allGems[x + 1, y].color == color &&
            allGems[x + 2, y] != null && allGems[x + 2, y].color == color) return true;

        if (x >= 1 && x < width - 1 && allGems[x - 1, y] != null && allGems[x - 1, y].color == color &&
            allGems[x + 1, y] != null && allGems[x + 1, y].color == color) return true;

        // Вертикаль
        if (y >= 2 && allGems[x, y - 1] != null && allGems[x, y - 1].color == color &&
            allGems[x, y - 2] != null && allGems[x, y - 2].color == color) return true;

        if (y <= height - 3 && allGems[x, y + 1] != null && allGems[x, y + 1].color == color &&
            allGems[x, y + 2] != null && allGems[x, y + 2].color == color) return true;

        if (y >= 1 && y < height - 1 && allGems[x, y - 1] != null && allGems[x, y - 1].color == color &&
            allGems[x, y + 1] != null && allGems[x, y + 1].color == color) return true;

        return false;
    }

    public void SwapGems(Gem currentGem, Vector2 direction)
    {
        if (!isTurnActive || currentGem.isMatched) return;

        int targetX = currentGem.xIndex + (int)direction.x;
        int targetY = currentGem.yIndex + (int)direction.y;

        // Проверяем, не выходит ли свайп за границы экрана
        if (targetX < 0 || targetX >= width || targetY < 0 || targetY >= height)
        {
            return; // Игнорируем неверный свайп
        }

        Gem targetGem = allGems[targetX, targetY];

        // Проверяем, не заблокирован ли целевой кристалл
        if (targetGem == null || targetGem.isMatched) return;

        // Запускаем таймер, если это первый свайп хода
        if (!isTimerRunning && isTurnActive)
        {
            isTimerRunning = true;
            currentTimer = turnDuration;
            Debug.Log($"Таймер запущен на {turnDuration} секунд!");
        }

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

        StartCoroutine(CheckMatchesAfterSwap(currentGem, targetGem));
    }

    private IEnumerator CheckMatchesAfterSwap(Gem gem1, Gem gem2)
    {
        yield return new WaitForSeconds(swapDuration);

        bool hasMatches = FindAllMatches();

        // Свайп, который не приводит к комбинации, завершает ход
        if (!hasMatches && isTimerRunning)
        {
            Debug.Log("Нет комбинаций после свайпа! Возвращаем обратно и завершаем ход.");

            // Логический возврат
            int tempX = gem1.xIndex;
            int tempY = gem1.yIndex;
            gem1.xIndex = gem2.xIndex;
            gem1.yIndex = gem2.yIndex;
            gem2.xIndex = tempX;
            gem2.yIndex = tempY;

            allGems[gem1.xIndex, gem1.yIndex] = gem1;
            allGems[gem2.xIndex, gem2.yIndex] = gem2;

            // Визуальный возврат
            StartCoroutine(MoveGemVisual(gem1, gem2.transform.position));
            StartCoroutine(MoveGemVisual(gem2, gem1.transform.position));

            // Ждем завершения анимации возврата
            yield return new WaitForSeconds(swapDuration);

            isTimerRunning = false;
            isTurnActive = false;
            ResolveTurn();
        }
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

    private void Update()
    {
        if (isTimerRunning)
        {
            currentTimer -= Time.deltaTime;

            if (currentTimer <= 0)
            {
                isTimerRunning = false;
                isTurnActive = false;
                ResolveTurn();
            }
        }
    }

    private bool FindAllMatches()
    {
        bool foundNewMatch = false;

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                Gem currentGem = allGems[x, y];
                if (currentGem != null)
                {
                    // Горизонтальный поиск
                    if (x > 0 && x < width - 1)
                    {
                        Gem leftGem = allGems[x - 1, y];
                        Gem rightGem = allGems[x + 1, y];
                        if (leftGem != null && rightGem != null)
                        {
                            if (leftGem.color == currentGem.color && rightGem.color == currentGem.color)
                            {
                                if (!leftGem.isMatched || !currentGem.isMatched || !rightGem.isMatched)
                                {
                                    leftGem.SetMatched();
                                    currentGem.SetMatched();
                                    rightGem.SetMatched();
                                    foundNewMatch = true;
                                }
                            }
                        }
                    }

                    // Вертикальный поиск
                    if (y > 0 && y < height - 1)
                    {
                        Gem downGem = allGems[x, y - 1];
                        Gem upGem = allGems[x, y + 1];
                        if (downGem != null && upGem != null)
                        {
                            if (downGem.color == currentGem.color && upGem.color == currentGem.color)
                            {
                                if (!downGem.isMatched || !currentGem.isMatched || !upGem.isMatched)
                                {
                                    downGem.SetMatched();
                                    currentGem.SetMatched();
                                    upGem.SetMatched();
                                    foundNewMatch = true;
                                }
                            }
                        }
                    }
                }
            }
        }

        return foundNewMatch;
    }

    private void ResolveTurn()
    {
        turnCount++;
        Dictionary<Gem.GemColor, int> scores = new Dictionary<Gem.GemColor, int>();

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                Gem gem = allGems[x, y];
                if (gem != null && gem.isMatched)
                {
                    if (!scores.ContainsKey(gem.color))
                    {
                        scores[gem.color] = 0;
                    }
                    scores[gem.color]++;

                    Destroy(gem.gameObject);
                    allGems[x, y] = null;
                }
            }
        }

        Debug.Log($"--- Завершен ход {turnCount} ---");
        foreach (var kvp in scores)
        {
            Debug.Log($"Очки за цвет {kvp.Key}: {kvp.Value}");
        }
        if (scores.Count == 0)
        {
            Debug.Log("Нет собранных комбинаций.");
        }
        Debug.Log("-------------------------");

        StartCoroutine(RefillBoard());
    }

    private IEnumerator RefillBoard()
    {
        // 1. Сдвигаем оставшиеся кристаллы вниз
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                if (allGems[x, y] == null)
                {
                    // Ищем первый не null кристалл выше
                    for (int yTarget = y + 1; yTarget < height; yTarget++)
                    {
                        if (allGems[x, yTarget] != null)
                        {
                            Gem gemToMove = allGems[x, yTarget];

                            // Перемещаем в массиве
                            allGems[x, y] = gemToMove;
                            allGems[x, yTarget] = null;

                            // Обновляем логические индексы
                            gemToMove.yIndex = y;

                            // Вычисляем новую позицию в мире (учитывая смещение)
                            Vector3 offset = new Vector3((width - 1) * tileWidth / 2f, (height - 1) * tileHeight / 2f, 0);
                            Vector3 targetPos = transform.position + new Vector3(x * tileWidth, y * tileHeight, 0) - offset;

                            // Запускаем визуальное перемещение вниз с небольшой задержкой "вразнобой"
                            float dropDelay = Random.Range(0f, 0.15f);
                            LeanTween.move(gemToMove.gameObject, targetPos, swapDuration * 1.5f)
                                     .setDelay(dropDelay)
                                     .setEaseInOutSine(); // Мягкое опускание

                            break;
                        }
                    }
                }
            }
        }

        yield return new WaitForSeconds(swapDuration);

        // 2. Генерируем новые кристаллы на пустых местах сверху
        Vector3 globalOffset = new Vector3((width - 1) * tileWidth / 2f, (height - 1) * tileHeight / 2f, 0);

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                if (allGems[x, y] == null)
                {
                    // Точка появления (сразу на нужном месте)
                    Vector3 targetPos = transform.position + new Vector3(x * tileWidth, y * tileHeight, 0) - globalOffset;

                    GameObject gemObject = Instantiate(gemPrefab, targetPos, Quaternion.identity);
                    gemObject.transform.SetParent(this.transform);
                    gemObject.name = $"Gem_{x}_{y}";

                    // Запоминаем изначальный размер префаба
                    Vector3 originalScale = gemObject.transform.localScale;

                    // Устанавливаем размер в 0 для анимации появления (вырастания)
                    gemObject.transform.localScale = Vector3.zero;

                    Gem gem = gemObject.GetComponent<Gem>();
                    if (gem != null)
                    {
                        // Запрашиваем цвет, который гарантированно не создаст совпадений
                        int safeColorIndex = GetValidColorIndex(x, y);
                        Gem.GemColor randomColor = (Gem.GemColor)safeColorIndex;
                        Sprite randomSprite = gemSprites[safeColorIndex];

                        gem.Setup(randomColor, randomSprite, x, y, this);
                        allGems[x, y] = gem;

                        float dropDelay = Random.Range(0f, 0.2f) + (height - y) * 0.05f; // Задержка появления

                        // Анимируем размер (вырастание) до исходного масштаба префаба
                        LeanTween.scale(gemObject, originalScale, swapDuration * 1.5f)
                                 .setDelay(dropDelay)
                                 .setEaseOutBack(); // Плавное увеличение
                    }
                }
            }
        }

        // Ждем дольше, чтобы все кристаллы успели допрыгать
        yield return new WaitForSeconds(swapDuration * 2.5f);

        isTurnActive = true;
    }
}
