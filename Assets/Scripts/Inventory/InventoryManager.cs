using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections.Generic;

public class InventoryManager : MonoBehaviour
{
    public static InventoryManager Instance;

    public List<InventoryItemSO> itemLibrary; 

    private InventoryItem selectedItem; 
    private Inventory currentHoveredInventory; 
    
    [Header("Drag & Drop")]
    public Transform dragLayer; 

    [Header("Validation Colors")]
    public Color validHighlight = new Color(0, 1, 0, 0.5f);     
    public Color invalidHighlight = new Color(1, 0, 0, 0.5f);   
    public Color swapHighlight = new Color(1, 0.5f, 0, 0.5f);   

    void Awake()
    {
        Instance = this;
    }

    void Update()
    {
        if (selectedItem != null)
        {
            selectedItem.transform.position = Input.mousePosition;
            if (Input.GetMouseButtonDown(1)) selectedItem.Rotate();
            if (Input.GetKeyDown(KeyCode.Alpha0)) { Destroy(selectedItem.gameObject); selectedItem = null; }

            HandleHighlighting();
        }
    }

    public void OnItemClicked(InventoryItem item, PointerEventData eventData)
    {
        // Middle Click -> Delete
        if (eventData.button == PointerEventData.InputButton.Middle)
        {
            if (selectedItem == item) { Destroy(selectedItem.gameObject); selectedItem = null; return; }
            Inventory owner = item.GetComponentInParent<Inventory>();
            if (owner != null) owner.ClearSlotReferences(item);
            Destroy(item.gameObject);
            return;
        }

        // Left Click -> Pickup / Place
        if (eventData.button == PointerEventData.InputButton.Left)
        {
            if (selectedItem == null)
            {
                PickUpItem(item);
            }
            else
            {
                Inventory targetInv = GetTargetInventory();
                if (targetInv != null)
                {
                    TryPlaceItem(targetInv);
                }
            }
        }
    }

    // --- HELPER: DETECTS ITEMS, SLOTS, OR BACKGROUND ---
    private Inventory GetTargetInventory()
    {
        if (currentHoveredInventory != null) return currentHoveredInventory;

        PointerEventData pointerData = new PointerEventData(EventSystem.current)
        {
            position = Input.mousePosition
        };

        List<RaycastResult> results = new List<RaycastResult>();
        EventSystem.current.RaycastAll(pointerData, results);

        foreach (RaycastResult result in results)
        {
            InventoryItem item = result.gameObject.GetComponent<InventoryItem>();
            if (item != null) return item.GetComponentInParent<Inventory>();

            InventorySlot slot = result.gameObject.GetComponent<InventorySlot>();
            if (slot != null) return slot.GetComponentInParent<Inventory>();

            Inventory inv = result.gameObject.GetComponent<Inventory>();
            if (inv != null) return inv;
        }

        return null;
    }
    // ------------------------------------------------

    public void OnSlotClicked(Inventory inventory, InventorySlot slot)
    {
        if (selectedItem != null) TryPlaceItem(inventory);
    }
    public void OnSlotEnter(Inventory inventory) { currentHoveredInventory = inventory; }
    public void OnSlotExit(Inventory inventory)
    {
        if (currentHoveredInventory == inventory)
        {
            currentHoveredInventory.ClearHighlights();
            currentHoveredInventory = null;
        }
    }

    private void PickUpItem(InventoryItem item)
    {
        selectedItem = item;
        
        // --- FIX IS HERE: Updated to new Unity Syntax ---
        Inventory[] allInventories = FindObjectsByType<Inventory>(FindObjectsSortMode.None);
        // ------------------------------------------------
        
        foreach(var inv in allInventories) { if (inv.ContainsItem(item)) { inv.ClearSlotReferences(item); break; } }
        selectedItem.SetPickedUpState(true);
        if (dragLayer != null) selectedItem.transform.SetParent(dragLayer);
        selectedItem.transform.localScale = Vector3.one; 
    }

    private void TryPlaceItem(Inventory targetInventory)
    {
        Vector2Int origin = targetInventory.GetGridIndexFromMouse(selectedItem);

        // 1. Normal Place
        if (targetInventory.CheckIfItemFits(selectedItem, origin.x, origin.y))
        {
            targetInventory.PlaceItem(selectedItem, origin.x, origin.y);
            selectedItem.SetPickedUpState(false);
            selectedItem = null;
            targetInventory.ClearHighlights();
        }
        // 2. Swap Logic
        else
        {
            InventoryItem overlapItem = targetInventory.GetObstructingItem(selectedItem, origin.x, origin.y);

            if (overlapItem != null && targetInventory.CheckIfItemFits(selectedItem, origin.x, origin.y, overlapItem))
            {
                targetInventory.ClearSlotReferences(overlapItem);
                targetInventory.PlaceItem(selectedItem, origin.x, origin.y);
                selectedItem.SetPickedUpState(false);
                
                PickUpItem(overlapItem);
                targetInventory.ClearHighlights();
            }
        }
    }

    private void HandleHighlighting()
    {
        Inventory targetInv = GetTargetInventory();

        if (targetInv != null)
        {
            Vector2Int origin = targetInv.GetGridIndexFromMouse(selectedItem);

            if (targetInv.CheckIfItemFits(selectedItem, origin.x, origin.y))
            {
                targetInv.HighlightGrid(selectedItem, validHighlight);
            }
            else
            {
                InventoryItem overlapItem = targetInv.GetObstructingItem(selectedItem, origin.x, origin.y);
                if (overlapItem != null && targetInv.CheckIfItemFits(selectedItem, origin.x, origin.y, overlapItem))
                {
                    targetInv.HighlightGrid(selectedItem, swapHighlight);
                }
                else
                {
                    targetInv.HighlightGrid(selectedItem, invalidHighlight);
                }
            }
        }
    }

    public InventoryItemSO GetItemByName(string name) { return itemLibrary.Find(i => i.name == name); }
}