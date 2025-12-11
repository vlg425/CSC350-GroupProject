using UnityEngine;

/// <summary>
/// Simple data class representing a fish in the game.
/// </summary>

[System.Serializable]
public enum FishSize
{
    Small,   // 1x1
    Medium,  // L-shape
    Large    // 4x4 block
}

[System.Serializable]
public class Fish
{
    // Basic data
    public string Name;       // e.g. "Sardine", "Tuna"
    public int Value;         // How much money you get for selling it
    public int Rarity;        // 1 = common, 5 = very rare
    public int QteDifficulty; // Higher = harder QTE (faster arrow, smaller green zone)

    // New: size + which inventory item this fish uses
    public FishSize Size;             // Small / Medium / Large
    public InventoryItemSO ItemData;  // ScriptableObject used by the inventory

    // Main constructor used by FishingManager when we build fish presets
    public Fish(
        string name,
        int value,
        int rarity,
        int qteDifficulty,
        FishSize size,
        InventoryItemSO itemData)
    {
        Name         = name;
        Value        = value;
        Rarity       = rarity;
        QteDifficulty = qteDifficulty;
        Size         = size;
        ItemData     = itemData;
    }

    // Optional: old constructor kept for compatibility (if anything still uses it)
    public Fish(string name, int value, int rarity, int qteDifficulty)
        : this(name, value, rarity, qteDifficulty, FishSize.Small, null)
    {
    }
}
