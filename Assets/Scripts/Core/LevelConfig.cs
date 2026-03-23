using UnityEngine;
using System.Collections.Generic;
using Enums;

[CreateAssetMenu(fileName = "New LevelConfig", menuName = "Hearthside/Level Config")]
public class LevelConfig : ScriptableObject
{
    [Header("Level Info")]
    public int levelNumber = 1;

    [Header("Heroes (Включенные Герои)")]
    [Tooltip("Список ID героев, которые будут активны на уровне, например 'hero_fox'")]
    public List<string> activeHeroIds;

    // В будущем тут можно хранить список возможных заказов, но пока заказ
    // будет браться из того, что могут приготовить герои на сцене.

    [Header("Goals (Цель Уровня)")]
    // Пример: сделать 3 красных маленьких напитка
    public Gem.GemColor targetDrinkColor = Gem.GemColor.Red;
    public DrinkSize targetDrinkSize = DrinkSize.Small;
    public int targetDrinksCount = 3;

    [Header("Rewards (Награды за прохождение)")]
    public int rewardGold = 50;
    public int rewardXP = 30;
    public int rewardKeys = 1;

    [Header("Stages (Тайминг & Ходы)")]
    public int turnsMorning = 3;
    public int turnsDay = 2;
    public int turnsEvening = 2;
    
    [Tooltip("Длительность одного хода в секундах (например, 15)")]
    public float turnDurationSeconds = 15f;
    
    // Суммарное количество ходов до ночи
    public int TotalTurns => turnsMorning + turnsDay + turnsEvening;
}
