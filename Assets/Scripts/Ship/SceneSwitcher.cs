using UnityEngine;
using UnityEngine.SceneManagement;
public class SceneSwitcher : MonoBehaviour
{
    [SerializeField] GameObject playerInventoryUI;
    [SerializeField] GameObject externalInventoryUI;
    [SerializeField] GameObject fishingUI;
    private void OnTriggerEnter2D(Collider2D collision)
    {
        Debug.Log("Trigger Hit!");
        if (collision.CompareTag("Dock"))
        {
            
            playerInventoryUI.SetActive(true);
            externalInventoryUI.SetActive(true);
        }
        if (collision.CompareTag("FishNode"))
        {
            fishingUI.SetActive(true);
            externalInventoryUI.SetActive(true);

        }

    }
}
