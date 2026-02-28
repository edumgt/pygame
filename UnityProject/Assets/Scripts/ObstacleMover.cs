using UnityEngine;

public class ObstacleMover : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 4f;
    [SerializeField] private float despawnY = -7f;

    public void SetSpeed(float speed)
    {
        moveSpeed = speed;
    }

    private void Update()
    {
        transform.Translate(Vector3.down * moveSpeed * Time.deltaTime);

        if (transform.position.y < despawnY)
        {
            GameManager.Instance?.AddScore(1);
            Destroy(gameObject);
        }
    }
}
