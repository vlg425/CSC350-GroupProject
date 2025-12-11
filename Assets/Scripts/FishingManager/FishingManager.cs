using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

/// <summary>
/// Controls the fishing Quick Time Event (QTE).
/// </summary>
public class FishingManager : MonoBehaviour
{
    // ---------------- INVENTORY ----------------
    [SerializeField] private GameObject inventoryItemPrefab;

    public bool IsFishing => isFishing;

    // ---------------- UI COMPONENTS ----------------
    [Header("UI")]
    [SerializeField] private Slider catchSlider;
    [SerializeField] private Slider escapeSlider;

    // ---------------- ZONE BACKGROUND IMAGES ----------------
    [Header("Zone Backgrounds")]
    [SerializeField] private RectTransform zonesContainer;
    [SerializeField] private RectTransform redZoneRect;
    [SerializeField] private RectTransform yellowZoneRect;
    [SerializeField] private RectTransform greenZoneRect;

    // ---------------- QTE BASE VALUES ----------------
    [Header("QTE Settings (Base Values)")]
    [SerializeField] private float baseArrowSpeed = 2f;
    [SerializeField] private float baseEscapeDrainRate = 0.05f;

    // ---------------- DIFFICULTY MULTIPLIERS ----------------
    [Header("QTE Difficulty Multipliers")]
    [SerializeField] private float arrowSpeedPerDifficulty = 0.5f;
    [SerializeField] private float escapeDrainPerDifficulty = 0.02f;
    [SerializeField] private float greenRadiusPerDifficulty = -0.02f;
    [SerializeField] private float yellowRadiusPerDifficulty = -0.01f;

    // ---------------- ARROW MOVEMENT ----------------
    [Header("Catch Bar Settings")]
    [SerializeField] private float arrowMin = 0f;
    [SerializeField] private float arrowMax = 1f;

    private float arrowSpeed;
    private float escapeDrainRate;
    private float arrowValue = 0.5f;
    private int arrowDirection = 1;

    private Fish currentFish;
    private bool isFishing;

    // ---------------- COLOR FEEDBACK ----------------
    [Header("Catch Slider Visuals")]
    [SerializeField] private Image catchSliderFill;
    [SerializeField] private Image catchHandleImage;

    // ---------------- ZONE SIZE SETTINGS ----------------
    [Header("Catch Zones")]
    [SerializeField] private float greenCenter = 0.5f;
    [SerializeField] private float baseGreenRadius = 0.1f;
    [SerializeField] private float baseYellowRadius = 0.2f;

    [SerializeField] private float baseGreenBonus = 0.25f;
    [SerializeField] private float baseYellowBonus = 0.15f;
    [SerializeField] private float baseMissPenalty = -0.15f;

    private float greenRadius;
    private float yellowRadius;

    [SerializeField] private float bonusMultiplierPerDifficulty = -0.05f;
    [SerializeField] private float penaltyMultiplierPerDifficulty = 0.05f;

    // ---------------- UNITY START ----------------
    private void Start()
    {
        UpdateZoneStrips();
    }


    // ======================================================================
    //                        START FISHING
    // ======================================================================
    public void StartFishing(Fish fish)
    {
        currentFish = fish;
        isFishing = true;

        int d = Mathf.Max(1, fish.QteDifficulty);

        // ----- SCALE SPEED VALUES -----
        arrowSpeed = baseArrowSpeed + arrowSpeedPerDifficulty * (d - 1);
        escapeDrainRate = baseEscapeDrainRate + escapeDrainPerDifficulty * (d - 1);

        // ----- SCALE ZONE SIZES -----
        greenRadius = Mathf.Clamp(
            baseGreenRadius + greenRadiusPerDifficulty * (d - 1),
            0.03f, 0.15f);

        yellowRadius = Mathf.Clamp(
            baseYellowRadius + yellowRadiusPerDifficulty * (d - 1),
            greenRadius + 0.02f, 0.45f);

        // ----- RESET UI -----
        arrowValue = greenCenter;

        catchSlider.minValue = arrowMin;
        catchSlider.maxValue = arrowMax;
        catchSlider.value = arrowValue;

        escapeSlider.minValue = 0f;
        escapeSlider.maxValue = 1f;
        escapeSlider.value = 0.9f;

        // ----- UPDATE VISUAL BACKGROUNDS -----
        UpdateZoneStrips();

        Debug.Log($"[FishingManager] START QTE - difficulty {d}, greenRadius={greenRadius}, yellowRadius={yellowRadius}");
    }


    // ======================================================================
    //                        UPDATE LOOP
    // ======================================================================
    private void Update()
    {
        if (!isFishing) return;

        MoveArrow();
        DrainEscapeOverTime();
        UpdateZoneVisual();

        if (Input.GetKeyDown(KeyCode.Space))
            CheckCatch();
    }


    // ======================================================================
    //                        ARROW MOVEMENT
    // ======================================================================
    private void MoveArrow()
    {
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
        escapeSlider.value -= escapeDrainRate * Time.deltaTime;

        if (escapeSlider.value <= 0f)
        {
            escapeSlider.value = 0f;
            Fail();
        }
    }


    // ======================================================================
    //                        ZONE BACKGROUND LAYOUT
    // ======================================================================
    private void UpdateZoneStrips()
    {
        if (zonesContainer == null) return;

        float fullWidth = zonesContainer.rect.width;

        // Force all pivots to left for easy math
        void ForceLeft(RectTransform rt)
        {
            if (rt == null) return;
            rt.pivot = new Vector2(0f, 0.5f);
            rt.anchorMin = new Vector2(0f, 0.5f);
            rt.anchorMax = new Vector2(0f, 0.5f);
        }

        ForceLeft(redZoneRect);
        ForceLeft(yellowZoneRect);
        ForceLeft(greenZoneRect);

        // FULL RED BAR
        redZoneRect.sizeDelta = new Vector2(fullWidth, redZoneRect.sizeDelta.y);
        redZoneRect.anchoredPosition = new Vector2(0f, 0f);

        // YELLOW BAND (centered)
        float yellowWidth = fullWidth * Mathf.Clamp01(yellowRadius * 2f);
        float yellowX = (fullWidth - yellowWidth) * 0.5f;

        yellowZoneRect.sizeDelta = new Vector2(yellowWidth, yellowZoneRect.sizeDelta.y);
        yellowZoneRect.anchoredPosition = new Vector2(yellowX, 0f);

        // GREEN BAND (centered)
        float greenWidth = fullWidth * Mathf.Clamp01(greenRadius * 2f);
        float greenX = (fullWidth - greenWidth) * 0.5f;

        greenZoneRect.sizeDelta = new Vector2(greenWidth, greenZoneRect.sizeDelta.y);
        greenZoneRect.anchoredPosition = new Vector2(greenX, 0f);
    }


    // ======================================================================
    //                        FEEDBACK COLORING
    // ======================================================================
    private void UpdateZoneVisual()
    {
        float distance = Mathf.Abs(arrowValue - greenCenter);

        Color c =
            distance <= greenRadius ? Color.green :
            distance <= yellowRadius ? Color.yellow :
            Color.red;

        if (catchSliderFill != null) catchSliderFill.color = c;
        if (catchHandleImage != null) catchHandleImage.color = c;
    }


    // ======================================================================
    //                        CATCH LOGIC
    // ======================================================================
    public void CheckCatch()
    {
        float distance = Mathf.Abs(arrowValue - greenCenter);
        float delta;

        if (distance <= greenRadius)
        {
            delta = baseGreenBonus;
            Debug.Log("GREEN timing!");
        }
        else if (distance <= yellowRadius)
        {
            delta = baseYellowBonus;
            Debug.Log("YELLOW timing!");
        }
        else
        {
            delta = baseMissPenalty;
            Debug.Log("RED miss!");
        }

        escapeSlider.value = Mathf.Clamp01(escapeSlider.value + delta);

        if (escapeSlider.value >= 1f)
            Success();
        else if (escapeSlider.value <= 0f)
            Fail();
    }


    private void Success()
    {
        isFishing = false;
        Debug.Log($"Caught {currentFish.Name}!");

        InventoryItemSO fishSO = InventoryManager.Instance.GetItemByName(currentFish.Name);

        GiveFishToInventory(fishSO);
    }


    private void Fail()
    {
        isFishing = false;
        Debug.Log("Fish escaped!");
    }


    private void GiveFishToInventory(InventoryItemSO data)
    {
        if (data == null) return;

        GameObject newItemObj = Instantiate(inventoryItemPrefab, transform.root);

        InventoryItem itemScript = newItemObj.GetComponent<InventoryItem>();
        itemScript.Initialize(data);

        PointerEventData fakeEvent = new PointerEventData(EventSystem.current);
        fakeEvent.button = PointerEventData.InputButton.Left;

        InventoryManager.Instance.OnItemClicked(itemScript, fakeEvent);
    }
}
