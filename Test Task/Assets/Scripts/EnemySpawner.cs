using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    [Header("Spawn Settings")]
    [SerializeField] private GameObject enemyPrefab1; // ������ ������ ������� (Enemy)
    [SerializeField] private GameObject enemyPrefab2; // ������ ������ ������� (Enemy)
    [SerializeField] private Transform[] spawnPoints; // ������ �� 4 ����� ������
    [SerializeField] private int enemiesToSpawn = 3; // ���������� �������� ��� ������

    private void Start()
    {
        // �������� ��������
        if (enemyPrefab1 == null || enemyPrefab2 == null)
        {
            Debug.LogError("One or both enemy prefabs not assigned in EnemySpawner!");
            return;
        }

        // �������� ����� ������
        if (spawnPoints == null || spawnPoints.Length < enemiesToSpawn)
        {
            Debug.LogError($"Spawn points array must contain at least {enemiesToSpawn} points! Found: {spawnPoints?.Length ?? 0}");
            return;
        }

        // �������� 3 ��������� ������� �� 4
        Transform[] selectedPoints = GetRandomSpawnPoints(spawnPoints, enemiesToSpawn);

        // ������, ����� ������ ��������� 2 ����
        bool spawnFirstTwice = Random.value < 0.5f;
        GameObject frequentPrefab = spawnFirstTwice ? enemyPrefab1 : enemyPrefab2;
        GameObject singlePrefab = spawnFirstTwice ? enemyPrefab2 : enemyPrefab1;

        // ������� �������� ��� �������� �������
        GameObject enemy1 = Instantiate(frequentPrefab, selectedPoints[0].position, Quaternion.identity, transform);
        Debug.Log($"Spawned {enemy1.name} at {selectedPoints[0].name} (Position: {selectedPoints[0].position}) as child of {gameObject.name}");

        GameObject enemy2 = Instantiate(frequentPrefab, selectedPoints[1].position, Quaternion.identity, transform);
        Debug.Log($"Spawned {enemy2.name} at {selectedPoints[1].name} (Position: {selectedPoints[1].position}) as child of {gameObject.name}");

        GameObject enemy3 = Instantiate(singlePrefab, selectedPoints[2].position, Quaternion.identity, transform);
        Debug.Log($"Spawned {enemy3.name} at {selectedPoints[2].name} (Position: {selectedPoints[2].position}) as child of {gameObject.name}");
    }

    private Transform[] GetRandomSpawnPoints(Transform[] points, int count)
    {
        if (points.Length < count)
        {
            Debug.LogWarning($"Not enough spawn points ({points.Length}) for {count} enemies!");
            return points;
        }

        // ������ ����� �������
        Transform[] availablePoints = new Transform[points.Length];
        System.Array.Copy(points, availablePoints, points.Length);

        // ������������ ������ (�������� ������-�����)
        for (int i = availablePoints.Length - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);
            (availablePoints[i], availablePoints[j]) = (availablePoints[j], availablePoints[i]);
        }

        // ���������� ������ count ���������
        Transform[] result = new Transform[count];
        System.Array.Copy(availablePoints, result, count);
        return result;
    }
}