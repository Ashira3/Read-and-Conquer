using UnityEngine;
using UnityEngine.SceneManagement;

public class GameInitializer : MonoBehaviour
{
    [SerializeField] private string mainMenuSceneName = "TitleScreen";

    void Awake()
    {
        // Force single scene loading mode to ensure proper initialization
        Application.backgroundLoadingPriority = ThreadPriority.Low;
    }

    void Start()
    {
        Debug.Log("Bootstrap initializing...");

        // Ensure required managers are created in the correct order

        // First check if GameHistoryManager exists
        GameHistoryManager historyManager = FindObjectOfType<GameHistoryManager>();
        if (historyManager == null)
        {
            GameObject historyObj = new GameObject("GameHistoryManager");
            historyManager = historyObj.AddComponent<GameHistoryManager>();
            Debug.Log("GameHistoryManager created by bootstrap");
        }

        // Then check if AudioManager exists
        AudioManager audioManager = FindObjectOfType<AudioManager>();
        if (audioManager == null)
        {
            GameObject audioObj = new GameObject("AudioManager");
            audioManager = audioObj.AddComponent<AudioManager>();
            Debug.Log("AudioManager created by bootstrap");
        }

        // Give time for initialization before loading next scene
        Invoke("LoadMainMenu", 0.5f);
    }

    void LoadMainMenu()
    {
        Debug.Log("Loading main menu: " + mainMenuSceneName);
        SceneManager.LoadScene(mainMenuSceneName);
    }
}