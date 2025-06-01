using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class GameHistoryManager : MonoBehaviour
{

    // Singleton implementation
    private static GameHistoryManager _instance;
    public static GameHistoryManager Instance
    {
        get
        {
            // If destroyed but still being accessed, don't create a new one
            if (_instance == null && applicationIsQuitting)
            {
                return null;
            }
            return _instance;
        }
        private set { _instance = value; }
    }

    // Flag to handle destruction sequence
    private static bool applicationIsQuitting = false;

    [System.Serializable]
        public class GameResult
        {
            public string mode;
            public string difficulty;
            public int score;
            public int correctAnswers;
            public int totalQuestions;
            public string dateTime;
            public int stage;
        }

        // Lists to store results
        [SerializeField] private List<GameResult> classicEasyResults = new List<GameResult>();
        [SerializeField] private List<GameResult> classicMediumResults = new List<GameResult>();
        [SerializeField] private List<GameResult> classicHardResults = new List<GameResult>();
        [SerializeField] private List<GameResult> timeAttackEasyResults = new List<GameResult>();
        [SerializeField] private List<GameResult> timeAttackMediumResults = new List<GameResult>();
        [SerializeField] private List<GameResult> timeAttackHardResults = new List<GameResult>();
        [SerializeField] private List<GameResult> bossRushResults = new List<GameResult>();

        // Maximum number of results to store per mode/difficulty
        [SerializeField] private int maxResultsToStore = 5;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;

            // Make sure we're at root level
            transform.SetParent(null);

            DontDestroyOnLoad(gameObject);
            Debug.Log("GameHistoryManager initialized as singleton");

            // Load results
            LoadAllResults();
        }
        else
        {
            Debug.Log("Duplicate GameHistoryManager destroyed");
            Destroy(gameObject);
        }
    }

    void OnDestroy()
    {
        // Mark as quitting to prevent phantom references
        if (Instance == this)
        {
            applicationIsQuitting = true;
        }
    }

    // Call this at the end of each game to save the result
    public void SaveGameResult(string mode, string difficulty, int score, int correctAnswers, int totalQuestions, int stage)
    {
        GameResult result = new GameResult
        {
            mode = mode,
            difficulty = difficulty,
            score = score,
            correctAnswers = correctAnswers,
            totalQuestions = totalQuestions,
            dateTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm"),
            stage = stage
        };

        // Add to appropriate list based on mode and difficulty
        List<GameResult> targetList = GetResultList(mode, difficulty);

        // Add new result at the beginning (most recent first)
        targetList.Insert(0, result);

        // Trim list if it exceeds maximum size
        if (targetList.Count > maxResultsToStore)
        {
            targetList.RemoveRange(maxResultsToStore, targetList.Count - maxResultsToStore);
        }

        // Save updated results
        SaveAllResults();
    }

    // Get the appropriate result list based on mode and difficulty
    private List<GameResult> GetResultList(string mode, string difficulty)
    {
        switch (mode)
        {
            case "Classic":
                switch (difficulty)
                {
                    case "Easy": return classicEasyResults;
                    case "Medium": return classicMediumResults;
                    case "Hard": return classicHardResults;
                    default: return classicEasyResults;
                }

            case "TimeAttack":
                switch (difficulty)
                {
                    case "Easy": return timeAttackEasyResults;
                    case "Medium": return timeAttackMediumResults;
                    case "Hard": return timeAttackHardResults;
                    default: return timeAttackEasyResults;
                }

            case "BossRush":
                return bossRushResults;

            default:
                Debug.LogWarning($"Unknown mode: {mode}");
                return new List<GameResult>();
        }
    }

    // Save all results to PlayerPrefs (could be improved with JSON file saving)
    private void SaveAllResults()
    {
        // Save TimeAttack results
        SaveResultListToPrefs("TimeAttackEasy", timeAttackEasyResults);
        SaveResultListToPrefs("TimeAttackMedium", timeAttackMediumResults);
        SaveResultListToPrefs("TimeAttackHard", timeAttackHardResults);

        // Save Classic results
        SaveResultListToPrefs("ClassicEasy", classicEasyResults);
        SaveResultListToPrefs("ClassicMedium", classicMediumResults);
        SaveResultListToPrefs("ClassicHard", classicHardResults);

        // Save BossRush results
        SaveResultListToPrefs("BossRush", bossRushResults);

        // Save changes
        PlayerPrefs.Save();
    }

    // Helper method to serialize and save a list to PlayerPrefs
    private void SaveResultListToPrefs(string key, List<GameResult> results)
    {
        // Convert list to serialized string (simple format for demonstration)
        string serialized = "";
        foreach (var result in results)
        {
            string entry = $"{result.mode},{result.difficulty},{result.score},{result.correctAnswers},{result.totalQuestions},{result.dateTime},{result.stage}";
            serialized += entry + "|";
        }

        // Save to PlayerPrefs
        PlayerPrefs.SetString(key, serialized);
    }

    // Load all results from PlayerPrefs
    private void LoadAllResults()
    {
        // Load TimeAttack results
        timeAttackEasyResults = LoadResultListFromPrefs("TimeAttackEasy");
        timeAttackMediumResults = LoadResultListFromPrefs("TimeAttackMedium");
        timeAttackHardResults = LoadResultListFromPrefs("TimeAttackHard");

        // Load Classic results
        classicEasyResults = LoadResultListFromPrefs("ClassicEasy");
        classicMediumResults = LoadResultListFromPrefs("ClassicMedium");
        classicHardResults = LoadResultListFromPrefs("ClassicHard");

        // Load BossRush results
        bossRushResults = LoadResultListFromPrefs("BossRush");
    }

    // Helper method to deserialize and load a list from PlayerPrefs
    private List<GameResult> LoadResultListFromPrefs(string key)
    {
        List<GameResult> results = new List<GameResult>();

        string serialized = PlayerPrefs.GetString(key, "");
        if (string.IsNullOrEmpty(serialized))
            return results;

        string[] entries = serialized.Split('|');
        foreach (string entry in entries)
        {
            if (string.IsNullOrEmpty(entry))
                continue;

            string[] parts = entry.Split(',');
            if (parts.Length >= 7) // Updated to check for at least 7 parts
            {
                GameResult result = new GameResult
                {
                    mode = parts[0],
                    difficulty = parts[1],
                    score = int.Parse(parts[2]),
                    correctAnswers = int.Parse(parts[3]),
                    totalQuestions = int.Parse(parts[4]),
                    dateTime = parts[5],
                    stage = int.Parse(parts[6])
                };

                results.Add(result);
            }
        }

        return results;
    }

    // Reset results for a specific mode and difficulty
    public void ResetResults(string mode, string difficulty)
    {
        List<GameResult> targetList = GetResultList(mode, difficulty);
        targetList.Clear();
        SaveAllResults();
    }

    // Get results for a specific mode and difficulty
    public List<GameResult> GetResults(string mode, string difficulty)
    {
        return GetResultList(mode, difficulty);
    }
}