using UnityEngine;

public class ObstacleMover : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 6f;
    [SerializeField] private float escapeZ = -11.2f;

    private bool isAlive = true;

    public bool IsAlive => isAlive;

    public void SetSpeed(float speed)
    {
        moveSpeed = Mathf.Max(1f, speed);
    }

    private void Update()
    {
        if (!isAlive)
        {
            return;
        }

        transform.Translate(Vector3.back * moveSpeed * Time.deltaTime, Space.World);

        if (transform.position.z < escapeZ)
        {
            ReachBase();
        }
    }

    public void ApplyMissileHit()
    {
        if (!isAlive)
        {
            return;
        }

        isAlive = false;
        SpawnImpact(Color.yellow, 0.65f);
        GameManager.Instance?.HandleTargetDestroyed();
        Destroy(gameObject);
    }

    public void ReachBase()
    {
        if (!isAlive)
        {
            return;
        }

        isAlive = false;
        SpawnImpact(new Color(1f, 0.35f, 0.2f, 1f), 0.9f);
        GameManager.Instance?.HandleTargetEscaped();
        Destroy(gameObject);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!isAlive)
        {
            return;
        }

        if (other.TryGetComponent<PlayerCarController>(out _))
        {
            ReachBase();
        }
    }

    private void SpawnImpact(Color color, float scale)
    {
        var blast = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        blast.name = "Impact";
        blast.transform.position = transform.position;
        blast.transform.localScale = new Vector3(scale, scale, scale);
        blast.GetComponent<Renderer>().material = RuntimeMaterialFactory.Create(color);

        Collider col = blast.GetComponent<Collider>();
        if (col != null)
        {
            Destroy(col);
        }

        Destroy(blast, 0.12f);
    }
}
