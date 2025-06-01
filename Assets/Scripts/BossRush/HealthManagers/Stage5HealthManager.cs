using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class Stage5HealthManager : MonoBehaviour
{
    // Default values that can be overridden by stage managers
    public int defaultEnemyHealth = 10;
    public int defaultPlayerHealth = 5;

    // Actual runtime health values
    public int enemyHealth;
    public int playerHealth;
    public int playerScore = 0; // Adding score tracking

    public Image[] enemyHearts; // Array of enemy heart UI images  
    public Image[] playerHearts; // Array of player heart UI images

    public TextMeshProUGUI scoreText; // Reference to display the score

    // We're removing the singleton pattern
    // public static HealthManager Instance;

    private void Awake()
    {
        // Initialize health with default values
        enemyHealth = defaultEnemyHealth;
        playerHealth = defaultPlayerHealth;

        // Check if we're starting a new game
        bool isNewGame = PlayerPrefs.HasKey("StartingNewGame");

        // Load the score from PlayerPrefs if available and not starting new game
        if (!isNewGame && PlayerPrefs.HasKey("PlayerScore"))
        {
            playerScore = PlayerPrefs.GetInt("PlayerScore");
        }
        else
        {
            // Clear the flag after using it
            PlayerPrefs.DeleteKey("StartingNewGame");
            // Ensure score is reset
            playerScore = 0;
            SaveScore();
        }

        Debug.Log($"HealthManager: Initialized in new scene. Score loaded: {playerScore}, IsNewGame: {isNewGame}");
    }

    private void Start()
    {
        // Find and update UI references in the current scene
        

        // Update the UI based on loaded values
        UpdateScoreDisplay();

        Debug.Log($"HealthManager Start completed - PlayerHealth: {playerHealth}, PlayerScore: {playerScore}");
    }

    // Method to set stage-specific health values
    public void SetupStageHealth(int playerHP, int enemyHP)
    {
        playerHealth = playerHP;
        enemyHealth = enemyHP;

        UpdatePlayerHearts();
        UpdateEnemyHearts();

        Debug.Log($"Stage health configured - Player: {playerHealth}, Enemy: {enemyHealth}");
    }

    public void IncreaseScore(int points)
    {
        playerScore += points;
        UpdateScoreDisplay();
        SaveScore(); // Save score immediately on change
        Debug.Log("Score increased. Current score: " + playerScore);
    }

    public void DecreaseScore(int points)
    {
        playerScore = Mathf.Max(0, playerScore - points); // Prevent negative scores
        UpdateScoreDisplay();
        SaveScore(); // Save score immediately on change
        Debug.Log("Score decreased. Current score: " + playerScore);
    }

    public void UpdateScoreDisplay()
    {
        if (scoreText != null)
        {
            scoreText.text = "Score: " + playerScore;
        }
    }

    public void DecreaseEnemyHealth()
    {
        if (enemyHealth > 0)
        {
            enemyHealth--;
            UpdateEnemyHearts();
            Debug.Log("Enemy health decreased. Current health: " + enemyHealth);
        }
    }

    public void DecreasePlayerHealth()
    {
        if (playerHealth > 0)
        {
            playerHealth--;
            UpdatePlayerHearts();
            Debug.Log("Player health decreased. Current health: " + playerHealth);
        }
    }

    public void UpdateEnemyHearts()
    {
        if (enemyHearts == null || enemyHearts.Length == 0) return;

        for (int i = 0; i < enemyHearts.Length; i++)
        {
            if (i < enemyHearts.Length && enemyHearts[i] != null)
            {
                enemyHearts[i].gameObject.SetActive(i < enemyHealth);
            }
        }
    }

    public void UpdatePlayerHearts()
    {
        if (playerHearts == null || playerHearts.Length == 0) return;

        for (int i = 0; i < playerHearts.Length; i++)
        {
            if (i < playerHearts.Length && playerHearts[i] != null)
            {
                playerHearts[i].gameObject.SetActive(i < playerHealth);
            }
        }
    }

    // Reset enemy health but keep player health persistent
    public void ResetEnemyHealth()
    {
        enemyHealth = defaultEnemyHealth; // Reset to default value
        UpdateEnemyHearts();
        Debug.Log("Enemy health reset to: " + enemyHealth);
    }

    // Reset all health (for complete game restart)
    public void ResetHealth()
    {
        enemyHealth = defaultEnemyHealth;
        playerHealth = defaultPlayerHealth;
        UpdateEnemyHearts();
        UpdatePlayerHearts();
        Debug.Log("All health values reset");
    }

    // Reset health values but keep score (for stage transitions)
    public void ResetHealthKeepScore()
    {
        playerHealth = defaultPlayerHealth;
        enemyHealth = defaultEnemyHealth;
        UpdatePlayerHearts();
        UpdateEnemyHearts();
        Debug.Log("Reset all health values while preserving score");
    }

    // Reset score to zero (for complete game restart)
    public void ResetScore()
    {
        playerScore = 0;
        UpdateScoreDisplay();
        SaveScore();
        Debug.Log("Score reset to 0");
    }

    // We only save score now, not health
    public void SaveScore()
    {
        PlayerPrefs.SetInt("PlayerScore", playerScore);
        PlayerPrefs.Save();
        Debug.Log("Saved score to PlayerPrefs: " + playerScore);
    }

    // Start a new run
    public void StartNewRun()
    {
        // Reset health to starting values
        playerHealth = defaultPlayerHealth;
        enemyHealth = defaultEnemyHealth;

        // Reset score for the new run
        playerScore = 0;

        // Update UI
        UpdatePlayerHearts();
        UpdateEnemyHearts();
        UpdateScoreDisplay();

        // Save the reset score to PlayerPrefs
        SaveScore();

        Debug.Log("Starting new run: Health and score reset and saved");
    }

    // Reset for a new game within the same session
    public void ResetForNewGame()
    {
        // Reset to starting values
        playerHealth = defaultPlayerHealth;
        enemyHealth = defaultEnemyHealth;
        playerScore = 0;

        // Update UI
        UpdatePlayerHearts();
        UpdateEnemyHearts();
        UpdateScoreDisplay();

        // Save the reset score
        SaveScore();

        Debug.Log("Reset player health and score for new game session");
    }

    // Find and update UI references in the current scene
    public void FindAndUpdateUIReferences()
    {
        // Try to find UI elements in the current scene
        GameObject uiObject = GameObject.Find("UI");
        if (uiObject != null)
        {
            // Find score text
            Transform scoreTextTransform = uiObject.transform.Find("ScoreText");
            if (scoreTextTransform != null)
            {
                scoreText = scoreTextTransform.GetComponent<TextMeshProUGUI>();
                Debug.Log("Found score text");
            }
        }
        else
        {
            Debug.LogWarning("UI object not found in current scene");
        }
    }
}