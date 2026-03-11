using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class BoardManager : MonoBehaviour
{
    [Header("Board Dimensions")]
    public int width = 5;  // столбцы
    public int height = 7; // ряды

    [Header("Tile Settings")]
    public float tileWidth = 1.0f;  // расстояние между центрами по X
    public float tileHeight = 1.0f; // расстояние между центрами по Y
    public float swapDuration = 0.25f; // Длительность анимации обмена

    [System.Serializable]
    public struct FruitData
    {
        public Gem.GemColor color;
        public Sprite fruitSprite;
    }

    [System.Serializable]
    public struct GemVisualData
    {
        public Gem.GemColor color;
        public Sprite defaultSprite;
        public Sprite glowSprite;
        public Color lineColor; // Цвет соединяющей линии для комбо
    }

    [Header("References")]
    public GameObject gemPrefab;

    [Header("Gem Graphics (5 Colors)")]
    // Старый массив спрайтов больше не используется напрямую, данные берутся из gemVisuals
    public GemVisualData[] gemVisuals;
    public FruitData[] fruitTypes; // Настройка фруктов для каждого цвета

    [Header("Combo Lines")]
    public Material lineMaterial; // Материал для линии (желательно Sprite-Default или Unlit)
    public float lineWidth = 0.15f;
    private List<GameObject> activeComboLines = new List<GameObject>(); // Список активных линий для удаления в конце хода

    [Header("Game State Integration")]
    public GameManager gameManager; // Менеджер состояния игры (счетчик ходов, таймер)

    [Header("Heroes Integration")]
    public HeroManager heroManager; // Менеджер для распределения очков между героями

    [Header("Environment")]
    public EnvironmentManager environmentManager;

    private bool isAnimating = false; // Блокировка ввода во время анимаций (корутин)

    private Gem[,] allGems;

    // Object Pooling (Оптимизация)
    private Queue<Gem> gemPool = new Queue<Gem>();
    private Queue<GameObject> linePool = new Queue<GameObject>();

    void Start()
    {
        GenerateBoard();
    }

    public void ClearBoard()
    {
        // Вместо удаления (Destroy) всех дочерних объектов, мы отключаем их и сортируем в пулы
        if (allGems != null)
        {
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    if (allGems[x, y] != null)
                    {
                        ReturnGem(allGems[x, y]);
                    }
                }
            }
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
        if (gemPrefab == null || gemVisuals.Length == 0)
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

                // Получаем кристалл из пула (вместо Instantiate)
                Gem gem = GetGem(spawnPosition);
                
                if (gem != null)
                {
                    gem.gameObject.name = $"Gem_{x}_{y}";
                    // Выбираем случайный цвет, избегая совпадений 3 в ряд
                    int randomColorIndex = GetValidColorIndex(x, y);
                    Gem.GemColor randomColor = (Gem.GemColor)randomColorIndex;

                    // Получаем визуальные данные для этого цвета
                    Sprite randomSprite = null;
                    Sprite glowSprite = null;

                    foreach (var visual in gemVisuals)
                    {
                        if (visual.color == randomColor)
                        {
                            randomSprite = visual.defaultSprite;
                            glowSprite = visual.glowSprite;
                            break;
                        }
                    }

                    Sprite fruitSprite = null;
                    // Ищем фрукт для этого цвета
                    foreach (var fruitData in fruitTypes)
                    {
                        if (fruitData.color == randomColor)
                        {
                            fruitSprite = fruitData.fruitSprite;
                            break;
                        }
                    }

                    // Настраиваем кристалл (передаем координаты x, y и ссылку на BoardManager)
                    gem.Setup(randomColor, randomSprite, glowSprite, x, y, this, fruitSprite);

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
        // Перебираем все возможные цвета из перечисления, а не длину массива (чтобы 100% совпадало с enum)
        foreach (int colorVal in System.Enum.GetValues(typeof(Gem.GemColor)))
        {
            availableColors.Add(colorVal);
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
            return Random.Range(0, System.Enum.GetValues(typeof(Gem.GemColor)).Length);
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
        // Не разрешаем свайп, если ход заблокирован, кристалл уже собран, или идет анимация
        if ((gameManager != null && !gameManager.IsTurnActive()) || currentGem.isMatched || isAnimating) return;

        // Жесткая защита от свайпа в последнюю миллисекунду перед концом таймера (состояние гонки)
        if (gameManager != null && gameManager.IsTimerRunning() && gameManager.GetCurrentTimer() <= 0.1f)
        {
            Debug.Log("Свайп отклонен: таймер почти истек!");
            return;
        }

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

        isAnimating = true; // Блокируем ввод на время анимации обмена

        // 2. Визуальный обмен через LeanTween
        LeanTween.move(currentGem.gameObject, targetGem.transform.position, swapDuration);
        LeanTween.move(targetGem.gameObject, currentGem.transform.position, swapDuration);

        StartCoroutine(CheckMatchesAfterSwap(currentGem, targetGem));
    }

    private IEnumerator CheckMatchesAfterSwap(Gem gem1, Gem gem2)
    {
        yield return new WaitForSeconds(swapDuration);

        bool hasMatches = FindMatchesAt(new List<Gem> { gem1, gem2 });

        if (hasMatches)
        {
            if (gameManager != null)
            {
                gameManager.OnFirstComboMatch();
            }

            isAnimating = false; // Разрешаем ввод, так как свап завершился комбоком (основной таймер идет)
        }
        else
        {
            Debug.Log("Нет комбинаций после свайпа! Возвращаем обратно.");

            // Логический возврат
            int tempX = gem1.xIndex;
            int tempY = gem1.yIndex;
            gem1.xIndex = gem2.xIndex;
            gem1.yIndex = gem2.yIndex;
            gem2.xIndex = tempX;
            gem2.yIndex = tempY;

            allGems[gem1.xIndex, gem1.yIndex] = gem1;
            allGems[gem2.xIndex, gem2.yIndex] = gem2;

            // Визуальный возврат через LeanTween
            LeanTween.move(gem1.gameObject, gem2.transform.position, swapDuration);
            LeanTween.move(gem2.gameObject, gem1.transform.position, swapDuration);

            // Ждем завершения анимации возврата (штрафует игроков только потерей времени, если таймер уже шел)
            yield return new WaitForSeconds(swapDuration);

            isAnimating = false; // Разрешаем ввод после возврата
        }
    }



    public void OnTurnTimeUp()
    {
        isAnimating = true;   // Включаем статус анимации, чтобы ничего не двигалось в момент расчета
        ResolveTurn();
    }

    private bool FindAllMatches()
    {
        List<Gem> allGemsList = new List<Gem>();
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                if (allGems[x, y] != null) allGemsList.Add(allGems[x, y]);
            }
        }
        return FindMatchesAt(allGemsList);
    }

    private bool FindMatchesAt(List<Gem> gemsToCheck)
    {
        bool foundNewMatch = false;

        foreach (Gem gem in gemsToCheck)
        {
            if (gem == null) continue;
            
            int x = gem.xIndex;
            int y = gem.yIndex;
            Gem.GemColor color = gem.color;

            // Горизонтальный поиск
            List<Gem> horizGems = new List<Gem> { gem };
            for (int i = x - 1; i >= 0; i--)
            {
                if (allGems[i, y] != null && allGems[i, y].color == color) horizGems.Add(allGems[i, y]);
                else break;
            }
            for (int i = x + 1; i < width; i++)
            {
                if (allGems[i, y] != null && allGems[i, y].color == color) horizGems.Add(allGems[i, y]);
                else break;
            }

            if (horizGems.Count >= 3)
            {
                horizGems.Sort((a, b) => a.xIndex.CompareTo(b.xIndex));
                bool hasNew = false;
                foreach (var g in horizGems) if (!g.isMatched) hasNew = true;

                if (hasNew)
                {
                    foreach (var g in horizGems) g.SetMatched();
                    DrawComboLine(horizGems, color);
                    foundNewMatch = true;
                }
            }

            // Вертикальный поиск
            List<Gem> vertGems = new List<Gem> { gem };
            for (int j = y - 1; j >= 0; j--)
            {
                if (allGems[x, j] != null && allGems[x, j].color == color) vertGems.Add(allGems[x, j]);
                else break;
            }
            for (int j = y + 1; j < height; j++)
            {
                if (allGems[x, j] != null && allGems[x, j].color == color) vertGems.Add(allGems[x, j]);
                else break;
            }

            if (vertGems.Count >= 3)
            {
                vertGems.Sort((a, b) => a.yIndex.CompareTo(b.yIndex));
                bool hasNew = false;
                foreach (var g in vertGems) if (!g.isMatched) hasNew = true;

                if (hasNew)
                {
                    foreach (var g in vertGems) g.SetMatched();
                    DrawComboLine(vertGems, color);
                    foundNewMatch = true;
                }
            }
        }

        return foundNewMatch;
    }

    private void DrawComboLine(List<Gem> matchedGems, Gem.GemColor color)
    {
        if (matchedGems.Count < 2) return;

        // Ищем цвет линии в настройках
        Color lineColor = Color.white;
        foreach (var data in gemVisuals)
        {
            if (data.color == color)
            {
                lineColor = data.lineColor;
                break;
            }
        }

        // Берем объект линии из пула
        GameObject lineObj = GetLine();

        LineRenderer lr = lineObj.GetComponent<LineRenderer>();
        lr.positionCount = matchedGems.Count;

        // Настраиваем внешний вид линии
        lr.startWidth = lineWidth;
        lr.endWidth = lineWidth;
        lr.material = lineMaterial;

        // Настройка цветов (градиент)
        lr.startColor = lineColor;
        lr.endColor = lineColor;

        // Округляем углы линии для красоты
        lr.numCapVertices = 5;
        lr.numCornerVertices = 5;
        lr.sortingOrder = 0; // Отрисовка поверх заднего фона
        lr.useWorldSpace = true;

        // Задаем точки по центрам кристаллов, чуть сдвигая по Z, чтобы они были перед фоном
        for (int i = 0; i < matchedGems.Count; i++)
        {
            Vector3 pos = matchedGems[i].transform.position;
            pos.z = -0.5f; // Задники у нас на -1, а кристаллы на 0. Помещаем линию между ними.
            lr.SetPosition(i, pos);
        }

        // Сохраняем линию, чтобы удалить её в конце хода
        activeComboLines.Add(lineObj);
    }

    private void ResolveTurn()
    {
        if (gameManager != null)
        {
            gameManager.CompleteTurn();
        }

        ResolveMatches();
    }

    private void ResolveMatches()
    {

        // АВАРИЙНАЯ ОЧИСТКА:
        // Если игрок сделал свайп в последнюю секунду и таймер истек раньше, чем FindAllMatches успел сработать,
        // некоторые кристаллы могут навсегда "зависнуть" с флагом isMatched = true (хотя комбо на самом деле нет).
        // Поэтому перед подсчетом мы прогоняем алгоритм поиска еще раз, чтобы точно подтвердить их статус,
        // и снимаем блокировку со всех "ложных" совпадений.

        // АВАРИЙНАЯ ОЧИСТКА ложных комбо и линий
        foreach (var line in activeComboLines)
        {
            if (line != null) Destroy(line);
        }
        activeComboLines.Clear();

        // Сначала сбрасываем всем кристаллам флаг
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                if (allGems[x, y] != null)
                {
                    allGems[x, y].isMatched = false;
                }
            }
        }

        // Затем ищем реальные матчи (если они успели собраться в последний момент)
        FindAllMatches();

        Dictionary<Gem.GemColor, int> scores = new Dictionary<Gem.GemColor, int>();

        // Собираем данные для анимации фруктов, чтобы запустить их последовательно
        List<FruitAnimationData> fruitsToAnimate = new List<FruitAnimationData>();

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

                    // Проверяем, есть ли на кристалле фрукт для анимации
                    if (gem.hasFruit && gem.fruitSprite != null && heroManager != null)
                    {
                        Vector3 targetPos = heroManager.GetHeroPosition(gem.color);
                        if (targetPos != Vector3.zero) // Если герой этого цвета существует на сцене
                        {
                            // Сохраняем в список, чтобы потом запустить друг за другом
                            fruitsToAnimate.Add(new FruitAnimationData
                            {
                                startPos = gem.transform.position,
                                endPos = targetPos,
                                heroColor = gem.color,
                                fruitSprite = gem.fruitSprite
                            });
                        }
                        else
                        {
                            // Если героя нет - просто начисляем очки без анимации
                            heroManager.AddEnergyToColor(gem.color, 1);
                        }
                    }
                    else if (heroManager != null)
                    {
                        // Если фрукта нет на кристалле, все равно даем базовую энергию за уничтожение кристалла
                        heroManager.AddEnergyToColor(gem.color, 1);
                    }

                    // Уничтожаем (возвращаем в пул) связанные линии перед возвращением самого кристалла
                    foreach (var line in activeComboLines)
                    {
                        if (line != null) ReturnLine(line);
                    }
                    activeComboLines.Clear();

                    ReturnGem(gem);
                    allGems[x, y] = null;
                }
            }
        }

        Debug.Log($"--- Завершен ход ---");
        foreach (var kvp in scores)
        {
            Debug.Log($"Уничтожено кристаллов цвета {kvp.Key}: {kvp.Value}");
        }
        if (scores.Count == 0)
        {
            Debug.Log("Нет собранных комбинаций.");
        }
        Debug.Log("-------------------------");

        if (environmentManager != null)
        {
            environmentManager.OnTurnCompleted();
        }

        // Запускаем последовательную анимацию фруктов, а досочку будем рефиллить параллельно
        StartCoroutine(AnimateFruitsSequentially(fruitsToAnimate));

        StartCoroutine(RefillBoard());
    }

    private struct FruitAnimationData
    {
        public Vector3 startPos;
        public Vector3 endPos;
        public Gem.GemColor heroColor;
        public Sprite fruitSprite;
    }

    private IEnumerator AnimateFruitsSequentially(List<FruitAnimationData> fruits)
    {
        float delayBetweenFruits = 0.15f; // Пауза между вылетом соседних фруктов

        foreach (var fruitData in fruits)
        {
            // Запускаем фрукты один за другим с задержкой
            StartCoroutine(AnimateFruitToHero(fruitData.startPos, fruitData.endPos, fruitData.heroColor, fruitData.fruitSprite));
            yield return new WaitForSeconds(delayBetweenFruits);
        }
    }

    private IEnumerator AnimateFruitToHero(Vector3 startPos, Vector3 endPos, Gem.GemColor heroColor, Sprite fruitSprite)
    {
        // 1. Создаем временный объект фрукта
        GameObject flyingFruit = new GameObject("FlyingFruit");
        flyingFruit.transform.position = startPos;
        // Задаем сортировку поверх всех кристаллов
        SpriteRenderer sr = flyingFruit.AddComponent<SpriteRenderer>();
        sr.sprite = fruitSprite;
        sr.sortingOrder = 100;

        Vector3 originalFruitScale = flyingFruit.transform.localScale;

        // 2. Анимируем полет к рамке героя
        float flyDuration = 0.5f;
        LeanTween.move(flyingFruit, endPos, flyDuration).setEaseInQuad();

        yield return new WaitForSeconds(flyDuration);

        // 3. Фрукт долетел -> Запускаем пульсацию и начисление
        if (heroManager != null)
        {
            heroManager.AddEnergyToColor(heroColor, 1);
        }

        // Анимация пульсации: слегка увеличиваемся и исчезаем
        float pulseDuration = 0.2f;
        LeanTween.scale(flyingFruit, originalFruitScale * 1.3f, pulseDuration).setEaseOutBack();

        yield return new WaitForSeconds(pulseDuration);

        // Исчезновение (сжатие в 0)
        LeanTween.scale(flyingFruit, Vector3.zero, 0.15f).setEaseInBack();

        yield return new WaitForSeconds(0.15f);

        Destroy(flyingFruit);
    }

    private IEnumerator RefillBoard()
    {
        List<Gem> changedGems = new List<Gem>();

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

                            changedGems.Add(gemToMove);

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

                    // Берем кристалл из пула
                    Gem gem = GetGem(targetPos);
                    gem.gameObject.name = $"Gem_{x}_{y}";

                    // Запоминаем изначальный размер из префаба, чтобы кристалл вырастал до правильного размера
                    Vector3 originalScale = gemPrefab.transform.localScale;

                    // Устанавливаем размер в 0 для анимации появления (вырастания)
                    gem.transform.localScale = Vector3.zero;

                    if (gem != null)
                    {
                        int safeColorIndex = GetValidColorIndex(x, y);
                        Gem.GemColor randomColor = (Gem.GemColor)safeColorIndex;

                        // Получаем визуальные данные для этого цвета
                        Sprite randomSprite = null;
                        Sprite glowSprite = null;

                        foreach (var visual in gemVisuals)
                        {
                            if (visual.color == randomColor)
                            {
                                randomSprite = visual.defaultSprite;
                                glowSprite = visual.glowSprite;
                                break;
                            }
                        }

                        Sprite fruitSprite = null;
                        if (fruitTypes != null)
                        {
                            foreach (var fruitData in fruitTypes)
                            {
                                if (fruitData.color == randomColor)
                                {
                                    fruitSprite = fruitData.fruitSprite;
                                    break;
                                }
                            }
                        }

                        gem.Setup(randomColor, randomSprite, glowSprite, x, y, this, fruitSprite);
                        allGems[x, y] = gem;

                        float dropDelay = Random.Range(0f, 0.2f) + (height - y) * 0.05f; // Задержка появления

                        // Анимируем размер (вырастание) до исходного масштаба префаба
                        LeanTween.scale(gem.gameObject, originalScale, swapDuration * 1.5f)
                                 .setDelay(dropDelay)
                                 .setEaseOutBack(); // Плавное увеличение

                        changedGems.Add(gem);
                    }
                }
            }
        }

        // Ждем дольше, чтобы все кристаллы успели допрыгать
        yield return new WaitForSeconds(swapDuration * 2.5f);

        // КАСКАД: Проверяем новые совпадения после падения
        if (changedGems.Count > 0 && FindMatchesAt(changedGems))
        {
            // Нашлись новые комбо! Собираем их (рекурсивный каскад)
            ResolveMatches();
        }
        else
        {
            // Если игра закончилась, оставляем поле заблокированным
            if (environmentManager != null && environmentManager.isGameOver)
            {
                Debug.Log("Игра окончена, ходы заблокированы.");
            }
            else
            {
                if (gameManager != null)
                {
                    gameManager.UnlockTurn();
                }
                isAnimating = false; // Снимаем блокировку после полного обновления доски
            }
        }
    }

    #region Object Pooling

    private Gem GetGem(Vector3 position)
    {
        if (gemPool.Count > 0)
        {
            Gem gem = gemPool.Dequeue();
            gem.transform.position = position;
            gem.gameObject.SetActive(true);
            return gem;
        }
        else
        {
            GameObject gemObject = Instantiate(gemPrefab, position, Quaternion.identity);
            gemObject.transform.SetParent(this.transform);
            return gemObject.GetComponent<Gem>();
        }
    }

    private void ReturnGem(Gem gem)
    {
        gem.gameObject.SetActive(false);
        // Скидываем анимации LeanTween (чтобы они не доигрывались на выключенном объекте)
        LeanTween.cancel(gem.gameObject);
        gemPool.Enqueue(gem);
    }

    private GameObject GetLine()
    {
        if (linePool.Count > 0)
        {
            GameObject lineObj = linePool.Dequeue();
            lineObj.SetActive(true);
            return lineObj;
        }
        else
        {
            GameObject lineObj = new GameObject("ComboLine");
            lineObj.transform.SetParent(this.transform);
            lineObj.AddComponent<LineRenderer>();
            return lineObj;
        }
    }

    private void ReturnLine(GameObject lineObj)
    {
        lineObj.SetActive(false);
        linePool.Enqueue(lineObj);
    }

    #endregion
}
