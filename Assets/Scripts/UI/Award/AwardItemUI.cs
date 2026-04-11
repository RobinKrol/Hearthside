using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class AwardItemUI : MonoBehaviour
{
    public Image itemIcon;
    public TextMeshProUGUI itemNameText;
    public TextMeshProUGUI itemCountText;

    public void Setup(RewardItemConfig config)
    {
        if (itemIcon != null && config.itemSprite != null) 
        {
            itemIcon.sprite = config.itemSprite;
            itemIcon.gameObject.SetActive(true);
        }
        else if (itemIcon != null)
        {
            itemIcon.gameObject.SetActive(false);
        }

        if (itemNameText != null) 
        {
            itemNameText.text = config.itemName;
        }

        if (itemCountText != null) 
        {
            itemCountText.text = config.itemCount.ToString();
            // Если нужно, чтобы писалось "+1", можно использовать:
            // itemCountText.text = $"+{config.itemCount}";
        }
    }
}
