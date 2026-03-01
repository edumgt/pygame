using UnityEngine;

public class ShieldPickup : MonoBehaviour
{
    [SerializeField] private float spinSpeed = 160f;
    [SerializeField] private float lifeSeconds = 2f;

    private void Update()
    {
        transform.Rotate(Vector3.up, spinSpeed * Time.deltaTime, Space.World);
        lifeSeconds -= Time.deltaTime;
        if (lifeSeconds <= 0f)
        {
            Destroy(gameObject);
        }
    }
}
