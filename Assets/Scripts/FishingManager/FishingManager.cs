using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

/// <summary>
/// Controls the fishing Quick Time Event (QTE).
/// </summary>
public class FishingManager : MonoBehaviour
{

    [SerializeField] private GameObject inventoryItemPrefab;

    public bool IsFishing => isFishing;

    [Header("UI")]
    [SerializeField] private Slider catchSlider;
    [SerializeField] private Slider escapeSlider;

   [Header("QTE Settings (Base Values)")]
   [SerializeField] private float arrowSpeed;
    [SerializeField] private float baseArrowSpeed = 2f;
    [SerializeField] private float baseEscapeDrainRate = 0.05f;
    [SerializeField] private float escapeDrainRate;

    [Header("QTE Difficulty Multipliers")]
    [SerializeField] private float arrowSpeedPerDifficulty = 0.5f;
    [SerializeField] private float escapeDrainPerDifficulty = 0.02f;
    [SerializeField] private float greenRadiusPerDifficulty = -0.02f; // gets smaller

    [Header("Catch Bar Settings")]
    [SerializeField] private float arrowMin = 0f;    // left-most
    [SerializeField] private float arrowMax = 1f;    // right-most

    private float arrowValue = 0.5f;    // current arrow position (0–1)
    private int arrowDirection = 1;     // 1 = right, -1 = left
    private Fish currentFish;
    private bool isFishing;

    [Header("Catch Slider Visuals")]
    [SerializeField] private Image catchSliderFill;   // the bar (optional)
    [SerializeField] private Image catchHandleImage;  // the knob


    [Header("Catch Zones")]
    [SerializeField] private float greenCenter = 0.5f;   // middle of the bar
    [SerializeField] private float greenRadius = 0.1f;   // how wide the green zone is (0.40–0.60)
    [SerializeField] private float yellowRadius = 0.2f;  // yellow is a bit wider (0.30–0.70)

    [SerializeField] private float greenBonus = 0.25f;   // how much to add to escape bar on perfect timing
    [SerializeField] private float yellowBonus = 0.15f;  // medium reward
    [SerializeField] private float missPenalty = -0.15f; // how much to subtract on bad timing

    [SerializeField] private float bonusMultiplierPerDifficulty = -0.05f; // every level makes bonus smaller
    [SerializeField] private float penaltyMultiplierPerDifficulty = 0.05f; // every level makes penalties harsher

    /// <summary>
    /// Begins the fishing QTE for the given fish.
    /// </summary>
    public void StartFishing(Fish fish)
    {
       currentFish = fish;
        isFishing = true;
        int d = Mathf.Max(1, currentFish.QteDifficulty);

        // Rewards get smaller as difficulty increases
        greenBonus = 0.25f + bonusMultiplierPerDifficulty * (d - 1);
        yellowBonus = 0.15f + bonusMultiplierPerDifficulty * (d - 1);

        // Penalties get larger as difficulty increases
        missPenalty = -0.15f - penaltyMultiplierPerDifficulty * (d - 1);

        // Clamp to safe ranges (not too big, not too small)
        greenBonus = Mathf.Clamp(greenBonus, 0.05f, 0.3f);
        yellowBonus = Mathf.Clamp(yellowBonus, 0.02f, 0.2f);
        missPenalty = Mathf.Clamp(missPenalty, -0.4f, -0.05f);

        Debug.Log($"[FishingManager] Started fishing for {currentFish.Name} (Difficulty {currentFish.QteDifficulty})");

        // Reset arrow position
        arrowValue = 0.5f;
        catchSlider.value = arrowValue;

        // Set escape bar start
        escapeSlider.value = 0.9f;

        // ----- Apply difficulty scaling ----

        // Arrow moves faster with higher difficulty
        float arrowSpeed = baseArrowSpeed + arrowSpeedPerDifficulty * (d - 1);

        // Escape drains faster with higher difficulty
        escapeDrainRate = baseEscapeDrainRate + escapeDrainPerDifficulty * (d - 1);

        // Green zone gets slightly smaller with higher difficulty
        greenRadius = Mathf.Clamp(0.1f + greenRadiusPerDifficulty * (d - 1), 0.03f, 0.15f);

        Debug.Log($"ArrowSpeed: {arrowSpeed}, EscapeDrain: {escapeDrainRate}, GreenRadius: {greenRadius}");

        // Store local arrowSpeed into the field you already use in MoveArrow
        this.arrowSpeed = arrowSpeed;
    }

    private void Update()
    {
        if (!isFishing) return;

        MoveArrow();
        DrainEscapeOverTime();
        UpdateZoneVisual();

        // When player presses Space, try to catch
        if (Input.GetKeyDown(KeyCode.Space))
        {
            CheckCatch();
        }
    }

    private void DrainEscapeOverTime()
{
    if (escapeSlider == null) return;

    // Decrease the value over time
    escapeSlider.value -= escapeDrainRate * Time.deltaTime;

    // If it reaches 0, the fish escapes
    if (escapeSlider.value <= 0f)
    {
        escapeSlider.value = 0f;
        Fail();
    }
}

    private void MoveArrow()
    {
    // Move arrow based on direction and speed
    arrowValue += arrowDirection * arrowSpeed * Time.deltaTime;

    // If arrow hits the left side, change direction to right
    if (arrowValue <= arrowMin)
    {
        arrowValue = arrowMin;
        arrowDirection = 1;
    }
    // If arrow hits the right side, change direction to left
    else if (arrowValue >= arrowMax)
    {
        arrowValue = arrowMax;
        arrowDirection = -1;
    }

    // Update the slider visually
    catchSlider.value = arrowValue;
    }
    
    /// <summary>
    /// Called when the player presses the catch button.
    /// </summary>
    public void CheckCatch()
    {
        if (!isFishing) return;

        // How far is the arrow from the center (0.5)?
        float distanceFromCenter = Mathf.Abs(arrowValue - greenCenter);

        float delta; // how much to change the escape bar

        if (distanceFromCenter <= greenRadius)
        {
            // GREEN zone: best timing
            delta = greenBonus;
            Debug.Log("Perfect hit (GREEN)!");
        }
        else if (distanceFromCenter <= yellowRadius)
        {
            // YELLOW zone: okay timing
            delta = yellowBonus;
            Debug.Log("Decent hit (YELLOW).");
        }
        else
        {
            // RED zone: bad timing
            delta = missPenalty;
            Debug.Log("Bad timing (RED).");
        }

        // Apply the change
        escapeSlider.value = Mathf.Clamp01(escapeSlider.value + delta);

        // Check for win / lose
        if (escapeSlider.value >= 1f)
        {
            escapeSlider.value = 1f;
            Success();
        }
        else if (escapeSlider.value <= 0f)
        {
            escapeSlider.value = 0f;
            Fail();
        }
    }

   private void Success()
{
    isFishing = false;

    Debug.Log($"Caught {currentFish.Name} worth {currentFish.Value} coins!");

    InventoryItemSO fishSO = InventoryManager.Instance.GetItemByName(currentFish.Name);
    GiveFishToInventory(fishSO);
}

    private void Fail()
    {
        isFishing = false;

        Debug.Log("The fish escaped...");
    }

    private void UpdateZoneVisual()
    {
        // How far is the arrow from the center?
        float distanceFromCenter = Mathf.Abs(arrowValue - greenCenter);

        Color zoneColor;

        if (distanceFromCenter <= greenRadius)
        {
            zoneColor = Color.green;
        }
        else if (distanceFromCenter <= yellowRadius)
        {
            zoneColor = Color.yellow;
        }
        else
        {
            zoneColor = Color.red;
        }

        // Apply the color to the bar fill (optional)
        if (catchSliderFill != null)
        {
            catchSliderFill.color = zoneColor;
        }

        // Apply the color to the knob/handle
        if (catchHandleImage != null)
        {
            catchHandleImage.color = zoneColor;
        }
    }
    

 private void GiveFishToInventory(InventoryItemSO data)
{
    if (data == null) return;

    // Create a new inventory item object from prefab
    GameObject newItemObj = Instantiate(inventoryItemPrefab, transform.root);

    // Initialize the item with our ScriptableObject data
    InventoryItem itemScript = newItemObj.GetComponent<InventoryItem>();
    itemScript.Initialize(data);

    // Fake a left mouse click so the inventory system picks it up automatically
    PointerEventData fakeEvent = new PointerEventData(EventSystem.current);
    fakeEvent.button = PointerEventData.InputButton.Left;

    InventoryManager.Instance.OnItemClicked(itemScript, fakeEvent);
}



}
