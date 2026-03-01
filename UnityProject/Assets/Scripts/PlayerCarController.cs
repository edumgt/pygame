using UnityEngine;

public class PlayerCarController : MonoBehaviour
{
    [Header("Hull Movement")]
    [SerializeField] private float maxForwardSpeed = 11.8f;
    [SerializeField] private float maxReverseSpeed = 5.4f;
    [SerializeField] private float steerMixSpeed = 3.3f;
    [SerializeField] private float pivotTrackSpeed = 4.9f;
    [SerializeField] private float trackAcceleration = 16f;
    [SerializeField] private float trackBrakeAcceleration = 24f;
    [SerializeField] private float trackIdleDeceleration = 9f;
    [SerializeField] private float hullTrackWidth = 1.95f;
    [SerializeField] private float boundaryBrakeMultiplier = 1.55f;
    [SerializeField] private float arenaMinX = -28f;
    [SerializeField] private float arenaMaxX = 28f;
    [SerializeField] private float arenaMinZ = -28f;
    [SerializeField] private float arenaMaxZ = 28f;

    [Header("Turret")]
    [SerializeField] private Transform turretPivot;
    [SerializeField] private Transform muzzlePoint;
    [SerializeField] private TankVFXController tankVfx;
    [SerializeField] private TankAudioController tankAudio;
    [SerializeField] private float turretTurnSpeed = 135f;
    [SerializeField] private float autoTrackSpeed = 170f;
    [SerializeField] private float autoTrackRange = 26f;
    [SerializeField] private float autoTrackCone = 65f;

    [Header("Cannon")]
    [SerializeField] private float fireCooldown = 0.55f;
    [SerializeField] private float shellSpeed = 30f;
    [SerializeField] private float shellDamage = 35f;

    private float fireTimer;
    private Vector3 initialPosition;
    private Quaternion initialRotation;
    private float leftTrackSpeed;
    private float rightTrackSpeed;
    private Vector3 hullVelocity;
    private float currentTurnMagnitude;
    private float targetRefreshTimer;
    private ObstacleMover cachedTarget;
    private bool isDestroyed;

    public bool IsMissileReady => fireTimer <= 0f;
    public bool HasLockedTarget => cachedTarget != null && cachedTarget.IsAlive;
    public float CurrentHullSpeedAbs => hullVelocity.magnitude;
    public float CurrentTurnMagnitude => currentTurnMagnitude;

    private void Awake()
    {
        initialPosition = transform.position;
        initialRotation = transform.rotation;
        EnsurePhysicsSetup();
        EnsureTankRig();
    }

    private void Update()
    {
        if (isDestroyed || (GameManager.Instance != null && GameManager.Instance.IsGameOver))
        {
            return;
        }

        if (fireTimer > 0f)
        {
            fireTimer -= Time.deltaTime;
        }

        HandleMovement();
        HandleTurretAim();
        HandleFire();
    }

    private void HandleMovement()
    {
        float dt = Time.deltaTime;
        float throttleInput = ReadForwardInput();
        float steerInput = ReadTurnInput();
        RuntimeRoadFactory.SurfaceProfile surface = RuntimeRoadFactory.SampleSurfaceProfile(transform.position);

        float targetLeftSpeed;
        float targetRightSpeed;

        bool pivotTurn = Mathf.Abs(throttleInput) < 0.05f && Mathf.Abs(steerInput) > 0.01f;
        if (pivotTurn)
        {
            float pivot = steerInput * pivotTrackSpeed * surface.TurnFactor;
            targetLeftSpeed = -pivot;
            targetRightSpeed = pivot;
        }
        else
        {
            float baseTrackSpeed = throttleInput >= 0f
                ? throttleInput * maxForwardSpeed
                : throttleInput * maxReverseSpeed;
            float reverseSteerFactor = throttleInput < -0.01f ? 0.72f : 1f;
            float steerMix = steerInput * steerMixSpeed * reverseSteerFactor * surface.TurnFactor;

            targetLeftSpeed = baseTrackSpeed - steerMix;
            targetRightSpeed = baseTrackSpeed + steerMix;
        }

        float maxForwardOnSurface = maxForwardSpeed * surface.SpeedFactor;
        float maxReverseOnSurface = maxReverseSpeed * Mathf.Lerp(surface.SpeedFactor, 1f, 0.2f);
        targetLeftSpeed = Mathf.Clamp(targetLeftSpeed, -maxReverseOnSurface, maxForwardOnSurface);
        targetRightSpeed = Mathf.Clamp(targetRightSpeed, -maxReverseOnSurface, maxForwardOnSurface);

        leftTrackSpeed = MoveTrackSpeed(leftTrackSpeed, targetLeftSpeed, dt, surface.Traction);
        rightTrackSpeed = MoveTrackSpeed(rightTrackSpeed, targetRightSpeed, dt, surface.Traction);

        float forwardSpeed = (leftTrackSpeed + rightTrackSpeed) * 0.5f;
        float angularSpeedDeg = ((rightTrackSpeed - leftTrackSpeed) / Mathf.Max(0.1f, hullTrackWidth)) * Mathf.Rad2Deg * surface.TurnFactor;
        currentTurnMagnitude = Mathf.Clamp01(Mathf.Abs(angularSpeedDeg) / 120f);

        transform.Rotate(0f, angularSpeedDeg * dt, 0f, Space.World);

        Vector3 forward = transform.forward;
        Vector3 right = transform.right;
        float forwardVelocity = Vector3.Dot(hullVelocity, forward);
        float lateralVelocity = Vector3.Dot(hullVelocity, right);

        float forwardFollow = Mathf.Lerp(3.2f, 12f, surface.Traction);
        float lateralDamping = Mathf.Lerp(1.2f, 10.5f, surface.Traction);

        forwardVelocity = Mathf.MoveTowards(forwardVelocity, forwardSpeed, forwardFollow * dt);
        lateralVelocity = Mathf.MoveTowards(lateralVelocity, 0f, lateralDamping * dt);

        hullVelocity = forward * forwardVelocity + right * lateralVelocity;
        Vector3 unclampedPosition = transform.position + hullVelocity * dt;
        Vector3 position = unclampedPosition;
        position.x = Mathf.Clamp(position.x, arenaMinX, arenaMaxX);
        position.z = Mathf.Clamp(position.z, arenaMinZ, arenaMaxZ);
        position.y = initialPosition.y;

        bool touchedBoundary =
            unclampedPosition.x < arenaMinX ||
            unclampedPosition.x > arenaMaxX ||
            unclampedPosition.z < arenaMinZ ||
            unclampedPosition.z > arenaMaxZ;
        if (touchedBoundary)
        {
            float boundaryBrake = trackBrakeAcceleration * boundaryBrakeMultiplier * dt;
            leftTrackSpeed = Mathf.MoveTowards(leftTrackSpeed, 0f, boundaryBrake);
            rightTrackSpeed = Mathf.MoveTowards(rightTrackSpeed, 0f, boundaryBrake);
            hullVelocity = Vector3.MoveTowards(hullVelocity, Vector3.zero, boundaryBrake * 0.8f);
        }

        transform.position = position;
    }

    private void HandleTurretAim()
    {
        if (turretPivot == null)
        {
            return;
        }

        float manualInput = ReadTurretInput();
        if (Mathf.Abs(manualInput) > 0.01f)
        {
            cachedTarget = null;
            targetRefreshTimer = 0f;
            turretPivot.Rotate(0f, manualInput * turretTurnSpeed * Time.deltaTime, 0f, Space.World);
            return;
        }

        cachedTarget = ResolveAutoTarget();

        Vector3 aimDirection = transform.forward;
        float turnSpeed = turretTurnSpeed;
        if (cachedTarget != null)
        {
            aimDirection = cachedTarget.transform.position - turretPivot.position;
            turnSpeed = autoTrackSpeed;
        }

        aimDirection.y = 0f;
        if (aimDirection.sqrMagnitude < 0.001f)
        {
            return;
        }

        Quaternion desiredRotation = Quaternion.LookRotation(aimDirection.normalized, Vector3.up);
        turretPivot.rotation = Quaternion.RotateTowards(
            turretPivot.rotation,
            desiredRotation,
            turnSpeed * Time.deltaTime);
    }

    private void HandleFire()
    {
        bool firePressed = Input.GetKeyDown(KeyCode.Space) || Input.GetMouseButtonDown(0);
        if (!firePressed || fireTimer > 0f)
        {
            return;
        }

        fireTimer = fireCooldown;

        Vector3 spawnPosition = muzzlePoint != null
            ? muzzlePoint.position
            : transform.position + transform.forward * 1.8f + Vector3.up * 0.9f;

        Vector3 shootDirection = muzzlePoint != null ? muzzlePoint.forward : transform.forward;
        MissileProjectile.CreatePlayerShell(spawnPosition, shootDirection, shellSpeed, shellDamage, transform);
        tankAudio?.PlayCannon(spawnPosition);
        GameManager.Instance?.RegisterPlayerShot();
    }

    public void ApplyDamage(float damage)
    {
        if (isDestroyed || GameManager.Instance == null || GameManager.Instance.IsGameOver)
        {
            return;
        }

        tankAudio?.PlayHullHit(transform.position + Vector3.up * 0.7f);
        GameManager.Instance.HandlePlayerDamaged(damage);
    }

    public void HandleDestroyed()
    {
        if (isDestroyed)
        {
            return;
        }

        isDestroyed = true;
        fireTimer = Mathf.Max(fireTimer, fireCooldown);
        leftTrackSpeed = 0f;
        rightTrackSpeed = 0f;
        hullVelocity = Vector3.zero;
        currentTurnMagnitude = 0f;
        SetCollidersEnabled(false);
        tankVfx?.PlayDestructionEffect();
    }

    public void ResetPlayer()
    {
        isDestroyed = false;
        fireTimer = 0f;
        leftTrackSpeed = 0f;
        rightTrackSpeed = 0f;
        hullVelocity = Vector3.zero;
        currentTurnMagnitude = 0f;
        targetRefreshTimer = 0f;
        cachedTarget = null;
        SetCollidersEnabled(true);
        transform.position = initialPosition;
        transform.rotation = initialRotation;

        if (turretPivot != null)
        {
            turretPivot.rotation = transform.rotation;
        }

        if (tankVfx != null)
        {
            tankVfx.ResetEffects();
        }

        if (tankAudio != null)
        {
            tankAudio.ResetAudioState();
        }
    }

    private float ReadForwardInput()
    {
        float axis = Input.GetAxisRaw("Vertical");
        if (axis > 0.01f || Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow))
        {
            return 1f;
        }

        if (axis < -0.01f || Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow))
        {
            return -1f;
        }

        return 0f;
    }

    private float ReadTurnInput()
    {
        float axis = Input.GetAxisRaw("Horizontal");
        if (axis > 0.01f || Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow))
        {
            return 1f;
        }

        if (axis < -0.01f || Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow))
        {
            return -1f;
        }

        return 0f;
    }

    private float MoveTrackSpeed(float current, float target, float dt, float traction)
    {
        float accel = trackIdleDeceleration;
        if (Mathf.Abs(target) > 0.01f)
        {
            bool reversing = Mathf.Abs(current) > 0.05f && Mathf.Sign(current) != Mathf.Sign(target);
            accel = reversing ? trackBrakeAcceleration : trackAcceleration;
        }

        float tractionScale = Mathf.Lerp(0.45f, 1f, Mathf.Clamp01(traction));
        accel *= tractionScale;
        return Mathf.MoveTowards(current, target, accel * dt);
    }

    private static float ReadTurretInput()
    {
        if (Input.GetKey(KeyCode.Q))
        {
            return -1f;
        }

        if (Input.GetKey(KeyCode.E))
        {
            return 1f;
        }

        return 0f;
    }

    private ObstacleMover ResolveAutoTarget()
    {
        targetRefreshTimer -= Time.deltaTime;
        if (targetRefreshTimer > 0f && IsValidAutoTarget(cachedTarget))
        {
            return cachedTarget;
        }

        targetRefreshTimer = 0.2f;
        cachedTarget = FindBestTargetInCone();
        return cachedTarget;
    }

    private ObstacleMover FindBestTargetInCone()
    {
        ObstacleMover[] enemies = FindObjectsByType<ObstacleMover>(FindObjectsSortMode.None);
        ObstacleMover best = null;
        float bestScore = float.MaxValue;

        for (int i = 0; i < enemies.Length; i++)
        {
            ObstacleMover enemy = enemies[i];
            if (enemy == null || !enemy.IsAlive)
            {
                continue;
            }

            Vector3 toEnemy = enemy.transform.position - turretPivot.position;
            toEnemy.y = 0f;
            float distance = toEnemy.magnitude;
            if (distance > autoTrackRange || distance < 0.1f)
            {
                continue;
            }

            float angle = Vector3.Angle(turretPivot.forward, toEnemy.normalized);
            if (angle > autoTrackCone)
            {
                continue;
            }

            float score = distance + angle * 0.12f;
            if (score < bestScore)
            {
                best = enemy;
                bestScore = score;
            }
        }

        return best;
    }

    private bool IsValidAutoTarget(ObstacleMover candidate)
    {
        if (candidate == null || !candidate.IsAlive || turretPivot == null)
        {
            return false;
        }

        Vector3 toEnemy = candidate.transform.position - turretPivot.position;
        toEnemy.y = 0f;
        float distance = toEnemy.magnitude;
        if (distance > autoTrackRange || distance < 0.1f)
        {
            return false;
        }

        float angle = Vector3.Angle(turretPivot.forward, toEnemy.normalized);
        return angle <= autoTrackCone;
    }

    private void EnsurePhysicsSetup()
    {
        var collider = GetComponent<BoxCollider>();
        if (collider == null)
        {
            collider = gameObject.AddComponent<BoxCollider>();
        }

        collider.isTrigger = false;
        collider.center = new Vector3(0f, 0.7f, 0f);
        collider.size = new Vector3(1.9f, 1.2f, 2.6f);

        var rb = GetComponent<Rigidbody>();
        if (rb == null)
        {
            rb = gameObject.AddComponent<Rigidbody>();
        }

        rb.useGravity = false;
        rb.isKinematic = true;
        rb.interpolation = RigidbodyInterpolation.Interpolate;
        rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;
    }

    private void EnsureTankRig()
    {
        if (turretPivot == null)
        {
            Transform existingPivot = transform.Find("TurretPivot");
            if (existingPivot != null)
            {
                turretPivot = existingPivot;
            }
            else
            {
                var turretGo = new GameObject("TurretPivot");
                turretGo.transform.SetParent(transform, false);
                turretGo.transform.localPosition = new Vector3(0f, 0.85f, -0.05f);
                turretPivot = turretGo.transform;
            }
        }

        if (muzzlePoint == null && turretPivot != null)
        {
            Transform existingMuzzle = turretPivot.Find("MuzzlePoint");
            if (existingMuzzle != null)
            {
                muzzlePoint = existingMuzzle;
            }
            else
            {
                var muzzleGo = new GameObject("MuzzlePoint");
                muzzleGo.transform.SetParent(turretPivot, false);
                muzzleGo.transform.localPosition = new Vector3(0f, 0.02f, 1.7f);
                muzzlePoint = muzzleGo.transform;
            }
        }

        if (tankVfx == null)
        {
            tankVfx = GetComponent<TankVFXController>();
        }

        if (tankVfx == null)
        {
            tankVfx = gameObject.AddComponent<TankVFXController>();
        }

        if (tankAudio == null)
        {
            tankAudio = GetComponent<TankAudioController>();
        }

        if (tankAudio == null)
        {
            tankAudio = gameObject.AddComponent<TankAudioController>();
        }
    }

    private void SetCollidersEnabled(bool enabled)
    {
        Collider[] colliders = GetComponentsInChildren<Collider>(true);
        for (int i = 0; i < colliders.Length; i++)
        {
            colliders[i].enabled = enabled;
        }
    }
}
