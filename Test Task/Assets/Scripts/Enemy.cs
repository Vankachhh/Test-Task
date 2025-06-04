using UnityEngine;
using UnityEngine.UI;

public class Enemy : MonoBehaviour
{
    [Header("Detection Settings")]
    [SerializeField] private float detectionRadius = 5f; // ������ ����������� ������
    [SerializeField] private float attackDistance = 1f; // ��������� ��� �����

    [Header("Movement Settings")]
    [SerializeField] private float moveSpeed = 2f; // �������� �������� ����

    [Header("Attack Settings")]
    [SerializeField] private float attackDelay = 1f; // �������� ����� �������
    [SerializeField] private int damage = 1; // ���� �� �����

    [Header("Health Settings")]
    [SerializeField] private int maxHealth = 4; // ������������ ��������
    [SerializeField] private int currentHealth; // ������� ��������
    [SerializeField] private Image healthBarImage; // ������ �� Image ����� ��������

    [Header("Drop Settings")]
    [SerializeField] private GameObject[] dropPrefabs; // ������� ��� ����� (gun_makarov_obj, gun_ak_obj, bullet_obj)

    private Transform player; // ������ �� ��������� ������
    private float lastAttackTime; // ����� ��������� �����
    private bool isPlayerInRange; // ����, ��������� �� ����� � �������

    private void Start()
    {
        // ������������� ��������
        currentHealth = maxHealth;

        // ������� Image �� �������� ������� � �������� 1
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

        // ������� ������
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

        // ��������� ������� ��� �����
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

        // ������������� ��������� �������� ����� ��������
        UpdateHealthBar();
    }

    private void Update()
    {
        if (player == null) return;

        // ��������� ���������� �� ������
        float distanceToPlayer = Vector2.Distance(transform.position, player.position);
        isPlayerInRange = distanceToPlayer <= detectionRadius;

        if (isPlayerInRange)
        {
            // �������� � ������, ���� �� ��� ��������� �����
            if (distanceToPlayer > attackDistance)
            {
                MoveTowardsPlayer();
            }
            // �����, ���� ����� � ������� �����
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

        // ��������� ����� ��������
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

        // ��������� ���� ������ ��������
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

        // ����������� ������ ����������� (��� � �������� ����)
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