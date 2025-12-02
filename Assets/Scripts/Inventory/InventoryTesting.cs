using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections.Generic;

public class InventoryTesting : MonoBehaviour
{
    [Header("References")]
    public GameObject itemPrefab;
    public GameObject inventoryUI; 
    
    [Header("Data")]
    public List<InventoryItemSO> debugItems;
    public InventoryItemSO fishItem; 

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1)) SpawnItem(0);
        if (Input.GetKeyDown(KeyCode.Alpha2)) SpawnItem(1);
        if (Input.GetKeyDown(KeyCode.Alpha3)) SpawnItem(2);
        if (Input.GetKeyDown(KeyCode.Alpha4)) SpawnItem(3);
        if (Input.GetKeyDown(KeyCode.R)) SpawnItem(Random.Range(0, debugItems.Count));

        if (Input.GetKeyDown(KeyCode.Space)) SimulateFishCatch();
    }

    private void SpawnItem(int index)
    {
        if (index < 0 || index >= debugItems.Count) return;
        CreateAndPickup(debugItems[index]);
    }

    private void SimulateFishCatch()
    {
        if (fishItem == null) return;
        if (inventoryUI != null) inventoryUI.SetActive(true);
        CreateAndPickup(fishItem);
        Debug.Log("Simulation: Fish Caught!");
    }

    private void CreateAndPickup(InventoryItemSO data)
    {
        if (itemPrefab == null) return;
        GameObject newItemObj = Instantiate(itemPrefab, transform.root);
        InventoryItem itemScript = newItemObj.GetComponent<InventoryItem>();
        itemScript.Initialize(data);

        PointerEventData fakeEvent = new PointerEventData(EventSystem.current);
        fakeEvent.button = PointerEventData.InputButton.Left;
        
        InventoryManager.Instance.OnItemClicked(itemScript, fakeEvent);
    }
}