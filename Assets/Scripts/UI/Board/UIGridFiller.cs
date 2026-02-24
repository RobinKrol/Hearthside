using UnityEngine;
using UnityEngine.UI;

public class UIGridFiller : MonoBehaviour
{

    [Header("Настройки сетки")]
    public GameObject cellPrefab;
    public int columns = 7;
    public int rows = 5;

    [Header("Визуальные данные")]
    public Sprite[] gemSprites;

    private BoardManager boardManager;

    void Start()
    {
        boardManager = FindAnyObjectByType<BoardManager>();
        ClearExistingCells();
        GenerateGrid();
    }

    void ClearExistingCells()
    {
        int childCount = transform.childCount;
        for (int i = childCount - 1; i >= 0; i--)
        {
            Destroy(transform.GetChild(i).gameObject);
        }
    }

    void GenerateGrid()
    {
        for (int row = 0; row < rows; row++)
        {
            for (int col = 0; col < columns; col++)
            {
                CreateGemCell(row, col);
            }
        }
        
        // После создания всех кристаллов, покажем состояние сетки
        if (boardManager != null)
        {
            boardManager.ShowGrid();
        }
    }

    void CreateGemCell(int row, int col)
    {
        GameObject newCell = Instantiate(cellPrefab, transform);
        int randomIndex = Random.Range(0, gemSprites.Length);
        
        // Настраиваем изображение
        Image cellImage = newCell.GetComponent<Image>();
        if (cellImage != null)
        {
            cellImage.sprite = gemSprites[randomIndex];
        }
        
        // Добавляем и настраиваем компонент Gem
        Gem gem = newCell.GetComponent<Gem>();
        if (gem == null)
        {
            gem = newCell.AddComponent<Gem>();
        }
        
        gem.Setup(randomIndex);
        newCell.name = $"Gem_{row}_{col}_{randomIndex}";
        
        // Регистрируем кристалл в BoardManager
        if (boardManager != null)
        {
            boardManager.RegisterGem(gem, row, col);
        }
    }
}

