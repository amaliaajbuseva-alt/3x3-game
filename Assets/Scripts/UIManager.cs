using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    public GameObject gameOverMenu;
    public TextMeshProUGUI finalScoreText;
    public Button restartButtonGO;
    public Button mainMenuButtonGO;
    public Button quitButtonGO;
    public TextMeshProUGUI scoreText;
    public TextMeshProUGUI movesText;
    public GameObject pauseMenu;
    public Button pauseButton;
    public Button continueButton;
    public Button restartButton;
    public Button quitButton;

    private int score = 0;
    private int movesLeft = 20;
    private GameBoard gameBoard;
    private bool isPaused = false;

    void Start()
    {
        gameBoard = FindObjectOfType<GameBoard>();

        // Назначаем обработчики кнопок
        if (pauseButton != null)
            pauseButton.onClick.AddListener(TogglePause);

        if (continueButton != null)
            continueButton.onClick.AddListener(TogglePause);

        if (restartButton != null)
            restartButton.onClick.AddListener(RestartGame);

        if (quitButton != null)
            quitButton.onClick.AddListener(QuitGame);

        // Подключаем Game Over кнопки
        if (restartButtonGO != null)
            restartButtonGO.onClick.AddListener(RestartGame);

        if (mainMenuButtonGO != null)
            mainMenuButtonGO.onClick.AddListener(LoadMainMenu);

        if (quitButtonGO != null)
            quitButtonGO.onClick.AddListener(QuitGame);

        // Скрываем Game Over меню
        if (gameOverMenu != null)
            gameOverMenu.SetActive(false);

        // Скрываем меню паузы
        if (pauseMenu != null)
            pauseMenu.SetActive(false);

        UpdateUI();
    }

    void Update()
    {
        // Клавиша Escape для паузы
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            TogglePause();
        }
    }

    public void AddScore(int points)
    {
        score += points;
        UpdateUI();

        // Сохраняем лучший счёт
        if (score > PlayerPrefs.GetInt("HighScore", 0))
        {
            PlayerPrefs.SetInt("HighScore", score);
            Debug.Log($"Новый рекорд: {score} очков!");
        }
    }

    public void UseMove()
    {
        Debug.Log($"=== UseMove() вызван ===");
        Debug.Log($"До: movesLeft = {movesLeft}");

        if (movesLeft > 0)
        {
            movesLeft--;
            Debug.Log($"После: movesLeft = {movesLeft}");
            UpdateUI();

            if (movesLeft <= 0)
            {
                Debug.Log("GameOver! Ходы закончились");
                GameOver();
            }
        }
        else
        {
            Debug.LogWarning("Попытка использовать ход, когда movesLeft <= 0");
        }
    }

    void UpdateUI()
    {
        Debug.Log($"=== UpdateUI() вызван ===");
        Debug.Log($"score = {score}, movesLeft = {movesLeft}");

        if (scoreText != null)
        {
            scoreText.text = "Очки: " + score;
            Debug.Log($"scoreText установлен: {scoreText.text}");
        }
        else
        {
            Debug.LogError("scoreText равен null!");
        }

        if (movesText != null)
        {
            movesText.text = "Ходов: " + movesLeft;
            Debug.Log($"movesText установлен: {movesText.text}");
        }
        else
        {
            Debug.LogError("movesText равен null!");
        }
    }

    public void TogglePause()
    {
        isPaused = !isPaused;

        if (pauseMenu != null)
            pauseMenu.SetActive(isPaused);

        Time.timeScale = isPaused ? 0f : 1f;

        // Блокируем взаимодействие с игрой при паузе
        if (gameBoard != null)
        {
            // Можно добавить флаг блокировки в GameBoard
        }

        Debug.Log(isPaused ? "Игра на паузе" : "Игра продолжается");
    }

    public void RestartGame()
    {
        Time.timeScale = 1f; // Сбрасываем паузу
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void QuitGame()
    {
        // Сохраняем прогресс перед выходом
        PlayerPrefs.Save();

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    void GameOver()
    {


        if (AudioManager.instance != null)
        {
            AudioManager.instance.PlayGameOverSound();
        }

        Debug.Log("=== GAME OVER ===");
        Debug.Log($"Итоговый счёт: {score}");

        // Показываем Game Over меню
        if (gameOverMenu != null)
        {
            gameOverMenu.SetActive(true);

            // Обновляем текст с итоговым счётом
            if (finalScoreText != null)
            {
                finalScoreText.text = "Итоговый счёт: " + score;
            }

            // Останавливаем время (пауза)
            Time.timeScale = 0f;

            // Блокируем игровые взаимодействия
            if (pauseButton != null)
                pauseButton.interactable = false;
        }
        else
        {
            Debug.LogError("GameOverMenu не назначен!");
        }
    }

    public void LoadMainMenu()
    {
        Time.timeScale = 1f; // Сбрасываем паузу
        SceneManager.LoadScene("MainMenu");
    }

    // Метод для получения текущего счёта (для сохранения)
    public int GetCurrentScore()
    {
        return score;
    }

    // Метод для получения оставшихся ходов
    public int GetRemainingMoves()
    {
        return movesLeft;
    }
}