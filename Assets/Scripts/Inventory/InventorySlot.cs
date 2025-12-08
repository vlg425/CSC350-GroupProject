using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class InventorySlot : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
{
    private Inventory inventory;
    private Image img;
    private Color baseColor;
    public InventoryItem currentItem; // What is inside me right now?

    public void Initialize(Inventory inv, int x, int y)
    {
        this.inventory = inv;
        img = GetComponent<Image>();
        baseColor = img.color;
    }

    public void OnPointerClick(PointerEventData d) 
    {
        if (d.button == PointerEventData.InputButton.Left) 
            InventoryManager.Instance.OnSlotClicked(inventory);
    }

    // IMPORTANT: We pass 'this' (the slot itself) so the Manager knows EXACTLY which slot is hovered.
    public void OnPointerEnter(PointerEventData d) => InventoryManager.Instance.OnSlotEnter(inventory, this);
    
    public void OnPointerExit(PointerEventData d) => InventoryManager.Instance.OnSlotExit(inventory, this);

    public void SetColor(Color c) => img.color = c;
    public void ResetColor() => img.color = baseColor;
    public bool IsEmpty() => currentItem == null;
}