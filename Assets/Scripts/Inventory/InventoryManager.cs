using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections.Generic;

public class InventoryManager : MonoBehaviour
{
    // --- Configuration in the Inspector (RENAMED FIELDS) ---


    public int width = 3;   
    public int height = 4;
    public float slotSize = 100f;

    public GameObject inventorySlotPrefab; 
   

    // --- Private Fields ---
    private InventoryGrid inventoryGrid; 
    private GridLayoutGroup gridLayoutGroup;
    private InventorySlot[,] inventorySlots; 

    
    void Awake()
    {   
        // Grid now uses (Rows, Columns) in its constructor
        inventoryGrid = new InventoryGrid(height, width, slotSize);
        // FIX: Array initialized as [Rows, Columns]
        inventorySlots = new InventorySlot[height, width]; 
        
        gridLayoutGroup = GetComponent<GridLayoutGroup>();
        
        if (gridLayoutGroup == null)
        {
            Debug.LogError("InventoryManager requires a GridLayoutGroup component on the same GameObject.");
            return;
        }

        ConfigureGridLayout();
        GenerateSlots();
    }
    
    private void ConfigureGridLayout()
    {
        gridLayoutGroup.cellSize = new Vector2(slotSize, slotSize);
        gridLayoutGroup.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
        // The GridLayoutGroup still defines its layout by the Column Count (width)
        gridLayoutGroup.constraintCount = width; 
    }

    private void GenerateSlots()
    {
        int totalSlots = height * width; 

        for (int i = 0; i < totalSlots; i++)
        {
            GameObject inventorySlot = Instantiate(inventorySlotPrefab, transform);
            
            InventorySlot inventorySlotScript = inventorySlot.GetComponent<InventorySlot>();
            
            if (inventorySlotScript != null)
            {
                // Pass the Column Count
                inventorySlotScript.Initialize(this, width); 
                
                inventorySlotScript.UpdatePositionText(); 
                
                // Read from the slot's public properties:
                int x = inventorySlotScript.X; // Row
                int y = inventorySlotScript.Y; // Column
                
                // Store in [Row, Column] order
                inventorySlots[x, y] = inventorySlotScript; 
                inventorySlot.name = $"Slot ({x},{y})";
            }
        }
    }
    
}