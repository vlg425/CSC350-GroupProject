using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections.Generic;

public class InventoryManager : MonoBehaviour
{
    public static InventoryManager Instance;

    [Header("References")]
    public List<InventoryItemSO> itemDatabase;
    public Transform heldItemLayer; // Where the item goes visually when we drag it
    [SerializeField] private CurrencyManager currencyManager;

    [Header("Highlight Colors")]
    public Color validColor = new Color(0, 1, 0, 0.5f);
    public Color invalidColor = new Color(1, 0, 0, 0.5f);
    public Color swapColor = new Color(1, 0.5f, 0, 0.5f);

    // Trackers
    private InventoryItem heldItem; // What is on the mouse cursor?
    private Inventory currentHoveredInventory; // Which grid are we over?
    private InventoryItem currentHoveredItem; // Which specific item are we over?
    private InventorySlot currentHoveredSlot; 
    private Inventory lastHighlightedInventory;

    void Awake() => Instance = this;

    void Update()
    {
        // Update the visual tooltip box based on what we are hovering
        HandleTooltip();

        // Selling Logic: Only active if we are Docked
        if (GameManager.Instance != null && GameManager.Instance.CurrentState == GameState.Docked)
        {
            if (Input.GetKeyDown(KeyCode.F)) // F key to sell
            {
                TrySellHoveredItem();
            }
        }

        // Held Item Movement Logic
        if (heldItem == null) return;

        // Make the item follow the mouse
        heldItem.transform.position = Input.mousePosition;

        // Right Click to Rotate
        if (Input.GetMouseButtonDown(1)) heldItem.Rotate(); 
        
        // Update the Green/Red grid highlights
        UpdateHighlight();
    }

    private void HandleTooltip()
    {
        if (TooltipManager.Instance == null) return;

        // Priority 1: If holding an item, show its info
        if (heldItem != null)
        {
            TooltipManager.Instance.ShowTooltip(heldItem.data, false);
            return;
        }

        // Priority 2: If hovering over an item
        if (currentHoveredItem != null)
        {
            // Check if the item is in a Shop so we can show "Buy Price" instead of "Sell Price"
            bool isShop = currentHoveredInventory != null && currentHoveredInventory.isShop;
            TooltipManager.Instance.ShowTooltip(currentHoveredItem.data, isShop);
        }
        else
        {
            TooltipManager.Instance.HideTooltip();
        }
    }

    private void TrySellHoveredItem()
    {
        if (currentHoveredItem == null) return;

        // Security Check: Prevent selling items that belong to the shop
        if (currentHoveredInventory != null && currentHoveredInventory.isShop) 
        {
            Debug.Log("You cannot sell items back to the shop shelf. Buy them first!");
            return;
        }

        InventoryItem itemToSell = currentHoveredItem; 
        
        // Calculate Value
        int value = 0;
        if (itemToSell.data != null) value = itemToSell.data.sellPrice;

        // Give Gold
        if (currencyManager != null)
        {
            currencyManager.AddGold(value);
            Debug.Log($"Sold {itemToSell.data.name} for {value} Gold.");
        }

        // Delete the item
        DestroyItem(itemToSell);
    }

    // --- MOUSE EVENT HANDLERS ---
    // These functions are called by the Items/Slots themselves
    
    public void OnItemEnter(InventoryItem item)
    {
        currentHoveredItem = item;
        currentHoveredInventory = item.GetComponentInParent<Inventory>();
    }

    public void OnItemExit(InventoryItem item)
    {
        // Only clear if we are exiting the specific item we were tracking
        if (currentHoveredItem == item)
        {
            currentHoveredItem = null;
            currentHoveredInventory = null;
        }
    }

    public void OnItemClicked(InventoryItem item, PointerEventData eventData)
    {
        if (eventData.button == PointerEventData.InputButton.Left)
        {
            // If hand is empty, pick it up. If hand full, try to swap.
            if (heldItem == null) PickUpItem(item);
            else PlaceItemLogic();
        }
    }

    public void OnSlotClicked(Inventory inventory)
    {
        // Clicking an empty slot only matters if we are holding something
        if (heldItem != null) PlaceItemLogic();
    }

    public void OnSlotEnter(Inventory inventory, InventorySlot slot) 
    {
        currentHoveredInventory = inventory;
        currentHoveredSlot = slot;
    }
    
    public void OnSlotExit(Inventory inventory, InventorySlot slot)
    {
        if (currentHoveredInventory == inventory)
        {
            currentHoveredInventory = null;
            currentHoveredSlot = null;
        }
    }

    // --- PICKUP & PLACEMENT LOGIC ---

    public void PickUpItem(InventoryItem item)
    {
        Inventory sourceInventory = item.GetComponentInParent<Inventory>();

        // BUYING CHECK: Check if we are taking this from a Shop
        if (sourceInventory != null && sourceInventory.isShop)
        {
            int cost = (item.data != null) ? item.data.buyPrice : 0;

            // Try to spend gold. If false, we fail the pickup.
            if (currencyManager != null && !currencyManager.SpendGold(cost))
            {
                Debug.Log("Not enough gold!");
                return; 
            }
        }

        heldItem = item;
        
        // Remove the item reference from the grid logic (it's floating now)
        if (sourceInventory != null) sourceInventory.ClearSlotReferences(item);

        // Visuals: Make it transparent and movable
        heldItem.SetPickedUpState(true);
        if (heldItemLayer != null) heldItem.transform.SetParent(heldItemLayer);
        
        // Safety: Ensure it's removed from all grids in the scene
        var allInventories = FindObjectsByType<Inventory>(FindObjectsSortMode.None);
        foreach(var inv in allInventories) 
        { 
            if (inv.ContainsItem(item)) 
            { 
                inv.ClearSlotReferences(item); 
                break; 
            } 
        }
    }

    private void PlaceItemLogic()
    {
        Inventory target = DetectInventory();
        if (target == null) return;
        
        // Prevent placing items INTO the shop manually
        if (target.isShop) return;

        Vector2Int pos = target.MouseToGrid(heldItem);

        // Case 1: Empty spot? Place it.
        if (target.CanPlaceItem(heldItem, pos.x, pos.y))
        {
            target.PlaceItem(heldItem, pos.x, pos.y);
            DropItem();
        }
        // Case 2: Occupied? Try to swap.
        else
        {
            InventoryItem blocker = target.GetSwapTarget(heldItem, pos.x, pos.y);
            // If there is only ONE item blocking us, we can swap.
            if (blocker != null && target.CanPlaceItem(heldItem, pos.x, pos.y, blocker))
            {
                target.ClearSlotReferences(blocker);
                target.PlaceItem(heldItem, pos.x, pos.y);
                DropItem();
                PickUpItem(blocker); // Automatically pickup the item we swapped with
            }
        }
    }

    // Highlights slots green/red under the mouse
    private void UpdateHighlight()
    {
        Inventory target = DetectInventory();

        // Clear old highlights if we moved to a different grid
        if (lastHighlightedInventory != null && lastHighlightedInventory != target)
        {
            lastHighlightedInventory.ClearHighlights();
        }
        lastHighlightedInventory = target;

        if (target == null) return;

        Vector2Int pos = target.MouseToGrid(heldItem);

        // Decide color based on if placement is valid
        if (target.CanPlaceItem(heldItem, pos.x, pos.y))
        {
            target.HighlightSlots(heldItem, validColor);
        }
        else
        {
            InventoryItem blocker = target.GetSwapTarget(heldItem, pos.x, pos.y);
            bool canSwap = blocker != null && target.CanPlaceItem(heldItem, pos.x, pos.y, blocker);
            target.HighlightSlots(heldItem, canSwap ? swapColor : invalidColor);
        }
    }

    private void DropItem()
    {
        heldItem.SetPickedUpState(false);
        heldItem = null;
        if (lastHighlightedInventory != null) lastHighlightedInventory.ClearHighlights();
        lastHighlightedInventory = null;
    }

    public void DestroyItem(InventoryItem item)
    {
        if (item == heldItem) 
        {
             heldItem = null;
             if (lastHighlightedInventory != null) lastHighlightedInventory.ClearHighlights();
        }

        var owner = item.GetComponentInParent<Inventory>();
        if (owner != null) owner.ClearSlotReferences(item);
        Destroy(item.gameObject);
    }

    // Uses Raycasting to find out which Inventory UI Panel the mouse is currently hovering
    private Inventory DetectInventory()
    {
        if (currentHoveredInventory != null) return currentHoveredInventory;

        PointerEventData pointerData = new PointerEventData(EventSystem.current) { position = Input.mousePosition };
        List<RaycastResult> results = new List<RaycastResult>();
        EventSystem.current.RaycastAll(pointerData, results);

        foreach (var result in results)
        {
            Inventory inv = result.gameObject.GetComponentInParent<Inventory>();
            if (inv != null) return inv;
        }
        return null;
    }

    public bool IsHoldingItem() => heldItem != null;
    public InventoryItem GetHeldItem() => heldItem;
}