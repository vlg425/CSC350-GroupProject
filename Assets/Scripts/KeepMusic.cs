using UnityEngine;

public class KeepMusic : MonoBehaviour
{
    private static KeepMusic instance;

    void Awake()
    {
        // Singleton Pattern: Prevents duplicate music players
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject); // The magic line: keeps this object alive across scenes
        }
        else
        {
            Destroy(gameObject); // If we load the menu again, destroy the new copy so we don't hear echo
        }
    }
}