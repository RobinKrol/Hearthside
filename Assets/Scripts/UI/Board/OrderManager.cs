using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OrderManager : MonoBehaviour
{
    // Синглтон для легкого доступа из HeroUI
    public static OrderManager Instance { get; private set; }

    [Header("Настройки спавна")]
    public GameObject customerPrefab;                // Префаб с компонентом Customer
    public Transform[] customerSpawnPoints;          // Точки появления посетителей (макс. 2)
    public float delayBetweenCustomers = 2.0f;       // Пауза перед приходом нового посетителя
    public int maxCustomers = 2;                     // Максимум посетителей одновременно

    [Header("Связь с героями")]
    public HeroManager heroManager; // Ссылка на менеджер героев для определения возможных заказов

    [System.Serializable]
    public struct CustomerVisuals
    {
        public Sprite defaultFace;
        public Sprite happyFace;
    }

    [Header("База Посетителей (Лица)")]
    public CustomerVisuals[] customerFaces;

    // Список активных посетителей (максимум maxCustomers)
    private List<Customer> activeCustomers = new List<Customer>();

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private void Start()
    {
        // Запускаем посетителей для каждого слота с небольшой задержкой между ними
        for (int i = 0; i < maxCustomers; i++)
        {
            TrySpawnCustomer(1.0f + i * 0.5f); // Небольшой разброс, чтобы не появлялись одновременно
        }
    }

    /// <summary>
    /// Обрабатывает клик по герою (приготовление ульты).
    /// Ищет подходящего посетителя среди всех активных.
    /// </summary>
    public void TryFulfillOrder(Gem.GemColor heroColor, Sprite ultimateDrink)
    {
        // Ищем подходящего посетителя среди активных
        Customer matching = null;
        foreach (Customer c in activeCustomers)
        {
            if (c != null && c.requestedColor == heroColor && !c.IsBeingServed)
            {
                matching = c;
                break;
            }
        }

        if (matching == null)
        {
            // Находим первого активного не-обслуживаемого для сообщения об ошибке
            Customer any = activeCustomers.Find(c => c != null && !c.IsBeingServed);
            if (any != null)
                Debug.Log($"[OrderManager] Посетителю нужен {any.requestedColor}, а подали {heroColor}. Отказ.");
            else
                Debug.Log("[OrderManager] Нет подходящего посетителя для этого напитка.");
            return;
        }

        Debug.Log($"[OrderManager] Успех! Посетитель получил желаемый {heroColor} напиток.");
        matching.ServeDrink(ultimateDrink);
    }

    /// <summary>
    /// Вызывается из Customer.cs, когда посетитель ушёл.
    /// </summary>
    public void OnCustomerLeft(Customer customer)
    {
        activeCustomers.Remove(customer);

        // Спавним следующего с задержкой
        TrySpawnCustomer(delayBetweenCustomers);
    }

    /// <summary>
    /// Запускает спавн нового посетителя, если есть свободное место.
    /// </summary>
    private void TrySpawnCustomer(float delay)
    {
        // Считаем количество реально ненулевых посетителей
        activeCustomers.RemoveAll(c => c == null);

        if (activeCustomers.Count >= maxCustomers)
        {
            Debug.Log("[OrderManager] Все места заняты, новый посетитель ждет.");
            return;
        }

        StartCoroutine(SpawnNextCustomerWithDelay(delay));
    }

    private Transform GetFreeSpawnPoint()
    {
        if (customerSpawnPoints == null || customerSpawnPoints.Length == 0) return null;

        // Собираем уже занятые точки спавна
        HashSet<Transform> usedPoints = new HashSet<Transform>();
        foreach (Customer c in activeCustomers)
        {
            if (c != null)
                usedPoints.Add(c.transform.parent);
        }

        // Возвращаем первую свободную точку
        foreach (Transform point in customerSpawnPoints)
        {
            if (point != null && !usedPoints.Contains(point))
                return point;
        }

        return null;
    }

    private IEnumerator SpawnNextCustomerWithDelay(float delay)
    {
        yield return new WaitForSeconds(delay);

        // Перепроверяем — вдруг за время ожидания пришёл кто-то ещё
        activeCustomers.RemoveAll(c => c == null);
        if (activeCustomers.Count >= maxCustomers) yield break;

        if (customerPrefab == null || customerFaces.Length == 0)
        {
            Debug.LogWarning("[OrderManager] Не настроены префаб или лица посетителей!");
            yield break;
        }

        Transform spawnPoint = GetFreeSpawnPoint();
        if (spawnPoint == null)
        {
            Debug.LogWarning("[OrderManager] Нет свободной точки спавна!");
            yield break;
        }

        // Выбираем случайную внешность
        CustomerVisuals randomFace = customerFaces[Random.Range(0, customerFaces.Length)];

        // Выбираем заказ только из цветов АКТИВНЫХ героев на сцене
        Gem.GemColor randomOrder;
        if (heroManager != null && heroManager.activeHeroes.Count > 0)
        {
            // Фильтруем нулевые ссылки (на случай, если герой был уничтожен)
            var validHeroes = heroManager.activeHeroes.FindAll(h => h != null);
            if (validHeroes.Count > 0)
            {
                Hero randomHero = validHeroes[Random.Range(0, validHeroes.Count)];
                randomOrder = randomHero.heroColor;
            }
            else
            {
                randomOrder = (Gem.GemColor)Random.Range(0, System.Enum.GetValues(typeof(Gem.GemColor)).Length);
                Debug.LogWarning("[OrderManager] Все герои уничтожены, заказ — случайный цвет.");
            }
        }
        else
        {
            randomOrder = (Gem.GemColor)Random.Range(0, System.Enum.GetValues(typeof(Gem.GemColor)).Length);
            Debug.LogWarning("[OrderManager] HeroManager не назначен! Заказ — случайный цвет.");
        }


        // Создаём нового посетителя
        GameObject newObj = Instantiate(customerPrefab, spawnPoint);
        Customer newCustomer = newObj.GetComponent<Customer>();

        if (newCustomer != null)
        {
            // Передаём колбэк с ссылкой на самого посетителя
            newCustomer.Setup(randomOrder, randomFace.defaultFace, randomFace.happyFace, () => OnCustomerLeft(newCustomer));
            activeCustomers.Add(newCustomer);
            Debug.Log($"[OrderManager] Новый посетитель (слот {activeCustomers.Count}/{maxCustomers}). Хочет: {randomOrder}");
        }
    }
}
