using System;
using System.Collections.Generic;

[Serializable]
public class InventorySaveData
{
    // A list of all items currently in this specific inventory box
    public List<ItemSaveData> items = new List<ItemSaveData>();
}

[Serializable]
public class ItemSaveData
{
    public string itemID; // Which item is it? (e.g. "sword_1")
    public int x;         // Grid Row
    public int y;         // Grid Col
    public bool isRotated;
}