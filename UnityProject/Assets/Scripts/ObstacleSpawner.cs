using UnityEngine;

public class ObstacleSpawner : MonoBehaviour
{
    [Header("Target Prefab")]
    [SerializeField] private GameObject targetPrefab;

    [Header("Spawn Area")]
    [SerializeField] private float minX = -5.2f;
    [SerializeField] private float maxX = 5.2f;
    [SerializeField] private float spawnY = 0.7f;
    [SerializeField] private float spawnZ = 24f;

    [Header("Spawn Timing")]
    [SerializeField] private float spawnInterval = 1.25f;

    [Header("Target Movement")]
    [SerializeField] private float initialTargetSpeed = 5.8f;
    [SerializeField] private float speedIncreasePerSpawn = 0.08f;

    private float spawnTimer;
    private float currentTargetSpeed;

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

        spawnTimer += Time.deltaTime;

        if (spawnTimer >= spawnInterval)
        {
            spawnTimer = 0f;
            SpawnTarget();
        }
    }

    public void ResetSpawner()
    {
        spawnTimer = 0f;
        currentTargetSpeed = initialTargetSpeed;
    }

    private void SpawnTarget()
    {
        Vector3 spawnPosition = new Vector3(Random.Range(minX, maxX), spawnY, spawnZ);
        GameObject target = targetPrefab != null
            ? Instantiate(targetPrefab, spawnPosition, Quaternion.identity)
            : CreateRuntimeTarget(spawnPosition);

        if (target.TryGetComponent(out ObstacleMover mover))
        {
            mover.SetSpeed(currentTargetSpeed);
        }

        currentTargetSpeed += speedIncreasePerSpawn;
    }

    private static GameObject CreateRuntimeTarget(Vector3 spawnPosition)
    {
        var root = new GameObject("Target");
        root.transform.position = spawnPosition;

        var collider = root.AddComponent<BoxCollider>();
        collider.isTrigger = true;
        collider.center = new Vector3(0f, 0.45f, 0f);
        collider.size = new Vector3(1.4f, 0.9f, 1.6f);

        var rb = root.AddComponent<Rigidbody>();
        rb.useGravity = false;
        rb.isKinematic = true;

        Material bodyMat = RuntimeMaterialFactory.Create(new Color(0.82f, 0.18f, 0.2f, 1f));
        Material detailMat = RuntimeMaterialFactory.Create(new Color(0.24f, 0.16f, 0.12f, 1f));
        Material markerMat = RuntimeMaterialFactory.Create(new Color(1f, 0.92f, 0.26f, 1f));

        CreatePart(root.transform, PrimitiveType.Cube, new Vector3(0f, 0.35f, 0f), new Vector3(1.2f, 0.5f, 1.4f), bodyMat);
        CreatePart(root.transform, PrimitiveType.Cylinder, new Vector3(0f, 0.72f, 0.05f), new Vector3(0.35f, 0.13f, 0.35f), detailMat);
        CreatePart(root.transform, PrimitiveType.Cube, new Vector3(0f, 0.74f, 0.7f), new Vector3(0.16f, 0.12f, 0.85f), detailMat);
        CreatePart(root.transform, PrimitiveType.Sphere, new Vector3(0f, 1.08f, 0f), new Vector3(0.22f, 0.22f, 0.22f), markerMat);

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
