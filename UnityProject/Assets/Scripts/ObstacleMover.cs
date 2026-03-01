using UnityEngine;

public class ObstacleMover : MonoBehaviour
{
    [Header("Enemy Stats")]
    [SerializeField] private float moveSpeed = 3f;
    [SerializeField] private float turnSpeed = 80f;
    [SerializeField] private float turretTurnSpeed = 120f;
    [SerializeField] private float preferredDistance = 11f;
    [SerializeField] private float fireInterval = 2.4f;
    [SerializeField] private float shellSpeed = 20f;
    [SerializeField] private float shellDamage = 8f;
    [SerializeField] private float maxHealth = 42f;

    [Header("Bounds")]
    [SerializeField] private float arenaMinX = -28f;
    [SerializeField] private float arenaMaxX = 28f;
    [SerializeField] private float arenaMinZ = -28f;
    [SerializeField] private float arenaMaxZ = 28f;

    [Header("Rig")]
    [SerializeField] private Transform turretPivot;
    [SerializeField] private Transform muzzlePoint;

    private PlayerCarController player;
    private float health;
    private float fireTimer;
    private float strafeTimer;
    private float strafeSign;

    private bool isAlive = true;
    public bool IsAlive => isAlive;

    private void Awake()
    {
        EnsureEnemyRig();
        health = maxHealth;
        fireTimer = Random.Range(0.35f, fireInterval);
        strafeTimer = Random.Range(0.6f, 1.6f);
        strafeSign = Random.value > 0.5f ? 1f : -1f;
    }

    public void Configure(
        PlayerCarController target,
        float configuredMoveSpeed,
        float configuredTurnSpeed,
        float configuredFireInterval,
        float configuredHealth,
        float configuredShellSpeed,
        float configuredShellDamage,
        float configuredPreferredDistance)
    {
        player = target;
        moveSpeed = Mathf.Max(1f, configuredMoveSpeed);
        turnSpeed = Mathf.Max(25f, configuredTurnSpeed);
        fireInterval = Mathf.Max(0.65f, configuredFireInterval);
        maxHealth = Mathf.Max(10f, configuredHealth);
        shellSpeed = Mathf.Max(8f, configuredShellSpeed);
        shellDamage = Mathf.Max(1f, configuredShellDamage);
        preferredDistance = Mathf.Clamp(configuredPreferredDistance, 6f, 18f);

        health = maxHealth;
        fireTimer = Random.Range(0.25f, fireInterval);
    }

    private void Update()
    {
        if (!isAlive)
        {
            return;
        }

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

        TickMovement();
        TickTurret();
        TickFire();
    }

    public void ApplyDamage(float damage)
    {
        if (!isAlive)
        {
            return;
        }

        health -= Mathf.Max(0f, damage);
        if (health > 0f)
        {
            return;
        }

        DestroyEnemy();
    }

    private void TickMovement()
    {
        Vector3 toPlayer = player.transform.position - transform.position;
        toPlayer.y = 0f;
        float distance = toPlayer.magnitude;
        if (distance > 0.05f)
        {
            Quaternion desired = Quaternion.LookRotation(toPlayer.normalized, Vector3.up);
            transform.rotation = Quaternion.RotateTowards(transform.rotation, desired, turnSpeed * Time.deltaTime);
        }

        strafeTimer -= Time.deltaTime;
        if (strafeTimer <= 0f)
        {
            strafeTimer = Random.Range(0.8f, 1.8f);
            strafeSign = Random.value > 0.5f ? 1f : -1f;
        }

        Vector3 moveDirection;
        if (distance > preferredDistance + 1.8f)
        {
            moveDirection = transform.forward;
        }
        else if (distance < preferredDistance - 1.6f)
        {
            moveDirection = -transform.forward;
        }
        else
        {
            moveDirection = (transform.forward * 0.15f + transform.right * strafeSign).normalized;
        }

        Vector3 position = transform.position + moveDirection * moveSpeed * Time.deltaTime;
        position.x = Mathf.Clamp(position.x, arenaMinX, arenaMaxX);
        position.z = Mathf.Clamp(position.z, arenaMinZ, arenaMaxZ);
        position.y = 0.55f;
        transform.position = position;
    }

    private void TickTurret()
    {
        if (turretPivot == null || player == null)
        {
            return;
        }

        Vector3 toPlayer = player.transform.position - turretPivot.position;
        toPlayer.y = 0f;
        if (toPlayer.sqrMagnitude < 0.01f)
        {
            return;
        }

        Quaternion desired = Quaternion.LookRotation(toPlayer.normalized, Vector3.up);
        turretPivot.rotation = Quaternion.RotateTowards(turretPivot.rotation, desired, turretTurnSpeed * Time.deltaTime);
    }

    private void TickFire()
    {
        fireTimer -= Time.deltaTime;
        if (fireTimer > 0f || player == null)
        {
            return;
        }

        Vector3 from = muzzlePoint != null ? muzzlePoint.position : transform.position + Vector3.up * 0.8f;
        Vector3 toPlayer = player.transform.position + Vector3.up * 0.7f - from;
        if (toPlayer.sqrMagnitude < 1f)
        {
            fireTimer = fireInterval;
            return;
        }

        Vector3 shellDirection = muzzlePoint != null ? muzzlePoint.forward : toPlayer.normalized;
        float alignment = Vector3.Dot(shellDirection.normalized, toPlayer.normalized);
        if (alignment < 0.78f)
        {
            fireTimer = 0.08f;
            return;
        }

        MissileProjectile.CreateEnemyShell(from, shellDirection, shellSpeed, shellDamage, transform);
        RuntimeAudioFactory.PlayOneShotAt(from, RuntimeAudioFactory.GetEnemyShotClip(), 0.62f, 1f, 0.94f, 1.05f);
        fireTimer = fireInterval + Random.Range(-0.2f, 0.35f);
    }

    private void DestroyEnemy()
    {
        if (!isAlive)
        {
            return;
        }

        isAlive = false;
        RuntimeAudioFactory.PlayOneShotAt(transform.position + Vector3.up * 0.3f, RuntimeAudioFactory.GetExplosionClip(), 0.85f, 1f, 0.95f, 1.03f);
        SpawnImpact(new Color(1f, 0.72f, 0.2f, 1f), 0.85f);
        GameManager.Instance?.HandleEnemyDestroyed(transform.position);
        Destroy(gameObject);
    }

    private void SpawnImpact(Color color, float scale)
    {
        var blast = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        blast.name = "EnemyExplosion";
        blast.transform.position = transform.position + Vector3.up * 0.3f;
        blast.transform.localScale = new Vector3(scale, scale, scale);
        blast.GetComponent<Renderer>().material = RuntimeMaterialFactory.Create(color);

        Collider col = blast.GetComponent<Collider>();
        if (col != null)
        {
            Destroy(col);
        }

        Destroy(blast, 0.16f);
    }

    private void EnsureEnemyRig()
    {
        var collider = GetComponent<BoxCollider>();
        if (collider == null)
        {
            collider = gameObject.AddComponent<BoxCollider>();
        }

        collider.isTrigger = false;
        collider.center = new Vector3(0f, 0.7f, 0f);
        collider.size = new Vector3(1.8f, 1.2f, 2.4f);

        var rb = GetComponent<Rigidbody>();
        if (rb == null)
        {
            rb = gameObject.AddComponent<Rigidbody>();
        }

        rb.useGravity = false;
        rb.isKinematic = true;
        rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;

        if (turretPivot == null)
        {
            turretPivot = transform.Find("TurretPivot");
        }

        if (turretPivot == null)
        {
            var turretGo = new GameObject("TurretPivot");
            turretGo.transform.SetParent(transform, false);
            turretGo.transform.localPosition = new Vector3(0f, 0.85f, -0.05f);
            turretPivot = turretGo.transform;
        }

        if (muzzlePoint == null && turretPivot != null)
        {
            muzzlePoint = turretPivot.Find("MuzzlePoint");
        }

        if (muzzlePoint == null && turretPivot != null)
        {
            var muzzleGo = new GameObject("MuzzlePoint");
            muzzleGo.transform.SetParent(turretPivot, false);
            muzzleGo.transform.localPosition = new Vector3(0f, 0.02f, 1.7f);
            muzzlePoint = muzzleGo.transform;
        }
    }
}
