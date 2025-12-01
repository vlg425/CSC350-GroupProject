using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class InventoryManager : MonoBehaviour
{
    public static InventoryManager Instance;

    // --- State ---
    private InventoryItem selectedItem;
    private Inventory currentHoveredInventory;
    
    // --- REFERENCES ---
    [Header("Drag & Drop")]
    public Transform dragLayer; // ASSIGN THIS IN INSPECTOR!

    public Color validHighlight = new Color(0, 1, 0, 0.2f);
    public Color invalidHighlight = new Color(1, 0, 0, 0.2f);

    void Awake()
    {
        Instance = this;
    }

    void Update()
    {
        if (selectedItem != null)
        {
            selectedItem.transform.position = Input.mousePosition;

            if (Input.GetMouseButtonDown(1))
            {
                selectedItem.Rotate();
            }

            HandleHighlighting();
        }
    }

    // --- INTERACTION ---

    public void OnItemClicked(InventoryItem item)
    {
        if (selectedItem == null)
        {
            PickUpItem(item);
        }
        else
        {
            if (currentHoveredInventory != null)
            {
                TryPlaceItem(currentHoveredInventory);
            }
        }
    }

    public void OnSlotClicked(Inventory inventory, InventorySlot slot)
    {
        if (selectedItem != null)
        {
            TryPlaceItem(inventory);
        }
    }

    public void OnSlotEnter(Inventory inventory)
    {
        currentHoveredInventory = inventory;
    }

    public void OnSlotExit(Inventory inventory)
    {
        if (currentHoveredInventory == inventory)
        {
            currentHoveredInventory.ClearHighlights();
            currentHoveredInventory = null;
        }
    }

    // --- LOGIC ---

    private void PickUpItem(InventoryItem item)
    {
        selectedItem = item;
        
        Inventory[] allInventories = FindObjectsOfType<Inventory>();
        foreach(var inv in allInventories)
        {
            if (inv.ContainsItem(item))
            {
                inv.ClearSlotReferences(item);
                break;
            }
        }

        selectedItem.SetPickedUpState(true);
        
        // FIX: Parent to the specific DragLayer
        if (dragLayer != null)
        {
            selectedItem.transform.SetParent(dragLayer);
        }
        else
        {
            // Fallback if you forgot to assign it
            selectedItem.transform.SetParent(this.transform.root); 
        }

        // FIX: Ensure scale is reset, sometimes parenting changes this
        selectedItem.transform.localScale = Vector3.one; 
    }

    private void TryPlaceItem(Inventory targetInventory)
    {
        Vector2Int origin = targetInventory.GetGridIndexFromMouse(selectedItem);

        if (targetInventory.CheckIfItemFits(selectedItem, origin.x, origin.y))
        {
            targetInventory.PlaceItem(selectedItem, origin.x, origin.y);
            
            selectedItem.SetPickedUpState(false);
            selectedItem = null;
            targetInventory.ClearHighlights();
        }
        else
        {
            Debug.Log("Item does not fit.");
        }
    }

    private void HandleHighlighting()
    {
        if (currentHoveredInventory != null)
        {
            currentHoveredInventory.HighlightGrid(selectedItem, validHighlight, invalidHighlight);
        }
    }
}