using UnityEngine;
using System.Collections.Generic;

public class Inventory : MonoBehaviour
{
    [Header("Config")]
    public bool isShop; 
    public string inventoryName = "PlayerInventory";
    public int width = 3, height = 4;
    public float slotSize = 100f;

    [Header("Refs")]
    public GameObject inventorySlotPrefab;
    public GameObject inventoryItemPrefab;
    public Transform slotsContainer, itemsContainer;

    public List<StartingItem> startingItems;

    private InventorySlot[,] inventorySlots;
    private List<InventorySlot> highlights = new List<InventorySlot>();
    private RectTransform slotsRect;

    void Awake()
    {
        inventorySlots = new InventorySlot[height, width];
        slotsRect = slotsContainer.GetComponent<RectTransform>();
        GenerateGrid();
    }

    void Start()
    {
        SpawnStartingItems();
    }

    public void SpawnStartingItems()
    {
        foreach (var startItem in startingItems)
        {
            if (startItem.item != null)
            {
                var obj = Instantiate(inventoryItemPrefab, transform.root);
                var itemScript = obj.GetComponent<InventoryItem>();
                itemScript.Initialize(startItem.item);
                
                for (int i = 0; i < startItem.rotation; i++) itemScript.Rotate();

                if (IsBounds(startItem.x, startItem.y)) 
                {
                    PlaceItem(itemScript, startItem.x, startItem.y);
                }
                else 
                {
                    Destroy(obj);
                }
            }
        }
    }

    private void GenerateGrid()
    {
        foreach (Transform t in slotsContainer) Destroy(t.gameObject);

        Vector2 startPos = new Vector2(-(width * slotSize) / 2f, (height * slotSize) / 2f);

        for (int i = 0; i < height * width; i++) 
        {
            GameObject slotObj = Instantiate(inventorySlotPrefab, slotsContainer);
            InventorySlot slot = slotObj.GetComponent<InventorySlot>();
            
            int r = i / width; 
            int c = i % width;
            
            slot.Initialize(this, r, c);
            slotObj.name = $"Slot ({r},{c})";
            inventorySlots[r, c] = slot;

            RectTransform rt = slotObj.GetComponent<RectTransform>();
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.anchorMin = new Vector2(0.5f, 0.5f); 
            rt.anchorMax = new Vector2(0.5f, 0.5f);

            float x = startPos.x + (c * slotSize) + (slotSize / 2);
            float y = startPos.y - (r * slotSize) - (slotSize / 2);
            
            rt.anchoredPosition = new Vector2(x, y);
        }
    }

     public Vector2Int MouseToGrid(InventoryItem item)
    {
        RectTransformUtility.ScreenPointToLocalPointInRectangle(slotsRect, Input.mousePosition, null, out Vector2 localPos);

        Vector2 startPos = new Vector2(-(width * slotSize) / 2f, (height * slotSize) / 2f);
        Vector2 offset = new Vector2(localPos.x - startPos.x, startPos.y - localPos.y);

        float gridX = offset.x / slotSize;
        float gridY = offset.y / slotSize;

        return new Vector2Int(
            Mathf.RoundToInt(gridY - (item.Height / 2f)),
            Mathf.RoundToInt(gridX - (item.Width / 2f))
        );
    }

    // --- FIX 1: Add Shape Check to CanPlaceItem ---
    public bool CanPlaceItem(InventoryItem item, int r, int c, InventoryItem ignore = null)
    {
        for (int i = 0; i < item.Height; i++)
        {
            for (int j = 0; j < item.Width; j++)
            {
                // SKIP empty parts of the shape
                if (!item.IsPartOfShape(i, j)) continue; 
                
                int rr = r + i, cc = c + j;
                
                // If the solid part is out of bounds, fail
                if (!IsBounds(rr, cc)) return false;
                
                var slotItem = inventorySlots[rr, cc].currentItem;
                if (slotItem != null && slotItem != item && slotItem != ignore) return false;
            }
        }
        return true;
    }

    // --- FIX 2: Add Shape Check to GetSwapTarget ---
    public InventoryItem GetSwapTarget(InventoryItem item, int r, int c)
    {
        InventoryItem obs = null;
        for (int i = 0; i < item.Height; i++)
        {
            for (int j = 0; j < item.Width; j++)
            {
                // SKIP empty parts of the shape
                if (!item.IsPartOfShape(i, j)) continue;

                int rr = r + i, cc = c + j;
                if (!IsBounds(rr, cc)) return null;
                
                var slotItem = inventorySlots[rr, cc].currentItem;
                if (slotItem != null && slotItem != item)
                {
                    if (obs == null) obs = slotItem;
                    else if (obs != slotItem) return null; 
                }
            }
        }
        return obs;
    }

    public void PlaceItem(InventoryItem item, int r, int c)
    {
        item.transform.SetParent(itemsContainer);
        item.transform.position = inventorySlots[r, c].transform.position; 
        item.transform.localScale = Vector3.one;
        item.SetGridPosition(r, c);

        Vector2 pos = item.GetComponent<RectTransform>().anchoredPosition;
        pos.x += (item.Width - 1) * slotSize / 2;
        pos.y -= (item.Height - 1) * slotSize / 2;
        item.GetComponent<RectTransform>().anchoredPosition = pos;

        for (int i = 0; i < item.Height; i++)
        {
            for (int j = 0; j < item.Width; j++)
            {
                // Only occupy the slot if this part of the item is solid
                if (item.IsPartOfShape(i, j)) 
                {
                    inventorySlots[r + i, c + j].currentItem = item;
                }
            }
        }
    }

    // --- FIX 3: Add Shape Check to Highlights ---
    public void HighlightSlots(InventoryItem item, Color color)
    {
        ClearHighlights();
        Vector2Int pos = MouseToGrid(item);
        for (int i = 0; i < item.Height; i++)
        {
            for (int j = 0; j < item.Width; j++)
            {
                // Don't highlight the empty corner!
                if (!item.IsPartOfShape(i, j)) continue;

                int rr = pos.x + i, cc = pos.y + j;
                if (IsBounds(rr, cc))
                {
                    inventorySlots[rr, cc].SetColor(color);
                    highlights.Add(inventorySlots[rr, cc]);
                }
            }
        }
    }

    public void ClearHighlights() { foreach (var s in highlights) s.ResetColor(); highlights.Clear(); }
    
    public void ClearSlotReferences(InventoryItem item) 
    { 
        for (int i = 0; i < height; i++) 
            for (int j = 0; j < width; j++) 
                if (inventorySlots[i, j].currentItem == item) 
                    inventorySlots[i, j].currentItem = null; 
    }
    
    public bool ContainsItem(InventoryItem item) 
    { 
        for (int i = 0; i < height; i++) 
            for (int j = 0; j < width; j++) 
                if (inventorySlots[i, j].currentItem == item) 
                    return true; 
        return false; 
    }
    
    private bool IsBounds(int r, int c) => r >= 0 && r < height && c >= 0 && c < width;
}

[System.Serializable]
public struct StartingItem
{
    public InventoryItemSO item;
    public int x;
    public int y;
    [Range(0, 3)] public int rotation;
}