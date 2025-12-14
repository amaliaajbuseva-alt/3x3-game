using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using UnityEngine.UI;

public class MainMenuManager : MonoBehaviour
{
    public Button startButton;
    public Button quitButton;

    void Start()
    {
        // Подключаем кнопки
        if (startButton != null)
            startButton.onClick.AddListener(StartGame);

        if (quitButton != null)
            quitButton.onClick.AddListener(QuitGame);

        Debug.Log("Главное меню загружено");
    }

    void StartGame()
    {
        Debug.Log("Загружаем игровую сцену...");
        SceneManager.LoadScene("MainScene"); // Твоя игровая сцена должна так называться
    }

    void QuitGame()
    {
        Debug.Log("Выход из игры");

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}