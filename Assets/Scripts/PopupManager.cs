using UnityEngine;

public class PopupManager : MonoBehaviour
{
    // Reference to the Pop-Up Panel GameObject
    public GameObject popUpPanel;

    [SerializeField] GameObject playerInventoryUI;
    [SerializeField] GameObject externalInventoryUI;
    [SerializeField] GameObject fishingUI;
    [SerializeField] GameObject Ship;
    void Update()
    {
        // Check if the specified key is pressed down in this frame
        if (popUpPanel.activeSelf)
        {

            if (Input.GetKeyDown(KeyCode.Space))
            {
                openInventory();
            }
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                HidePopUp();
            }
        }
        
   
        
    }
    // Function to show the panel
    public void ShowPopUp()
    {
        if (popUpPanel != null)
        {
            popUpPanel.SetActive(true);
        }
    }

    // Function to hide the panel (e.g., if a 'Cancel' key is pressed)
    public void HidePopUp()
    {
        if (popUpPanel != null)
        {
            popUpPanel.SetActive(false);
        }
    }

    // The function that moves ship to empty place with inventories open
    public void openInventory()
    {
        Debug.Log("Key pressed! Executing code");
        playerInventoryUI.SetActive(true);
        externalInventoryUI.SetActive(true);
        Vector3 newPosition = new(-1040f, -603f, 0f);
        Quaternion newRotation = Quaternion.Euler(0f, 0f, 0f);
        Ship.transform.SetPositionAndRotation(newPosition, newRotation);
        HidePopUp();

    }

 
}