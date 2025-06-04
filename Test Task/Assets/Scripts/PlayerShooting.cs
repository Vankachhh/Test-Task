using UnityEngine;
using UnityEngine.UI;

public class PlayerShooting : MonoBehaviour
{
    [SerializeField] private float shootRange = 5f; // Радиус поражения
    [SerializeField] private int shootDamage = 1; // Урон от выстрела
    [SerializeField] private Button shootButton; // Ссылка на UI-кнопку
    [SerializeField] private AudioClip shootSound; // Звук выстрела
    [SerializeField] private AudioClip noAmmoSound; // Звук при отсутствии патронов
    [SerializeField] private AudioSource audioSource; // Компонент AudioSource

    private DataSaver dataSaver; // Ссылка на DataSaver

    private void Start()
    {
        // Находим DataSaver
        dataSaver = DataSaver.Instance;
        if (dataSaver == null)
        {
            Debug.LogError("DataSaver instance not found!");
        }
        else
        {
            Debug.Log("DataSaver found successfully.");
        }

        // Находим AudioSource
        if (audioSource == null)
        {
            audioSource = GetComponent<AudioSource>();
            if (audioSource == null)
            {
                Debug.LogWarning("No AudioSource found on PlayerShooting, adding one.");
                audioSource = gameObject.AddComponent<AudioSource>();
            }
        }

        // Проверяем, назначена ли кнопка
        if (shootButton != null)
        {
            shootButton.onClick.AddListener(Shoot);
            Debug.Log("Shoot button assigned.");
        }
        else
        {
            Debug.LogWarning("Shoot Button not assigned in PlayerShooting!");
        }
    }

    private void Shoot()
    {
        Debug.Log("Shoot button pressed.");

        if (dataSaver == null)
        {
            Debug.LogError("Cannot shoot: DataSaver not found!");
            return;
        }

        // Проверяем наличие активного оружия
        string activeWeaponId = dataSaver.GetActiveWeaponId();
        if (string.IsNullOrEmpty(activeWeaponId) || (activeWeaponId != "gun_makarov" && activeWeaponId != "gun_ak"))
        {
            Debug.Log("No action taken: No active weapon equipped!");
            return;
        }

        // Проверяем наличие патронов и проигрываем соответствующий звук
        if (dataSaver.SpendBullets(1))
        {
            if (audioSource != null && shootSound != null)
            {
                audioSource.PlayOneShot(shootSound);
                Debug.Log("Played shoot sound.");
            }
            else
            {
                Debug.LogWarning("Shoot sound or AudioSource is not assigned!");
            }

            // Находим всех монстров в радиусе
            Enemy[] enemies = FindObjectsOfType<Enemy>();
            Enemy closestEnemy = null;
            float closestDistance = float.MaxValue;

            foreach (Enemy enemy in enemies)
            {
                if (enemy == null) continue;

                float distanceToEnemy = Vector2.Distance(transform.position, enemy.transform.position);
                if (distanceToEnemy <= shootRange && distanceToEnemy < closestDistance)
                {
                    closestEnemy = enemy;
                    closestDistance = distanceToEnemy;
                }
            }

            // Наносим урон ближайшему монстру, если он есть
            if (closestEnemy != null)
            {
                closestEnemy.TakeDamage(shootDamage);
                Debug.Log($"Player shot {closestEnemy.name}, dealt {shootDamage} damage. Distance: {closestDistance}");
            }
            else
            {
                Debug.Log("No enemy within shoot range!");
            }
        }
        else
        {
            if (audioSource != null && noAmmoSound != null)
            {
                audioSource.PlayOneShot(noAmmoSound);
                Debug.Log("Played no ammo sound.");
            }
            else
            {
                Debug.LogWarning("No ammo sound or AudioSource is not assigned!");
            }
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, shootRange);
    }
}