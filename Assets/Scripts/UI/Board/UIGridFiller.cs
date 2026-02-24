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
        foreach (Transform child in transform)
        {
            Destroy(child.gameObject);
        }


        for (int row = 0; row < rows; row++)
        {
            for (int col = 0; col < columns; col++)
            {
                GameObject newCell = Instantiate(cellPrefab, transform);
                int randomIndex = Random.Range(0, gemSprites.Length);
                newCell.GetComponent<Image>().sprite = gemSprites[randomIndex];

                Gem gem = newCell.GetComponent<Gem>();
                if (gem == null)
                    gem = newCell.AddComponent<Gem>();

                gem.Setup(row,col,randomIndex);
                //BoardManager.Instance.RegisterGem(gem,row,col);


                newCell.name = $"Gem_{row}_{col}_{randomIndex}";

            }

            
        }

   
    }
}

