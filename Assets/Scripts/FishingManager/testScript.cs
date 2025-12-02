using UnityEngine;

public class TestScript : MonoBehaviour
{
    private void Start()
    {
        // Example of creating a test fish
        Fish testFish = new Fish("Test Tuna", 50, 3, 2);

        // Get the FishingManager from the same GameObject
        FishingManager fm = GetComponent<FishingManager>();

        fm.StartFishing(testFish);
    }
}
