using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class InventoryItem : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
{
    [Header("Item Data")]
    public InventoryItemSO data; 

    // Logical Size (e.g., 2x3)
    public int Height { get; private set; }
    public int Width { get; private set; }
    public int rotationIndex = 0; 

    // Where am I in the grid?
    public int onGridPositionX;
    public int onGridPositionY;

    private Image itemImage;
    private RectTransform rectTransform;

    private void Awake()
    {
        itemImage = GetComponent<Image>();
        rectTransform = GetComponent<RectTransform>();
    }

    public void Initialize(InventoryItemSO newItem)
    {
        data = newItem;
        itemImage.sprite = data.icon;
        itemImage.color = data.color; 
        
        Width = data.width;
        Height = data.height;
        // Sets the pixel size based on grid slots (assuming 100px per slot)
        rectTransform.sizeDelta = new Vector2(Width * 100, Height * 100); 
    }

    // --- MOUSE EVENTS ---
    // These notify the InventoryManager that interaction is happening.
    
    public void OnPointerClick(PointerEventData eventData)
    {
        InventoryManager.Instance.OnItemClicked(this, eventData);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        InventoryManager.Instance.OnItemEnter(this);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        InventoryManager.Instance.OnItemExit(this);
    }

    // Rotates the item 90 degrees
    public void Rotate()
    {
        rotationIndex++;
        if (rotationIndex > 3) rotationIndex = 0;

        rectTransform.rotation = Quaternion.Euler(0, 0, -90 * rotationIndex);

        // Important: If we rotate 90 degrees, Width becomes Height and vice versa.
        if (rotationIndex == 1 || rotationIndex == 3)
        {
            Width = data.height;
            Height = data.width;
        }
        else
        {
            Width = data.width;
            Height = data.height;
        }
    }

    // Changes visuals when dragging
    public void SetPickedUpState(bool pickedUp)
    {
        // We use CanvasGroup to make the item "invisible" to the mouse while dragging.
        // This allows us to click the Slot underneath it to place it down.
        if (GetComponent<CanvasGroup>() != null)
        {
            GetComponent<CanvasGroup>().blocksRaycasts = !pickedUp;
            GetComponent<CanvasGroup>().alpha = pickedUp ? 0.7f : 1f;
        }
    }

    public void SetGridPosition(int x, int y)
    {
        onGridPositionX = x;
        onGridPositionY = y;
    }

    // Complex Math: Determines if a specific cell in the grid is filled by this shape.
    // This allows for L-Shapes, T-Shapes, etc., instead of just Blocks.
    public bool IsPartOfShape(int r, int c)
    {
        if (data.shape == null || data.shape.Length != data.width * data.height) return true;

        int originalRow, originalCol;
        
        // We map current rotated coordinates back to original data coordinates
        switch (rotationIndex)
        {
            case 0: originalRow = r; originalCol = c; break;
            case 1: originalRow = data.height - 1 - c; originalCol = r; break;
            case 2: originalRow = data.height - 1 - r; originalCol = data.width - 1 - c; break;
            case 3: originalRow = c; originalCol = data.width - 1 - r; break;
            default: return true;
        }

        int index = originalRow * data.width + originalCol;
        
        if (index >= 0 && index < data.shape.Length) return data.shape[index];
        return true;
    }
}