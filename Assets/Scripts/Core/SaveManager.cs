using System.IO;
using UnityEngine;

/// <summary>
/// Синглтон для сохранения и загрузки данных игрока.
/// Данные хранятся в JSON-файле в Application.persistentDataPath.
/// Доступ: SaveManager.Instance.Data
/// </summary>
public class SaveManager : MonoBehaviour
{
    public static SaveManager Instance { get; private set; }

    // Текущие данные игрока (весь прогресс)
    public PlayerData Data { get; private set; }

    private string SaveFilePath => Path.Combine(Application.persistentDataPath, "player_save.json");

    private void Awake()
    {
        // Синглтон + DontDestroyOnLoad: живёт между всеми сценами
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            Load();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    /// <summary>
    /// Сохраняет текущие данные игрока в JSON-файл.
    /// </summary>
    public void Save()
    {
        string json = JsonUtility.ToJson(Data, prettyPrint: true);
        File.WriteAllText(SaveFilePath, json);
        Debug.Log($"[SaveManager] Сохранено → {SaveFilePath}");
    }

    /// <summary>
    /// Загружает данные игрока из JSON-файла.
    /// Если файла нет — создаёт нового игрока с дефолтными значениями.
    /// </summary>
    public void Load()
    {
        if (File.Exists(SaveFilePath))
        {
            string json = File.ReadAllText(SaveFilePath);
            Data = JsonUtility.FromJson<PlayerData>(json);
            Debug.Log($"[SaveManager] Данные загружены. Уровень: {Data.currentLevel}, Монеты: {Data.coins}");
        }
        else
        {
            Data = new PlayerData();
            Debug.Log("[SaveManager] Новый игрок — созданы данные по умолчанию.");
            Save(); // Сразу сохраняем чистый файл
        }
    }

    /// <summary>
    /// Сбрасывает прогресс игрока (для дебага/тестирования).
    /// </summary>
    public void ResetData()
    {
        Data = new PlayerData();
        Save();
        Debug.Log("[SaveManager] Прогресс сброшен!");
    }

    /// <summary>
    /// Начислить монеты и сразу сохранить.
    /// </summary>
    public void AddCoinsAndSave(int amount)
    {
        Data.AddCoins(amount);
        Save();
    }

    /// <summary>
    /// Записать выполненный заказ (добавить в инвентарь) и сохранить.
    /// </summary>
    public void RecordOrderFulfilled(string color)
    {
        Data.totalOrdersFulfilled++;
        Data.AddDrink(color, 1);
        Save();
    }

    // Автосохранение при выходе из игры
    private void OnApplicationQuit()
    {
        Save();
    }

    // Автосохранение при сворачивании приложения (мобильные)
    private void OnApplicationPause(bool paused)
    {
        if (paused) Save();
    }
}
