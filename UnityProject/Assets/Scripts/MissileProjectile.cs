using UnityEngine;

public class MissileProjectile : MonoBehaviour
{
    private enum OwnerType
    {
        Player,
        Enemy
    }

    [SerializeField] private float speed = 28f;
    [SerializeField] private float maxLifetime = 3f;
    [SerializeField] private float hitRadius = 0.35f;
    [SerializeField] private float damage = 20f;

    private OwnerType ownerType;
    private Vector3 direction = Vector3.forward;
    private float lifetimeRemaining;
    private Transform ownerRoot;
    private Color impactColor = Color.yellow;

    public static MissileProjectile CreatePlayerShell(Vector3 position, Vector3 direction, float speed, float damage, Transform ownerRoot)
    {
        return CreateShell(position, direction, speed, damage, OwnerType.Player, new Color(1f, 0.7f, 0.2f, 1f), Color.yellow, ownerRoot);
    }

    public static MissileProjectile CreateEnemyShell(Vector3 position, Vector3 direction, float speed, float damage, Transform ownerRoot)
    {
        return CreateShell(position, direction, speed, damage, OwnerType.Enemy, new Color(1f, 0.28f, 0.2f, 1f), new Color(1f, 0.4f, 0.2f, 1f), ownerRoot);
    }

    private static MissileProjectile CreateShell(
        Vector3 position,
        Vector3 direction,
        float speed,
        float damage,
        OwnerType owner,
        Color shellColor,
        Color hitColor,
        Transform ownerRoot)
    {
        var go = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        go.name = owner == OwnerType.Player ? "PlayerShell" : "EnemyShell";
        go.transform.position = position;
        go.transform.localScale = new Vector3(0.24f, 0.24f, 0.24f);

        Vector3 heading = direction.sqrMagnitude > 0.001f ? direction.normalized : Vector3.forward;
        go.transform.rotation = Quaternion.LookRotation(heading, Vector3.up);

        var renderer = go.GetComponent<Renderer>();
        if (renderer != null)
        {
            renderer.material = RuntimeMaterialFactory.Create(shellColor);
        }

        Collider col = go.GetComponent<Collider>();
        if (col != null)
        {
            Object.Destroy(col);
        }

        var projectile = go.AddComponent<MissileProjectile>();
        projectile.Initialize(owner, heading, speed, damage, hitColor, ownerRoot);
        return projectile;
    }

    private void Initialize(OwnerType owner, Vector3 heading, float shellSpeed, float shellDamage, Color shellImpactColor, Transform shellOwner)
    {
        ownerType = owner;
        direction = heading.sqrMagnitude > 0.001f ? heading.normalized : Vector3.forward;
        speed = Mathf.Max(8f, shellSpeed);
        damage = Mathf.Max(1f, shellDamage);
        impactColor = shellImpactColor;
        ownerRoot = shellOwner;
        lifetimeRemaining = maxLifetime;
    }

    private void Update()
    {
        if (GameManager.Instance != null && GameManager.Instance.IsGameOver)
        {
            Destroy(gameObject);
            return;
        }

        lifetimeRemaining -= Time.deltaTime;
        if (lifetimeRemaining <= 0f)
        {
            Destroy(gameObject);
            return;
        }

        Vector3 nextPosition = transform.position + direction * (speed * Time.deltaTime);
        if (TryResolveHit(nextPosition))
        {
            return;
        }

        transform.position = nextPosition;
        transform.rotation = Quaternion.LookRotation(direction, Vector3.up);
    }

    private bool TryResolveHit(Vector3 probePosition)
    {
        Collider[] hits = Physics.OverlapSphere(probePosition, hitRadius, ~0, QueryTriggerInteraction.Collide);
        for (int i = 0; i < hits.Length; i++)
        {
            Collider hit = hits[i];
            if (ownerRoot != null && (hit.transform == ownerRoot || hit.transform.IsChildOf(ownerRoot)))
            {
                continue;
            }

            if (ownerType == OwnerType.Player)
            {
                ObstacleMover enemy = hit.GetComponentInParent<ObstacleMover>();
                if (enemy != null && enemy.IsAlive)
                {
                    enemy.ApplyDamage(damage);
                    GameManager.Instance?.RegisterPlayerHit();
                    SpawnImpact(probePosition);
                    Destroy(gameObject);
                    return true;
                }
            }
            else
            {
                PlayerCarController player = hit.GetComponentInParent<PlayerCarController>();
                if (player != null)
                {
                    player.ApplyDamage(damage);
                    SpawnImpact(probePosition);
                    Destroy(gameObject);
                    return true;
                }
            }
        }

        return false;
    }

    private void SpawnImpact(Vector3 position)
    {
        var blast = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        blast.name = "ShellImpact";
        blast.transform.position = position;
        blast.transform.localScale = new Vector3(0.45f, 0.45f, 0.45f);
        blast.GetComponent<Renderer>().material = RuntimeMaterialFactory.Create(impactColor);

        Collider col = blast.GetComponent<Collider>();
        if (col != null)
        {
            Destroy(col);
        }

        Destroy(blast, 0.14f);
    }
}
