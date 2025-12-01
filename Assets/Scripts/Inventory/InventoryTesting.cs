using UnityEngine;
using System.Collections.Generic;

public class InventoryTesting : MonoBehaviour
{
    // Assign your Item Prefab here
    public GameObject itemPrefab;

    // Drag your ScriptableObjects (Shapes) here
    public List<InventoryItemSO> debugItems;

    void Update()
    {
        // Press 1, 2, 3... to spawn items
        if (Input.GetKeyDown(KeyCode.Alpha1)) SpawnItem(0);
        if (Input.GetKeyDown(KeyCode.Alpha2)) SpawnItem(1);
        if (Input.GetKeyDown(KeyCode.Alpha3)) SpawnItem(2);
        if (Input.GetKeyDown(KeyCode.Alpha4)) SpawnItem(3);
        if (Input.GetKeyDown(KeyCode.Alpha5)) SpawnItem(4);
        
        // Press R to randomize
        if (Input.GetKeyDown(KeyCode.R)) SpawnItem(Random.Range(0, debugItems.Count));
    }

    private void SpawnItem(int index)
    {
        // Safety check
        if (index < 0 || index >= debugItems.Count) return;
        if (itemPrefab == null) return;

        // 1. Create the new item object
        // We instantiate it at the root initially, the Manager will move it to the "Items" layer
        GameObject newItemObj = Instantiate(itemPrefab, transform.root);
        
        // 2. Setup the data
        InventoryItem itemScript = newItemObj.GetComponent<InventoryItem>();
        itemScript.Initialize(debugItems[index]);

        // 3. Force the Manager to pick it up immediately
        // This makes it snap to the mouse and follow it
        InventoryManager.Instance.OnItemClicked(itemScript);
    }
}