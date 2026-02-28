using UnityEngine;

public class RuntimeLoopMover : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 12f;
    [SerializeField] private float minZ = -32f;
    [SerializeField] private float maxZ = 32f;

    private void Update()
    {
        if (GameManager.Instance != null && GameManager.Instance.IsGameOver)
        {
            return;
        }

        Vector3 p = transform.position;
        p.z -= moveSpeed * Time.deltaTime;
        if (p.z < minZ)
        {
            p.z += (maxZ - minZ);
        }

        transform.position = p;
    }

    public void Configure(float speed, float loopMinZ, float loopMaxZ)
    {
        moveSpeed = speed;
        minZ = loopMinZ;
        maxZ = loopMaxZ;
    }
}
