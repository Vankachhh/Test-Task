using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameOverUI : MonoBehaviour
{
    [SerializeField] private Button restartButton; // Кнопка рестарта

    void Start()
    {
        if (restartButton != null)
        {
            restartButton.onClick.AddListener(RestartScene);
            Debug.Log("Restart button assigned.");
        }
        else
        {
            Debug.LogError("Restart button not assigned in GameOverUI!");
        }
    }

    private void RestartScene()
    {
        Debug.Log("Restarting scene...");
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}