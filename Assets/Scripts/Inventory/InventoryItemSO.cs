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

    [Header("Shape Definition")]
    [Tooltip("Check the box for Solid, Uncheck for Empty. Index = Row * Width + Col")]
    public bool[] shape; 
}