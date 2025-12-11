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
    

    // ----------------------- UI -----------------------
    [Header("UI")]
    [SerializeField] private Slider catchSlider;   // arrow bar
    [SerializeField] private Slider escapeSlider;  // fish "HP"

    // -------- Base QTE Values ----------
    [Header("QTE Settings (Base Values)")]
    [SerializeField] private float baseArrowSpeed = 2f;
    [SerializeField] private float baseEscapeDrainRate = 0.05f;

    // -------- Difficulty Multipliers ----------
    [Header("QTE Difficulty Multipliers")]
    [SerializeField] private float arrowSpeedPerDifficulty = 0.5f;
    [SerializeField] private float escapeDrainPerDifficulty = 0.02f;
    [SerializeField] private float greenRadiusPerDifficulty = -0.02f;  // shrinks green zone
    [SerializeField] private float yellowRadiusPerDifficulty = -0.01f; // shrinks yellow zone

    // -------- Arrow Movement ----------
    [Header("Catch Bar Settings")]
    [SerializeField] private float arrowMin = 0f;
    [SerializeField] private float arrowMax = 1f;

    private float arrowSpeed;
    private float escapeDrainRate;
    private float arrowValue = 0.5f;
    private int arrowDirection = 1;
    private Fish currentFish;
    private bool isFishing;

    // -------- Slider Visual Color Feedback ----------
    [Header("Catch Slider Visuals")]
    [SerializeField] private Image catchSliderFill;   // the colored bar
    [SerializeField] private Image catchHandleImage;  // the knob image

        [Header("Zone Backgrounds")]
    [SerializeField] private RectTransform zonesContainer;
    [SerializeField] private RectTransform redZoneRect;
    [SerializeField] private RectTransform yellowZoneRect;
    [SerializeField] private RectTransform greenZoneRect;


    // -------- Timing Zone Radii ----------
    [Header("Catch Zones")]
    [SerializeField] private float greenCenter = 0.5f;        // center of the bar
    [SerializeField] private float baseGreenRadius = 0.1f;    // base width of perfect zone
    [SerializeField] private float baseYellowRadius = 0.2f;   // base width of okay zone

    private float greenRadius;   // actual radius after difficulty
    private float yellowRadius;  // actual radius after difficulty

    [SerializeField] private float baseGreenBonus = 0.25f;
    [SerializeField] private float baseYellowBonus = 0.15f;
    [SerializeField] private float baseMissPenalty = -0.15f;

    private float greenBonus;
    private float yellowBonus;
    private float missPenalty;

    [SerializeField] private float bonusMultiplierPerDifficulty = -0.05f;   // harder fish -> smaller bonus
    [SerializeField] private float penaltyMultiplierPerDifficulty = 0.05f;  // harder fish -> harsher penalty

    private void Start()
    {
        UpdateZoneBackgrounds();
    }


    private void Awake()
    {
        // initialize with base values
        greenRadius = baseGreenRadius;
        yellowRadius = baseYellowRadius;
        greenBonus = baseGreenBonus;
        yellowBonus = baseYellowBonus;
        missPenalty = baseMissPenalty;
    }

    /// <summary>
    /// Begins the fishing QTE for the given fish.
    /// </summary>
    public void StartFishing(Fish fish)
{
    currentFish = fish;
    isFishing   = true;

    // Make sure difficulty is at least 1
    int d = Mathf.Max(1, currentFish.QteDifficulty);

    // --------- Speeds based on difficulty ----------
    arrowSpeed      = baseArrowSpeed      + arrowSpeedPerDifficulty      * (d - 1);
    escapeDrainRate = baseEscapeDrainRate + escapeDrainPerDifficulty     * (d - 1);

    // --------- Zone size based on difficulty ----------
    greenRadius = Mathf.Clamp(
        baseGreenRadius + greenRadiusPerDifficulty * (d - 1),
        0.03f, 0.15f
    );

    yellowRadius = Mathf.Clamp(
        baseYellowRadius + yellowRadiusPerDifficulty * (d - 1),
        greenRadius + 0.02f, 0.45f
    );

    // --------- Rewards / penalties based on difficulty ----------
    greenBonus = Mathf.Clamp(
        baseGreenBonus + bonusMultiplierPerDifficulty * (d - 1),
        0.05f, 0.3f
    );

    yellowBonus = Mathf.Clamp(
        baseYellowBonus + bonusMultiplierPerDifficulty * (d - 1),
        0.02f, 0.2f
    );

    missPenalty = Mathf.Clamp(
        baseMissPenalty - penaltyMultiplierPerDifficulty * (d - 1),
        -0.4f, -0.05f
    );

    // --------- Reset bars ----------
    arrowValue = greenCenter;   // usually 0.5

    if (catchSlider != null)
    {
        catchSlider.minValue = arrowMin;
        catchSlider.maxValue = arrowMax;
        catchSlider.value    = arrowValue;
    }

    if (escapeSlider != null)
    {
        escapeSlider.minValue = 0f;
        escapeSlider.maxValue = 1f;
        escapeSlider.value    = 0.9f;
    }

    // 🔹 IMPORTANT: update the background strips so they match
    UpdateZoneBackgrounds();

    Debug.Log(
        $"[FishingManager] Started fishing for {currentFish.Name} (difficulty {d}), " +
        $"greenRadius={greenRadius}, yellowRadius={yellowRadius}, " +
        $"arrowSpeed={arrowSpeed}, drain={escapeDrainRate}"
    );
}


    private void Update()
    {
        if (!isFishing) return;

        MoveArrow();
        DrainEscapeOverTime();
        UpdateZoneVisual();

        if (Input.GetKeyDown(KeyCode.Space))
        {
            CheckCatch();
        }
    }

    private void MoveArrow()
    {
        if (catchSlider == null) return;

        arrowValue += arrowDirection * arrowSpeed * Time.deltaTime;

        if (arrowValue <= arrowMin)
        {
            arrowValue = arrowMin;
            arrowDirection = 1;
        }
        else if (arrowValue >= arrowMax)
        {
            arrowValue = arrowMax;
            arrowDirection = -1;
        }

        catchSlider.value = arrowValue;
    }

    private void DrainEscapeOverTime()
    {
        if (escapeSlider == null) return;

        escapeSlider.value -= escapeDrainRate * Time.deltaTime;

        if (escapeSlider.value <= 0f)
        {
            escapeSlider.value = 0f;
            Fail();
        }
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

        if (escapeSlider != null)
        {
            escapeSlider.value = Mathf.Clamp01(escapeSlider.value + delta);

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
        else
        {
            // fallback if escapeSlider isn't wired
            if (delta > 0)
                Success();
            else
                Fail();
        }
    }

    /// <summary>
    /// Changes fill and handle color based on which zone the arrow is in.
    /// </summary>
    private void UpdateZoneVisual()
    {
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

        if (catchSliderFill != null)
        {
            catchSliderFill.color = zoneColor;
        }

        if (catchHandleImage != null)
        {
            catchHandleImage.color = zoneColor;
        }
    }

        // -------------------------------------------------
    //  Zone background layout (red / yellow / green)
    // -------------------------------------------------
    private void UpdateZoneBackgrounds()
    {
        if (zonesContainer == null) return;

        float fullWidth = zonesContainer.rect.width;

        // Helper: force left pivot/anchors so X is measured from the left edge
        void SetupLeft(RectTransform rt)
        {
            if (rt == null) return;
            rt.pivot     = new Vector2(0f, 0.5f);
            rt.anchorMin = new Vector2(0f, 0.5f);
            rt.anchorMax = new Vector2(0f, 0.5f);
        }

        SetupLeft(redZoneRect);
        SetupLeft(yellowZoneRect);
        SetupLeft(greenZoneRect);

        // --- RED: full bar from the left edge ---
        if (redZoneRect != null)
        {
            redZoneRect.sizeDelta        = new Vector2(fullWidth, redZoneRect.sizeDelta.y);
            redZoneRect.anchoredPosition = new Vector2(0f, 0f);
        }

        // Clamp radii so they stay inside 0..0.5
        float clampedGreenRadius  = Mathf.Clamp01(greenRadius);
        float clampedYellowRadius = Mathf.Clamp01(yellowRadius);

        // --- GREEN band (centered around greenCenter) ---
        if (greenZoneRect != null)
        {
            float left01  = Mathf.Clamp01(greenCenter - clampedGreenRadius);
            float right01 = Mathf.Clamp01(greenCenter + clampedGreenRadius);

            float left   = left01 * fullWidth;
            float width  = (right01 - left01) * fullWidth;

            greenZoneRect.sizeDelta        = new Vector2(width, greenZoneRect.sizeDelta.y);
            greenZoneRect.anchoredPosition = new Vector2(left, 0f);
        }

        // --- YELLOW band (wider than green, still centered) ---
        if (yellowZoneRect != null)
        {
            float left01  = Mathf.Clamp01(greenCenter - clampedYellowRadius);
            float right01 = Mathf.Clamp01(greenCenter + clampedYellowRadius);

            float left   = left01 * fullWidth;
            float width  = (right01 - left01) * fullWidth;

            yellowZoneRect.sizeDelta        = new Vector2(width, yellowZoneRect.sizeDelta.y);
            yellowZoneRect.anchoredPosition = new Vector2(left, 0f);
        }
    }


    private void Success()
    {
        isFishing = false;

        Debug.Log($"Caught {currentFish.Name} worth {currentFish.Value} coins!");

        if (InventoryManager.Instance != null)
        {
            InventoryItemSO fishSO = InventoryManager.Instance.GetItemByName(currentFish.Name);
            GiveFishToInventory(fishSO);
        }
    }

    private void Fail()
    {
        isFishing = false;

        Debug.Log("The fish escaped...");
    }

    private void GiveFishToInventory(InventoryItemSO data)
    {
        if (data == null) return;
        if (inventoryItemPrefab == null) return;
        if (InventoryManager.Instance == null) return;

        // Create a new inventory item object from prefab
        GameObject newItemObj = Instantiate(inventoryItemPrefab, transform.root);

        // Initialize the item with our ScriptableObject data
        InventoryItem itemScript = newItemObj.GetComponent<InventoryItem>();
        if (itemScript != null)
        {
            itemScript.Initialize(data);
        }

        // Fake a left mouse click so the inventory system picks it up automatically
        PointerEventData fakeEvent = new PointerEventData(EventSystem.current);
        fakeEvent.button = PointerEventData.InputButton.Left;

        InventoryManager.Instance.OnItemClicked(itemScript, fakeEvent);
    }
}
