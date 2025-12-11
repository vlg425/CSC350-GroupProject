using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

/// <summary>
/// Controls the fishing Quick Time Event (QTE).
/// </summary>
public class FishingManager : MonoBehaviour
{
    // ---------- INVENTORY / ITEMS ----------
    [Header("Inventory")]
    [SerializeField] private GameObject inventoryItemPrefab;   // prefab for UI inventory item

    // ScriptableObjects that define the shapes/sizes (1x1, L, 4x4)
    [SerializeField] private InventoryItemSO easyFishItem;     // 1x1
    [SerializeField] private InventoryItemSO mediumFishItem;   // L-shape
    [SerializeField] private InventoryItemSO hardFishItem;     // 4x4 cube

    // ---------- UI ----------
    [Header("UI")]
    [SerializeField] private Slider catchSlider;
    [SerializeField] private Slider escapeSlider;

    // Text above the slider (“Press 1 for easy…” / “Aim for the green zone…”)
    [SerializeField] private Text hintText;

    // Optional: color feedback on bar + knob
    [Header("Catch Slider Visuals")]
    [SerializeField] private Image catchSliderFill;
    [SerializeField] private Image catchHandleImage;

    // ---------- QTE BASE SETTINGS ----------
    [Header("QTE Settings (Base Values)")]
    [SerializeField] private float baseArrowSpeed = 2f;
    [SerializeField] private float baseEscapeDrainRate = 0.05f;

    [Header("QTE Difficulty Multipliers")]
    [SerializeField] private float arrowSpeedPerDifficulty = 0.5f;
    [SerializeField] private float escapeDrainPerDifficulty = 0.02f;
    [SerializeField] private float greenRadiusPerDifficulty = -0.02f;
    [SerializeField] private float yellowRadiusPerDifficulty = -0.01f;

    [Header("Catch Bar Settings")]
    [SerializeField] private float arrowMin = 0f;
    [SerializeField] private float arrowMax = 1f;

    // ---------- ZONE BACKGROUND STRIPS ----------
    [Header("Zone Backgrounds")]
    [SerializeField] private RectTransform zonesContainer;  // parent rect
    [SerializeField] private RectTransform redZoneRect;
    [SerializeField] private RectTransform yellowZoneRect;
    [SerializeField] private RectTransform greenZoneRect;

    // ---------- ZONE NUMBERS ----------
    [Header("Catch Zones")]
    [SerializeField] private float greenCenter = 0.5f;

    [SerializeField] private float baseGreenRadius = 0.1f;
    [SerializeField] private float baseYellowRadius = 0.2f;

    [SerializeField] private float baseGreenBonus = 0.25f;
    [SerializeField] private float baseYellowBonus = 0.15f;
    [SerializeField] private float baseMissPenalty = -0.15f;

    [SerializeField] private float bonusMultiplierPerDifficulty = -0.05f;
    [SerializeField] private float penaltyMultiplierPerDifficulty = 0.05f;

    public bool IsFishing => isFishing;   
    // Runtime values (change with difficulty)
    private float arrowSpeed;
    private float escapeDrainRate;
    private float greenRadius;
    private float yellowRadius;
    private float greenBonus;
    private float yellowBonus;
    private float missPenalty;

    // ---------- INTERNAL STATE ----------
    private float arrowValue = 0.5f;
    private int arrowDirection = 1;        // 1 = right, -1 = left
    private Fish currentFish;
    private bool isFishing = false;

    // Waiting for player to pick 1/2/3 before QTE starts
    private bool waitingForDifficulty = true;

    private void Start()
    {
        ShowDifficultyHint();
        // Make sure sliders are in a sane state
        if (catchSlider != null)
        {
            catchSlider.minValue = arrowMin;
            catchSlider.maxValue = arrowMax;
            catchSlider.value = greenCenter;
        }

        if (escapeSlider != null)
        {
            escapeSlider.minValue = 0f;
            escapeSlider.maxValue = 1f;
            escapeSlider.value = 0.9f;
        }

        UpdateZoneStrips();
    }

    private void Update()
    {
        // 1) Waiting for player to choose easy / medium / hard
        if (waitingForDifficulty)
        {
            HandleDifficultyInput();
            return;
        }

        // 2) QTE running
        if (!isFishing) return;

        MoveArrow();
        DrainEscapeOverTime();
        UpdateZoneVisual();

        if (Input.GetKeyDown(KeyCode.Space))
        {
            CheckCatch();
        }
    }

    // =========================================================
    //  DIFFICULTY PICKER
    // =========================================================

    private void ShowDifficultyHint()
    {
        if (hintText != null)
        {
            hintText.text = "Press 1 for Easy, 2 for Medium, 3 for Hard";
        }
    }

    private void ShowQTEHint()
    {
        if (hintText != null)
        {
            hintText.text = "Aim for the GREEN zone and press Space to catch!";
        }
    }

    private void HandleDifficultyInput()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            StartFishingPreset(1);
        }
        else if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            StartFishingPreset(2);
        }
        else if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            StartFishingPreset(3);
        }
    }

    /// <summary>
    /// Creates a Fish with the right size/item for difficulty 1/2/3 and starts QTE.
    /// </summary>
    private void StartFishingPreset(int difficulty)
    {
        InventoryItemSO itemSO = null;
        string fishName = "Fish";
        int value = 10;
        FishSize size = FishSize.Small;

        switch (difficulty)
        {
            case 1:
                itemSO = easyFishItem;
                fishName = "Small Fish";
                value = 10;
                size = FishSize.Small;      // 1x1
                break;

            case 2:
                itemSO = mediumFishItem;
                fishName = "Medium Fish";
                value = 25;
                size = FishSize.Medium;     // L-shape
                break;

            case 3:
                itemSO = hardFishItem;
                fishName = "Large Fish";
                value = 50;
                size = FishSize.Large;      // 4x4
                break;
        }

        currentFish = new Fish(
            fishName,
            value,
            rarity: difficulty,          // simple: use difficulty as rarity
            qteDifficulty: difficulty,
            size: size,
            itemData: itemSO
        );

        waitingForDifficulty = false;
        StartFishing(currentFish);
    }

    // =========================================================
    //  MAIN PUBLIC START METHOD (also usable from other scripts)
    // =========================================================

    public void StartFishing(Fish fish)
    {
        currentFish = fish;
        isFishing = true;

        int d = Mathf.Max(1, currentFish.QteDifficulty);

        // --------- Speeds based on difficulty ----------
        arrowSpeed = baseArrowSpeed + arrowSpeedPerDifficulty * (d - 1);
        escapeDrainRate = baseEscapeDrainRate + escapeDrainPerDifficulty * (d - 1);

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
        arrowValue = greenCenter;

        if (catchSlider != null)
        {
            catchSlider.minValue = arrowMin;
            catchSlider.maxValue = arrowMax;
            catchSlider.value = arrowValue;
        }

        if (escapeSlider != null)
        {
            escapeSlider.minValue = 0f;
            escapeSlider.maxValue = 1f;
            escapeSlider.value = 0.9f;
        }

        UpdateZoneStrips();
        ShowQTEHint();

        Debug.Log($"[FishingManager] Started fishing for {currentFish.Name} (difficulty {d}) " +
                  $"greenRadius={greenRadius}, yellowRadius={yellowRadius}, arrowSpeed={arrowSpeed}, drain={escapeDrainRate}");
    }

    // =========================================================
    //  CORE QTE LOGIC
    // =========================================================

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

    public void CheckCatch()
    {
        if (!isFishing) return;

        float distanceFromCenter = Mathf.Abs(arrowValue - greenCenter);
        float delta;

        if (distanceFromCenter <= greenRadius)
        {
            delta = greenBonus;
            Debug.Log("Perfect hit (GREEN)!");
        }
        else if (distanceFromCenter <= yellowRadius)
        {
            delta = yellowBonus;
            Debug.Log("Decent hit (YELLOW).");
        }
        else
        {
            delta = missPenalty;
            Debug.Log("Bad timing (RED).");
        }

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

    // =========================================================
    //  ZONE BACKGROUND STRIPS
    // =========================================================

    private void UpdateZoneStrips()
    {
        if (zonesContainer == null) return;

        float fullWidth = zonesContainer.rect.width;

        // local helper to force left-anchored bars
        void SetupLeft(RectTransform rt)
        {
            if (rt == null) return;
            rt.pivot = new Vector2(0f, 0.5f);
            rt.anchorMin = new Vector2(0f, 0.5f);
            rt.anchorMax = new Vector2(0f, 0.5f);
        }

        SetupLeft(redZoneRect);
        SetupLeft(yellowZoneRect);
        SetupLeft(greenZoneRect);

        // RED = whole bar
        if (redZoneRect != null)
        {
            redZoneRect.sizeDelta = new Vector2(fullWidth, redZoneRect.sizeDelta.y);
            redZoneRect.anchoredPosition = Vector2.zero;
        }

        // YELLOW = centered, wider band
        if (yellowZoneRect != null)
        {
            float yellowWidth = fullWidth * Mathf.Clamp01(yellowRadius * 2f);
            float yellowX = (fullWidth - yellowWidth) * 0.5f;

            yellowZoneRect.sizeDelta = new Vector2(yellowWidth, yellowZoneRect.sizeDelta.y);
            yellowZoneRect.anchoredPosition = new Vector2(yellowX, 0f);
        }

        // GREEN = centered, narrow band
        if (greenZoneRect != null)
        {
            float greenWidth = fullWidth * Mathf.Clamp01(greenRadius * 2f);
            float greenX = (fullWidth - greenWidth) * 0.5f;

            greenZoneRect.sizeDelta = new Vector2(greenWidth, greenZoneRect.sizeDelta.y);
            greenZoneRect.anchoredPosition = new Vector2(greenX, 0f);
        }
    }

    // =========================================================
    //  VISUAL FEEDBACK + INVENTORY
    // =========================================================

    private void UpdateZoneVisual()
    {
        float distanceFromCenter = Mathf.Abs(arrowValue - greenCenter);
        Color zoneColor;

        if (distanceFromCenter <= greenRadius)
            zoneColor = Color.green;
        else if (distanceFromCenter <= yellowRadius)
            zoneColor = Color.yellow;
        else
            zoneColor = Color.red;

        if (catchSliderFill != null)
            catchSliderFill.color = zoneColor;

        if (catchHandleImage != null)
            catchHandleImage.color = zoneColor;
    }

    private void Success()
    {
        isFishing = false;

        Debug.Log($"Caught {currentFish.Name} worth {currentFish.Value} coins!");

        InventoryItemSO data = currentFish.ItemData;
        if (data == null)
        {
            // fallback: look up by name if needed
            data = InventoryManager.Instance.GetItemByName(currentFish.Name);
        }

        GiveFishToInventory(data);

        // Go back to difficulty selection
        waitingForDifficulty = true;
        ShowDifficultyHint();
    }

    private void Fail()
    {
        isFishing = false;
        Debug.Log("The fish escaped...");

        waitingForDifficulty = true;
        ShowDifficultyHint();
    }

    private void GiveFishToInventory(InventoryItemSO data)
    {
        if (data == null || inventoryItemPrefab == null) return;

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
