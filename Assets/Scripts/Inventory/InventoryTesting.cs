using UnityEngine;
using UnityEngine.EventSystems; // Required for the fake click event
using System.Collections.Generic;

public class InventoryTesting : MonoBehaviour
{
    public GameObject itemPrefab;
    public List<InventoryItemSO> debugItems;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1)) SpawnItem(0);
        if (Input.GetKeyDown(KeyCode.Alpha2)) SpawnItem(1);
        if (Input.GetKeyDown(KeyCode.Alpha3)) SpawnItem(2);
        if (Input.GetKeyDown(KeyCode.Alpha4)) SpawnItem(3);
        
        if (Input.GetKeyDown(KeyCode.R)) SpawnItem(Random.Range(0, debugItems.Count));
    }

    private void SpawnItem(int index)
    {
        if (index < 0 || index >= debugItems.Count) return;
        if (itemPrefab == null) return;

        GameObject newItemObj = Instantiate(itemPrefab, transform.root);
        InventoryItem itemScript = newItemObj.GetComponent<InventoryItem>();
        itemScript.Initialize(debugItems[index]);

        // --- THE FIX ---
        // We create a "Fake" Left Click event to satisfy the new Manager signature
        PointerEventData fakeEvent = new PointerEventData(EventSystem.current);
        fakeEvent.button = PointerEventData.InputButton.Left;
        
        InventoryManager.Instance.OnItemClicked(itemScript, fakeEvent);
        // ----------------
    }
}