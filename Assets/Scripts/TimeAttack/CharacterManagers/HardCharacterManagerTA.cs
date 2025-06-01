using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HardCharacterManagerTA : MonoBehaviour
{
    [System.Serializable]
    public class CharacterData
    {
        public GameObject characterPrefab;
        public Sprite idleSprite;
        public Sprite happySprite;
        public Sprite sadSprite;
        public Sprite annoyedSprite;
        public string happyText = "Thank you for helping me!";
        public string sadText = "Oh no, that's not right...";
        public string annoyedText = "I don't have all day!";
    }

    [Header("Character Settings")]
    [SerializeField] private List<CharacterData> characters = new List<CharacterData>();
    [SerializeField] private Transform characterSpawnPoint;
    [SerializeField] private float characterTransitionTime = 1.5f;

    // Keep track of instantiated character objects
    private List<GameObject> characterInstances = new List<GameObject>();
    private TAMediumCharacterAnimator currentAnimator;
    private int currentCharacterIndex = -1;
    private bool isTransitioning = false;

    [Header("Shuffle Settings")]
    [SerializeField] private bool shuffleCharactersOnStart = true;

    // In MediumCharacterManagerTA.cs
    // Modify the Start() method to ensure the character appears immediately

    private void Start()
    {
        if (shuffleCharactersOnStart)
        {
            ShuffleCharacters();
        }

        // Instantiate all characters at start but keep them inactive
        PreInstantiateCharacters();

        // Register with quiz manager
        HardTimeAttackManager quizManager = FindFirstObjectByType<HardTimeAttackManager>();
        if (quizManager != null)
        {
            quizManager.OnQuizReset += ResetCharacters;
            quizManager.OnBeforeQuestionDisplayed += SpawnNextCharacter;
            quizManager.OnTimeUp += HandleTimeUp;

            // IMPORTANT: Don't call SpawnNextCharacter() directly here
            // Let the MediumTimeAttackManager trigger it through its DisplayCurrentQuestion method
        }
        else
        {
            Debug.LogError("Quiz Manager not found. Character system won't function properly.");
        }
    }

    // Add this new method
    public void HandleTimeUp()
    {
        if (currentCharacterIndex >= 0 && currentCharacterIndex < characterInstances.Count)
        {
            StartCoroutine(SlideCharacterAway(characterInstances[currentCharacterIndex]));
        }
    }

    // Add this coroutine to handle the slide-away animation
    private IEnumerator SlideCharacterAway(GameObject character)
    {
        if (character == null || !character.activeInHierarchy)
            yield break;

        // Get starting position (current position)
        Vector3 startPosition = character.transform.position;
        // Set target position to right of screen
        Vector3 targetPosition = startPosition + new Vector3(10f, 0f, 0f); // 10 units to the right

        // Slide animation
        float elapsedTime = 0;
        float slideDuration = 0.8f; // Adjust this to control slide speed

        while (elapsedTime < slideDuration)
        {
            elapsedTime += Time.deltaTime;
            float t = Mathf.Clamp01(elapsedTime / slideDuration);
            // Use an easing function for smoother animation (ease-in)
            float smoothT = t * t; // Quadratic ease-in
            character.transform.position = Vector3.Lerp(startPosition, targetPosition, smoothT);
            yield return null;
        }

        // Hide character after it's slid away
        character.SetActive(false);
    }

    private void PreInstantiateCharacters()
    {
        // Clear any existing instances
        foreach (var instance in characterInstances)
        {
            if (instance != null)
                instance.SetActive(false);
        }
        characterInstances.Clear();

        // Instantiate all characters but keep them inactive
        foreach (var character in characters)
        {
            GameObject characterObj = Instantiate(character.characterPrefab, characterSpawnPoint.position, Quaternion.identity);
            characterObj.SetActive(false);

            // Set up the character animator
            EasyCharacterAnimator animator = characterObj.GetComponent<EasyCharacterAnimator>();
            if (animator != null)
            {
                animator.SetSprites(
                    character.idleSprite,
                    character.happySprite,
                    character.sadSprite,
                    character.annoyedSprite
                );

                animator.SetSpeechTexts(
                    character.happyText,
                    character.sadText,
                    character.annoyedText
                );
            }

            characterInstances.Add(characterObj);
        }
    }

    private void ShuffleCharacters()
    {
        for (int i = 0; i < characters.Count; i++)
        {
            CharacterData temp = characters[i];
            int randomIndex = Random.Range(i, characters.Count);
            characters[i] = characters[randomIndex];
            characters[randomIndex] = temp;

            // Also shuffle the instantiated objects if they exist
            if (characterInstances.Count > i && characterInstances.Count > randomIndex)
            {
                GameObject tempObj = characterInstances[i];
                characterInstances[i] = characterInstances[randomIndex];
                characterInstances[randomIndex] = tempObj;
            }
        }
    }

    public void ResetCharacters()
    {
        // Deactivate all characters
        foreach (var instance in characterInstances)
        {
            if (instance != null)
                instance.SetActive(false);
        }

        currentCharacterIndex = -1;

        if (shuffleCharactersOnStart)
        {
            ShuffleCharacters();
        }
    }

    public void SpawnNextCharacter()
    {
        if (isTransitioning)
            return;

        StartCoroutine(TransitionToNextCharacter());
    }

    private IEnumerator TransitionToNextCharacter()
    {
        isTransitioning = true;

        // Hide current character with animation if it exists
        if (currentCharacterIndex >= 0 && currentCharacterIndex < characterInstances.Count)
        {
            GameObject currentChar = characterInstances[currentCharacterIndex];
            // Add any exit animation here if needed

            yield return new WaitForSeconds(characterTransitionTime);
            currentChar.SetActive(false);
        }

        // Move to next character or loop back to beginning
        currentCharacterIndex = (currentCharacterIndex + 1) % characters.Count;

        // Get the pre-instantiated character and activate it
        if (currentCharacterIndex < characterInstances.Count)
        {
            GameObject nextChar = characterInstances[currentCharacterIndex];

            // Get the character's original position (spawn point)
            Vector3 targetPosition = characterSpawnPoint.position;
            // Set initial position to the left of the spawn point
            Vector3 startPosition = targetPosition + new Vector3(-10f, 0f, 0f); // 10 units to the left
            nextChar.transform.position = startPosition;

            // Activate the GameObject BEFORE trying to animate it
            nextChar.SetActive(true);

            // Slide animation
            float elapsedTime = 0;
            float slideDuration = 0.8f; // Adjust this to control slide speed

            while (elapsedTime < slideDuration)
            {
                elapsedTime += Time.deltaTime;
                float t = Mathf.Clamp01(elapsedTime / slideDuration);
                // Use an easing function for smoother animation (ease-out)
                float smoothT = 1 - Mathf.Pow(1 - t, 2); // Quadratic ease-out
                nextChar.transform.position = Vector3.Lerp(startPosition, targetPosition, smoothT);
                yield return null;
            }

            // Ensure the character ends up exactly at the target position
            nextChar.transform.position = targetPosition;

            currentAnimator = nextChar.GetComponent<TAMediumCharacterAnimator>();

            // Optionally reset the character to idle state
            if (currentAnimator != null)
            {
                currentAnimator.ResetToIdle();
            }
        }

        // Add enter animation delay
        yield return new WaitForSeconds(0.5f);

        isTransitioning = false;
    }

    // Helper method to check if we have enough characters for all questions
    public bool HasEnoughCharacters(int questionCount)
    {
        return characters.Count >= questionCount;
    }

    public bool IsTransitioning()
    {
        return isTransitioning;
    }
}