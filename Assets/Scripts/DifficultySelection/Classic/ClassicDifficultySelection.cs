using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class ClassicDifficultySelection : MonoBehaviour
{
    [Header("Button References")]
    [SerializeField] private Button EasyDiffButton;
    [SerializeField] private Button MidDiffButton;
    [SerializeField] private Button HardDiffButton;
    [SerializeField] private Button backButton;

    [Header("Scene Names")]
    [SerializeField] private string EasyDiffSceneName = "ClassicEasyResults";
    [SerializeField] private string MidDiffSceneName = "ClassicMediumResults";
    [SerializeField] private string HardDiffSceneName = "ClassicHardResults";
    [SerializeField] private string mainMenuSceneName = "MainHistory";

    private void Start()
    {
        // Set up button listeners
        if (EasyDiffButton != null)
        {
            EasyDiffButton.onClick.AddListener(() => LoadScene(EasyDiffSceneName));
        }

        if (MidDiffButton != null)
        {
            MidDiffButton.onClick.AddListener(() => LoadScene(MidDiffSceneName));
        }

        if (HardDiffButton != null)
        {
            HardDiffButton.onClick.AddListener(() => LoadScene(HardDiffSceneName));
        }

        if (backButton != null)
        {
            backButton.onClick.AddListener(() => LoadScene(mainMenuSceneName));
        }
    }

    private void LoadScene(string sceneName)
    {
        SceneManager.LoadScene(sceneName);
    }
}