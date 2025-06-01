using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Audio;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [Header("UI Sounds")]
    public AudioClip buttonClickSound;

    [Header("Title Screen")]
    public AudioClip titleScreenMusic;

    [Header("Classic Mode")]
    public AudioClip[] classicBackgroundMusic; // One for each stage
    public AudioClip classicCorrectAnswerSound;
    public AudioClip classicIncorrectAnswerSound;
    public AudioClip classicSuccessSound;
    public AudioClip classicFailSound;

    [Header("Time Attack Mode")]
    public AudioClip[] timeAttackBackgroundMusic; // One for each stage
    public AudioClip timeAttackCorrectAnswerSound;
    public AudioClip timeAttackIncorrectAnswerSound;
    public AudioClip timeAttackSuccessSound;
    public AudioClip timeAttackFailSound;
    public AudioClip timeAttackTickingSound;

    [Header("Boss Rush Mode")]
    public AudioClip[] bossRushBackgroundMusic; // Five stages
    public AudioClip bossRushCorrectAnswerSound;
    public AudioClip bossRushIncorrectAnswerSound;
    public AudioClip bossRushSuccessSound;
    public AudioClip bossRushFailSound;
    public AudioClip playerAttackSound;
    public AudioClip enemyAttackSound;
    public AudioClip playerHurtSound;
    public AudioClip enemyHurtSound;
    public AudioClip playerDeathSound;
    public AudioClip enemyDeathSound;

    private AudioSource musicSource; // For background music
    private AudioSource effectsSource; // For sound effects
    private AudioClip currentlyPlayingResultSound = null; // Track result sounds

    // To track current game mode and stage
    public enum GameMode { TitleScreen, Classic, TimeAttack, BossRush }
    private GameMode currentGameMode = GameMode.TitleScreen;
    private int currentStage = 0;

    void Awake()
    {
        // Singleton pattern
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);

            // Make sure we're at the root level for DontDestroyOnLoad to work
            if (transform.parent != null)
            {
                transform.SetParent(null);
            }

            // Setup audio sources
            SetupAudioSources();

            // Load and apply saved volume
            LoadAndApplyVolume();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void SetupAudioSources()
    {
        // Get or create music source
        musicSource = GetComponent<AudioSource>();
        if (musicSource == null)
        {
            musicSource = gameObject.AddComponent<AudioSource>();
        }

        // Set loop for background music
        musicSource.loop = true;

        // Create a separate source for effects
        effectsSource = gameObject.AddComponent<AudioSource>();
    }

    // Load volume from PlayerPrefs (used by your Settings controller)
    private void LoadAndApplyVolume()
    {
        float savedVolume = PlayerPrefs.GetFloat("MasterVolume", 1.0f);
        ApplyVolume(savedVolume);
    }

    // Apply volume to all audio sources
    public void ApplyVolume(float volume)
    {
        if (musicSource != null)
        {
            musicSource.volume = volume;
        }

        if (effectsSource != null)
        {
            effectsSource.volume = volume;
        }
    }

    // Save volume to PlayerPrefs (called from your Settings controller)
    public void SaveVolume(float volume)
    {
        PlayerPrefs.SetFloat("MasterVolume", volume);
        PlayerPrefs.Save();
        ApplyVolume(volume);
    }

    // Set the game mode and stage
    public void SetGameModeAndStage(GameMode mode, int stage)
    {
        currentGameMode = mode;
        currentStage = stage;

        // Play appropriate background music for the mode and stage
        PlayBackgroundMusicForCurrentMode();
    }

    // Increment the stage (keeping the same game mode)
    public void IncrementStage()
    {
        currentStage++;

        // Play appropriate background music for the mode and new stage
        PlayBackgroundMusicForCurrentMode();
    }

    // Play background music based on current mode and stage
    public void PlayBackgroundMusicForCurrentMode()
    {
        switch (currentGameMode)
        {
            case GameMode.TitleScreen:
                PlayBackgroundMusic(titleScreenMusic);
                break;

            case GameMode.Classic:
                if (classicBackgroundMusic != null && classicBackgroundMusic.Length > 0)
                {
                    int index = Mathf.Clamp(currentStage, 0, classicBackgroundMusic.Length - 1);
                    PlayBackgroundMusic(classicBackgroundMusic[index]);
                }
                break;

            case GameMode.TimeAttack:
                if (timeAttackBackgroundMusic != null && timeAttackBackgroundMusic.Length > 0)
                {
                    int index = Mathf.Clamp(currentStage, 0, timeAttackBackgroundMusic.Length - 1);
                    PlayBackgroundMusic(timeAttackBackgroundMusic[index]);
                }
                break;

            case GameMode.BossRush:
                if (bossRushBackgroundMusic != null && bossRushBackgroundMusic.Length > 0)
                {
                    int index = Mathf.Clamp(currentStage, 0, bossRushBackgroundMusic.Length - 1);
                    PlayBackgroundMusic(bossRushBackgroundMusic[index]);
                }
                break;
        }
    }

    // Play a specific background music clip
    private void PlayBackgroundMusic(AudioClip music)
    {
        if (music == null)
        {
            Debug.LogWarning("Attempted to play null background music clip");
            return;
        }

        // Always play the music, even if it's the same clip
        musicSource.clip = music;
        musicSource.Play();
    }

    // Stop background music
    public void StopBackgroundMusic()
    {
        if (musicSource != null && musicSource.isPlaying)
        {
            musicSource.Stop();
        }
    }

    // Stop result sound and restart background music
    public void StopResultSoundAndRestartMusic()
    {
        // Stop any sound effects playing
        if (effectsSource != null)
        {
            effectsSource.Stop();
            currentlyPlayingResultSound = null;
        }

        // Restart the background music for the current stage/mode
        PlayBackgroundMusicForCurrentMode();
    }

    // UI Sound Methods
    public void PlayButtonClickSound()
    {
        PlayEffect(buttonClickSound);
    }

    // Classic Mode Sound Methods
    public void PlayClassicCorrectSound()
    {
        PlayEffect(classicCorrectAnswerSound);
    }

    public void PlayClassicIncorrectSound()
    {
        PlayEffect(classicIncorrectAnswerSound);
    }

    public void PlayClassicSuccessSound()
    {
        PlayEffect(classicSuccessSound);
        currentlyPlayingResultSound = classicSuccessSound;
    }

    public void PlayClassicFailSound()
    {
        PlayEffect(classicFailSound);
        currentlyPlayingResultSound = classicFailSound;
    }

    // Time Attack Mode Sound Methods
    public void PlayTimeAttackCorrectSound()
    {
        PlayEffect(timeAttackCorrectAnswerSound);
    }

    public void PlayTimeAttackIncorrectSound()
    {
        PlayEffect(timeAttackIncorrectAnswerSound);
    }

    public void PlayTimeAttackSuccessSound()
    {
        PlayEffect(timeAttackSuccessSound);
        currentlyPlayingResultSound = timeAttackSuccessSound;
    }

    public void PlayTimeAttackFailSound()
    {
        PlayEffect(timeAttackFailSound);
        currentlyPlayingResultSound = timeAttackFailSound;
    }

    public void PlayTimeAttackTickingSound()
    {
        PlayEffect(timeAttackTickingSound);
    }

    // Boss Rush Mode Sound Methods
    public void PlayBossRushCorrectSound()
    {
        PlayEffect(bossRushCorrectAnswerSound);
    }

    public void PlayBossRushIncorrectSound()
    {
        PlayEffect(bossRushIncorrectAnswerSound);
    }

    public void PlayBossRushSuccessSound()
    {
        PlayEffect(bossRushSuccessSound);
        currentlyPlayingResultSound = bossRushSuccessSound;
    }

    public void PlayBossRushFailSound()
    {
        PlayEffect(bossRushFailSound);
        currentlyPlayingResultSound = bossRushFailSound;
    }

    public void PlayPlayerAttackSound()
    {
        PlayEffect(playerAttackSound);
    }

    public void PlayEnemyAttackSound()
    {
        PlayEffect(enemyAttackSound);
    }

    public void PlayPlayerHurtSound()
    {
        PlayEffect(playerHurtSound);
    }

    public void PlayEnemyHurtSound()
    {
        PlayEffect(enemyHurtSound);
    }

    public void PlayPlayerDeathSound()
    {
        PlayEffect(playerDeathSound);
    }

    public void PlayEnemyDeathSound()
    {
        PlayEffect(enemyDeathSound);
    }

    // Generic method to play a sound effect
    private void PlayEffect(AudioClip clip)
    {
        if (clip == null || effectsSource == null)
        {
            return;
        }

        effectsSource.clip = clip;
        effectsSource.Play();
    }

    // Add this method to your AudioManager class
    public void ResetToBossRushStage1()
    {
        // Stop any current sounds
        StopAllSounds();

        // Set game mode to Boss Rush and stage to 0 (first stage)
        currentGameMode = GameMode.BossRush;
        currentStage = 0;

        // Play the Stage 1 background music
        PlayBackgroundMusicForCurrentMode();
        Debug.Log("Reset to Boss Rush Stage 1");
    }

    // Stop all sounds (both music and effects)
    public void StopAllSounds()
    {
        // Stop background music
        StopBackgroundMusic();

        // Stop any sound effects playing
        if (effectsSource != null)
        {
            effectsSource.Stop();
            currentlyPlayingResultSound = null;
        }
    }
}