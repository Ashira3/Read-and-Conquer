using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class SettingsManager : MonoBehaviour
{
    [SerializeField] private GameObject settingsPanel;
    [SerializeField] private Slider volumeSlider;
    [SerializeField] private Button mainMenuButton;
    [SerializeField] private Button closeButton;
    [SerializeField] private string mainMenuSceneName = "TitleScreen";

    private AudioManager audioManager;

    void Start()
    {
        // Find the AudioManager
        audioManager = FindFirstObjectByType<AudioManager>();

        // Set up button listeners
        if (mainMenuButton != null)
        {
            mainMenuButton.onClick.AddListener(ReturnToMainMenu);
        }

        if (closeButton != null)
        {
            closeButton.onClick.AddListener(CloseSettings);
        }

        // Set up volume slider
        if (volumeSlider != null && audioManager != null)
        {
            // Load saved volume
            volumeSlider.value = PlayerPrefs.GetFloat("MasterVolume", 1.0f);

            // Add listener for volume changes
            volumeSlider.onValueChanged.AddListener(ChangeVolume);
        }

        // Hide settings panel initially
        if (settingsPanel != null)
        {
            settingsPanel.SetActive(false);
        }
    }

    public void OpenSettings()
    {
        if (audioManager != null)
        {
            audioManager.PlayButtonClickSound();
        }

        if (settingsPanel != null)
        {
            settingsPanel.SetActive(true);
        }
    }

    public void CloseSettings()
    {
        if (audioManager != null)
        {
            audioManager.PlayButtonClickSound();
        }

        if (settingsPanel != null)
        {
            settingsPanel.SetActive(false);
        }
    }

    private void ChangeVolume(float volume)
    {
        // Save volume to PlayerPrefs
        PlayerPrefs.SetFloat("MasterVolume", volume);
        PlayerPrefs.Save();

        // Use AudioManager's method to set volume for all sources
        if (audioManager != null)
        {
            audioManager.SaveVolume(volume);
        }
    }

    public void ReturnToMainMenu()
    {
        if (audioManager != null)
        {
            audioManager.PlayButtonClickSound();
        }

        // Check if we're in a Boss Rush scene
        string currentSceneName = SceneManager.GetActiveScene().name;

        if (currentSceneName.Contains("Stage") || currentSceneName.Contains("BossRush"))
        {
            // Extract stage number from scene name
            int stageNumber = ExtractStageNumberFromScene(currentSceneName);

            // Get current score from PlayerPrefs (most reliable source since all stages save here)
            int playerScore = PlayerPrefs.GetInt("PlayerScore", 0);

            // Save to GameHistoryManager
            if (GameHistoryManager.Instance != null)
            {
                GameHistoryManager.Instance.SaveGameResult(
                    "BossRush",             // Game mode
                    "Stage " + stageNumber, // Stage indicator for difficulty display 
                    playerScore,            // Current score from PlayerPrefs
                    0,                      // Correct answers (not applicable for Boss Rush)
                    0,                      // Total questions (not applicable for Boss Rush)
                    stageNumber             // Stage number (1-5)
                );

                Debug.Log($"Saved Boss Rush progress from Stage {stageNumber} with score {playerScore} to history");
            }
        }
        // Handle Classic Mode - this needs to work for all difficulties
        else if (FindAnyGameManager<ClassicModeManager, MediumClassicModeManager, HardClassicModeManager>(out object manager))
        {
            string difficultyLevel = "Easy";
            int stageNumber = 1;
            int currentScore = 0;
            int correctAnswers = 0;
            int totalQuestions = 0;

            // Check which difficulty manager is active and get its data
            if (manager is ClassicModeManager classicManager)
            {
                difficultyLevel = "Easy";
                stageNumber = 1;
                currentScore = classicManager.GetCurrentScore();
                correctAnswers = classicManager.GetCorrectAnswers();
                totalQuestions = classicManager.GetTotalQuestions();
            }
            else if (manager is MediumClassicModeManager mediumManager)
            {
                difficultyLevel = "Medium";
                stageNumber = 2;
                currentScore = mediumManager.GetCurrentScore();
                correctAnswers = mediumManager.GetCorrectAnswers();
                totalQuestions = mediumManager.GetTotalQuestions();
            }
            else if (manager is HardClassicModeManager hardManager)
            {
                difficultyLevel = "Hard";
                stageNumber = 3;
                currentScore = hardManager.GetCurrentScore();
                correctAnswers = hardManager.GetCorrectAnswers();
                totalQuestions = hardManager.GetTotalQuestions();
            }

            if (GameHistoryManager.Instance != null)
            {
                GameHistoryManager.Instance.SaveGameResult(
                    "Classic",
                    difficultyLevel,
                    currentScore,
                    correctAnswers,
                    totalQuestions,
                    stageNumber
                );

                Debug.Log($"Saved Classic {difficultyLevel} progress with score {currentScore} to history");
            }
        }
        // Handle Time Attack Mode - this needs to work for all difficulties
        else if (FindAnyGameManager<TimeAttackManager, MediumTimeAttackManager, HardTimeAttackManager>(out object timeManager))
        {
            string difficultyLevel = "Easy";
            int stageNumber = 1;
            int currentScore = 0;
            int correctAnswers = 0;
            int totalQuestions = 0;

            // Check which difficulty manager is active and get its data
            if (timeManager is TimeAttackManager easyManager)
            {
                difficultyLevel = "Easy";
                stageNumber = 1;
                currentScore = easyManager.GetCurrentScore();
                correctAnswers = easyManager.GetCorrectAnswers();
                totalQuestions = easyManager.GetTotalQuestions();
            }
            else if (timeManager is MediumTimeAttackManager mediumManager)
            {
                difficultyLevel = "Medium";
                stageNumber = 2;
                currentScore = mediumManager.GetCurrentScore();
                correctAnswers = mediumManager.GetCorrectAnswers();
                totalQuestions = mediumManager.GetTotalQuestions();
            }
            else if (timeManager is HardTimeAttackManager hardManager)
            {
                difficultyLevel = "Hard";
                stageNumber = 3;
                currentScore = hardManager.GetCurrentScore();
                correctAnswers = hardManager.GetCorrectAnswers();
                totalQuestions = hardManager.GetTotalQuestions();
            }

            if (GameHistoryManager.Instance != null)
            {
                GameHistoryManager.Instance.SaveGameResult(
                    "TimeAttack",
                    difficultyLevel,
                    currentScore,
                    correctAnswers,
                    totalQuestions,
                    stageNumber
                );

                Debug.Log($"Saved TimeAttack {difficultyLevel} progress with score {currentScore} to history");
            }
        }

        SceneManager.LoadScene(mainMenuSceneName);
    }

    // Helper method to find any of the specified game managers
    private bool FindAnyGameManager<T1, T2, T3>(out object manager) where T1 : MonoBehaviour where T2 : MonoBehaviour where T3 : MonoBehaviour
    {
        T1 manager1 = FindFirstObjectByType<T1>();
        if (manager1 != null)
        {
            manager = manager1;
            return true;
        }

        T2 manager2 = FindFirstObjectByType<T2>();
        if (manager2 != null)
        {
            manager = manager2;
            return true;
        }

        T3 manager3 = FindFirstObjectByType<T3>();
        if (manager3 != null)
        {
            manager = manager3;
            return true;
        }

        manager = null;
        return false;
    }

    // Helper method to extract stage number from scene name
    private int ExtractStageNumberFromScene(string sceneName)
    {
        // Default to stage 1
        int stageNumber = 1;

        // Try to extract stage number from scene name
        // Assuming format like "Stage1", "Stage2", etc.
        if (sceneName.StartsWith("Stage") && sceneName.Length > 5)
        {
            char stageChar = sceneName[5];
            if (char.IsDigit(stageChar))
            {
                stageNumber = stageChar - '0'; // Convert char to int
            }
        }

        return stageNumber;
    }
}