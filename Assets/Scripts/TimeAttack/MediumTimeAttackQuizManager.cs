using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using System.Collections;
using System.Collections.Generic;
using System; // Add for Action delegates
using Random = UnityEngine.Random;

public class MediumTimeAttackManager : MonoBehaviour
{
    // Add event handlers for character animations
    public event Action OnCorrectAnswer;
    public event Action OnWrongAnswer;
    public event Action OnQuizReset;
    public event Action OnTimeUp;

    // Add new event to trigger character appearance before each question
    public event Action OnBeforeQuestionDisplayed;

    [Header("UI References")]
    [SerializeField] private TMP_Text questionText;
    [SerializeField] private Button[] answerButtons;
    [SerializeField] private TMP_Text[] buttonTexts;
    [SerializeField] private TMP_Text timerText;
    [SerializeField] private TMP_Text questionNumberText;
    [SerializeField] private TMP_Text scoreText;
    [SerializeField] private TMP_Text finalScoreText;
    [SerializeField] private TMP_Text gameOverReasonText;
    [SerializeField] private GameObject gameOverPanel;
    [SerializeField] private TMP_Text totalScoreText;
    [SerializeField] private GameObject scrollView;

    [Header("Success/Fail UI")]
    [SerializeField] private TMP_Text successText;
    [SerializeField] private TMP_Text failText;

    [Header("Difficulty Progression")]
    [SerializeField] private Button nextDifficultyButton;
    [SerializeField] private int scoreThresholdForMedium = 70;
    [SerializeField] private string mediumDifficultySceneName = "MediumQuiz";

    [Header("Game Over UI")]
    [SerializeField] private Button restartButton;
    [SerializeField] private Button mainMenuButton;
    [SerializeField] private Button proceedButton;

    [Header("UI Customization")]
    [SerializeField] private Color correctColor = Color.green;
    [SerializeField] private Color wrongColor = Color.red;
    [SerializeField] private Color normalColor = Color.white;

    [Header("Timer Settings")]
    [SerializeField] private float timePerQuestion = 10f;
    private float currentTime;
    private bool isTimerRunning = false;
    private IEnumerator timerCoroutine;

    [Header("Animation Settings")]
    [SerializeField] private float animationDelayBeforeNextQuestion = 3f;
    [SerializeField] private float delayAfterCharacterAppears = 1.0f; // Added delay after character appears

    [Header("Scoring Settings")]
    [SerializeField] private int pointsPerCorrectAnswer = 10;
    private int currentScore = 0;
    private int totalQuestionsAnswered = 0;
    private int correctAnswers = 0;

    [Header("Scene Management")]
    [SerializeField] private string mainMenuSceneName = "TitleScreen";

    [System.Serializable]
    public class TimeAttackQuestion
    {
        public string questionText;
        public string[] answers = new string[4];
        public int correctAnswerIndex;
        public float timeLimit = 10f;
    }

    [Header("Quiz Content")]
    [SerializeField] private List<TimeAttackQuestion> questions = new List<TimeAttackQuestion>();
    private int currentQuestionIndex = 0;
    private bool canAnswer = true;
    private bool hasMetScoreThreshold = false;

    // Add this to the top of TimeAttackManager class
    private MediumCharacterManagerTA characterManager;

    private void Start()
    {
        // Set the current game mode and stage
    AudioManager.Instance.SetGameModeAndStage(AudioManager.GameMode.TimeAttack, 1); // Mid Diff

        // Get reference to the character manager
        characterManager = FindFirstObjectByType<MediumCharacterManagerTA>();

        InitializeUI();
        StartQuiz();
    }

    private void InitializeUI()
    {
        buttonTexts = new TMP_Text[answerButtons.Length];
        for (int i = 0; i < answerButtons.Length; i++)
        {
            buttonTexts[i] = answerButtons[i].GetComponentInChildren<TMP_Text>();
            int buttonIndex = i;
            answerButtons[i].onClick.AddListener(() => OnAnswerSelected(buttonIndex));
        }

        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(false);
        }

        if (nextDifficultyButton != null)
        {
            nextDifficultyButton.gameObject.SetActive(false);
            nextDifficultyButton.onClick.AddListener(ProceedToMediumDifficulty);
        }

        // Initialize success/fail text objects
        if (successText != null && failText != null)
        {
            successText.gameObject.SetActive(false);
            failText.gameObject.SetActive(false);
        }

        // Set up button listeners for game over panel
        if (restartButton != null)
        {
            restartButton.onClick.AddListener(RestartQuiz);
        }
        if (mainMenuButton != null)
        {
            mainMenuButton.onClick.AddListener(ReturnToMainMenu);
        }
        if (proceedButton != null)
        {
            proceedButton.onClick.AddListener(ProceedToMediumDifficulty);
        }

        // Initialize score displays
        UpdateScoreDisplay();
        ResetScoreDisplays();
    }

    private void ResetScoreDisplays()
    {
        if (finalScoreText != null)
        {
            finalScoreText.text = "Correct Answers: 0/0";
        }
        if (totalScoreText != null)
        {
            totalScoreText.text = "Total Score: 0";
        }
    }

    private void StartQuiz()
    {
        // Stop any existing coroutines to ensure a clean state
        StopAllCoroutines();

        ShuffleQuestions();
        currentQuestionIndex = 0;
        currentScore = 0;
        totalQuestionsAnswered = 0;
        correctAnswers = 0;
        hasMetScoreThreshold = false;

        // Reset interactive flags
        canAnswer = true;
        isTimerRunning = false;

        if (nextDifficultyButton != null)
        {
            nextDifficultyButton.gameObject.SetActive(false);
        }

        // Reset success/fail text objects
        if (successText != null && failText != null)
        {
            successText.gameObject.SetActive(false);
            failText.gameObject.SetActive(false);
        }

        // Make sure scroll view is visible when starting quiz
        if (scrollView != null)
        {
            scrollView.SetActive(true);
        }

        ResetScoreDisplays();
        UpdateScoreDisplay();

        // Trigger event for animations
        OnQuizReset?.Invoke();

        // Start with the first question (and character)
        // Use a small delay to ensure all resets are complete
        StartCoroutine(DelayedQuizStart());
    }

    private void ShuffleQuestions()
    {
        int n = questions.Count;
        while (n > 1)
        {
            n--;
            int k = Random.Range(0, n + 1);
            TimeAttackQuestion temp = questions[k];
            questions[k] = questions[n];
            questions[n] = temp;
        }
    }

    // Modify DisplayCurrentQuestion to handle ONLY question display, not character spawning
    private void DisplayCurrentQuestion()
    {
        if (currentQuestionIndex >= questions.Count)
        {
            GameOver(true);
            return;
        }

        // Display the current question
        TimeAttackQuestion question = questions[currentQuestionIndex];
        questionText.text = question.questionText;
        questionNumberText.text = $"Question {currentQuestionIndex + 1}/{questions.Count}";

        for (int i = 0; i < answerButtons.Length; i++)
        {
            buttonTexts[i].text = question.answers[i];
            answerButtons[i].image.color = normalColor;
        }

        StartTimer();
    }

    // Modify StartTimer to keep canAnswer as it is (don't auto-enable it)
    private void StartTimer()
    {
        if (timerCoroutine != null)
        {
            StopCoroutine(timerCoroutine);
        }

        isTimerRunning = true;
        // Remove the line that sets canAnswer = true
        // canAnswer = true;  // <-- Remove this line
        timerCoroutine = TimerCoroutine();
        StartCoroutine(timerCoroutine);
    }

    private IEnumerator TimerCoroutine()
    {
        currentTime = questions[currentQuestionIndex].timeLimit;

        while (currentTime > 0 && isTimerRunning)
        {
            yield return new WaitForSeconds(0.1f);
            currentTime -= 0.1f;
            UpdateTimerDisplay();

            if (currentTime <= 0)
            {
                TimeUp();
            }
        }
    }

    private void StopTimer()
    {
        isTimerRunning = false;
        if (timerCoroutine != null)
        {
            StopCoroutine(timerCoroutine);
            timerCoroutine = null;
        }
    }

    private void UpdateTimerDisplay()
    {
        if (timerText != null)
        {
            timerText.text = $"Time: {Mathf.CeilToInt(currentTime)}";
        }
    }

    private void UpdateScoreDisplay()
    {
        if (scoreText != null)
        {
            scoreText.text = $"Score: {currentScore}";
        }

        // Check if score is above threshold
        hasMetScoreThreshold = currentScore >= scoreThresholdForMedium;

        if (nextDifficultyButton != null)
        {
            nextDifficultyButton.gameObject.SetActive(hasMetScoreThreshold);
        }
    }

    private void TimeUp()
    {
        isTimerRunning = false;
        canAnswer = false;

        // Trigger the time-up event
        OnTimeUp?.Invoke();

        // Add a delay before game over to allow character animation to play
        StartCoroutine(GameOverAfterDelay(animationDelayBeforeNextQuestion));
    }

    private IEnumerator GameOverAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        GameOver(false);
    }


    // Modify the OnAnswerSelected method
    private void OnAnswerSelected(int selectedAnswerIndex)
    {
        // Block new answer attempts if we can't answer
        if (!canAnswer) return;

        // Immediately disable answering while processing
        canAnswer = false;

        TimeAttackQuestion currentQuestion = questions[currentQuestionIndex];
        bool isCorrect = selectedAnswerIndex == currentQuestion.correctAnswerIndex;

        totalQuestionsAnswered++;
        answerButtons[selectedAnswerIndex].image.color = isCorrect ? correctColor : wrongColor;

        if (isCorrect)
        {
            AudioManager.Instance.PlayClassicCorrectSound();
            currentScore += pointsPerCorrectAnswer;
            correctAnswers++;
            UpdateScoreDisplay();

            // Trigger correct answer animation
            OnCorrectAnswer?.Invoke();
        }
        else
        {
            AudioManager.Instance.PlayClassicIncorrectSound();
            answerButtons[currentQuestion.correctAnswerIndex].image.color = correctColor;

            // Trigger wrong answer animation
            OnWrongAnswer?.Invoke();
        }

        StartCoroutine(ProceedToNextQuestion());
    }

    // Modify the ProceedToNextQuestion coroutine
    private IEnumerator ProceedToNextQuestion()
    {
        // Keep answer buttons disabled
        canAnswer = false;
        StopTimer();

        // Short delay after answering to show correct/wrong color
        yield return new WaitForSeconds(2.0f);

        // Reset button colors and move to next question
        ResetButtonColors();
        currentQuestionIndex++;

        // Show next question IMMEDIATELY, don't wait for character
        if (currentQuestionIndex < questions.Count)
        {
            TimeAttackQuestion question = questions[currentQuestionIndex];
            questionText.text = question.questionText;
            questionNumberText.text = $"Question {currentQuestionIndex + 1}/{questions.Count}";

            for (int i = 0; i < answerButtons.Length; i++)
            {
                buttonTexts[i].text = question.answers[i];
                answerButtons[i].image.color = normalColor;
            }

            // THEN trigger character transition (in parallel)
            OnBeforeQuestionDisplayed?.Invoke();

            // Start timer after a delay, but ONLY enable answers after character animation completes
            StartCoroutine(StartTimerAfterCharacterTransition());
        }
        else
        {
            // If no more questions, just end the game
            GameOver(true);
        }
    }


    // Helper method to start timer with a delay
    private IEnumerator StartTimerWithDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        StartTimer();
    }

    private void ResetButtonColors()
    {
        foreach (Button button in answerButtons)
        {
            button.image.color = normalColor;
        }
    }

    private void GameOver(bool completed)
    {
        StopAllCoroutines();

        // Hide the scroll view when game is over
        if (scrollView != null)
        {
            scrollView.SetActive(false);
        }

        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(true);

            // Update final score text
            if (finalScoreText != null)
            {
                finalScoreText.text = $"Correct Answers: {correctAnswers}/{questions.Count}";
            }

            // Update total score text
            if (totalScoreText != null)
            {
                int totalPoints = correctAnswers * pointsPerCorrectAnswer;
                totalScoreText.text = $"Total Score: {totalPoints}";
            }

            if (GameHistoryManager.Instance != null)
            {
                // Get the current stage number (default to 1 for Classic mode)
                int stageNumber = 1;

                // Save game result with the new stage parameter
                GameHistoryManager.Instance.SaveGameResult(
                    "TimeAttack",
                    "Medium", // Current difficulty level
                    currentScore,
                    correctAnswers,
                    questions.Count,
                    stageNumber    // Added stage parameter to match the updated method signature
                );
            }

            // Check if the score threshold was met
            bool metThreshold = currentScore >= scoreThresholdForMedium;

            // Show success or fail text based on threshold
            if (successText != null && failText != null)
            {
                successText.gameObject.SetActive(metThreshold);
                failText.gameObject.SetActive(!metThreshold);
            }

            // Show/hide buttons based on score threshold
            if (restartButton != null)
            {
                restartButton.gameObject.SetActive(!metThreshold);
            }
            if (mainMenuButton != null)
            {
                mainMenuButton.gameObject.SetActive(!metThreshold); // Always show main menu
            }
            if (proceedButton != null)
            {
                proceedButton.gameObject.SetActive(metThreshold);
            }
        }
    }

    public void ReturnToMainMenu()
    {
        SceneManager.LoadScene(mainMenuSceneName);
    }

    // In TimeAttackManager.cs, modify the RestartQuiz() method
    public void RestartQuiz()
    {
        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(false);
        }

        // Show the scroll view again when restarting
        if (scrollView != null)
        {
            scrollView.SetActive(true);
        }

        // Stop any running coroutines first
        StopAllCoroutines();

        // Reset button colors and states
        ResetButtonColors();

        // Important: Reset the canAnswer flag
        canAnswer = true;

        // Reset the timer state
        isTimerRunning = false;
        timerCoroutine = null;

        // Make sure to call StartQuiz which will properly reinitialize everything
        StartQuiz();
    }

    public void ProceedToMediumDifficulty()
    {
        // Update the AudioManager - using mode and letting AudioManager increment the stage
        AudioManager.Instance.SetGameModeAndStage(AudioManager.GameMode.TimeAttack, 2); // to Hard Diff

        AudioManager.Instance.IncrementStage(); // Then increment stage
        SceneManager.LoadScene(mediumDifficultySceneName);
    }

    public void AddQuestion(string questionText, string[] answers, int correctIndex, float timeLimit)
    {
        TimeAttackQuestion newQuestion = new TimeAttackQuestion
        {
            questionText = questionText,
            answers = answers,
            correctAnswerIndex = correctIndex,
            timeLimit = timeLimit
        };

        questions.Add(newQuestion);
    }

    public int GetCurrentScore() => currentScore;
    public int GetCorrectAnswers() => correctAnswers;
    public int GetTotalQuestionsAnswered() => totalQuestionsAnswered;
    public int GetTotalQuestions() => questions.Count;

    // Add this new coroutine to handle the first question with proper timing
    private IEnumerator DelayedFirstQuestion()
    {
        // Short delay to ensure everything is initialized
        yield return new WaitForSeconds(0.1f);

        // First trigger the character appearance
        OnBeforeQuestionDisplayed?.Invoke();

        // Short delay to let character animation complete
        yield return new WaitForSeconds(0.5f);

        // Then display the first question
        DisplayCurrentQuestion();
    }

    // New method to handle timing properly
    private IEnumerator StartTimerAfterCharacterTransition()
    {
        // Wait for character transition to complete if we have a character manager
        if (characterManager != null)
        {
            // Wait until character is no longer transitioning
            while (characterManager.IsTransitioning())
            {
                yield return null;
            }

            // Add a small additional delay after transition completes
            yield return new WaitForSeconds(0.2f);
        }
        else
        {
            // No character manager, use a fixed delay
            yield return new WaitForSeconds(1.0f);
        }

        // Start the timer
        StartTimer();

        // Only now enable answering
        canAnswer = true;
    }

    private IEnumerator DelayedQuizStart()
    {
        // Small delay to allow everything to reset
        yield return new WaitForSeconds(0.2f);

        // Trigger character appearance first
        OnBeforeQuestionDisplayed?.Invoke();

        // Wait for character to appear
        yield return new WaitForSeconds(0.5f);

        // Then show the first question
        TimeAttackQuestion question = questions[currentQuestionIndex];
        questionText.text = question.questionText;
        questionNumberText.text = $"Question {currentQuestionIndex + 1}/{questions.Count}";

        for (int i = 0; i < answerButtons.Length; i++)
        {
            buttonTexts[i].text = question.answers[i];
            answerButtons[i].image.color = normalColor;
        }

        // Start the timer
        StartTimer();
    }

}