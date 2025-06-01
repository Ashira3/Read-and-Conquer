using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic;

public class Stage5Manager : MonoBehaviour
{
    [Header("Health Settings")]
    [SerializeField] private int stagePlayerHealth = 5;
    [SerializeField] private int stageEnemyHealth = 10;
    [SerializeField] private string NEXT_STAGE_SCENE = "Stage5";

    [Header("UI References")]
    public TextMeshProUGUI passageText;
    public TextMeshProUGUI failText;
    public TextMeshProUGUI successText;
    public Button[] answerButtons;
    public Color correctColor = Color.green;
    public Color wrongColor = Color.red;
    public Color normalColor = Color.white;

    [Header("Game Over UI")]
    public Button retryButton;
    public Button mainMenuButton;
    public Button nextStageButton;

    [Header("Questions")]
    public Question[] questions;
    private List<Question> shuffledQuestions; // New list for shuffled questions

    [Header("Score Settings")]
    public int correctAnswerPoints = 100; // Points awarded for correct answers
    public int incorrectAnswerPenalty = 50; // Points deducted for incorrect answers

    [Header("Game Mode Settings")]
    public string gameMode = "BossRush"; // Default mode
    public string difficulty = "Easy";    // Default difficulty

    [Header("Animation References")]
    [SerializeField] private Animator playerAnimator;
    [SerializeField] private Animator enemyAnimator;

    [Header("Animation Settings")]
    [SerializeField] private float animationDelay = 0.5f;
    [SerializeField] private float attackAnimationLength = 1.0f;
    [SerializeField] private float hurtAnimationLength = 0.7f;
    [SerializeField] private float deathAnimationLength = 1.5f;

    [SerializeField] private GameObject HiddenSettings;

    private const string TITLE_SCENE = "TitleScreen";
    [Header("Scene Names")]


    private int currentQuestionIndex = 0;
    private bool canAnswer = true;
    private bool animationInProgress = false;

    // Animation trigger parameters
    private const string TRIGGER_ATTACK = "Attack";
    private const string TRIGGER_HURT = "Hurt";
    private const string TRIGGER_DEATH = "Death";
    private const string TRIGGER_IDLE = "Idle";

    // Track correct answers for history
    private int correctAnswersCount = 0;
    private int totalAnsweredQuestions = 0;

    private Stage5HealthManager healthManager;
    private AudioManager audioManager;
    private GameHistoryManager historyManager;

    void Start()
    {
        // Set the current game mode and stage
        AudioManager.Instance.SetGameModeAndStage(AudioManager.GameMode.BossRush, 5); // Stage 4

        // Find the stage-specific health manager in this scene
        healthManager = FindObjectOfType<Stage5HealthManager>();

        // Find other managers
        audioManager = FindObjectOfType<AudioManager>();
        historyManager = FindObjectOfType<GameHistoryManager>();

        if (healthManager == null)
        {
            Debug.LogError("HealthManager not found in the scene! Please ensure it is present.");
            return;
        }

        if (audioManager == null)
        {
            Debug.LogError("AudioManager not found in the scene! Please ensure it is present.");
        }

        if (historyManager == null)
        {
            Debug.LogWarning("GameHistoryManager not found! Game history will not be recorded.");
        }

        // Validate animator references
        if (playerAnimator == null)
        {
            Debug.LogError("Player Animator not assigned! Please assign it in the inspector.");
        }

        if (enemyAnimator == null)
        {
            Debug.LogError("Enemy Animator not assigned! Please assign it in the inspector.");
        }

        // Set stage-specific health values
        healthManager.SetupStageHealth(stagePlayerHealth, stageEnemyHealth);

        // Make sure UI is updated with these values
        healthManager.FindAndUpdateUIReferences();
        healthManager.UpdatePlayerHearts();
        healthManager.UpdateEnemyHearts();
        healthManager.UpdateScoreDisplay();

        // Hide game over buttons at start
        if (retryButton != null) retryButton.gameObject.SetActive(false);
        if (mainMenuButton != null) mainMenuButton.gameObject.SetActive(false);
        if (nextStageButton != null) nextStageButton.gameObject.SetActive(false);

        // Hide both success and fail text at start
        if (failText != null) failText.gameObject.SetActive(false);
        if (successText != null) successText.gameObject.SetActive(false);

        // Make sure settings is visible when starting quiz
        if (HiddenSettings != null)
        {
            HiddenSettings.SetActive(true);
        }

        Debug.Log($"Total Questions: {questions.Length}");

        // Reset tracking variables for this stage
        correctAnswersCount = 0;
        totalAnsweredQuestions = 0;

        // Shuffle questions at the start
        ShuffleQuestions();
        DisplayQuestion();

        // Set idle animations for both characters
        SetIdleAnimations();
    }

    private void SetIdleAnimations()
    {
        if (playerAnimator != null)
            playerAnimator.SetTrigger(TRIGGER_IDLE);

        if (enemyAnimator != null)
            enemyAnimator.SetTrigger(TRIGGER_IDLE);
    }

    // New method to shuffle questions
    void ShuffleQuestions()
    {
        // Create a new list from the questions array
        shuffledQuestions = new List<Question>(questions);

        // Fisher-Yates shuffle algorithm
        for (int i = shuffledQuestions.Count - 1; i > 0; i--)
        {
            int randomIndex = Random.Range(0, i + 1);
            Question temp = shuffledQuestions[i];
            shuffledQuestions[i] = shuffledQuestions[randomIndex];
            shuffledQuestions[randomIndex] = temp;
        }

        Debug.Log("Questions have been shuffled.");
    }

    void DisplayQuestion()
    {
        if (currentQuestionIndex >= shuffledQuestions.Count)
        {
            EndQuiz(true);
            return;
        }

        Question currentQuestion = shuffledQuestions[currentQuestionIndex];

        if (currentQuestion == null)
        {
            Debug.LogError($"Question at index {currentQuestionIndex} is null!");
            return;
        }

        Debug.Log($"Displaying Question: {currentQuestion.passageText}");

        passageText.text = currentQuestion.passageText;
        passageText.gameObject.SetActive(true);

        for (int i = 0; i < answerButtons.Length; i++)
        {
            TextMeshProUGUI buttonText = answerButtons[i].GetComponentInChildren<TextMeshProUGUI>();
            if (buttonText == null)
            {
                Debug.LogError($"Button text component is missing for button at index {i}!");
                continue;
            }

            buttonText.text = currentQuestion.answers[i];
            answerButtons[i].GetComponent<Image>().color = normalColor;
            answerButtons[i].interactable = true;

            Debug.Log($"Button {i}: {currentQuestion.answers[i]}");
        }

        canAnswer = true;
        animationInProgress = false;

        // Reset to idle animations when showing a new question
        SetIdleAnimations();
    }

    public void OnAnswerSelected(int answerIndex)
    {
        if (!canAnswer || animationInProgress) return;

        canAnswer = false;
        animationInProgress = true;

        bool isCorrect = answerIndex == shuffledQuestions[currentQuestionIndex].correctAnswer;

        // Increment total answered questions
        totalAnsweredQuestions++;

        answerButtons[answerIndex].GetComponent<Image>().color =
            isCorrect ? correctColor : wrongColor;

        // If incorrect, immediately highlight the correct answer
        if (!isCorrect)
        {
            answerButtons[shuffledQuestions[currentQuestionIndex].correctAnswer]
                .GetComponent<Image>().color = correctColor;
        }

        Debug.Log($"Answer Selected: {answerIndex}, Correct: {isCorrect}");

        // Disable all answer buttons immediately to prevent multiple clicks
        foreach (Button button in answerButtons)
        {
            button.interactable = false;
        }

        // Start the animation sequence based on answer correctness
        StartCoroutine(PlayAnswerAnimations(isCorrect));
    }

    IEnumerator PlayAnswerAnimations(bool isCorrect)
    {
        if (isCorrect)
        {
            AudioManager.Instance.PlayClassicCorrectSound();
            correctAnswersCount++; // Increment correct answers count

            // Player attacks first
            if (playerAnimator != null)
                playerAnimator.SetTrigger(TRIGGER_ATTACK);

            // Wait for attack animation to complete
            yield return new WaitForSeconds(attackAnimationLength);

            // Enemy gets hurt
            if (enemyAnimator != null)
                enemyAnimator.SetTrigger(TRIGGER_HURT);

            // Process game logic
            healthManager.DecreaseEnemyHealth();
            healthManager.IncreaseScore(correctAnswerPoints);

            // Wait for hurt animation to complete
            yield return new WaitForSeconds(hurtAnimationLength);

            // Check if enemy died
            if (healthManager.enemyHealth <= 0)
            {
                if (enemyAnimator != null)
                    enemyAnimator.SetTrigger(TRIGGER_DEATH);

                yield return new WaitForSeconds(deathAnimationLength);
                EndQuiz(true);
                yield break;
            }
        }
        else
        {
            AudioManager.Instance.PlayClassicIncorrectSound();

            // Enemy attacks first
            if (enemyAnimator != null)
                enemyAnimator.SetTrigger(TRIGGER_ATTACK);

            // Wait for attack animation to complete
            yield return new WaitForSeconds(attackAnimationLength);

            // Player gets hurt
            if (playerAnimator != null)
                playerAnimator.SetTrigger(TRIGGER_HURT);

            // Process game logic
            healthManager.DecreasePlayerHealth();
            healthManager.DecreaseScore(incorrectAnswerPenalty);

            // Wait for hurt animation to complete
            yield return new WaitForSeconds(hurtAnimationLength);

            // Check if player died
            if (healthManager.playerHealth <= 0)
            {
                if (playerAnimator != null)
                    playerAnimator.SetTrigger(TRIGGER_DEATH);

                yield return new WaitForSeconds(deathAnimationLength);
                EndQuiz(false);
                yield break;
            }
        }

        // Disable answer buttons during transition
        foreach (Button button in answerButtons)
        {
            button.interactable = false;
        }

        // Reset to idle animations after the sequence
        SetIdleAnimations();

        // Wait a moment before moving to the next question
        yield return new WaitForSeconds(animationDelay);

        animationInProgress = false;
        MoveToNextQuestion();
    }

    void MoveToNextQuestion()
    {
        currentQuestionIndex++;
        DisplayQuestion();
    }

    void EndQuiz(bool victory)
    {
        // Stop current background music
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.StopBackgroundMusic();

            // Play appropriate sound based on victory or defeat
            if (victory)
            {
                AudioManager.Instance.PlayBossRushSuccessSound();
            }
            else
            {
                AudioManager.Instance.PlayBossRushFailSound();
            }

            // Save results to game history
            SaveResultsToHistory(victory);

            // Hide the passage text
            passageText.gameObject.SetActive(false);

            // Make sure settings is visible when starting quiz
            if (HiddenSettings != null)
            {
                HiddenSettings.SetActive(false);
            }

            // Show appropriate message based on victory or defeat
            if (victory)
            {
                if (successText != null)
                {
                    successText.gameObject.SetActive(true);
                }
                else
                {
                    Debug.LogError("Success Text TMP is not assigned!");
                }

                // Save score before moving to next stage
                healthManager.SaveScore();
                Debug.Log($"Saved score for next stage: {healthManager.playerScore}");

                // Show next stage button on victory
                if (nextStageButton != null) nextStageButton.gameObject.SetActive(true);
            }
            else
            {
                if (failText != null)
                {
                    failText.gameObject.SetActive(true);
                }
                else
                {
                    Debug.LogError("Fail Text TMP is not assigned!");
                }

                // Show retry and main menu buttons on defeat
                if (retryButton != null) retryButton.gameObject.SetActive(true);
                if (mainMenuButton != null) mainMenuButton.gameObject.SetActive(true);
            }

            // Hide the answer buttons
            foreach (Button button in answerButtons)
            {
                button.gameObject.SetActive(false);
            }
        }

        // New method to save results to game history
        void SaveResultsToHistory(bool victory)
        {
            // Get the current scene name
            string currentSceneName = SceneManager.GetActiveScene().name;

            // Save results if:
            // 1. Player lost (ran out of HP)
            // 2. This is the final stage that was completed successfully
            // 3. Current scene is named "FinalStage" and player won

            bool isLastStage = (NEXT_STAGE_SCENE == "FinalStage" || NEXT_STAGE_SCENE == "Results" || NEXT_STAGE_SCENE == "");
            bool isFinalStage = currentSceneName == "Stage5";

            if (!victory || (victory && (isLastStage || isFinalStage)))
            {
                if (historyManager != null)
                {
                    // Calculate final values for this stage
                    int finalScore = healthManager.playerScore;
                    // Get the current stage number from the scene name
                    int stageNumber = 1; // Default to 1
                                         // Try to extract the stage number from the scene name
                    if (currentSceneName.StartsWith("Stage") && currentSceneName.Length > 5)
                    {
                        int.TryParse(currentSceneName.Substring(5), out stageNumber);
                    }
                    // If we're in the final stage, we want to record that explicitly
                    if (isFinalStage)
                    {
                        stageNumber = 5; // Special value to indicate final stage
                                         // Or alternatively, you could use a fixed number like 999
                    }

                    // Determine total questions
                    int totalQuestions = Mathf.Max(totalAnsweredQuestions, shuffledQuestions.Count);

                    // Record the result in history manager
                    historyManager.SaveGameResult(
                        gameMode,              // Current game mode (BossRush, Classic, etc)
                        difficulty,            // Current difficulty
                        finalScore,            // Final score for this stage
                        correctAnswersCount,   // Number of correct answers
                        totalQuestions,        // Total questions in this stage
                        stageNumber            // Current stage number
                    );

                    Debug.Log($"Saved result to history: Mode={gameMode}, Difficulty={difficulty}, " +
                              $"Score={finalScore}, Correct={correctAnswersCount}, Total={totalQuestions}, Stage={stageNumber}, " +
                              $"IsFinalStage={isFinalStage}, Victory={victory}");
                }
            }
        }
    }

    // Methods to handle button click events
    public void RetryStage()
    {
        // Play button click sound if available
        AudioManager.Instance.PlayButtonClickSound();

        // For Boss Rush mode, when retrying we go back to Stage 1
        if (AudioManager.Instance != null)
        {
            // Reset to Boss Rush Stage 1
            AudioManager.Instance.ResetToBossRushStage1();
        }

        // Reset both health and score
        if (healthManager != null)
        {
            // Reset health to stage defaults
            healthManager.SetupStageHealth(stagePlayerHealth, stageEnemyHealth);
            // Reset score to 0 for Stage 5 retry
            healthManager.ResetScore();
            // Save the reset score
            healthManager.SaveScore();
        }

        // Make sure settings is visible when starting quiz
        if (HiddenSettings != null)
        {
            HiddenSettings.SetActive(true);
        }

        // Reset the current question index
        currentQuestionIndex = 0;

        // Reset tracking variables
        correctAnswersCount = 0;
        totalAnsweredQuestions = 0;

        // Reset animations to idle
        SetIdleAnimations();

        // Hide game over buttons
        if (retryButton != null) retryButton.gameObject.SetActive(false);
        if (mainMenuButton != null) mainMenuButton.gameObject.SetActive(false);
        if (nextStageButton != null) nextStageButton.gameObject.SetActive(false);

        // Hide both success and fail texts
        if (failText != null) failText.gameObject.SetActive(false);
        if (successText != null) successText.gameObject.SetActive(false);

        // Show passage text again
        passageText.gameObject.SetActive(true);

        // Show answer buttons again
        foreach (Button button in answerButtons)
        {
            button.gameObject.SetActive(true);
        }

        // Reshuffle questions for the retry
        ShuffleQuestions();

        // Start quiz again
        DisplayQuestion();

        Debug.Log("Score and health reset for stage retry");
        // Reset score in PlayerPrefs before going back to Stage1
        PlayerPrefs.SetInt("PlayerScore", 0);
        PlayerPrefs.Save();
        Debug.Log("Reset player score before returning to Stage1");

        // Add a small delay before loading to allow sound to play and UI to update
        StartCoroutine(LoadStage1WithDelay());
    }

    private System.Collections.IEnumerator LoadStage1WithDelay()
    {
        yield return new WaitForSeconds(0.1f); // Short delay for sound to play
        SceneManager.LoadScene("Stage 1");
    }

    public void ReturnToMainMenu()
    {
        // Play button click sound if available
        AudioManager.Instance.PlayButtonClickSound();

        // Save the score before returning to main menu
        if (healthManager != null)
        {
            healthManager.SaveScore();
        }

        // Load the main menu
        StartCoroutine(LoadMainMenuWithDelay());
    }

    public void ResetAndReturnToMainMenu()
    {
        // Play button click sound if available
        if (audioManager != null)
        {
            audioManager.PlayButtonClickSound();
        }

        // Clear PlayerPrefs values for score
        PlayerPrefs.DeleteKey("PlayerScore");
        PlayerPrefs.Save();

        // Add a small delay before loading to allow sound to play
        StartCoroutine(LoadMainMenuWithDelay());
    }

    public void GoToNextStage()
    {
        // Update the AudioManager - using mode and letting AudioManager increment the stage
        AudioManager.Instance.SetGameModeAndStage(AudioManager.GameMode.TitleScreen, 0); // to TitleScreen

        AudioManager.Instance.IncrementStage(); // Then increment stage

        // Play button click sound if available
        AudioManager.Instance.PlayButtonClickSound();

        // Score is saved automatically when changed, but let's save it explicitly here
        if (healthManager != null)
        {
            healthManager.SaveScore();
            Debug.Log($"Explicitly saved score before next stage: {healthManager.playerScore}");
        }

        // Add a small delay before loading to allow sound to play
        StartCoroutine(LoadNextStageWithDelay());
    }

    public void StartNewBossRushGame()
    {
        // Set flag for new game
        PlayerPrefs.SetInt("StartingNewGame", 1);
        PlayerPrefs.DeleteKey("PlayerScore");
        PlayerPrefs.Save();

        // Reset local HealthManager if present
        var healthManager = FindObjectOfType<Stage1HealthManager>();
        if (healthManager != null)
        {
            healthManager.ResetForNewGame();
        }

        Debug.Log("Starting new Boss Rush game with fresh score");

        // Load the first stage
        SceneManager.LoadScene("Stage1");
    }

    private System.Collections.IEnumerator LoadMainMenuWithDelay()
    {
        yield return new WaitForSeconds(0.1f); // Short delay for sound to play
        SceneManager.LoadScene(TITLE_SCENE);
    }

    private System.Collections.IEnumerator LoadNextStageWithDelay()
    {
        yield return new WaitForSeconds(0.1f); // Short delay for sound to play
        SceneManager.LoadScene(NEXT_STAGE_SCENE);
    }
}