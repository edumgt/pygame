using System.Collections.Generic;
using UnityEngine;

public class ObstacleSpawner : MonoBehaviour
{
    [Header("Enemy Prefab")]
    [SerializeField] private GameObject targetPrefab;

    [Header("Spawn Rules")]
    [SerializeField] private float spawnInterval = 2.2f;
    [SerializeField] private int baseMaxAlive = 4;
    [SerializeField] private float minSpawnRadius = 13f;
    [SerializeField] private float maxSpawnRadius = 22f;
    [SerializeField] private float waveDurationSeconds = 22f;

    [Header("Arena Bounds")]
    [SerializeField] private float arenaMinX = -28f;
    [SerializeField] private float arenaMaxX = 28f;
    [SerializeField] private float arenaMinZ = -28f;
    [SerializeField] private float arenaMaxZ = 28f;

    private readonly List<ObstacleMover> activeEnemies = new List<ObstacleMover>();

    private float spawnTimer;
    private float elapsedSeconds;
    private PlayerCarController player;

    public int CurrentWave { get; private set; } = 1;

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

        if (player == null)
        {
            player = FindAnyObjectByType<PlayerCarController>();
            if (player == null)
            {
                return;
            }
        }

        elapsedSeconds += Time.deltaTime;
        CurrentWave = 1 + Mathf.FloorToInt(elapsedSeconds / Mathf.Max(5f, waveDurationSeconds));

        CleanupEnemies();

        spawnTimer += Time.deltaTime;
        int maxAlive = baseMaxAlive + Mathf.FloorToInt((CurrentWave - 1) * 0.5f);
        float dynamicInterval = Mathf.Max(0.65f, spawnInterval - (CurrentWave - 1) * 0.12f);

        if (spawnTimer >= dynamicInterval && activeEnemies.Count < maxAlive)
        {
            spawnTimer = 0f;
            SpawnEnemy();
        }
    }

    public void ResetSpawner()
    {
        spawnTimer = 0f;
        elapsedSeconds = 0f;
        CurrentWave = 1;
        activeEnemies.Clear();
        player = FindAnyObjectByType<PlayerCarController>();
    }

    private void SpawnEnemy()
    {
        Vector3 spawnPosition = ResolveSpawnPosition();
        Vector3 toPlayer = player != null ? (player.transform.position - spawnPosition) : Vector3.back;
        toPlayer.y = 0f;
        if (toPlayer.sqrMagnitude < 0.001f)
        {
            toPlayer = Vector3.back;
        }

        Quaternion spawnRotation = Quaternion.LookRotation(toPlayer.normalized, Vector3.up);

        GameObject enemy = targetPrefab != null
            ? Instantiate(targetPrefab, spawnPosition, spawnRotation)
            : CreateRuntimeEnemyTank(spawnPosition, spawnRotation);

        if (!enemy.TryGetComponent(out ObstacleMover mover))
        {
            mover = enemy.AddComponent<ObstacleMover>();
        }

        float wave = CurrentWave - 1;
        float configuredMoveSpeed = 2.8f + wave * 0.35f + Random.Range(-0.25f, 0.25f);
        float configuredTurnSpeed = 80f + wave * 6f;
        float configuredFireInterval = Mathf.Max(0.85f, 2.5f - wave * 0.1f + Random.Range(-0.2f, 0.2f));
        float configuredHealth = 35f + wave * 6.5f;
        float configuredShellSpeed = 18f + wave * 0.9f;
        float configuredShellDamage = 7.5f + wave * 1.15f;
        float preferredDistance = Mathf.Max(7f, 11.5f - wave * 0.3f + Random.Range(-0.6f, 0.6f));

        mover.Configure(
            player,
            configuredMoveSpeed,
            configuredTurnSpeed,
            configuredFireInterval,
            configuredHealth,
            configuredShellSpeed,
            configuredShellDamage,
            preferredDistance);

        activeEnemies.Add(mover);
    }

    private Vector3 ResolveSpawnPosition()
    {
        Vector3 center = player != null ? player.transform.position : new Vector3(0f, 0.55f, 5f);
        center.y = 0.55f;

        for (int attempt = 0; attempt < 12; attempt++)
        {
            float angle = Random.Range(0f, Mathf.PI * 2f);
            float radius = Random.Range(minSpawnRadius, maxSpawnRadius);
            Vector3 offset = new Vector3(Mathf.Cos(angle) * radius, 0f, Mathf.Sin(angle) * radius);

            Vector3 candidate = center + offset;
            candidate.x = Mathf.Clamp(candidate.x, arenaMinX, arenaMaxX);
            candidate.z = Mathf.Clamp(candidate.z, arenaMinZ, arenaMaxZ);
            candidate.y = 0.55f;

            if (player == null)
            {
                return candidate;
            }

            float sqrDistance = (candidate - player.transform.position).sqrMagnitude;
            if (sqrDistance >= minSpawnRadius * minSpawnRadius * 0.45f)
            {
                return candidate;
            }
        }

        return new Vector3(Random.Range(arenaMinX, arenaMaxX), 0.55f, Random.Range(arenaMinZ, arenaMaxZ));
    }

    private void CleanupEnemies()
    {
        activeEnemies.RemoveAll(enemy => enemy == null || !enemy.IsAlive);
    }

    private static GameObject CreateRuntimeEnemyTank(Vector3 spawnPosition, Quaternion spawnRotation)
    {
        var root = new GameObject("EnemyTank");
        root.transform.position = spawnPosition;
        root.transform.rotation = spawnRotation;

        var collider = root.AddComponent<BoxCollider>();
        collider.isTrigger = false;
        collider.center = new Vector3(0f, 0.7f, 0f);
        collider.size = new Vector3(1.8f, 1.2f, 2.4f);

        var rb = root.AddComponent<Rigidbody>();
        rb.useGravity = false;
        rb.isKinematic = true;
        rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;

        Material hullMat = RuntimeMaterialFactory.Create(new Color(0.65f, 0.18f, 0.2f, 1f), RuntimeMaterialFactory.MaterialPreset.Metal);
        Material turretMat = RuntimeMaterialFactory.Create(new Color(0.46f, 0.14f, 0.17f, 1f), RuntimeMaterialFactory.MaterialPreset.Metal);
        Material trackMat = RuntimeMaterialFactory.Create(new Color(0.13f, 0.13f, 0.14f, 1f), RuntimeMaterialFactory.MaterialPreset.Boundary);
        Material detailMat = RuntimeMaterialFactory.Create(new Color(0.86f, 0.75f, 0.28f, 1f), RuntimeMaterialFactory.MaterialPreset.Metal);

        CreatePart(root.transform, PrimitiveType.Cube, new Vector3(0f, 0.4f, 0f), new Vector3(1.45f, 0.55f, 2f), hullMat);
        CreatePart(root.transform, PrimitiveType.Cube, new Vector3(-0.88f, 0.26f, 0f), new Vector3(0.28f, 0.24f, 2.1f), trackMat);
        CreatePart(root.transform, PrimitiveType.Cube, new Vector3(0.88f, 0.26f, 0f), new Vector3(0.28f, 0.24f, 2.1f), trackMat);

        var turretPivot = new GameObject("TurretPivot");
        turretPivot.transform.SetParent(root.transform, false);
        turretPivot.transform.localPosition = new Vector3(0f, 0.86f, -0.06f);

        CreatePart(turretPivot.transform, PrimitiveType.Cylinder, new Vector3(0f, 0f, 0f), new Vector3(0.48f, 0.2f, 0.48f), turretMat);
        GameObject barrel = CreatePart(turretPivot.transform, PrimitiveType.Cylinder, new Vector3(0f, 0.02f, 0.95f), new Vector3(0.11f, 0.7f, 0.11f), detailMat);
        barrel.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);

        var muzzlePoint = new GameObject("MuzzlePoint");
        muzzlePoint.transform.SetParent(turretPivot.transform, false);
        muzzlePoint.transform.localPosition = new Vector3(0f, 0.02f, 1.68f);

        root.AddComponent<ObstacleMover>();
        return root;
    }

    private static GameObject CreatePart(Transform parent, PrimitiveType primitiveType, Vector3 localPosition, Vector3 localScale, Material material)
    {
        var part = GameObject.CreatePrimitive(primitiveType);
        part.transform.SetParent(parent, false);
        part.transform.localPosition = localPosition;
        part.transform.localScale = localScale;
        part.GetComponent<Renderer>().material = material;

        Collider col = part.GetComponent<Collider>();
        if (col != null)
        {
            Destroy(col);
        }

        return part;
    }
}
