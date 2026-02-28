using UnityEngine;

public class ObstacleSpawner : MonoBehaviour
{
    [Header("Prefabs")]
    [SerializeField] private GameObject obstaclePrefab;
    [SerializeField] private GameObject shieldPrefab;

    [Header("Spawn Area")]
    [SerializeField] private float minX = -4f;
    [SerializeField] private float maxX = 4f;
    [SerializeField] private float spawnZ = 24f;

    [Header("Timing")]
    [SerializeField] private float obstacleSpawnInterval = 1.3f;
    [SerializeField] private float shieldSpawnInterval = 6f;

    [Header("Obstacle Speed")]
    [SerializeField] private float initialObstacleSpeed = 12f;
    [SerializeField] private float speedIncrease = 0.3f;

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
        Vector3 spawnPos = new Vector3(Random.Range(minX, maxX), 0.55f, spawnZ);
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
        Vector3 spawnPos = new Vector3(Random.Range(minX, maxX), 0.75f, spawnZ + 2f);
        if (shieldPrefab != null)
        {
            Instantiate(shieldPrefab, spawnPos, Quaternion.identity);
            return;
        }

        CreateRuntimeShield(spawnPos);
    }

    private static GameObject CreateRuntimeObstacle(Vector3 spawnPos)
    {
        var go = GameObject.CreatePrimitive(PrimitiveType.Cube);
        go.name = "Obstacle";
        go.transform.position = spawnPos;
        go.transform.localScale = new Vector3(1f, 1f, 2f);
        go.GetComponent<Renderer>().material = RuntimeMaterialFactory.Create(new Color(1f, 0.35f, 0.25f, 1f));

        var collider = go.GetComponent<BoxCollider>();
        collider.isTrigger = true;

        var rb = go.AddComponent<Rigidbody>();
        rb.useGravity = false;
        rb.isKinematic = true;

        go.AddComponent<ObstacleMover>();
        return go;
    }

    private static void CreateRuntimeShield(Vector3 spawnPos)
    {
        var go = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        go.name = "ShieldPickup";
        go.transform.position = spawnPos;
        go.transform.localScale = new Vector3(0.7f, 0.7f, 0.7f);
        go.GetComponent<Renderer>().material = RuntimeMaterialFactory.Create(new Color(1f, 0.95f, 0.2f, 1f));

        var collider = go.GetComponent<SphereCollider>();
        collider.isTrigger = true;

        var rb = go.AddComponent<Rigidbody>();
        rb.useGravity = false;
        rb.isKinematic = true;

        go.AddComponent<ShieldPickup>();
    }
}
