using UnityEngine;
using TMPro;
using System.Collections;

public class CharacterAnimator : MonoBehaviour
{
    [Header("Character Sprites")]
    [SerializeField] private SpriteRenderer characterRenderer;
    [SerializeField] private Sprite idleSprite;
    [SerializeField] private Sprite correctAnswerSprite;
    [SerializeField] private Sprite wrongAnswerSprite;

    [Header("Speech Bubble")]
    [SerializeField] private GameObject speechBubbleObject; // The GameObject containing the SpeechBubble component
    [SerializeField] private string correctAnswerText = "That's right! Great job!";
    [SerializeField] private string wrongAnswerText = "Oops! Try to remember this one.";
    [SerializeField] private float spriteDisplayDuration = 2f;

    // Reference to our speech bubble component
    private SpeechBubble speechBubble;
    private Coroutine currentAnimationCoroutine;

    private void Awake()
    {
        // Get the SpeechBubble component if available
        if (speechBubbleObject != null)
            speechBubble = speechBubbleObject.GetComponent<SpeechBubble>();
    }

    private void Start()
    {
        // Set default idle sprite
        if (characterRenderer != null && idleSprite != null)
            characterRenderer.sprite = idleSprite;

        // Register with the quiz manager
        ClassicModeManager quizManager = FindFirstObjectByType<ClassicModeManager>();
        if (quizManager != null)
        {
            Debug.Log("Quiz Manager found, registering for events");
            // Subscribe to events
            quizManager.OnCorrectAnswer += PlayCorrectAnimation;
            quizManager.OnWrongAnswer += PlayWrongAnimation;
            quizManager.OnQuizReset += ResetToIdle;
            Debug.Log("Event registration complete");
        }
        else
        {
            Debug.LogError("Quiz Manager not found. Character animations won't be triggered automatically.");
        }
    }

    public void PlayCorrectAnimation()
    {
        Debug.Log("PlayCorrectAnimation called");
        if (currentAnimationCoroutine != null)
            StopCoroutine(currentAnimationCoroutine);

        currentAnimationCoroutine = StartCoroutine(PlayAnimationSequence(correctAnswerSprite, correctAnswerText));
    }

    public void PlayWrongAnimation()
    {
        Debug.Log("PlayWrongAnimation called");
        if (currentAnimationCoroutine != null)
            StopCoroutine(currentAnimationCoroutine);

        currentAnimationCoroutine = StartCoroutine(PlayAnimationSequence(wrongAnswerSprite, wrongAnswerText));
    }

    public void ResetToIdle()
    {
        if (currentAnimationCoroutine != null)
            StopCoroutine(currentAnimationCoroutine);

        if (characterRenderer != null && idleSprite != null)
            characterRenderer.sprite = idleSprite;

        // Hide speech bubble if it's visible
        if (speechBubble != null)
            speechBubble.HideBubble();
    }

    private IEnumerator PlayAnimationSequence(Sprite reactionSprite, string message)
    {
        Debug.Log($"Playing animation with message: {message}");

        // Change character sprite
        if (characterRenderer != null && reactionSprite != null)
        {
            characterRenderer.sprite = reactionSprite;
            Debug.Log($"Changed sprite to reaction sprite");
        }

        // Show speech bubble with text
        if (speechBubble != null)
        {
            Debug.Log("Speech bubble component found, showing bubble");
            speechBubble.ShowBubble(message);
        }
        else
        {
            Debug.LogError("Speech bubble component is null!");
        }

        // Wait for the sprite display duration
        yield return new WaitForSeconds(spriteDisplayDuration);

        // Return to idle state
        if (characterRenderer != null && idleSprite != null)
            characterRenderer.sprite = idleSprite;

        // Note: We don't need to hide the speech bubble here as it's handled by the SpeechBubble component
    }

    // Add this method for testing in the Editor or through a UI button
    public void TestSpeechBubble()
    {
        Debug.Log("Testing speech bubble");
        if (speechBubble != null)
        {
            speechBubble.ShowBubble("Test speech bubble message");
            Debug.Log("Speech bubble test initiated");
        }
        else
        {
            Debug.LogError("Speech bubble component not found for testing");
        }
    }
}