using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class InventorySlot : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
{
    private Inventory myInventory;
    private Image slotImage;
    private Color defaultColor;

    public InventoryItem storedItem; 
    public int X { get; private set; }
    public int Y { get; private set; }

    public void Initialize(Inventory inventory, int x, int y)
    {
        myInventory = inventory;
        X = x;
        Y = y;
        
        slotImage = GetComponent<Image>();
        defaultColor = slotImage.color;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        // Only allow Left Click to place items
        if (eventData.button == PointerEventData.InputButton.Left)
        {
            InventoryManager.Instance.OnSlotClicked(myInventory, this);
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        InventoryManager.Instance.OnSlotEnter(myInventory);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        InventoryManager.Instance.OnSlotExit(myInventory);
    }

    public void SetHighlight(Color color)
    {
        slotImage.color = color;
    }

    public void ResetColor()
    {
        slotImage.color = defaultColor;
    }

    public bool IsEmpty()
    {
        return storedItem == null;
    }
}