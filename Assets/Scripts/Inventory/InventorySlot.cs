using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class InventorySlot : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
{
    private Inventory myInventory; // The grid this slot belongs to
    private Image slotImage;
    private Color defaultColor;

    public InventoryItem storedItem; 
    public int X { get; private set; }
    public int Y { get; private set; }

    // Initialize with the specific Inventory Grid instance
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
        if (eventData.button == PointerEventData.InputButton.Left)
        {
            // Tell the Global Manager that a slot in THIS inventory was clicked
            InventoryManager.Instance.OnSlotClicked(myInventory, this);
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        // Tell Manager we are hovering over THIS inventory
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