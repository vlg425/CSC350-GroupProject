using UnityEngine;

/// <summary>
/// Simple data class representing a fish in the game.
/// This is a placeholder and can be expanded later.
/// </summary>
public class Fish
{
    // Public fields so they are easy to tweak for now.
    // Later you can switch to properties or ScriptableObjects if needed.

    public string Name;          // e.g., "Sardine", "Tuna"
    public int Value;            // How much money you get for selling it
    public int Rarity;           // 1 = common, 5 = very rare
    public int QteDifficulty;    // Higher = harder QTE (faster arrow, smaller green zone)

    // Optional: simple constructor to quickly make test fish in code
    public Fish(string name, int value, int rarity, int qteDifficulty)
    {
        Name = name;
        Value = value;
        Rarity = rarity;
        QteDifficulty = qteDifficulty;
    }
}