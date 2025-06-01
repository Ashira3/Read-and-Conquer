using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;


public class BossRushResults : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private TMP_Text titleText;
    [SerializeField] private TMP_Text[] resultTexts;
    [SerializeField] private Button resetButton;
    [SerializeField] private Button exitButton;

    [Header("Mode Configuration")]
    [SerializeField] private string currentMode = "BossRush";
    [SerializeField] private string currentStage = "Stage 1";
    [SerializeField] private string exitSceneName = "MainHistory";

    private void Start()
    {
        // Set up button listeners
        if (resetButton != null)
        {
            resetButton.onClick.AddListener(ResetResults);
        }

        if (exitButton != null)
        {
            exitButton.onClick.AddListener(Exit);
        }



        // Load and display results
        LoadResults();
    }

    public void LoadResults()
    {
        // Check if GameHistoryManager exists
        if (GameHistoryManager.Instance == null)
        {
            Debug.LogError("GameHistoryManager not found in scene!");
            return;
        }

        // Get results for the current mode and difficulty
        List<GameHistoryManager.GameResult> results =
            GameHistoryManager.Instance.GetResults(currentMode, currentStage);

        // Display results in UI text elements
        for (int i = 0; i < resultTexts.Length; i++)
        {
            if (i < results.Count)
            {
                GameHistoryManager.GameResult result = results[i];
                resultTexts[i].text = $"{i + 1}. Stage: {result.stage} - Score: {result.score}";
            }
            else
            {
                // Clear text for unused slots
                resultTexts[i].text = $"No Result";
            }
        }
    }

    public void ResetResults()
    {
        if (GameHistoryManager.Instance != null)
        {
            // Reset results for current mode and difficulty
            GameHistoryManager.Instance.ResetResults(currentMode, currentStage);

            // Reload results to update UI
            LoadResults();
        }
    }

    public void Exit()
    {
        SceneManager.LoadScene(exitSceneName);
    }

    // Public method to change mode/difficulty - useful if you have tabs or difficulty buttons
    public void SetModeAndDifficulty(string mode, string stage)
    {
        currentMode = mode;
        currentStage = stage;

        // Update title
        if (titleText != null)
        {
            titleText.text = $"{currentMode} History\n{currentStage} Difficulty";
        }

        // Reload results for the new mode/difficulty
        LoadResults();
    }
}