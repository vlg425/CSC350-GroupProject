using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.IO; 

public class Inventory : MonoBehaviour
{
    // ... (Config / References / Awake / Save System - Keep all this unchanged) ...
    [Header("Grid Config")]
    public string inventoryID = "PlayerInventory"; 
    public int width = 3;  
    public int height = 4; 
    public float slotSize = 100f;
    
    [Header("References")]
    public GameObject inventorySlotPrefab;
    public GameObject inventoryItemPrefab; 
    public Transform slotsContainer; 
    public Transform itemsContainer; 

    private InventorySlot[,] inventorySlots;
    private List<InventorySlot> currentlyHighlighted = new List<InventorySlot>();
    private RectTransform slotsRect;

    void Awake()
    {
        inventorySlots = new InventorySlot[height, width];
        slotsRect = slotsContainer.GetComponent<RectTransform>();
        GenerateSlots();
    }

    // ... (Keep Save/Load Code Logic Same as Before) ...
    [System.Serializable] public class InventorySaveData { public List<ItemSaveData> items = new List<ItemSaveData>(); }
    [System.Serializable] public struct ItemSaveData { public string itemName; public int x; public int y; public bool isRotated; }
    public void QuickSave() => SaveInventory();
    public void QuickLoad() => LoadInventory();
    
    public void SaveInventory() {
        InventorySaveData saveData = new InventorySaveData();
        foreach (Transform child in itemsContainer) {
            InventoryItem item = child.GetComponent<InventoryItem>();
            if (item != null) {
                ItemSaveData data = new ItemSaveData { itemName = item.itemData.name, x = item.GridStartX, y = item.GridStartY, isRotated = item.IsRotated };
                saveData.items.Add(data);
            }
        }
        string json = JsonUtility.ToJson(saveData, true);
        File.WriteAllText(Application.persistentDataPath + $"/{inventoryID}.json", json);
        Debug.Log($"Saved to {Application.persistentDataPath}");
    }
    public void LoadInventory() {
        string path = Application.persistentDataPath + $"/{inventoryID}.json";
        if (!File.Exists(path)) return;
        string json = File.ReadAllText(path);
        InventorySaveData saveData = JsonUtility.FromJson<InventorySaveData>(json);
        ClearAllItems();
        foreach (ItemSaveData data in saveData.items) {
            InventoryItemSO itemSO = InventoryManager.Instance.GetItemByName(data.itemName);
            if (itemSO != null) {
                GameObject newItemObj = Instantiate(inventoryItemPrefab, transform.root);
                InventoryItem itemScript = newItemObj.GetComponent<InventoryItem>();
                itemScript.Initialize(itemSO);
                if (data.isRotated) itemScript.Rotate();
                PlaceItem(itemScript, data.x, data.y);
            }
        }
    }
    public void ClearAllItems() {
        foreach (Transform child in itemsContainer) Destroy(child.gameObject);
        for (int r = 0; r < height; r++) { for (int c = 0; c < width; c++) { inventorySlots[r, c].storedItem = null; } }
    }

    // --- GRID MATH & LOGIC ---

    public Vector2Int GetGridIndexFromMouse(InventoryItem item)
    {
        Vector2 localMouse;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(slotsRect, Input.mousePosition, null, out localMouse);
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

    // --- NEW: Better Logic to find the SINGLE item blocking us ---
    public InventoryItem GetObstructingItem(InventoryItem item, int startRow, int startCol)
    {
        InventoryItem foundObs = null;

        for (int r = 0; r < item.Height; r++)
        {
            for (int c = 0; c < item.Width; c++)
            {
                if (!IsPartOfShape(item, r, c)) continue;

                int rPos = startRow + r;
                int cPos = startCol + c;

                if (!IsWithinGrid(rPos, cPos)) return null; // Out of bounds = no swap possible

                if (!inventorySlots[rPos, cPos].IsEmpty() && inventorySlots[rPos, cPos].storedItem != item)
                {
                    InventoryItem detected = inventorySlots[rPos, cPos].storedItem;
                    
                    if (foundObs == null)
                    {
                        foundObs = detected; // Found first blocker
                    }
                    else if (foundObs != detected)
                    {
                        return null; // Found a SECOND different blocker. Cannot swap with two things at once!
                    }
                }
            }
        }
        return foundObs; // Returns the single item blocking us, or null if none/many
    }
    // -------------------------------------------------------------

    public bool CheckIfItemFits(InventoryItem item, int startRow, int startCol, InventoryItem itemToIgnore = null)
    {
        for (int r = 0; r < item.Height; r++)
        {
            for (int c = 0; c < item.Width; c++)
            {
                if (!IsPartOfShape(item, r, c)) continue;

                int rPos = startRow + r;
                int cPos = startCol + c;

                if (!IsWithinGrid(rPos, cPos)) return false;
                
                if (!inventorySlots[rPos, cPos].IsEmpty())
                {
                    // Ignore ourselves AND the swap target
                    if (inventorySlots[rPos, cPos].storedItem != item && 
                        inventorySlots[rPos, cPos].storedItem != itemToIgnore)
                    {
                        return false;
                    }
                }
            }
        }
        return true;
    }

    public void PlaceItem(InventoryItem item, int startRow, int startCol)
    {
        item.transform.SetParent(itemsContainer);
        InventorySlot rootSlot = inventorySlots[startRow, startCol];
        item.transform.position = rootSlot.transform.position;
        item.transform.localScale = Vector3.one;
        item.SetGridPosition(startRow, startCol);

        Vector2 newPos = item.GetComponent<RectTransform>().anchoredPosition;
        newPos.x += (item.Width - 1) * slotSize / 2;
        newPos.y -= (item.Height - 1) * slotSize / 2;
        item.GetComponent<RectTransform>().anchoredPosition = newPos;

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

    public void HighlightGrid(InventoryItem item, Color color)
    {
        ClearHighlights();
        Vector2Int origin = GetGridIndexFromMouse(item);
        
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
                    slot.SetHighlight(color);
                    currentlyHighlighted.Add(slot);
                }
            }
        }
    }

    // ... (Utilities: ClearHighlights, ClearSlotReferences, ContainsItem, IsWithinGrid, IsPartOfShape, GenerateSlots - Keep Unchanged) ...
    public void ClearHighlights() { foreach (InventorySlot slot in currentlyHighlighted) slot.ResetColor(); currentlyHighlighted.Clear(); }
    public void ClearSlotReferences(InventoryItem item) { for (int r = 0; r < height; r++) { for (int c = 0; c < width; c++) { if (inventorySlots[r, c].storedItem == item) inventorySlots[r, c].storedItem = null; } } }
    public bool ContainsItem(InventoryItem item) { for (int r = 0; r < height; r++) { for (int c = 0; c < width; c++) { if (inventorySlots[r, c].storedItem == item) return true; } } return false; }
    private bool IsWithinGrid(int r, int c) { return r >= 0 && r < height && c >= 0 && c < width; }
    private bool IsPartOfShape(InventoryItem item, int r, int c) {
        if (item.CurrentShape == null || item.CurrentShape.Length == 0) return true;
        int index = r * item.Width + c;
        if (index >= 0 && index < item.CurrentShape.Length) return item.CurrentShape[index];
        return true; 
    }
    private void GenerateSlots() {
        foreach (Transform child in slotsContainer) Destroy(child.gameObject);
        int totalSlots = height * width; 
        for (int i = 0; i < totalSlots; i++) {
            GameObject newSlotObj = Instantiate(inventorySlotPrefab, slotsContainer); 
            InventorySlot slotScript = newSlotObj.GetComponent<InventorySlot>();
            int r = i / width; int c = i % width;
            slotScript.Initialize(this, r, c); 
            newSlotObj.name = $"Slot ({r},{c})";
            inventorySlots[r, c] = slotScript;
        }
    }
}