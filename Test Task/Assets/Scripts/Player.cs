using UnityEngine;
using UnityEngine.UI;

public class Player : MonoBehaviour
{
    [Header("Health Settings")]
    [SerializeField] private int maxHealth = 10; // Максимальное здоровье игрока
    [SerializeField] private Image healthBarImage; // UI Image для HP-бара
    private int currentHealth; // Текущее здоровье

    [Header("UI")]
    [SerializeField] private GameObject gameOverPanel; // Панель поражения

    void Start()
    {
        currentHealth = maxHealth;
        if (healthBarImage != null)
        {
            healthBarImage.fillAmount = 1f;
            Debug.Log("Player health bar initialized.");
        }
        else
        {
            Debug.LogError("Health bar image not assigned in Player!");
        }

        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(false);
            Debug.Log("Game Over panel initialized as inactive.");
        }
        else
        {
            Debug.LogError("Game Over panel not assigned in Player!");
        }
    }

    public void TakeDamage(int damage)
    {
        currentHealth = Mathf.Max(0, currentHealth - damage);
        Debug.Log($"Player took {damage} damage. Health: {currentHealth}/{maxHealth}");

        // Обновляем HP-бар
        if (healthBarImage != null)
        {
            healthBarImage.fillAmount = (float)currentHealth / maxHealth;
            Debug.Log($"Player health bar updated: {healthBarImage.fillAmount}");
        }

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    private void Die()
    {
        Debug.Log("Player Died!");
        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(true);
            Debug.Log("Game Over panel activated.");
        }
        else
        {
            Debug.LogError("Game Over panel not found during player death!");
        }
    }
}