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

        if (shieldTimer >= shieldSpawnInterval)
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
        Vector3 spawnPos = new Vector3(Random.Range(minX, maxX), spawnY, 0f);
        GameObject obstacle = obstaclePrefab != null
            ? Instantiate(obstaclePrefab, spawnPos, Quaternion.identity)
            : CreateRuntimeObstacle(spawnPos);
        if (obstacle.TryGetComponent(out ObstacleMover mover))
        {
            mover.SetSpeed(currentObstacleSpeed);
        }
    }

    private void SpawnShield()
    {
        Vector3 spawnPos = new Vector3(Random.Range(minX, maxX), spawnY, 0f);
        if (shieldPrefab != null)
        {
            Instantiate(shieldPrefab, spawnPos, Quaternion.identity);
            return;
        }

        CreateRuntimeShield(spawnPos);
    }

    private static GameObject CreateRuntimeObstacle(Vector3 spawnPos)
    {
        var go = new GameObject("Obstacle");
        go.transform.position = spawnPos;

        var renderer = go.AddComponent<SpriteRenderer>();
        renderer.sprite = RuntimeSpriteFactory.GetSquareSprite();
        renderer.color = new Color(1f, 0.35f, 0.25f, 1f);
        go.transform.localScale = new Vector3(0.9f, 1.3f, 1f);

        var collider = go.AddComponent<BoxCollider2D>();
        collider.isTrigger = true;

        var rb = go.AddComponent<Rigidbody2D>();
        rb.gravityScale = 0f;
        rb.bodyType = RigidbodyType2D.Kinematic;

        go.AddComponent<ObstacleMover>();
        return go;
    }

    private static void CreateRuntimeShield(Vector3 spawnPos)
    {
        var go = new GameObject("ShieldPickup");
        go.transform.position = spawnPos;

        var renderer = go.AddComponent<SpriteRenderer>();
        renderer.sprite = RuntimeSpriteFactory.GetSquareSprite();
        renderer.color = new Color(1f, 0.95f, 0.2f, 1f);
        go.transform.localScale = new Vector3(0.7f, 0.7f, 1f);

        var collider = go.AddComponent<CircleCollider2D>();
        collider.isTrigger = true;

        var rb = go.AddComponent<Rigidbody2D>();
        rb.gravityScale = 0f;
        rb.bodyType = RigidbodyType2D.Kinematic;

        go.AddComponent<ShieldPickup>();
    }
}
