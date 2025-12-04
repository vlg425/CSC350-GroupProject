using UnityEngine;
using UnityEngine.SceneManagement;
public class SceneSwitcher : MonoBehaviour
{
    [SerializeField] GameObject playerInventoryUI;
    [SerializeField] GameObject externalInventoryUI;
    [SerializeField] GameObject fishingUI;
    [SerializeField] GameObject popupUI;
    [SerializeField] GameObject fishingPopupUI;
    private void OnTriggerEnter2D(Collider2D collision)
    {
        Debug.Log("Trigger Hit!");
        if (collision.CompareTag("Dock"))
        {
            popupUI.SetActive(true);
  
        }
        if (collision.CompareTag("FishNode"))
        {
            fishingPopupUI.SetActive(true);

        }

    }
}
