using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections.Generic;

public class InventoryManager : MonoBehaviour
{
    public static InventoryManager Instance;

    [Header("References")]
    public List<InventoryItemSO> itemDatabase; 
    public Transform heldItemLayer; 

    [Header("Highlight Colors")]
    public Color validColor = new Color(0, 1, 0, 0.5f);     
    public Color invalidColor = new Color(1, 0, 0, 0.5f);   
    public Color swapColor = new Color(1, 0.5f, 0, 0.5f);   

    private InventoryItem heldItem; 
    private Inventory currentHoveredInventory; 
    private Inventory lastHighlightedInventory; 

    void Awake() => Instance = this;

    void Update()
    {
        if (heldItem == null) return;

        heldItem.transform.position = Input.mousePosition;

        if (Input.GetMouseButtonDown(1)) heldItem.Rotate();
        if (Input.GetKeyDown(KeyCode.Alpha0)) DestroyItem(heldItem);

        UpdateHighlight();
    }

    public void OnItemClicked(InventoryItem item, PointerEventData eventData)
    {
        if (eventData.button == PointerEventData.InputButton.Middle)
        {
            DestroyItem(item);
            return;
        }

        if (eventData.button == PointerEventData.InputButton.Left)
        {
            if (heldItem == null) PickUpItem(item);
            else PlaceItemLogic();
        }
    }

    public void OnSlotClicked(Inventory inventory)
    {
        if (heldItem != null) PlaceItemLogic();
    }

    public void OnSlotEnter(Inventory inventory) => currentHoveredInventory = inventory;
    
    public void OnSlotExit(Inventory inventory)
    {
        if (currentHoveredInventory == inventory)
        {
            currentHoveredInventory = null; // Visuals cleared by UpdateHighlight
        }
    }

    public void PickUpItem(InventoryItem item)
    {
        heldItem = item;
        var oldInv = item.GetComponentInParent<Inventory>();
        if (oldInv != null) oldInv.ClearSlotReferences(item);

        heldItem.SetPickedUpState(true);
        if (heldItemLayer != null) heldItem.transform.SetParent(heldItemLayer);
        
        // Find other inventories to clear references (if moving between windows)
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

        Vector2Int pos = target.MouseToGrid(heldItem);

        // 1. Direct Placement
        if (target.CanPlaceItem(heldItem, pos.x, pos.y))
        {
            target.PlaceItem(heldItem, pos.x, pos.y);
            DropItem();
        }
        // 2. Swap Logic
        else
        {
            InventoryItem blocker = target.GetSwapTarget(heldItem, pos.x, pos.y);
            if (blocker != null && target.CanPlaceItem(heldItem, pos.x, pos.y, blocker))
            {
                target.ClearSlotReferences(blocker);
                target.PlaceItem(heldItem, pos.x, pos.y);
                DropItem();
                PickUpItem(blocker); 
            }
        }
    }

    private void UpdateHighlight()
    {
        Inventory target = DetectInventory();

        if (lastHighlightedInventory != null && lastHighlightedInventory != target)
        {
            lastHighlightedInventory.ClearHighlights();
        }
        lastHighlightedInventory = target;

        if (target == null) return;

        Vector2Int pos = target.MouseToGrid(heldItem);

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

    private void DestroyItem(InventoryItem item)
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

    public InventoryItemSO GetItemByName(string name) => itemDatabase.Find(i => i.name == name);
}