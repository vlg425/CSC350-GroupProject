using UnityEngine;
using System.Collections; // Required for Coroutines

// We use an Enum to define clear labels for every mode the game can be in.
public enum GameState
{
    Sailing,     // Standard movement
    Fishing,     // The Minigame
    Docked,      // The Shop/Upgrade menu
    PlayerMenu   // The Inventory (Tab key)
}

public class GameManager : MonoBehaviour
{
    // Singleton Pattern: Allows other scripts to call 'GameManager.Instance' easily.
    public static GameManager Instance;

    // 'private set' means other scripts can READ the state, but only this script can CHANGE it.
    public GameState CurrentState { get; private set; }

    [Header("References")]
    public UIManager uiManager;
    public FishingManager fishingManager;
    public CurrencyManager currencyManager;

    // Data passed from the Interactable to the Minigame
    private Fish pendingFish;
    
    // Flag to track if the arrow is actually moving in the minigame
    private bool hasFishingStarted; 
    
    // A timer to prevent "Input Bleed" (e.g., pressing Space to open a menu 
    // shouldn't immediately click a button inside that menu).
    private float inputCooldown; 

    private void Awake()
    {
        // Standard Singleton setup
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private void Start()
    {
        // Always start in sailing mode
        SetState(GameState.Sailing);
    }

    private void Update()
    {
        HandleInput();
        MonitorFishingState();
    }

    private void HandleInput()
    {
        // SECURITY CHECK 1: Global Cooldown
        // If we just switched states less than 0.2s ago, ignore all buttons.
        if (Time.time < inputCooldown) return;

        // SECURITY CHECK 2: Item Safety
        // If the player is dragging an item, we block ESC and SPACE.
        // This prevents the item from being deleted or stuck on the mouse if the menu closes.
        if (InventoryManager.Instance != null && InventoryManager.Instance.IsHoldingItem()) return;

        // --- FISHING LOGIC ---
        if (CurrentState == GameState.Fishing)
        {
            // If the minigame window is open, but the arrow isn't moving yet...
            if (!hasFishingStarted)
            {
                // Wait for the player to decide to cast the line
                if (Input.GetKeyDown(KeyCode.Space))
                {
                    hasFishingStarted = true;
                    // Fix: We use a Coroutine to wait 1 frame.
                    // This ensures the Space press doesn't "leak" into the minigame and trigger an instant win.
                    StartCoroutine(StartMinigameDelayed());
                }
            }
        }

        // --- TAB KEY (Toggle Inventory) ---
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            // Toggle between Sailing and Inventory
            if (CurrentState == GameState.Sailing) SetState(GameState.PlayerMenu);
            else if (CurrentState == GameState.PlayerMenu) SetState(GameState.Sailing);
        }

        // --- ESCAPE KEY (Exit Menus) ---
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            // Rule: You cannot quit if the fishing arrow is currently flying.
            if (CurrentState == GameState.Fishing && hasFishingStarted) return;

            // If we are in ANY menu, go back to sailing
            if (CurrentState == GameState.Docked || 
                CurrentState == GameState.PlayerMenu ||
                CurrentState == GameState.Fishing) 
            {
                SetState(GameState.Sailing);
            }
        }
    }

    // This Coroutine fixes the "Instant Win" bug.
    // It waits for the frame to finish rendering before turning on the fishing script.
    private IEnumerator StartMinigameDelayed()
    {
        yield return new WaitForEndOfFrame();

        if (fishingManager != null) 
        {
            fishingManager.StartFishing(pendingFish);
        }
    }

    // We check this every frame to see if the Minigame finished (Win or Lose)
    private void MonitorFishingState()
    {
        if (CurrentState == GameState.Fishing && hasFishingStarted)
        {
            // We check the boolean inside your teammate's script
            if (fishingManager != null && !fishingManager.IsFishing)
            {
                Debug.Log("Minigame finished.");
                
                // We turn off our flag, but we KEEP the UI open.
                // This forces the player to place the fish into their bag before leaving.
                hasFishingStarted = false; 
            }
        }
    }

    // This is the main function to switch modes.
    // It handles the UI switching logic so other scripts don't have to.
    public void SetState(GameState newState, Fish fishData = null)
    {
        CurrentState = newState;
        
        // Set the safety timer (0.2 seconds) to prevent double-clicks
        inputCooldown = Time.time + 0.2f;

        switch (newState)
        {
            case GameState.Sailing:
                Time.timeScale = 1f; // Unpause time
                uiManager.ShowSailingHUD();
                break;

            case GameState.Fishing:
                pendingFish = fishData;
                hasFishingStarted = false; // Reset logic
                uiManager.ShowFishingUI();
                break;

            case GameState.Docked:
                uiManager.ShowDockUI();
                break;

            case GameState.PlayerMenu:
                uiManager.ShowPlayerInventory();
                break;
        }
    }
}