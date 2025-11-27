using UnityEngine;

[CreateAssetMenu(fileName = "NewItemSO", menuName = "Inventory/ItemSO")]
public class InventoryItemSO : ScriptableObject
{
    [Header("Item Data")]
    public int itemID = 0;
    public string itemName = "New Item";
    [TextArea(3, 5)]
    public string description = "Item description here.";

    [Header("Dimensions")]
    // Renamed for clarity: height affects the Row dimension (X-index)
    public int height = 1; 
    // Renamed for clarity: width affects the Column dimension (Y-index)
    public int width = 1;  
    
    [Header("Visuals")]
    public Sprite icon; 
}