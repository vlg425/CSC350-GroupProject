using UnityEngine;
using UnityEngine.SceneManagement; // Required to change scenes

public class MainMenuManager : MonoBehaviour
{
    [Header("Settings")]
    // Type the EXACT name of your game scene here in the Inspector
    public string gameSceneName = "Main"; 

    public void PlayGame()
    {
        // Loads the scene with the name you typed above
        SceneManager.LoadScene(gameSceneName);
    }

    public void QuitGame()
    {
        Debug.Log("Quitting Game...");
        Application.Quit();
    }
}