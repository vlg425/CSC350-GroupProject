using UnityEngine;

[CreateAssetMenu(fileName = "New Item", menuName = "Inventory/Item")]
public class InventoryItemSO : ScriptableObject
{
    [Header("Grid Data")]
    [Min(1)] public int width = 1;
    [Min(1)] public int height = 1;
    
    [Header("Visuals")]
    public Sprite icon;
    public Color color = Color.white; 

    [Header("Economy & Info")]
    [TextArea(4, 4)] public string description; // Shown in tooltip
    public int buyPrice = 10;  
    public int sellPrice = 5;  

    [Header("Shape Definition")]
    // Checked = Solid, Unchecked = Empty Space
    [Tooltip("Check the box for Solid, Uncheck for Empty.")]
    public bool[] shape; 
}