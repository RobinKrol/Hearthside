using UnityEngine;
using UnityEngine.UI;

public class UIGridFiller : MonoBehaviour
{
    public GameObject cellPrefab;
    public int columns = 7;
    public int rows = 5;
    public Sprite[] gemSprites;

    void Start()
    {
        ClearExistingCells();
        GenerateGrid();
    }

    void ClearExistingCells()
    {
        // Оптимизация: используем цикл по дочерним элементам
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
    }

    void CreateGemCell(int row, int col)
    {
        GameObject newCell = Instantiate(cellPrefab, transform);
        int randomIndex = Random.Range(0, gemSprites.Length);
        
        Image cellImage = newCell.GetComponent<Image>();
        if (cellImage != null)
        {
            cellImage.sprite = gemSprites[randomIndex];
        }

        Gem gem = newCell.GetComponent<Gem>();
        if (gem == null)
        {
            gem = newCell.AddComponent<Gem>();
        }

        gem.Setup(row, col, randomIndex);
        newCell.name = $"Gem_{row}_{col}_{randomIndex}";
    }
}

