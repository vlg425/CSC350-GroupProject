using UnityEngine;

public class Ship : MonoBehaviour
{
    public float moveSpeed=5f;
    public float rotationSpeed=1f;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        Debug.Log("Ship!");
    }

    // Update is called once per frame
    void Update()
    {
        // Move forward with 'W' key
        if (Input.GetKey(KeyCode.W))
        {
            transform.Translate(Vector2.up * moveSpeed * Time.deltaTime);
        }

        // Move backward with 'S' key
        if (Input.GetKey(KeyCode.S))
        {
            transform.Translate(Vector2.down * moveSpeed * Time.deltaTime);
        }

        // Move left with 'A' key
        if (Input.GetKey(KeyCode.A))
        {
            transform.Rotate(0,0,rotationSpeed);
        }

        // Move right with 'D' key
        if (Input.GetKey(KeyCode.D))
        {
            transform.Rotate(0,0,-rotationSpeed);
        }
    }
}
