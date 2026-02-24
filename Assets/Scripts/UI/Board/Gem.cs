using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;


public class Gem : MonoBehaviour
{
    public int row;     // ряд (0-4)
    public int column;  // колонка (0-6)
    public int gemType; // тип: 0-красный, 1-фиолетовый, 2-желтый, 3-зеленый, 4-белый

    private Image image;
    private Button button;
    private BoardManager boardManagers;

    void Awake()
    {
        image = GetComponent<Image>();
        button = GetComponent<Button>();

        button.onClick.AddListener(OnClick);

        boardManagers = FindAnyObjectByType<BoardManager>();

    }

    public void Setup (int newRow, int newCol, int type)
    {
        row = newRow;
        column = newCol;
        gemType = type;

        if (BoardManager.Instance != null)
        {
            BoardManager.Instance.RegisterGem(this, row, column);
        }
    }

    void OnClick()
    {
        if (BoardManager.Instance != null)
        {
            BoardManager.Instance.OnGemClicked(this);
        }
    }
   
}
