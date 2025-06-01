using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class HistoryNavigationManager : MonoBehaviour
{
    [Header("Button References")]
    [SerializeField] private Button classicModeButton;
    [SerializeField] private Button bossRushButton;
    [SerializeField] private Button timeAttackButton;
    [SerializeField] private Button backButton;

    [Header("Scene Names")]
    [SerializeField] private string classicModeSceneName = "ClassicMode";
    [SerializeField] private string bossRushSceneName = "BossRushSurvival";
    [SerializeField] private string timeAttackSceneName = "TimeAttack";
    [SerializeField] private string mainMenuSceneName = "TitleScreen";

    private AudioManager audioManager;

    private void Start()
    {
        Debug.Log("Results scene starting");

        // Extra logging to debug manager access
        Debug.Log("AudioManager Instance is " + (AudioManager.Instance == null ? "NULL" : "available"));
        Debug.Log("GameHistoryManager Instance is " + (GameHistoryManager.Instance == null ? "NULL" : "available"));
        // Set up button listeners
        if (classicModeButton != null)
        {
            // Play button click sound if available
            AudioManager.Instance.PlayButtonClickSound();
            classicModeButton.onClick.AddListener(() => LoadScene(classicModeSceneName));
        }

        if (bossRushButton != null)
        {
            // Play button click sound if available
            AudioManager.Instance.PlayButtonClickSound();
            bossRushButton.onClick.AddListener(() => LoadScene(bossRushSceneName));
        }

        if (timeAttackButton != null)
        {
            // Play button click sound if available
            AudioManager.Instance.PlayButtonClickSound();
            timeAttackButton.onClick.AddListener(() => LoadScene(timeAttackSceneName));
        }

        if (backButton != null)
        {
            // Play button click sound if available
            AudioManager.Instance.PlayButtonClickSound();
            backButton.onClick.AddListener(() => LoadScene(mainMenuSceneName));
        }
    }

    private void LoadScene(string sceneName)
    {
        SceneManager.LoadScene(sceneName);
    }
}