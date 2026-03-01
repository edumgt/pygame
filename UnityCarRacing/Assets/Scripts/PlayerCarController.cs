using UnityEngine;

public class PlayerCarController : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float moveSpeed = 8.5f;
    [SerializeField] private float minX = -5.2f;
    [SerializeField] private float maxX = 5.2f;

    [Header("Weapon")]
    [SerializeField] private float fireCooldown = 0.35f;
    [SerializeField] private Transform missileSpawnPoint;
    [SerializeField] private TankAimZone aimZone;

    private float fireTimer;
    private Vector3 initialPosition;

    public bool HasTargetLock => aimZone != null && aimZone.HasTarget;
    public bool IsMissileReady => fireTimer <= 0f;

    private void Awake()
    {
        initialPosition = transform.position;
        EnsurePhysicsSetup();
        EnsureWeaponSetup();
    }

    private void Update()
    {
        if (GameManager.Instance != null && GameManager.Instance.IsGameOver)
        {
            return;
        }

        if (fireTimer > 0f)
        {
            fireTimer -= Time.deltaTime;
        }

        HandleMovement();
        HandleFire();
    }

    private void HandleMovement()
    {
        float input = Input.GetAxisRaw("Horizontal");
        if (Mathf.Approximately(input, 0f))
        {
            if (Input.GetKey(KeyCode.A))
            {
                input = -1f;
            }
            else if (Input.GetKey(KeyCode.D))
            {
                input = 1f;
            }
        }

        Vector3 position = transform.position;
        position.x += input * moveSpeed * Time.deltaTime;
        position.x = Mathf.Clamp(position.x, minX, maxX);
        transform.position = position;
    }

    private void HandleFire()
    {
        if (!Input.GetKeyDown(KeyCode.Space) || fireTimer > 0f)
        {
            return;
        }

        fireTimer = fireCooldown;

        ObstacleMover target = null;
        bool hasLock = false;
        if (aimZone != null)
        {
            hasLock = aimZone.TryGetLockedTarget(out target);
        }
        Vector3 spawnPosition = missileSpawnPoint != null
            ? missileSpawnPoint.position
            : transform.position + new Vector3(0f, 0.9f, 1.8f);

        Vector3 launchDirection = Vector3.forward;
        if (hasLock && target != null && target.IsAlive)
        {
            launchDirection = (target.transform.position - spawnPosition).normalized;
        }

        MissileProjectile.Create(spawnPosition, launchDirection, hasLock ? target : null);
        GameManager.Instance?.RegisterShotFired(hasLock);
    }

    public void ResetPlayer()
    {
        fireTimer = 0f;
        transform.position = initialPosition;
        aimZone?.Clear();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent<ObstacleMover>(out ObstacleMover target))
        {
            target.ReachBase();
        }
    }

    private void EnsurePhysicsSetup()
    {
        var collider = GetComponent<BoxCollider>();
        if (collider == null)
        {
            collider = gameObject.AddComponent<BoxCollider>();
        }

        collider.isTrigger = true;
        collider.center = new Vector3(0f, 0.65f, 0f);
        collider.size = new Vector3(1.9f, 1.2f, 2.6f);

        var rb = GetComponent<Rigidbody>();
        if (rb == null)
        {
            rb = gameObject.AddComponent<Rigidbody>();
        }

        rb.useGravity = false;
        rb.isKinematic = true;
        rb.constraints = RigidbodyConstraints.FreezeRotation;
    }

    private void EnsureWeaponSetup()
    {
        if (missileSpawnPoint == null)
        {
            Transform existing = transform.Find("MissileSpawnPoint");
            if (existing != null)
            {
                missileSpawnPoint = existing;
            }
            else
            {
                var mount = new GameObject("MissileSpawnPoint");
                mount.transform.SetParent(transform, false);
                mount.transform.localPosition = new Vector3(0f, 0.95f, 1.9f);
                missileSpawnPoint = mount.transform;
            }
        }

        if (aimZone == null)
        {
            aimZone = GetComponentInChildren<TankAimZone>();
        }

        if (aimZone == null)
        {
            var zoneGo = new GameObject("AimZone");
            zoneGo.transform.SetParent(transform, false);
            zoneGo.transform.localPosition = new Vector3(0f, 0.9f, 8.6f);
            var sphere = zoneGo.AddComponent<SphereCollider>();
            sphere.isTrigger = true;
            sphere.radius = 1.6f;
            aimZone = zoneGo.AddComponent<TankAimZone>();
        }
    }
}
