using UnityEngine;

[RequireComponent(typeof(Camera))]
public class TankCameraController : MonoBehaviour
{
    [Header("Follow")]
    [SerializeField] private Transform target;
    [SerializeField] private Vector3 followOffset = new Vector3(0f, 4.1f, -7.4f);
    [SerializeField] private float lookAheadDistance = 5.3f;

    [Header("Smoothing")]
    [SerializeField] private float positionSmooth = 8f;
    [SerializeField] private float rotationSmooth = 8f;

    [Header("FOV")]
    [SerializeField] private float baseFov = 64f;
    [SerializeField] private float speedFovBoost = 7f;

    private Camera cachedCamera;
    private Vector3 lastTargetPosition;
    private bool hasLastPosition;

    public void SetTarget(Transform followTarget)
    {
        target = followTarget;
        hasLastPosition = false;
    }

    private void Awake()
    {
        cachedCamera = GetComponent<Camera>();
        if (cachedCamera != null)
        {
            cachedCamera.orthographic = false;
            cachedCamera.fieldOfView = baseFov;
        }
    }

    private void LateUpdate()
    {
        if (target == null)
        {
            PlayerCarController player = FindAnyObjectByType<PlayerCarController>();
            if (player == null)
            {
                return;
            }

            target = player.transform;
            hasLastPosition = false;
        }

        Vector3 desiredPosition = target.TransformPoint(followOffset);
        float posT = 1f - Mathf.Exp(-positionSmooth * Time.deltaTime);
        transform.position = Vector3.Lerp(transform.position, desiredPosition, posT);

        Vector3 lookPoint = target.position + target.forward * lookAheadDistance + Vector3.up * 1.2f;
        Quaternion desiredRotation = Quaternion.LookRotation(lookPoint - transform.position, Vector3.up);
        float rotT = 1f - Mathf.Exp(-rotationSmooth * Time.deltaTime);
        transform.rotation = Quaternion.Slerp(transform.rotation, desiredRotation, rotT);

        if (cachedCamera == null)
        {
            return;
        }

        float targetSpeed = 0f;
        if (hasLastPosition)
        {
            targetSpeed = (target.position - lastTargetPosition).magnitude / Mathf.Max(0.0001f, Time.deltaTime);
        }

        lastTargetPosition = target.position;
        hasLastPosition = true;

        float speedFactor = Mathf.InverseLerp(0f, 12f, targetSpeed);
        float wantedFov = baseFov + speedFactor * speedFovBoost;
        cachedCamera.fieldOfView = Mathf.Lerp(cachedCamera.fieldOfView, wantedFov, 1f - Mathf.Exp(-7f * Time.deltaTime));
    }
}
