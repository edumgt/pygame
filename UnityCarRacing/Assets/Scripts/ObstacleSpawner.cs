using UnityEngine;

public class ObstacleSpawner : MonoBehaviour
{
    [Header("Prefabs")]
    [SerializeField] private GameObject obstaclePrefab;
    [SerializeField] private GameObject shieldPrefab;

    [Header("Spawn Area")]
    [SerializeField] private float minX = -2.2f;
    [SerializeField] private float maxX = 2.2f;
    [SerializeField] private float spawnY = 6f;

    [Header("Timing")]
    [SerializeField] private float obstacleSpawnInterval = 1.3f;
    [SerializeField] private float shieldSpawnInterval = 6f;

    [Header("Obstacle Speed")]
    [SerializeField] private float initialObstacleSpeed = 4f;
    [SerializeField] private float speedIncrease = 0.15f;

    private float obstacleTimer;
    private float shieldTimer;
    private float currentObstacleSpeed;

    private void Start()
    {
        ResetSpawner();
    }

    private void Update()
    {
        if (GameManager.Instance != null && GameManager.Instance.IsGameOver)
        {
            return;
        }

        obstacleTimer += Time.deltaTime;
        shieldTimer += Time.deltaTime;

        if (obstacleTimer >= obstacleSpawnInterval)
        {
            obstacleTimer = 0f;
            SpawnObstacle();
            currentObstacleSpeed += speedIncrease;
        }

        if (shieldPrefab != null && shieldTimer >= shieldSpawnInterval)
        {
            shieldTimer = 0f;
            SpawnShield();
        }
    }

    public void ResetSpawner()
    {
        obstacleTimer = 0f;
        shieldTimer = 0f;
        currentObstacleSpeed = initialObstacleSpeed;
    }

    private void SpawnObstacle()
    {
        if (obstaclePrefab == null)
        {
            return;
        }

        Vector3 spawnPos = new Vector3(Random.Range(minX, maxX), spawnY, 0f);
        GameObject obstacle = Instantiate(obstaclePrefab, spawnPos, Quaternion.identity);
        if (obstacle.TryGetComponent(out ObstacleMover mover))
        {
            mover.SetSpeed(currentObstacleSpeed);
        }
    }

    private void SpawnShield()
    {
        Vector3 spawnPos = new Vector3(Random.Range(minX, maxX), spawnY, 0f);
        Instantiate(shieldPrefab, spawnPos, Quaternion.identity);
    }
}
