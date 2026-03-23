using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Enums;

public class OrderManager : MonoBehaviour
{
    public static OrderManager Instance { get; private set; }

    [Header("Настройки спавна")]
    public GameObject customerPrefab;
    public Transform[] customerSpawnPoints;       // Точки появления (макс. 2)
    public float delayBetweenCustomers = 2.0f;
    public int maxCustomers = 2;

    [Header("Связь с героями")]
    public HeroManager heroManager;

    // ─── Пул внешностей посетителей (до 5) ──────────────────
    // Каждый элемент — пара спрайтов одного персонажа
    [System.Serializable]
    public struct CustomerVisuals
    {
        public Sprite defaultFace;   // Обычное лицо
        public Sprite happyFace;     // Довольное лицо (после заказа)
    }

    [Header("Пул посетителей (до 5 персонажей)")]
    public CustomerVisuals[] customerPool; // Заполните в Inspector спрайтами посетителей

    // ─── Иконки заказов (для чат-бабла) ─────────────────────
    // Каждый элемент связывает цвет кристаллов с иконкой напитка
    [System.Serializable]
    public struct DrinkIcon
    {
        public Gem.GemColor color;
        public DrinkSize size;
        public Sprite icon;          // Иконка напитка этого цвета и размера для чат-бабла
    }

    [Header("Иконки напитков (для пузыря заказа)")]
    public DrinkIcon[] drinkIcons;   // Заполните в Inspector для каждого цвета

    // ─── Внутреннее состояние ───────────────────────────────
    private List<Customer> activeCustomers = new List<Customer>();
    
    // Счетчик отданных напитков (Цвет, Размер) -> Количество
    private Dictionary<(Gem.GemColor, DrinkSize), int> fulfilledDrinks = new Dictionary<(Gem.GemColor, DrinkSize), int>();

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private void Start()
    {
        for (int i = 0; i < maxCustomers; i++)
            TrySpawnCustomer(1.0f + i * 0.5f);
    }

    // ─── Публичные методы ────────────────────────────────────

    /// <summary>
    /// Вызывается из HeroUI при клике на героя с заряженной ультой.
    /// Ищет подходящего посетителя среди всех активных.
    /// </summary>
    public void TryFulfillOrder(Gem.GemColor heroColor, DrinkSize heroDrinkSize, Sprite ultimateDrink)
    {
        Customer matching = null;
        foreach (Customer c in activeCustomers)
        {
            if (c != null && c.requestedColor == heroColor && c.requestedSize == heroDrinkSize && !c.IsBeingServed)
            {
                matching = c;
                break;
            }
        }

        if (matching == null)
        {
            Customer any = activeCustomers.Find(c => c != null && !c.IsBeingServed);
            if (any != null)
                Debug.Log($"[OrderManager] Нужен {any.requestedColor}, а подали {heroColor}. Отказ.");
            else
                Debug.Log("[OrderManager] Нет подходящего посетителя.");
            return;
        }

        Debug.Log($"[OrderManager] Успех! {heroColor} {heroDrinkSize} напиток подан.");
        matching.ServeDrink(ultimateDrink);

        // Обновляем счетчик для текущего уровня
        var key = (heroColor, heroDrinkSize);
        if (!fulfilledDrinks.ContainsKey(key)) fulfilledDrinks[key] = 0;
        fulfilledDrinks[key]++;

        SaveManager.Instance?.RecordOrderFulfilled();
    }

    /// <summary>
    /// Возвращает количество отданных напитков заданного типа (для Award Screen)
    /// </summary>
    public int GetFulfilledCount(Gem.GemColor color, DrinkSize size)
    {
        return fulfilledDrinks.TryGetValue((color, size), out int count) ? count : 0;
    }

    /// <summary>
    /// Вызывается из Customer когда посетитель ушёл.
    /// </summary>
    public void OnCustomerLeft(Customer customer)
    {
        activeCustomers.Remove(customer);
        TrySpawnCustomer(delayBetweenCustomers);
    }

    // ─── Спавн ──────────────────────────────────────────────

    private void TrySpawnCustomer(float delay)
    {
        activeCustomers.RemoveAll(c => c == null);
        if (activeCustomers.Count >= maxCustomers) return;
        StartCoroutine(SpawnNextCustomerWithDelay(delay));
    }

    private Transform GetFreeSpawnPoint()
    {
        if (customerSpawnPoints == null || customerSpawnPoints.Length == 0) return null;

        HashSet<Transform> usedPoints = new HashSet<Transform>();
        foreach (Customer c in activeCustomers)
        {
            if (c != null) usedPoints.Add(c.transform.parent);
        }

        foreach (Transform point in customerSpawnPoints)
        {
            if (point != null && !usedPoints.Contains(point))
                return point;
        }
        return null;
    }

    /// <summary>
    /// Возвращает иконку напитка для бабла по цвету и размеру.
    /// </summary>
    private Sprite GetDrinkIcon(Gem.GemColor color, DrinkSize size)
    {
        if (drinkIcons == null) return null;
        foreach (var di in drinkIcons)
        {
            if (di.color == color && di.size == size) return di.icon;
        }
        return null;
    }

    /// <summary>
    /// Выбирает случайную пару спрайтов посетителя из пула.
    /// Если пул пуст — возвращает пустую структуру.
    /// </summary>
    private CustomerVisuals GetRandomCustomerVisuals()
    {
        if (customerPool == null || customerPool.Length == 0)
            return new CustomerVisuals();
        return customerPool[Random.Range(0, customerPool.Length)];
    }

    private IEnumerator SpawnNextCustomerWithDelay(float delay)
    {
        yield return new WaitForSeconds(delay);

        activeCustomers.RemoveAll(c => c == null);
        if (activeCustomers.Count >= maxCustomers) yield break;

        if (customerPrefab == null)
        {
            Debug.LogWarning("[OrderManager] Не задан customerPrefab!");
            yield break;
        }

        Transform spawnPoint = GetFreeSpawnPoint();
        if (spawnPoint == null)
        {
            Debug.LogWarning("[OrderManager] Нет свободной точки спавна!");
            yield break;
        }

        // Выбираем случайную внешность из пула
        CustomerVisuals visuals = GetRandomCustomerVisuals();

        // Выбираем заказ (цвет и размер) из активных героев
        PickRandomOrder(out Gem.GemColor randomOrderColor, out DrinkSize randomOrderSize);

        // Находим иконку для бабла
        Sprite orderIcon = GetDrinkIcon(randomOrderColor, randomOrderSize);

        // Создаём посетителя
        GameObject newObj = Instantiate(customerPrefab, spawnPoint);
        Customer newCustomer = newObj.GetComponent<Customer>();

        if (newCustomer != null)
        {
            newCustomer.Setup(randomOrderColor, randomOrderSize, visuals.defaultFace, visuals.happyFace,
                              orderIcon, () => OnCustomerLeft(newCustomer));
            activeCustomers.Add(newCustomer);
            Debug.Log($"[OrderManager] Посетитель {activeCustomers.Count}/{maxCustomers} хочет: {randomOrderColor} {randomOrderSize}");
        }
    }

    private void PickRandomOrder(out Gem.GemColor color, out DrinkSize size)
    {
        if (heroManager != null && heroManager.activeHeroes.Count > 0)
        {
            var validHeroes = heroManager.activeHeroes.FindAll(h => h != null);
            if (validHeroes.Count > 0)
            {
                Hero randomHero = validHeroes[Random.Range(0, validHeroes.Count)];
                color = randomHero.heroColor;
                size = randomHero.drinkSize;
                return;
            }
        }
        
        color = (Gem.GemColor)Random.Range(0, System.Enum.GetValues(typeof(Gem.GemColor)).Length);
        size = DrinkSize.Small;
    }
}
