using UnityEngine;

public class Interactable : MonoBehaviour
{
    public enum InteractionType { Dock, FishNode }
    public InteractionType type;

    [Header("Fishing Settings")]
    // We define the fish here so we can change it in the Inspector for different spots.
    public string fishName = "Tuna"; 
    public int difficulty = 2;
    public int fishValue = 50; 

    private bool playerInRange = false;

    // Detect when Ship enters the trigger zone
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player") || other.GetComponent<Ship>())
        {
            playerInRange = true;
            // Tell UI to show the "Press Space" prompt
            if (GameManager.Instance != null)
                GameManager.Instance.uiManager.ToggleInteractionPopup(true);
        }
    }

    // Detect when Ship leaves
    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player") || other.GetComponent<Ship>())
        {
            playerInRange = false;
            // Hide the prompt
            if (GameManager.Instance != null)
                GameManager.Instance.uiManager.ToggleInteractionPopup(false);
        }
    }

    private void Update()
    {
        // Only allow interaction if:
        // 1. Player is close enough
        // 2. We are currently Sailing (stops us from opening the menu twice)
        if (playerInRange && GameManager.Instance.CurrentState == GameState.Sailing)
        {
            if (Input.GetKeyDown(KeyCode.Space))
            {
                TriggerInteraction();
            }
        }
    }

    private void TriggerInteraction()
    {
        if (type == InteractionType.Dock)
        {
            GameManager.Instance.SetState(GameState.Docked);
        }
        else if (type == InteractionType.FishNode)
        {
            // Create a temporary Fish object to pass data to the minigame
            Fish fish = new Fish(fishName, fishValue, 1, difficulty); 
            GameManager.Instance.SetState(GameState.Fishing, fish);
        }
    }
}