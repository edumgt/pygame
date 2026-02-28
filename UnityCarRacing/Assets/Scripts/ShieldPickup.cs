using UnityEngine;

public class ShieldPickup : MonoBehaviour
{
    [SerializeField] private float fallSpeed = 2.5f;
    [SerializeField] private float despawnY = -7f;

    private void Update()
    {
        transform.Translate(Vector3.down * fallSpeed * Time.deltaTime);

        if (transform.position.y < despawnY)
        {
            Destroy(gameObject);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.TryGetComponent<PlayerCarController>(out _))
        {
            return;
        }

        GameManager.Instance?.AcquireShield();
        Destroy(gameObject);
    }
}
