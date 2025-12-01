using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class Inventory : MonoBehaviour
{
    // --- Configuration ---
    [Header("Grid Config")]
    public int width = 3;  
    public int height = 4; 
    public float slotSize = 100f;
    
    [Header("References")]
    public GameObject inventorySlotPrefab;
    public Transform slotsContainer; 
    public Transform itemsContainer; 

    // --- State ---
    private InventorySlot[,] inventorySlots;
    private List<InventorySlot> currentlyHighlighted = new List<InventorySlot>();
    private RectTransform slotsRect;

    void Awake()
    {
        inventorySlots = new InventorySlot[height, width];
        slotsRect = slotsContainer.GetComponent<RectTransform>();
        GenerateSlots();
    }

    // --- PUBLIC API (Called by Manager) ---

    public Vector2Int GetGridIndexFromMouse(InventoryItem item)
    {
        Vector2 localMouse;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            slotsRect, Input.mousePosition, null, out localMouse
        );

        Vector2 slot0Pos = inventorySlots[0, 0].transform.localPosition;
        Vector2 relativePos = localMouse - slot0Pos;

        float gridX = relativePos.x / slotSize;
        float gridY = -relativePos.y / slotSize; 

        float topLeftX = gridX - ((item.Width - 1) / 2f);
        float topLeftY = gridY - ((item.Height - 1) / 2f);

        int finalX = Mathf.RoundToInt(topLeftY); 
        int finalY = Mathf.RoundToInt(topLeftX); 

        return new Vector2Int(finalX, finalY);
    }

    public bool CheckIfItemFits(InventoryItem item, int startRow, int startCol)
    {
        for (int r = 0; r < item.Height; r++)
        {
            for (int c = 0; c < item.Width; c++)
            {
                if (!IsPartOfShape(item, r, c)) continue;

                int rPos = startRow + r;
                int cPos = startCol + c;

                if (!IsWithinGrid(rPos, cPos)) return false;
                if (!inventorySlots[rPos, cPos].IsEmpty() && inventorySlots[rPos, cPos].storedItem != item) 
                    return false;
            }
        }
        return true;
    }

    public void PlaceItem(InventoryItem item, int startRow, int startCol)
    {
        // Parent to THIS inventory's container
        item.transform.SetParent(itemsContainer);
        
        // Find reference slot for position
        InventorySlot rootSlot = inventorySlots[startRow, startCol];
        item.transform.position = rootSlot.transform.position;
        item.transform.localScale = Vector3.one;

        // Apply Pivot Offset
        Vector2 newPos = item.GetComponent<RectTransform>().anchoredPosition;
        newPos.x += (item.Width - 1) * slotSize / 2;
        newPos.y -= (item.Height - 1) * slotSize / 2;
        item.GetComponent<RectTransform>().anchoredPosition = newPos;

        // Logical Update
        for (int r = 0; r < item.Height; r++)
        {
            for (int c = 0; c < item.Width; c++)
            {
                if (IsPartOfShape(item, r, c))
                {
                    inventorySlots[startRow + r, startCol + c].storedItem = item;
                }
            }
        }
    }

    public void HighlightGrid(InventoryItem item, Color validColor, Color invalidColor)
    {
        ClearHighlights();
        
        Vector2Int origin = GetGridIndexFromMouse(item);
        bool fits = CheckIfItemFits(item, origin.x, origin.y);
        Color targetColor = fits ? validColor : invalidColor;

        for (int r = 0; r < item.Height; r++)
        {
            for (int c = 0; c < item.Width; c++)
            {
                if (!IsPartOfShape(item, r, c)) continue;

                int rPos = origin.x + r;
                int cPos = origin.y + c;

                if (IsWithinGrid(rPos, cPos))
                {
                    InventorySlot slot = inventorySlots[rPos, cPos];
                    slot.SetHighlight(targetColor);
                    currentlyHighlighted.Add(slot);
                }
            }
        }
    }

    public void ClearHighlights()
    {
        foreach (InventorySlot slot in currentlyHighlighted)
        {
            slot.ResetColor();
        }
        currentlyHighlighted.Clear();
    }

    public void ClearSlotReferences(InventoryItem item)
    {
        for (int r = 0; r < height; r++)
        {
            for (int c = 0; c < width; c++)
            {
                if (inventorySlots[r, c].storedItem == item)
                {
                    inventorySlots[r, c].storedItem = null;
                }
            }
        }
    }
    
    public bool ContainsItem(InventoryItem item)
    {
         for (int r = 0; r < height; r++)
        {
            for (int c = 0; c < width; c++)
            {
                if (inventorySlots[r, c].storedItem == item) return true;
            }
        }
        return false;
    }

    // --- HELPERS ---

    private bool IsWithinGrid(int r, int c)
    {
        return r >= 0 && r < height && c >= 0 && c < width;
    }

    private bool IsPartOfShape(InventoryItem item, int r, int c)
    {
        if (item.CurrentShape == null || item.CurrentShape.Length == 0) return true;
        int index = r * item.Width + c;
        if (index >= 0 && index < item.CurrentShape.Length) return item.CurrentShape[index];
        return true; 
    }

    private void GenerateSlots()
    {
        foreach (Transform child in slotsContainer) Destroy(child.gameObject);

        int totalSlots = height * width; 
        for (int i = 0; i < totalSlots; i++)
        {
            GameObject newSlotObj = Instantiate(inventorySlotPrefab, slotsContainer); 
            InventorySlot slotScript = newSlotObj.GetComponent<InventorySlot>();
            
            int r = i / width;
            int c = i % width;
            
            // Pass THIS inventory instance
            slotScript.Initialize(this, r, c); 
            newSlotObj.name = $"Slot ({r},{c})";
            inventorySlots[r, c] = slotScript;
        }
    }
}