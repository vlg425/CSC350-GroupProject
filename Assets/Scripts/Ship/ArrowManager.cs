using UnityEngine;
using UnityEngine.UI; // REQUIRED for using UI components like Text (if using legacy Text)
using System.Linq;
using TMPro; // Needed for the OrderBy method (optional, but clean)

public class NearestTargetArrow2D : MonoBehaviour
{
    public string targetTag = "Dock"; 

    [Tooltip("The distance the arrow should stay fixed away from object.")]
    public float offsetDistance = 1.5f; 
    
    [Tooltip("Distance at which the arrow hides because the target is too close.")]
    public float hideDistance = 5f; 

    [Header("UI Reference")]
    [Tooltip("Drag the UI Text component here to display the distance.")]
    public TextMeshProUGUI distanceTextComponent; 

    private Transform currentTarget = null;
    private float nearestDistance = Mathf.Infinity; // Stores the distance to the current target

    void Update()
    {
        // 1. Find the Nearest Target and calculate its distance
        FindClosestTarget();
        
        // 2. Handle Display and Visibility
        if (currentTarget == null || nearestDistance <= hideDistance)
        {
            // If no target is found or target is too close, hide the arrow and text
            GetComponent<SpriteRenderer>().enabled = false;
            if (distanceTextComponent != null)
            {
                distanceTextComponent.enabled = false;
            }
            return;
        }

        
        // Make sure arrow and text are visible
        GetComponent<SpriteRenderer>().enabled = true;
        if (distanceTextComponent != null)
        {
            distanceTextComponent.enabled = true;
            distanceTextComponent.text = nearestDistance.ToString("F1") + "m";
        }

        Vector3 sourcePosition = transform.parent.position; 
        Vector3 targetPosition = currentTarget.position;

        Vector3 direction = targetPosition - sourcePosition;
        Vector3 directionNormalized = direction.normalized;

        transform.position = sourcePosition + directionNormalized * offsetDistance;

        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        
        transform.rotation = Quaternion.Euler(0f, 0f, angle); 
        float rotationOffset = 90f;
        transform.rotation = Quaternion.Euler(0f, 0f, angle + rotationOffset);
    }


    private void FindClosestTarget()
    {
        Vector3 currentPosition = transform.parent.position; 
        GameObject[] potentialTargets = GameObject.FindGameObjectsWithTag(targetTag);
        
        if (potentialTargets.Length == 0)
        {
            currentTarget = null;
            nearestDistance = Mathf.Infinity;
            return;
        }
        
        // --- Standard Loop to find the distance and target simultaneously ---
        float minDistance = Mathf.Infinity;
        Transform closest = null;
        
        foreach (GameObject go in potentialTargets)
        {
            float dist = Vector3.Distance(currentPosition, go.transform.position);
            if (dist < minDistance)
            {
                minDistance = dist;
                closest = go.transform;
            }
        }
        
        // Store the result for use in Update()
        currentTarget = closest;
        nearestDistance = minDistance;
    }
}