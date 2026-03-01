using UnityEngine;

public class MissileProjectile : MonoBehaviour
{
    [SerializeField] private float speed = 24f;
    [SerializeField] private float maxLifetime = 2.6f;
    [SerializeField] private float hitRadius = 0.9f;
    [SerializeField] private float turnRate = 8f;

    private float lifetimeRemaining;
    private Vector3 travelDirection = Vector3.forward;
    private ObstacleMover lockedTarget;

    public static MissileProjectile Create(Vector3 position, Vector3 direction, ObstacleMover target)
    {
        var go = GameObject.CreatePrimitive(PrimitiveType.Capsule);
        go.name = "Missile";
        go.transform.position = position;
        go.transform.localScale = new Vector3(0.18f, 0.45f, 0.18f);

        Vector3 heading = direction.sqrMagnitude > 0.001f ? direction.normalized : Vector3.forward;
        go.transform.rotation = Quaternion.LookRotation(heading, Vector3.up);

        var renderer = go.GetComponent<Renderer>();
        if (renderer != null)
        {
            renderer.material = RuntimeMaterialFactory.Create(new Color(1f, 0.45f, 0.2f, 1f));
        }

        Collider col = go.GetComponent<Collider>();
        if (col != null)
        {
            Object.Destroy(col);
        }

        var projectile = go.AddComponent<MissileProjectile>();
        projectile.Initialize(heading, target);
        return projectile;
    }

    public void Initialize(Vector3 direction, ObstacleMover target)
    {
        travelDirection = direction.sqrMagnitude > 0.001f ? direction.normalized : Vector3.forward;
        lockedTarget = target;
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

        Vector3 desiredDirection = travelDirection;
        if (lockedTarget != null && lockedTarget.IsAlive)
        {
            Vector3 toTarget = lockedTarget.transform.position - transform.position;
            if (toTarget.sqrMagnitude > 0.001f)
            {
                desiredDirection = toTarget.normalized;
            }
        }

        travelDirection = Vector3.Slerp(travelDirection, desiredDirection, turnRate * Time.deltaTime).normalized;
        transform.position += travelDirection * speed * Time.deltaTime;
        transform.rotation = Quaternion.LookRotation(travelDirection, Vector3.up);

        if (lockedTarget == null || !lockedTarget.IsAlive)
        {
            return;
        }

        float sqrDistance = (lockedTarget.transform.position - transform.position).sqrMagnitude;
        if (sqrDistance <= hitRadius * hitRadius)
        {
            lockedTarget.ApplyMissileHit();
            Destroy(gameObject);
        }
    }
}
