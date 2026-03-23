using System;
using System.Collections.Generic;
using UnityEngine;
using Enums;

public class HeroSpawner : MonoBehaviour
{
    [Header("Префаб героя")]
    public GameObject heroPrefab; // Один универсальный префаб для всех героев

    [Header("Родительский контейнер (внутри Canvas)")]
    public RectTransform heroContainer; // Панель внутри Canvas, в которую будут помещаться герои

    [Header("Настройки позиции")]
    public float spawnY = -43f;   // Фиксированная Y-позиция (anchoredPosition) всех героев
    public float spacingX = 2.5f; // Расстояние между героями по X (в единицах Canvas)

    [Header("Конфигурация героев")]
    public HeroConfig[] heroConfigs; // Список героев, которых нужно спавнить

    [Header("Зависимости")]
    public HeroManager heroManager; // Менеджер, которому передаем спавнированных героев

    [Serializable]
    public struct HeroConfig
    {
        public Gem.GemColor color;          // Цвет кристаллов, которые заряжают этого героя
        public DrinkSize drinkSize;         // Размер напитка, который он готовит
        public int maxEnergy;               // Максимальная энергия (сложность зарядки)
        public Sprite ultimateDrinkSprite;  // Напиток, который герой готовит при ульте
    }

    private void Start()
    {
        SpawnHeroes();
    }

    private void SpawnHeroes()
    {
        if (heroPrefab == null || heroConfigs == null || heroConfigs.Length == 0)
        {
            Debug.LogWarning("[HeroSpawner] Не задан префаб или конфигурация героев!");
            return;
        }

        // Если контейнер не задан — ищем Canvas автоматически
        Transform parent = heroContainer != null ? heroContainer : transform;

        int count = heroConfigs.Length;

        // Центрируем всех героев по оси X
        float totalWidth = (count - 1) * spacingX;
        float startX = -totalWidth / 2f;

        for (int i = 0; i < count; i++)
        {
            HeroConfig config = heroConfigs[i];

            // Создаём объект из префаба внутри контейнера Canvas
            GameObject heroObj = Instantiate(heroPrefab, parent);
            heroObj.name = $"Hero_{config.color}";

            // Задаём позицию через RectTransform (anchoredPosition для UI)
            RectTransform rt = heroObj.GetComponent<RectTransform>();
            if (rt != null)
            {
                float posX = startX + i * spacingX;
                rt.anchoredPosition = new Vector2(posX, spawnY);
            }
            else
            {
                // Запасной вариант для не-UI объектов
                float posX = startX + i * spacingX;
                heroObj.transform.localPosition = new Vector3(posX, spawnY, 0f);
            }

            // Настраиваем героя
            Hero heroComponent = heroObj.GetComponent<Hero>();
            if (heroComponent != null)
            {
                heroComponent.heroColor = config.color;
                heroComponent.drinkSize = config.drinkSize;
                heroComponent.maxEnergy = config.maxEnergy;
                heroComponent.ultimateDrinkSprite = config.ultimateDrinkSprite;
                heroComponent.currentEnergy = 0;

                // Добавляем в HeroManager
                if (heroManager != null)
                {
                    heroManager.activeHeroes.Add(heroComponent);
                }
            }



            Debug.Log($"[HeroSpawner] Создан герой {config.color} (слот {i + 1}/{count})");

        }
    }
}
