using UnityEngine;

public class PlayerCarController : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float normalSpeed = 6f;
    [SerializeField] private float boostSpeed = 10f;
    [SerializeField] private float boostDuration = 0.75f;
    [SerializeField] private float boostCooldown = 4f;

    [Header("Road Bounds")]
    [SerializeField] private float minX = -4f;
    [SerializeField] private float maxX = 4f;

    private float boostTimer;
    private float cooldownTimer;
    private Vector3 initialPosition;

    private void Awake()
    {
        initialPosition = transform.position;
    }

    private void Update()
    {
        if (GameManager.Instance != null && GameManager.Instance.IsGameOver)
        {
            return;
        }

        HandleBoost();
        HandleMovement();
    }

    private void HandleBoost()
    {
        if (boostTimer > 0f)
        {
            boostTimer -= Time.deltaTime;
        }

        if (cooldownTimer > 0f)
        {
            cooldownTimer -= Time.deltaTime;
        }

        if (Input.GetKeyDown(KeyCode.LeftShift) && cooldownTimer <= 0f)
        {
            boostTimer = boostDuration;
            cooldownTimer = boostCooldown;
        }
    }

    private void HandleMovement()
    {
        float input = Input.GetAxisRaw("Horizontal");
        float currentSpeed = boostTimer > 0f ? boostSpeed : normalSpeed;

        Vector3 position = transform.position;
        position.x += input * currentSpeed * Time.deltaTime;
        position.x = Mathf.Clamp(position.x, minX, maxX);
        transform.position = position;
    }

    public void ResetPlayer()
    {
        boostTimer = 0f;
        cooldownTimer = 0f;
        transform.position = initialPosition;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent<ObstacleMover>(out _))
        {
            GameManager.Instance?.HandleObstacleCollision();
        }
    }
}
