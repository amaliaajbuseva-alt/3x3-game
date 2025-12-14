using UnityEngine;
using UnityEngine.SceneManagement;

public class AudioManager : MonoBehaviour
{
    public static AudioManager instance;

    public AudioSource musicSource;
    public AudioSource sfxSource;

    public AudioClip menuMusic;
    public AudioClip gameMusic;
    public AudioClip swapSound;
    public AudioClip matchSound;
    public AudioClip gameOverSound;

    private float originalMusicVolume;
    private bool isFading = false;

    void Awake()
    {
        // Singleton паттерн
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);

            // Сохраняем оригинальную громкость
            originalMusicVolume = musicSource.volume;

            // Подписываемся на событие смены сцены
            SceneManager.sceneLoaded += OnSceneLoaded;
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // Меняем музыку в зависимости от сцены
        if (scene.name == "MainMenu")
        {
            PlayMusic(menuMusic);
        }
        else if (scene.name == "GameScene")
        {
            PlayMusic(gameMusic);
        }
    }

    void PlayMusic(AudioClip clip)
    {
        if (clip == null || musicSource == null) return;

        if (musicSource.clip == clip && musicSource.isPlaying) return;

        musicSource.clip = clip;
        musicSource.loop = true;
        musicSource.Play();
    }

    public void PlaySwapSound()
    {
        PlaySFX(swapSound);
    }

    public void PlayMatchSound()
    {
        PlaySFX(matchSound);
    }

    public void PlayGameOverSound()
    {
        // Приглушаем фоновую музыку
        StartCoroutine(FadeMusic(0.2f));

        // Проигрываем звук Game Over
        PlaySFX(gameOverSound);

        // Через 3 секунды возвращаем громкость (если игра перезапустится)
        Invoke("RestoreMusicVolume", 3f);
    }

    void PlaySFX(AudioClip clip)
    {
        if (clip != null && sfxSource != null)
        {
            sfxSource.PlayOneShot(clip);
        }
    }

    // Плавное уменьшение громкости музыки
    System.Collections.IEnumerator FadeMusic(float targetVolume)
    {
        if (isFading) yield break;

        isFading = true;
        float startVolume = musicSource.volume;
        float duration = 0.5f;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            musicSource.volume = Mathf.Lerp(startVolume, targetVolume, elapsed / duration);
            elapsed += Time.deltaTime;
            yield return null;
        }

        musicSource.volume = targetVolume;
        isFading = false;
    }

    void RestoreMusicVolume()
    {
        StartCoroutine(FadeMusic(originalMusicVolume));
    }

    // Контроль громкости (можно вызвать из настроек)
    public void SetMusicVolume(float volume)
    {
        originalMusicVolume = volume;
        if (!isFading)
        {
            musicSource.volume = volume;
        }
    }

    public void SetSFXVolume(float volume)
    {
        sfxSource.volume = volume;
    }

    // Для титров или паузы
    public void PauseMusic()
    {
        if (musicSource.isPlaying)
            musicSource.Pause();
    }

    public void ResumeMusic()
    {
        if (!musicSource.isPlaying)
            musicSource.Play();
    }
}