using UnityEngine;
using TMPro; 

public class TooltipManager : MonoBehaviour
{
    public static TooltipManager Instance;

    [Header("UI References")]
    public GameObject tooltipPanel;
    public TextMeshProUGUI nameText;
    public TextMeshProUGUI descriptionText;
    public TextMeshProUGUI priceText;

    private void Awake() => Instance = this;

    private void Start() => HideTooltip(); 

    public void ShowTooltip(InventoryItemSO item, bool isShopItem)
    {
        if (item == null) return;

        tooltipPanel.SetActive(true);
        
        nameText.text = item.name;
        descriptionText.text = item.description;

        // Change text based on context (Buying vs Selling)
        if (isShopItem)
        {
            priceText.text = $"Buy: {item.buyPrice} G";
        }
        else
        {
            priceText.text = $"Sell: {item.sellPrice} G";
        }
    }

    public void HideTooltip()
    {
        tooltipPanel.SetActive(false);
    }
}