using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using System.Collections;
using System.Collections.Generic;
using System; // Add for Action delegates
using Random = UnityEngine.Random;

public class ClassicModeManager : MonoBehaviour
{
    // Add event handlers for character animations
    public event Action OnCorrectAnswer;
    public event Action OnWrongAnswer;
    public event Action OnQuizReset;

    [Header("UI References")]
    [SerializeField] private TMP_Text questionText;
    [SerializeField] private Button[] answerButtons;
    [SerializeField] private TMP_Text[] buttonTexts;
    [SerializeField] private TMP_Text questionNumberText;
    [SerializeField] private TMP_Text scoreText;
    [SerializeField] private TMP_Text finalScoreText;
    [SerializeField] private GameObject gameOverPanel;
    [SerializeField] private TMP_Text totalScoreText;
    [SerializeField] private GameObject scrollView;

    [Header("Success/Fail UI")]
    [SerializeField] private TMP_Text successText;
    [SerializeField] private TMP_Text failText;

    [Header("Difficulty Progression")]
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

    [Header("Scoring Settings")]
    [SerializeField] private int pointsPerCorrectAnswer = 10;
    private int currentScore = 0;
    private int totalQuestionsAnswered = 0;
    private int correctAnswers = 0;

    [Header("Scene Management")]
    [SerializeField] private string mainMenuSceneName = "TitleScreen";

    [System.Serializable]
    public class QuizQuestion
    {
        public string questionText;
        public string[] answers = new string[4];
        public int correctAnswerIndex;
    }

    [Header("Quiz Content")]
    [SerializeField] private List<QuizQuestion> questions = new List<QuizQuestion>();
    private int currentQuestionIndex = 0;
    private bool canAnswer = true;
    private bool hasMetScoreThreshold = false;

    [Header("Animation Settings")]
    [SerializeField] private float animationDelayBeforeNextQuestion = 3f;

    private void Start()
    {
        // Set the current game mode and stage
        AudioManager.Instance.SetGameModeAndStage(AudioManager.GameMode.Classic, 0); // Easy Diff

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
        ShuffleQuestions();
        currentQuestionIndex = 0;
        currentScore = 0;
        totalQuestionsAnswered = 0;
        correctAnswers = 0;
        hasMetScoreThreshold = false;

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
        DisplayCurrentQuestion();

        // Trigger event for animations
        OnQuizReset?.Invoke();
    }

    private void ShuffleQuestions()
    {
        int n = questions.Count;
        while (n > 1)
        {
            n--;
            int k = Random.Range(0, n + 1);
            QuizQuestion temp = questions[k];
            questions[k] = questions[n];
            questions[n] = temp;
        }
    }

    private void DisplayCurrentQuestion()
    {
        if (currentQuestionIndex >= questions.Count)
        {
            GameOver(true);
            return;
        }

        QuizQuestion question = questions[currentQuestionIndex];
        questionText.text = question.questionText;
        questionNumberText.text = $"Question {currentQuestionIndex + 1}/{questions.Count}";

        for (int i = 0; i < answerButtons.Length; i++)
        {
            buttonTexts[i].text = question.answers[i];
            answerButtons[i].image.color = normalColor;
        }

        canAnswer = true;
    }

    private void UpdateScoreDisplay()
    {
        if (scoreText != null)
        {
            scoreText.text = $"Score: {currentScore}";
        }

        // Check if score is above threshold
        hasMetScoreThreshold = currentScore >= scoreThresholdForMedium;
    }

    private void OnAnswerSelected(int selectedAnswerIndex)
    {
        if (!canAnswer) return;

        QuizQuestion currentQuestion = questions[currentQuestionIndex];
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

    private IEnumerator ProceedToNextQuestion()
    {
        canAnswer = false;

        // Wait longer to allow for character animation and speech bubble
        yield return new WaitForSeconds(animationDelayBeforeNextQuestion);

        ResetButtonColors();
        currentQuestionIndex++;
        DisplayCurrentQuestion();
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
                    "Classic",
                    "Easy", // Current difficulty level
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

        StopAllCoroutines();
        ResetButtonColors();
        StartQuiz();
    }

    public void ProceedToMediumDifficulty()
    {
        // Update the AudioManager - using mode and letting AudioManager increment the stage
        AudioManager.Instance.SetGameModeAndStage(AudioManager.GameMode.Classic, 1); // to Mid Diff

        AudioManager.Instance.IncrementStage(); // Then increment stage

        SceneManager.LoadScene(mediumDifficultySceneName);
    }

    public void AddQuestion(string questionText, string[] answers, int correctIndex)
    {
        QuizQuestion newQuestion = new QuizQuestion
        {
            questionText = questionText,
            answers = answers,
            correctAnswerIndex = correctIndex
        };

        questions.Add(newQuestion);
    }

    public int GetCurrentScore() => currentScore;
    public int GetCorrectAnswers() => correctAnswers;
    public int GetTotalQuestionsAnswered() => totalQuestionsAnswered;
    public int GetTotalQuestions() => questions.Count;
}