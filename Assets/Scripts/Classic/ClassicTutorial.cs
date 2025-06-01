using UnityEngine;
using UnityEngine.SceneManagement;

public class ClassicTutorialManager : MonoBehaviour
{
    // Add this variable to set your target scene
    [SerializeField] private string gameSceneName = "Classic"; // Name of the scene to load

    // Add this variable for your main menu scene
    [SerializeField] private string mainMenuSceneName = "TitleScreen"; // Name of the main menu scene

    // Call this from the button
    public void StartGame()
    {
        // Load the first stage
        SceneManager.LoadScene(gameSceneName);
    }

    // Call this from your main menu button
    public void ReturnToMainMenu()
    {
        // Load the main menu scene
        SceneManager.LoadScene(mainMenuSceneName);
    }
}
