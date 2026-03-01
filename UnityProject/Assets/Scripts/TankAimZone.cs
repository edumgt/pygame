using System.Collections.Generic;
using UnityEngine;

public class TankAimZone : MonoBehaviour
{
    [SerializeField] private float defaultRadius = 1.6f;

    private SphereCollider zoneCollider;
    private readonly List<ObstacleMover> trackedTargets = new List<ObstacleMover>();

    public bool HasTarget
    {
        get
        {
            CleanupMissingTargets();
            return trackedTargets.Count > 0;
        }
    }

    private void Awake()
    {
        zoneCollider = GetComponent<SphereCollider>();
        if (zoneCollider == null)
        {
            zoneCollider = gameObject.AddComponent<SphereCollider>();
            zoneCollider.radius = defaultRadius;
        }

        zoneCollider.isTrigger = true;
    }

    private void OnDisable()
    {
        trackedTargets.Clear();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.TryGetComponent(out ObstacleMover target) || !target.IsAlive)
        {
            return;
        }

        if (!trackedTargets.Contains(target))
        {
            trackedTargets.Add(target);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.TryGetComponent(out ObstacleMover target))
        {
            return;
        }

        trackedTargets.Remove(target);
    }

    public bool TryGetLockedTarget(out ObstacleMover target)
    {
        CleanupMissingTargets();
        target = null;
        if (trackedTargets.Count == 0)
        {
            return false;
        }

        float bestDistance = float.MaxValue;
        Vector3 zoneCenter = transform.position;
        for (int i = 0; i < trackedTargets.Count; i++)
        {
            ObstacleMover candidate = trackedTargets[i];
            if (candidate == null || !candidate.IsAlive)
            {
                continue;
            }

            float sqrDistance = (candidate.transform.position - zoneCenter).sqrMagnitude;
            if (sqrDistance < bestDistance)
            {
                bestDistance = sqrDistance;
                target = candidate;
            }
        }

        return target != null;
    }

    public void Clear()
    {
        trackedTargets.Clear();
    }

    private void CleanupMissingTargets()
    {
        trackedTargets.RemoveAll(entry => entry == null || !entry.IsAlive);
    }
}
