using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class Gem : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    public int row;
    public int column;
    public int gemType;

    private Image image;
    private BoardManager boardManager;

    void Awake()
    {
        image = GetComponent<Image>();
        boardManager = BoardManager.Instance; // Оптимизация: прямое обращение к Instance
    }

    public void Setup(int newRow, int newCol, int type)
    {
        row = newRow;
        column = newCol;
        gemType = type;

        if (boardManager != null)
        {
            boardManager.RegisterGem(this, row, column);
        }
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        boardManager?.StartDragging(this);
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        boardManager?.StopDragging(this);
    }
}
