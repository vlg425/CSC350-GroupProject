using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "New Inventory Config", menuName = "Inventory/Config")]
public class InventoryConfigSO : ScriptableObject
{
    [Header("Basic Settings")]
    public string inventoryName = "Chest";
    public int width = 3;
    public int height = 4;
    public float slotSize = 100f;

    [Header("Default Content")]
    // References the global StartingItem struct
    public List<StartingItem> startingItems; 
}