using UnityEngine;
using UnityEngine.UI;

public class Enemy : MonoBehaviour
{
    [Header("Detection Settings")]
    [SerializeField] private float detectionRadius = 5f; // Радиус обнаружения игрока
    [SerializeField] private float attackDistance = 1f; // Дистанция для атаки

    [Header("Movement Settings")]
    [SerializeField] private float moveSpeed = 2f; // Скорость движения моба

    [Header("Attack Settings")]
    [SerializeField] private float attackDelay = 1f; // Задержка между атаками
    [SerializeField] private int damage = 1; // Урон за атаку

    [Header("Health Settings")]
    [SerializeField] private int maxHealth = 4; // Максимальное здоровье
    [SerializeField] private int currentHealth; // Текущее здоровье
    [SerializeField] private Image healthBarImage; // Ссылка на Image шкалы здоровья

    [Header("Drop Settings")]
    [SerializeField] private GameObject[] dropPrefabs; // Префабы для дропа (gun_makarov_obj, gun_ak_obj, bullet_obj)

    private Transform player; // Ссылка на трансформ игрока
    private float lastAttackTime; // Время последней атаки
    private bool isPlayerInRange; // Флаг, находится ли игрок в радиусе

    private void Start()
    {
        // Инициализация здоровья
        currentHealth = maxHealth;

        // Находим Image на дочернем объекте с индексом 1
        if (transform.childCount > 1)
        {
            healthBarImage = transform.GetChild(1).GetComponent<Image>();
            if (healthBarImage != null)
            {
                if (healthBarImage.type != Image.Type.Filled)
                {
                    Debug.LogWarning($"Image Type on {gameObject.name} is not set to Filled! Setting it to Filled.");
                    healthBarImage.type = Image.Type.Filled;
                    healthBarImage.fillMethod = Image.FillMethod.Horizontal;
                    healthBarImage.fillOrigin = 0;
                }
                Debug.Log($"Health bar Image found for {gameObject.name}.");
            }
            else
            {
                Debug.LogWarning($"Image component not found on child object with index 1 for {gameObject.name}!");
            }
        }
        else
        {
            Debug.LogWarning($"Enemy {gameObject.name} does not have a child object with index 1!");
        }

        // Находим игрока
        GameObject playerObject = GameObject.Find("Player");
        if (playerObject != null)
        {
            player = playerObject.transform;
            Debug.Log($"Player found by {gameObject.name}.");
        }
        else
        {
            Debug.LogWarning("Player not found! Ensure GameObject named 'Player' exists in the scene.");
            enabled = false;
        }

        // Проверяем префабы для дропа
        if (dropPrefabs == null || dropPrefabs.Length == 0)
        {
            Debug.LogWarning($"Drop prefabs not assigned for {gameObject.name}. Attempting to load defaults.");
            dropPrefabs = new GameObject[]
            {
                Resources.Load<GameObject>("Prefabs/gun_makarov_obj"),
                Resources.Load<GameObject>("Prefabs/gun_ak_obj"),
                Resources.Load<GameObject>("Prefabs/bullet_obj")
            };
        }

        // Устанавливаем начальное значение шкалы здоровья
        UpdateHealthBar();
    }

    private void Update()
    {
        if (player == null) return;

        // Проверяем расстояние до игрока
        float distanceToPlayer = Vector2.Distance(transform.position, player.position);
        isPlayerInRange = distanceToPlayer <= detectionRadius;

        if (isPlayerInRange)
        {
            // Движение к игроку, если он вне дистанции атаки
            if (distanceToPlayer > attackDistance)
            {
                MoveTowardsPlayer();
            }
            // Атака, если игрок в радиусе атаки
            else if (Time.time >= lastAttackTime + attackDelay)
            {
                Attack();
            }
        }
    }

    private void MoveTowardsPlayer()
    {
        Vector2 direction = (player.position - transform.position).normalized;
        transform.position += (Vector3)direction * moveSpeed * Time.deltaTime;
    }

    private void Attack()
    {
        float distanceToPlayer = Vector2.Distance(transform.position, player.position);
        if (distanceToPlayer <= attackDistance && Time.time >= lastAttackTime + attackDelay)
        {
            Player playerScript = player.GetComponent<Player>();
            if (playerScript != null)
            {
                playerScript.TakeDamage(damage);
                Debug.Log($"Enemy attacked Player, dealt {damage} damage.");
            }
            lastAttackTime = Time.time;
        }
    }

    public void TakeDamage(int damage)
    {
        currentHealth = Mathf.Max(0, currentHealth - damage);
        Debug.Log($"{gameObject.name} took {damage} damage. Health: {currentHealth}/{maxHealth}");

        // Обновляем шкалу здоровья
        UpdateHealthBar();

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    private void UpdateHealthBar()
    {
        if (healthBarImage != null)
        {
            healthBarImage.fillAmount = (float)currentHealth / maxHealth;
            Debug.Log($"{gameObject.name} health bar fillAmount updated to: {healthBarImage.fillAmount}");
        }
    }

    private void Die()
    {
        Debug.Log($"{gameObject.name} Died!");

        // Случайный дроп одного предмета
        if (dropPrefabs != null && dropPrefabs.Length > 0)
        {
            int randomIndex = Random.Range(0, dropPrefabs.Length);
            GameObject selectedPrefab = dropPrefabs[randomIndex];

            if (selectedPrefab != null)
            {
                Instantiate(selectedPrefab, transform.position, Quaternion.identity);
                Debug.Log($"Dropped {selectedPrefab.name} from {gameObject.name}");
            }
            else
            {
                Debug.LogWarning($"Selected drop prefab at index {randomIndex} is null for {gameObject.name}");
            }
        }
        else
        {
            Debug.LogWarning($"No drop prefabs available for {gameObject.name}");
        }

        // Деактивация вместо уничтожения (как в исходном коде)
        gameObject.SetActive(false);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, attackDistance);
    }
}