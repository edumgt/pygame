using UnityEngine;

public class ShieldPickup : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 8f;
    [SerializeField] private float despawnZ = -12f;

    private void Update()
    {
        transform.Translate(Vector3.back * moveSpeed * Time.deltaTime, Space.World);

        if (transform.position.z < despawnZ)
        {
            Destroy(gameObject);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.TryGetComponent<PlayerCarController>(out _))
        {
            return;
        }

        GameManager.Instance?.AcquireShield();
        Destroy(gameObject);
    }
}
