using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class TitleScreenManager : MonoBehaviour
{
    public TextMeshProUGUI titleText;
    public TextMeshProUGUI gameModeText;
    public GameObject startButton;

    // Main menu buttons
    public GameObject playButton;
    public GameObject viewHistoryButton;
    public GameObject exitButton;
    public GameObject[] mainMenuButtons; // Array containing Play, View History, and Exit

    // Game mode buttons
    public GameObject[] gameModeButtons; // Array for Classic, Boss Rush, Time Attack

    private AudioManager audioManager;

    // Define game mode scene names
    private const string CLASSIC_SCENE = "ClassicTutorial";
    private const string BOSS_RUSH_SCENE = "Stage 1";
    private const string TIME_ATTACK_SCENE = "TimeAttackTutorial";
    private const string VIEWHISTORY_SCENE = "MainHistory";

    void Start()
    {
        AudioManager.Instance.SetGameModeAndStage(AudioManager.GameMode.TitleScreen, 0); //  TitleScreen
        audioManager = FindFirstObjectByType<AudioManager>();

        if (audioManager == null)
        {
            Debug.LogError("AudioManager not found in the scene! Please ensure it is present.");
            return;
        }

        // Make sure the start button is active initially
        if (startButton != null)
        {
            startButton.SetActive(true);
        }
        else
        {
            Debug.LogError("Start button reference is missing!");
        }

        // Hide main menu buttons initially
        foreach (GameObject button in mainMenuButtons)
        {
            if (button != null)
            {
                button.SetActive(false);
            }
        }

        // Hide game mode buttons initially
        foreach (GameObject button in gameModeButtons)
        {
            if (button != null)
            {
                button.SetActive(false);
            }
        }

        // Hide game mode text initially
        if (gameModeText != null)
        {
            gameModeText.gameObject.SetActive(false);
        }
    }

    public void OnStartButtonClick()
    {
        if (audioManager != null)
        {
            audioManager.PlayButtonClickSound();
        }

        // Hide the start button
        startButton.SetActive(false);

        // Show main menu buttons (Play, View History, Exit)
        foreach (GameObject button in mainMenuButtons)
        {
            if (button != null)
            {
                button.SetActive(true);
            }
        }
    }

    public void OnPlayButtonClick()
    {
        if (audioManager != null)
        {
            audioManager.PlayButtonClickSound();
        }

        // Hide main menu buttons
        foreach (GameObject button in mainMenuButtons)
        {
            if (button != null)
            {
                button.SetActive(false);
            }
        }

        // Show the game mode text
        if (gameModeText != null)
        {
            gameModeText.gameObject.SetActive(true);
            gameModeText.text = "Select Game Mode:";
        }

        // Show game mode buttons
        foreach (GameObject button in gameModeButtons)
        {
            if (button != null)
            {
                button.SetActive(true);
            }
        }
    }

    public void OnViewHistoryButtonClick()
    {
        if (audioManager != null)
        {
            audioManager.PlayButtonClickSound();
        }
        StartCoroutine(LoadGameModeWithDelay(VIEWHISTORY_SCENE));
        // Implement view history functionality here
        Debug.Log("View History clicked");
    }

    public void OnExitButtonClick()
    {
        if (audioManager != null)
        {
            audioManager.PlayButtonClickSound();
        }

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    // Game mode selection methods
    public void OnClassicModeClick()
    {
        if (audioManager != null)
        {
            audioManager.PlayButtonClickSound();
        }
        StartCoroutine(LoadGameModeWithDelay(CLASSIC_SCENE));
    }

    // In TitleScreenManager.cs
    public void OnBossRushModeClick()
    {
        if (audioManager != null)
        {
            audioManager.PlayButtonClickSound();
        }

        // Load the first stage
        SceneManager.LoadScene("BossRushTutorial");
    }

    public void OnTimeAttackModeClick()
    {
        if (audioManager != null)
        {
            audioManager.PlayButtonClickSound();
        }
        StartCoroutine(LoadGameModeWithDelay(TIME_ATTACK_SCENE));
    }

    // In your title screen script or button click handler
    

    private System.Collections.IEnumerator LoadGameModeWithDelay(string sceneName)
    {
        yield return new WaitForSeconds(0.1f); // Short delay for sound to play
        SceneManager.LoadScene(sceneName);
    }
}