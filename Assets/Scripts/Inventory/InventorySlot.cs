using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class InventorySlot : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
{
    private Inventory inventory;
    private Image img;
    private Color baseColor;
    public InventoryItem currentItem; 

    public void Initialize(Inventory inv, int x, int y)
    {
        this.inventory = inv;
        img = GetComponent<Image>();
        baseColor = img.color;
    }

    public void OnPointerClick(PointerEventData d) {
        if (d.button == PointerEventData.InputButton.Left) InventoryManager.Instance.OnSlotClicked(inventory);
    }

    public void OnPointerEnter(PointerEventData d) => InventoryManager.Instance.OnSlotEnter(inventory);
    public void OnPointerExit(PointerEventData d) => InventoryManager.Instance.OnSlotExit(inventory);

    public void SetColor(Color c) => img.color = c;
    public void ResetColor() => img.color = baseColor;
    public bool IsEmpty() => currentItem == null;
}