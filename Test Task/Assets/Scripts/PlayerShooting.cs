using UnityEngine;
using UnityEngine.UI;

public class PlayerShooting : MonoBehaviour
{
    [SerializeField] private float shootRange = 5f; // ������ ���������
    [SerializeField] private int shootDamage = 1; // ���� �� ��������
    [SerializeField] private Button shootButton; // ������ �� UI-������
    [SerializeField] private AudioClip shootSound; // ���� ��������
    [SerializeField] private AudioClip noAmmoSound; // ���� ��� ���������� ��������
    [SerializeField] private AudioSource audioSource; // ��������� AudioSource

    private DataSaver dataSaver; // ������ �� DataSaver

    private void Start()
    {
        // ������� DataSaver
        dataSaver = DataSaver.Instance;
        if (dataSaver == null)
        {
            Debug.LogError("DataSaver instance not found!");
        }
        else
        {
            Debug.Log("DataSaver found successfully.");
        }

        // ������� AudioSource
        if (audioSource == null)
        {
            audioSource = GetComponent<AudioSource>();
            if (audioSource == null)
            {
                Debug.LogWarning("No AudioSource found on PlayerShooting, adding one.");
                audioSource = gameObject.AddComponent<AudioSource>();
            }
        }

        // ���������, ��������� �� ������
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

        // ��������� ������� ��������� ������
        string activeWeaponId = dataSaver.GetActiveWeaponId();
        if (string.IsNullOrEmpty(activeWeaponId) || (activeWeaponId != "gun_makarov" && activeWeaponId != "gun_ak"))
        {
            Debug.Log("No action taken: No active weapon equipped!");
            return;
        }

        // ��������� ������� �������� � ����������� ��������������� ����
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

            // ������� ���� �������� � �������
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

            // ������� ���� ���������� �������, ���� �� ����
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