using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class BoardManager : MonoBehaviour
{
    public static BoardManager Instance;

    public int columns = 7;
    public int rows = 5;

    //      
    private Gem[,] grid;

    //   
    private Gem firstSelected;
    private Gem secondSelected;

    void Awake()
    {
        Instance = this;
        grid = new Gem[rows, columns];
    }

    //    
    public void RegisterGem (Gem gem, int roow, int col)
    {
        grid[roow, col] = gem;
    }

    //    
    public void OnGemClicked (Gem gem)
    {
        if (firstSelected == null)
        {
            firstSelected = gem;
            Debug.Log($"Выбран кристалл: ({gem.row}, {gem.column})  {gem.gemType}");
        }

        else if (secondSelected == null && gem != firstSelected)
        {
            if (AreNeighbors (firstSelected, gem))
            {
                secondSelected = gem;
                Debug.Log($"Второй кристалл: ({gem.row}, {gem.column})");

                SwapGems(firstSelected, secondSelected);
            }
            else
            {
                Debug.Log("Не соседи, выбираем заново");
                firstSelected = gem;
            }
        }
    }
    bool AreNeighbors(Gem a, Gem b)
    {
        int rowDiff = Mathf.Abs(a.row - b.row);
        int colDiff = Mathf.Abs(a.column - b.column);

        return (rowDiff == 1 && colDiff == 0) || (rowDiff == 0 && colDiff == 1);

    }
    
    //    
    void SwapGems(Gem a, Gem b)
    {
        Debug.Log("Меняем местами!");

        // 
        Image imgA = a.GetComponent<Image>();
        Image imgB = b.GetComponent<Image>();

        Sprite tempSprite = imgA.sprite;
        imgA.sprite = imgB.sprite;
        imgB.sprite = tempSprite;

        //   
        int tempType = a.gemType;
        a.gemType = b.gemType;
        b.gemType = tempType;

        //  
        grid[a.row, a.column] = b;
        grid[b.row, b.column] = a;

        //    
        int tempRow = a.row;
        int tempCol = a.column;
        a.row = b.row;
        a.column = b.column;
        b.row = tempRow;
        b.column = tempCol;


        // 
        firstSelected = null;
        secondSelected = null;
    }


}