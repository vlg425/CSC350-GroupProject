using UnityEngine;

public class InventoryTrigger : MonoBehaviour
{
    [Header("Settings")]
    public string containerUniqueID; 
    public InventoryConfigSO containerConfig; 

    [Header("References")]
    public Inventory externalPanel; 
    public GameObject uiPanelGameObject; 

    public void OpenContainer()
    {
        uiPanelGameObject.SetActive(true);
        externalPanel.inventoryName = containerUniqueID;
        externalPanel.config = containerConfig;
        externalPanel.Restock(); 
    }

    public void CloseContainer()
    {
        uiPanelGameObject.SetActive(false);
    }
    
    // Simple test interaction
    void OnMouseDown()
    {
        OpenContainer();
    }
}