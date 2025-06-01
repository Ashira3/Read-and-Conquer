using UnityEngine;
using TMPro;
using System.Collections;

public class SpeechBubble : MonoBehaviour
{
    [SerializeField] private TMP_Text bubbleText;
    [SerializeField] private float displayDuration = 3f;

    private Coroutine activeCoroutine;

    private void Awake()
    {
        // Make sure the bubble is hidden initially
        gameObject.SetActive(false);
    }

    public void ShowBubble(string text)
    {
        // Set the text
        if (bubbleText != null)
            bubbleText.text = text;

        // Show the bubble
        gameObject.SetActive(true);

        // Cancel any existing display coroutine
        if (activeCoroutine != null)
            StopCoroutine(activeCoroutine);

        // Start a new display coroutine
        activeCoroutine = StartCoroutine(HideBubbleAfterDelay());
    }

    private IEnumerator HideBubbleAfterDelay()
    {
        // Wait for the specified duration
        yield return new WaitForSeconds(displayDuration);

        // Hide the bubble
        gameObject.SetActive(false);
        activeCoroutine = null;
    }

    // Manually hide the bubble if needed
    public void HideBubble()
    {
        if (activeCoroutine != null)
            StopCoroutine(activeCoroutine);

        gameObject.SetActive(false);
        activeCoroutine = null;
    }
}