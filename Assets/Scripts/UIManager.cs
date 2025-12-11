using UnityEngine;

public class UIManager : MonoBehaviour
{
    [Header("Panels")]
    public GameObject hudPanel;           // Minimap, HP, etc.
    public GameObject playerInventoryUI;
    public GameObject fishingInventoryUI;
    public GameObject externalInventoryUI; // The Shop or Crate
    public GameObject fishingMinigameUI;
    public GameObject interactionPopup;    // "Press Space" prompt

    // Helper function to turn EVERYTHING off.
    // This prevents bugs where two menus overlap (e.g. Shop on top of Minigame).
    public void CloseAll()
    {
        hudPanel.SetActive(false);
        playerInventoryUI.SetActive(false);
        externalInventoryUI.SetActive(false);
        fishingMinigameUI.SetActive(false);
        interactionPopup.SetActive(false);
    }

    // Defines the "Sailing" visual state
    public void ShowSailingHUD()
    {
        CloseAll();
        hudPanel.SetActive(true);
    }

    // Defines the "Fishing" visual state
    public void ShowFishingUI()
    {
        CloseAll();
        playerInventoryUI.SetActive(true);
        fishingInventoryUI.SetActive(true);
        fishingMinigameUI.SetActive(true);
    }

    // Defines the "Docked" visual state
    public void ShowDockUI()
    {
        CloseAll();
        playerInventoryUI.SetActive(true);
        externalInventoryUI.SetActive(true); // Shows the Shop/External Grid
    }

    // Defines the "Tab Menu" visual state
    public void ShowPlayerInventory()
    {
        CloseAll();
        playerInventoryUI.SetActive(true);
    }

    public void ToggleInteractionPopup(bool show)
    {
        interactionPopup.SetActive(show);
    }
}