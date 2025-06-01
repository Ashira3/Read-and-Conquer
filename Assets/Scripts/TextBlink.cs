using UnityEngine;
using TMPro;

public class TextBlink : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI tmpText;  // Drag the TMP component here in Inspector
    public float blinkSpeed = 0.5f;

    void Start()
    {
        // If not assigned in Inspector, try to get it from this GameObject
        if (tmpText == null)
        {
            tmpText = GetComponent<TextMeshProUGUI>();
        }

        // Extra check to debug
        if (tmpText == null)
        {
            Debug.LogError("TextMeshProUGUI component not found!");
        }
    }

    void Update()
    {
        if (tmpText != null)
        {
            tmpText.alpha = Mathf.Sin(Time.time * blinkSpeed * Mathf.PI) > 0 ? 1 : 0;
        }
    }
}