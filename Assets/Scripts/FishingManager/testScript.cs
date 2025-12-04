using UnityEngine;

public class TestScript : MonoBehaviour
{
    private FishingManager fm;
    private Fish easyFish;
    private Fish mediumFish;
    private Fish hardFish;

    private void Awake()
    {
        fm = GetComponent<FishingManager>();

        // Set up 3 example fish with different difficulties
        easyFish = new Fish("Small Sardine", 10, 1, 1);
        mediumFish = new Fish("Tuna", 50, 2, 2);
        hardFish = new Fish("Legendary Whalefish", 200, 5, 3);
    }

    private void Start()
    {
        // Start with medium difficulty
        //fm.StartFishing(mediumFish);
    }

    private void Update()
    {
        // If not currently fishing, allow restart
        if (!fm.IsFishing)
        {
            if (Input.GetKeyDown(KeyCode.Alpha6))
            {
                fm.StartFishing(easyFish);
            }
            else if (Input.GetKeyDown(KeyCode.Alpha7))
            {
                fm.StartFishing(mediumFish);
            }
            else if (Input.GetKeyDown(KeyCode.Alpha8))
            {
                fm.StartFishing(hardFish);
            }
        }
    }
}
