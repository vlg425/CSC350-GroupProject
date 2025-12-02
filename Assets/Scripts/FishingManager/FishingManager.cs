using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Controls the fishing Quick Time Event (QTE).
/// </summary>
public class FishingManager : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private Slider catchSlider;
    [SerializeField] private Slider escapeSlider;

    [Header("QTE Settings")]
    [SerializeField] private float arrowSpeed = 2f;
    [SerializeField] private float escapeDrainRate = 0.05f; // percent per second
    [SerializeField] private float arrowMin = 0f;    // left-most
    [SerializeField] private float arrowMax = 1f;    // right-most
    private Fish currentFish;
    private bool isFishing;

    [Header("Catch Zones")]
    [SerializeField] private float greenCenter = 0.5f;   // middle of the bar
    [SerializeField] private float greenRadius = 0.1f;   // how wide the green zone is (0.40–0.60)
    [SerializeField] private float yellowRadius = 0.2f;  // yellow is a bit wider (0.30–0.70)

    [SerializeField] private float greenBonus = 0.25f;   // how much to add to escape bar on perfect timing
    [SerializeField] private float yellowBonus = 0.15f;  // medium reward
    [SerializeField] private float missPenalty = -0.15f; // how much to subtract on bad timing


private float arrowValue = 0.5f;  // current position of arrow on slider
private int arrowDirection = 1;   // 1 = moving right, -1 = moving left

    /// <summary>
    /// Begins the fishing QTE for the given fish.
    /// </summary>
    public void StartFishing(Fish fish)
    {
        currentFish = fish;
        isFishing = true;
        arrowValue = 0.5f;
        catchSlider.value = arrowValue;

        // Debug message to verify we're starting the QTE
        Debug.Log($"[FishingManager] Started fishing for {currentFish.Name}");

        // TODO: initialize sliders based on fish difficulty later
        if (escapeSlider != null)
            escapeSlider.value = 0.9f;
    }

    private void Update()
    {
        if (!isFishing) return;

        MoveArrow();
        DrainEscapeOverTime();

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
        // Later: Inventory.AddFish(currentFish);
    }

    private void Fail()
    {
        isFishing = false;

        Debug.Log("The fish escaped...");
    }
}
