using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class TankAudioController : MonoBehaviour
{
    [Header("Engine")]
    [SerializeField] private float idleVolume = 0.2f;
    [SerializeField] private float runVolume = 0.75f;
    [SerializeField] private float idlePitch = 0.78f;
    [SerializeField] private float runPitch = 1.7f;

    [Header("Cannon")]
    [SerializeField] private float cannonVolume = 0.95f;
    [SerializeField] private float hitVolume = 0.72f;

    private AudioSource engineSource;
    private PlayerCarController controller;
    private float smoothedSpeed;
    private float smoothedTurn;

    public void ResetAudioState()
    {
        smoothedSpeed = 0f;
        smoothedTurn = 0f;
        if (engineSource != null)
        {
            engineSource.pitch = idlePitch;
            engineSource.volume = idleVolume;
        }
    }

    public void PlayCannon(Vector3 worldPosition)
    {
        RuntimeAudioFactory.PlayOneShotAt(worldPosition, RuntimeAudioFactory.GetPlayerShotClip(), cannonVolume, 1f, 0.97f, 1.04f);
    }

    public void PlayHullHit(Vector3 worldPosition)
    {
        RuntimeAudioFactory.PlayOneShotAt(worldPosition, RuntimeAudioFactory.GetImpactClip(), hitVolume, 1f, 0.95f, 1.06f);
    }

    private void Awake()
    {
        controller = GetComponent<PlayerCarController>();
        engineSource = GetComponent<AudioSource>();

        engineSource.playOnAwake = true;
        engineSource.loop = true;
        engineSource.spatialBlend = 1f;
        engineSource.rolloffMode = AudioRolloffMode.Linear;
        engineSource.minDistance = 4f;
        engineSource.maxDistance = 68f;
        engineSource.volume = idleVolume;
        engineSource.pitch = idlePitch;
        engineSource.clip = RuntimeAudioFactory.GetEngineLoopClip();
        engineSource.Play();
    }

    private void Update()
    {
        if (engineSource == null || controller == null)
        {
            return;
        }

        float dt = Time.deltaTime;
        float t = 1f - Mathf.Exp(-6f * dt);

        float targetSpeed = Mathf.Clamp01(controller.CurrentHullSpeedAbs / 12f);
        float targetTurn = controller.CurrentTurnMagnitude;
        smoothedSpeed = Mathf.Lerp(smoothedSpeed, targetSpeed, t);
        smoothedTurn = Mathf.Lerp(smoothedTurn, targetTurn, t * 0.7f);

        float dynamic = Mathf.Clamp01(smoothedSpeed + smoothedTurn * 0.35f);
        engineSource.volume = Mathf.Lerp(idleVolume, runVolume, dynamic);
        engineSource.pitch = Mathf.Lerp(idlePitch, runPitch, dynamic);

        bool gameOver = GameManager.Instance != null && GameManager.Instance.IsGameOver;
        if (!engineSource.isPlaying && !gameOver)
        {
            engineSource.Play();
        }
    }
}
