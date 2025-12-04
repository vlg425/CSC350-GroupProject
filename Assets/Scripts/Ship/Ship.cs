using UnityEngine;
using UnityEngine.SceneManagement;
public class Ship : MonoBehaviour
{
    [SerializeField] GameObject playerInventoryUI;
    [SerializeField] GameObject externalInventoryUI;
    public GameObject popUpPanel;
    public float moveSpeed = 5f;
    public float rotationSpeed = 1f;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        Debug.Log("Ship!");
    }

    // Update is called once per frame
    void Update()
    {
        if (!(popUpPanel.activeSelf||playerInventoryUI.activeSelf))
            shipMovement();
        if (Input.GetKeyDown(KeyCode.F))
        {
            if (externalInventoryUI.activeSelf)
            {
                Debug.Log("F Key pressed! Executing code");
                Vector3 newPosition = new(0f, 0f, 0f);
                Quaternion newRotation = Quaternion.Euler(0f, 0f, 0f);
                transform.SetPositionAndRotation(newPosition, newRotation);
                externalInventoryUI.SetActive(false);
                playerInventoryUI.SetActive(false);
            }
        }
    }
    void shipMovement()
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
            transform.Rotate(0, 0, rotationSpeed);
        }

        // Move right with 'D' key
        if (Input.GetKey(KeyCode.D))
        {
            transform.Rotate(0, 0, -rotationSpeed);
        }
    }


}
