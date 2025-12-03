using UnityEngine;
using System.Collections.Generic;
using System.IO;

public class Inventory : MonoBehaviour
{
    [Header("Template (Optional)")]

    [Header("Config")]
    public string inventoryName = "PlayerInventory";
    public int width = 3, height = 4;
    public float slotSize = 100f;

    [Header("Refs")]
    public GameObject inventorySlotPrefab;
    public GameObject inventoryItemPrefab;
    public Transform slotsContainer, itemsContainer;

    //uses the GLOBAL struct defined at the bottom of this file
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

                if (IsBounds(startItem.x, startItem.y)) PlaceItem(itemScript, startItem.x, startItem.y);
                else Destroy(obj);
            }
        }
    }

    private void GenerateGrid()
    {
        foreach (Transform t in slotsContainer) Destroy(t.gameObject);

        // Calculate the Top-Left starting point relative to the CENTER (0,0)
        // Example: If grid is 300 wide, start at -150.
        Vector2 startPos = new Vector2(
            -(width * slotSize) / 2f, 
            (height * slotSize) / 2f
        );

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
            
            // Set Pivot and Anchor to Center so math is consistent
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.anchorMin = new Vector2(0.5f, 0.5f); 
            rt.anchorMax = new Vector2(0.5f, 0.5f);

            // Calculate position
            // We add (slotSize / 2) because the slot's pivot is in its center
            float x = startPos.x + (c * slotSize) + (slotSize / 2);
            float y = startPos.y - (r * slotSize) - (slotSize / 2);
            
            rt.anchoredPosition = new Vector2(x, y);
        }
    }

     public Vector2Int MouseToGrid(InventoryItem item)
    {
        // Get mouse position local to the SlotsContainer (0,0 is center)
        RectTransformUtility.ScreenPointToLocalPointInRectangle(slotsRect, Input.mousePosition, null, out Vector2 localPos);

        // Calculate that same Top-Left start point
        Vector2 startPos = new Vector2(
            -(width * slotSize) / 2f, 
            (height * slotSize) / 2f
        );

        // Calculate the offset from the Start Point
        // Invert Y because Unity UI Y goes UP, but Grid Rows go DOWN
        Vector2 offset = new Vector2(
            localPos.x - startPos.x,
            startPos.y - localPos.y 
        );

        // Convert pixels to indices
        float gridX = offset.x / slotSize;
        float gridY = offset.y / slotSize;

        // Apply Item Centering Logic
        return new Vector2Int(
            Mathf.RoundToInt(gridY - (item.Height / 2f)),
            Mathf.RoundToInt(gridX - (item.Width / 2f))
        );
    }

    public bool CanPlaceItem(InventoryItem item, int r, int c, InventoryItem ignore = null)
    {
        for (int i = 0; i < item.Height; i++)
        {
            for (int j = 0; j < item.Width; j++)
            {
                if (!item.IsPartOfShape(i, j)) continue;
                int rr = r + i, cc = c + j;
                if (!IsBounds(rr, cc)) return false;
                var slotItem = inventorySlots[rr, cc].currentItem;
                if (slotItem != null && slotItem != item && slotItem != ignore) return false;
            }
        }
        return true;
    }

    public InventoryItem GetSwapTarget(InventoryItem item, int r, int c)
    {
        InventoryItem obs = null;
        for (int i = 0; i < item.Height; i++)
        {
            for (int j = 0; j < item.Width; j++)
            {
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
                if (item.IsPartOfShape(i, j)) inventorySlots[r + i, c + j].currentItem = item;
            }
        }
    }

    public void HighlightSlots(InventoryItem item, Color color)
    {
        ClearHighlights();
        Vector2Int pos = MouseToGrid(item);
        for (int i = 0; i < item.Height; i++)
        {
            for (int j = 0; j < item.Width; j++)
            {
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
    public void ClearSlotReferences(InventoryItem item) { for (int i = 0; i < height; i++) for (int j = 0; j < width; j++) if (inventorySlots[i, j].currentItem == item) inventorySlots[i, j].currentItem = null; }
    public bool ContainsItem(InventoryItem item) { for (int i = 0; i < height; i++) for (int j = 0; j < width; j++) if (inventorySlots[i, j].currentItem == item) return true; return false; }
    private bool IsBounds(int r, int c) => r >= 0 && r < height && c >= 0 && c < width;
}

// --- GLOBAL STRUCT (Outside Class) ---
[System.Serializable]
public struct StartingItem
{
    public InventoryItemSO item;
    public int x;
    public int y;
    [Range(0, 3)] public int rotation;
}