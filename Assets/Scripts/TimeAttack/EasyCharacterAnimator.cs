using UnityEngine;
using TMPro;
using System.Collections;

public class EasyCharacterAnimator : MonoBehaviour
{
    [Header("Character Sprites")]
    [SerializeField] private SpriteRenderer characterRenderer;
    [SerializeField] private Sprite idleSprite;
    [SerializeField] private Sprite happySprite;    // Previously correctAnswerSprite
    [SerializeField] private Sprite sadSprite;      // Previously wrongAnswerSprite
    [SerializeField] private Sprite annoyedSprite;  // New sprite for time-up

    [Header("Speech Bubble")]
    [SerializeField] private GameObject speechBubbleObject;
    [SerializeField] private string happyText = "That's right! Great job!";      // Previously correctAnswerText
    [SerializeField] private string sadText = "Oops! Try to remember this one."; // Previously wrongAnswerText
    [SerializeField] private string annoyedText = "Time's up! You're too slow!"; // Previously timeUpText
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
        TimeAttackManager quizManager = FindFirstObjectByType<TimeAttackManager>();
        if (quizManager != null)
        {
            Debug.Log("Quiz Manager found, registering for events");
            // Subscribe to events
            quizManager.OnCorrectAnswer += PlayHappyAnimation;
            quizManager.OnWrongAnswer += PlaySadAnimation;
            quizManager.OnQuizReset += ResetToIdle;
            quizManager.OnTimeUp += PlayAnnoyedAnimation; // Make sure this line exists
            Debug.Log("Event registration complete");
        }
        else
        {
            Debug.LogError("Quiz Manager not found. Character animations won't be triggered automatically.");
        }
    }

    // Public methods to set sprites at runtime
    public void SetSprites(Sprite idle, Sprite happy, Sprite sad, Sprite annoyed)
    {
        idleSprite = idle;
        happySprite = happy;
        sadSprite = sad;
        annoyedSprite = annoyed;

        // Set the current sprite to idle
        if (characterRenderer != null && idleSprite != null)
            characterRenderer.sprite = idleSprite;
    }

    // Public method to set speech texts at runtime
    public void SetSpeechTexts(string happy, string sad, string annoyed)
    {
        happyText = happy;
        sadText = sad;
        annoyedText = annoyed;
    }

    public void PlayHappyAnimation()
    {
        Debug.Log("PlayHappyAnimation called");
        if (currentAnimationCoroutine != null)
            StopCoroutine(currentAnimationCoroutine);

        currentAnimationCoroutine = StartCoroutine(PlayAnimationSequence(happySprite, happyText));
    }

    public void PlaySadAnimation()
    {
        Debug.Log("PlaySadAnimation called");
        if (currentAnimationCoroutine != null)
            StopCoroutine(currentAnimationCoroutine);

        currentAnimationCoroutine = StartCoroutine(PlayAnimationSequence(sadSprite, sadText));
    }

    public void PlayAnnoyedAnimation()
    {
        Debug.Log("PlayAnnoyedAnimation called");
        if (currentAnimationCoroutine != null)
            StopCoroutine(currentAnimationCoroutine);

        currentAnimationCoroutine = StartCoroutine(PlayAnimationSequence(annoyedSprite, annoyedText));
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
        // Add a check to prevent execution if being destroyed
        if (this == null || !gameObject || !gameObject.activeInHierarchy)
        {
            yield break;
        }

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