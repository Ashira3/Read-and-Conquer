using UnityEngine;
using UnityEngine.SceneManagement;

public class BossRushTutorialManager : MonoBehaviour
{
    // Add this variable to set your target scene
    [SerializeField] private string gameSceneName = "Stage1"; // Name of the scene to load

    // Add this variable for your main menu scene
    [SerializeField] private string mainMenuSceneName = "Goback"; // Name of the main menu scene

    // Call this from the button
    public void StartGame()
    {
        // Reset the player score in PlayerPrefs before loading the stage
        PlayerPrefs.SetInt("PlayerScore", 0);
        PlayerPrefs.Save();
        Debug.Log("Reset player score before starting Boss Rush mode");

        // Load the first stage
        SceneManager.LoadScene("Stage1");
    }

    // Call this from your main menu button
    public void ReturnToMainMenu()
    {
        // Load the main menu scene
        SceneManager.LoadScene(mainMenuSceneName);
    }
}
