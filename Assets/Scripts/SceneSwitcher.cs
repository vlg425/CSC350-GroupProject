using UnityEngine;
using UnityEngine.SceneManagement;
public class SceneSwitcher : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D collision)
{
    Debug.Log("Trigger Hit!");
    if (collision.CompareTag("Dock"))
    {
        SceneManager.LoadScene("Inventory(Testing)");
    }
    if (collision.CompareTag("FishNode"))
    {
        SceneManager.LoadScene("Fishing Scene");
    }

}
}
