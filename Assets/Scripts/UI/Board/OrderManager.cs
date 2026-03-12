using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OrderManager : MonoBehaviour
{
    // Синглтон для легкого доступа из HeroUI
    public static OrderManager Instance { get; private set; }

    [Header("Настройки спавна")]
    public Transform customerSpawnPoint; // Место, где будет стоять посетитель
    public GameObject customerPrefab;    // Префаб с компонентом Customer
    public float delayBetweenCustomers = 2.0f; // Пауза перед приходом нового

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

    private Customer currentCustomer;
    private bool isServing = false;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private void Start()
    {
        // Вызываем первого посетителя с небольшой задержкой при старте
        StartCoroutine(SpawnNextCustomerWithDelay(1.0f));
    }

    /// <summary>
    /// Обрабатывает клик по герою (приготовление ульты)
    /// </summary>
    public void TryFulfillOrder(Gem.GemColor heroColor, Sprite ultimateDrink)
    {
        if (currentCustomer == null || isServing)
        {
            Debug.Log("[OrderManager] Заказ не принят: нет посетителя или он уже обслуживается.");
            return;
        }

        if (currentCustomer.requestedColor == heroColor)
        {
            Debug.Log($"[OrderManager] Успех! Посетитель получил желаемый {heroColor} напиток.");
            isServing = true;
            currentCustomer.ServeDrink(ultimateDrink);
        }
        else
        {
            Debug.Log($"[OrderManager] Посетителю нужен {currentCustomer.requestedColor}, а подали {heroColor}. Отказ.");
        }
    }

    /// <summary>
    /// Вызывается из Customer.cs, когда анимация ухода завершилась
    /// </summary>
    public void OnCustomerLeft()
    {
        currentCustomer = null;
        isServing = false;

        StartCoroutine(SpawnNextCustomerWithDelay(delayBetweenCustomers));
    }

    private IEnumerator SpawnNextCustomerWithDelay(float delay)
    {
        yield return new WaitForSeconds(delay);

        if (customerSpawnPoint == null || customerPrefab == null || customerFaces.Length == 0)
        {
            Debug.LogWarning("[OrderManager] Не настроены спавны или префабы посетителей!");
            yield break;
        }

        // Выбираем случайную внешность
        CustomerVisuals randomFace = customerFaces[Random.Range(0, customerFaces.Length)];

        // Выбираем заказ только из цветов АКТИВНЫХ героев на сцене
        Gem.GemColor randomOrder;
        if (heroManager != null && heroManager.activeHeroes.Count > 0)
        {
            // Берём случайного героя из списка и заказываем его цвет
            Hero randomHero = heroManager.activeHeroes[Random.Range(0, heroManager.activeHeroes.Count)];
            randomOrder = randomHero.heroColor;
        }
        else
        {
            // Запасной вариант: если героев нет, берём любой цвет
            randomOrder = (Gem.GemColor)Random.Range(0, System.Enum.GetValues(typeof(Gem.GemColor)).Length);
            Debug.LogWarning("[OrderManager] HeroManager не назначен или нет активных героев! Заказ — случайный цвет.");
        }

        // Создаём нового посетителя
        GameObject newObj = Instantiate(customerPrefab, customerSpawnPoint);
        currentCustomer = newObj.GetComponent<Customer>();

        if (currentCustomer != null)
        {
            currentCustomer.Setup(randomOrder, randomFace.defaultFace, randomFace.happyFace, OnCustomerLeft);
            Debug.Log($"[OrderManager] Пришел новый посетитель. Он хочет напиток цвета: {randomOrder}");
        }
    }
}
