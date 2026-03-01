using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("References")]
    [SerializeField] private PlayerCarController player;
    [SerializeField] private ObstacleSpawner targetSpawner;
    [SerializeField] private UIController uiController;

    [Header("Game Rules")]
    [SerializeField] private int targetDestroyGoal = 12;
    [SerializeField] private int baseLives = 3;
    [SerializeField] private float scoreTickSeconds = 1f;

    private int score;
    private int highScore;
    private int destroyedTargets;
    private int livesRemaining;
    private int shotsFired;
    private int successfulHits;

    private bool isGameOver;
    private bool isVictory;
    private float scoreTimer;
    private float statusTimer;
    private string statusMessage = string.Empty;

    private const string HighScoreKey = "TankMissileHighScore";

    public bool IsGameOver => isGameOver;
    public bool IsVictory => isVictory;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void Bootstrap()
    {
        if (FindAnyObjectByType<GameManager>() != null)
        {
            return;
        }

        var go = new GameObject("GameManager");
        go.AddComponent<GameManager>();
    }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        EnsureRuntimeSetup();
    }

    private void Start()
    {
        highScore = PlayerPrefs.GetInt(HighScoreKey, 0);
        StartGame();
    }

    private void Update()
    {
        if (isGameOver)
        {
            if (Input.GetKeyDown(KeyCode.R))
            {
                RestartGame();
            }

            UpdateStatusTimer();
            RefreshUI();
            return;
        }

        scoreTimer += Time.deltaTime;
        if (scoreTimer >= scoreTickSeconds)
        {
            scoreTimer = 0f;
            AddScore(1);
        }

        UpdateStatusTimer();
        RefreshUI();
    }

    public void StartGame()
    {
        isGameOver = false;
        isVictory = false;
        score = 0;
        destroyedTargets = 0;
        shotsFired = 0;
        successfulHits = 0;
        livesRemaining = Mathf.Max(1, baseLives);
        scoreTimer = 0f;
        statusTimer = 0f;
        statusMessage = "Destroy targets inside the aim zone";

        if (player != null)
        {
            player.ResetPlayer();
        }

        if (targetSpawner != null)
        {
            targetSpawner.ResetSpawner();
            targetSpawner.enabled = true;
        }

        RefreshUI();
        uiController?.ShowGameOver(false, false);
    }

    public void RegisterShotFired(bool wasLocked)
    {
        if (isGameOver)
        {
            return;
        }

        shotsFired++;
        if (wasLocked)
        {
            SetStatus("MISSILE LAUNCHED");
        }
        else
        {
            SetStatus("MISS - no target lock");
        }

        RefreshUI();
    }

    public void HandleTargetDestroyed()
    {
        if (isGameOver)
        {
            return;
        }

        destroyedTargets++;
        successfulHits++;
        AddScore(10);
        SetStatus("TARGET DESTROYED");

        if (destroyedTargets >= targetDestroyGoal)
        {
            Victory();
        }
    }

    public void HandleTargetEscaped()
    {
        if (isGameOver)
        {
            return;
        }

        livesRemaining--;
        SetStatus("BASE DAMAGED");

        if (livesRemaining <= 0)
        {
            GameOver();
            return;
        }

        RefreshUI();
    }

    public void AddScore(int value)
    {
        if (isGameOver)
        {
            return;
        }

        score += value;
        if (score > highScore)
        {
            highScore = score;
            PlayerPrefs.SetInt(HighScoreKey, highScore);
            PlayerPrefs.Save();
        }
    }

    public void GameOver()
    {
        if (isGameOver)
        {
            return;
        }

        isGameOver = true;
        isVictory = false;
        if (targetSpawner != null)
        {
            targetSpawner.enabled = false;
        }
        SetStatus("MISSION FAILED");
        uiController?.ShowGameOver(true, false);
    }

    private void Victory()
    {
        if (isGameOver)
        {
            return;
        }

        isGameOver = true;
        isVictory = true;
        if (targetSpawner != null)
        {
            targetSpawner.enabled = false;
        }
        SetStatus("MISSION COMPLETE");
        uiController?.ShowGameOver(true, true);
    }

    private void RestartGame()
    {
        foreach (ObstacleMover target in FindObjectsByType<ObstacleMover>(FindObjectsSortMode.None))
        {
            Destroy(target.gameObject);
        }

        foreach (MissileProjectile missile in FindObjectsByType<MissileProjectile>(FindObjectsSortMode.None))
        {
            Destroy(missile.gameObject);
        }

        StartGame();
    }

    private void EnsureRuntimeSetup()
    {
        EnsureMainCamera();
        EnsureEnvironment();

        if (player == null)
        {
            player = FindAnyObjectByType<PlayerCarController>();
        }

        if (player == null)
        {
            player = CreateDefaultPlayer();
        }

        if (targetSpawner == null)
        {
            targetSpawner = FindAnyObjectByType<ObstacleSpawner>();
        }

        if (targetSpawner == null)
        {
            var spawnerGo = new GameObject("TargetSpawner");
            targetSpawner = spawnerGo.AddComponent<ObstacleSpawner>();
        }

        if (uiController == null)
        {
            uiController = FindAnyObjectByType<UIController>();
        }
    }

    private void RefreshUI()
    {
        bool hasLock = player != null && player.HasTargetLock;
        bool missileReady = player != null && player.IsMissileReady;

        uiController?.UpdateCombat(
            score,
            highScore,
            destroyedTargets,
            targetDestroyGoal,
            livesRemaining,
            shotsFired,
            successfulHits,
            hasLock,
            missileReady);
        uiController?.SetTransientMessage(statusMessage);
    }

    private void SetStatus(string message)
    {
        statusMessage = message;
        statusTimer = 1.25f;
        uiController?.SetTransientMessage(statusMessage);
    }

    private void UpdateStatusTimer()
    {
        if (statusTimer <= 0f)
        {
            return;
        }

        statusTimer -= Time.deltaTime;
        if (statusTimer <= 0f)
        {
            statusMessage = string.Empty;
            uiController?.SetTransientMessage(string.Empty);
        }
    }

    private static void EnsureMainCamera()
    {
        Camera cam = Camera.main;
        if (cam == null)
        {
            var camGo = new GameObject("Main Camera");
            cam = camGo.AddComponent<Camera>();
            camGo.tag = "MainCamera";
        }

        cam.orthographic = false;
        cam.fieldOfView = 60f;
        cam.transform.position = new Vector3(0f, 12.5f, -17f);
        cam.transform.rotation = Quaternion.Euler(34f, 0f, 0f);
        cam.backgroundColor = new Color(0.2f, 0.27f, 0.35f, 1f);
    }

    private static PlayerCarController CreateDefaultPlayer()
    {
        var go = RuntimeCarFactory.CreateTank(new Vector3(0f, 0.55f, -8.5f));
        var controller = go.GetComponent<PlayerCarController>();
        if (controller == null)
        {
            controller = go.AddComponent<PlayerCarController>();
        }

        return controller;
    }

    private static void EnsureEnvironment()
    {
        RuntimeRoadFactory.BuildIfMissing();
    }

    private void OnGUI()
    {
        if (uiController != null)
        {
            return;
        }

        bool hasLock = player != null && player.HasTargetLock;
        bool missileReady = player != null && player.IsMissileReady;
        float hitRate = shotsFired > 0 ? (float)successfulHits / shotsFired * 100f : 0f;

        GUI.Label(new Rect(12, 8, 480, 24), $"Score: {score}   High Score: {highScore}");
        GUI.Label(new Rect(12, 28, 480, 24), $"Destroyed: {destroyedTargets}/{targetDestroyGoal}   Lives: {livesRemaining}");
        GUI.Label(new Rect(12, 48, 480, 24), $"Shots: {shotsFired}   Hits: {successfulHits}   Accuracy: {hitRate:0}%");
        GUI.Label(new Rect(12, 68, 480, 24), $"Lock: {(hasLock ? "ON" : "OFF")}   Missile: {(missileReady ? "READY" : "COOLDOWN")}");
        GUI.Label(new Rect(12, 88, 560, 24), "Controls: Left/Right or A/D move tank, Space fires missile, R restarts");

        if (!string.IsNullOrEmpty(statusMessage))
        {
            GUI.Label(new Rect(12, 112, 560, 24), statusMessage);
        }

        if (isGameOver)
        {
            GUI.Label(new Rect(12, 136, 560, 24), isVictory ? "Mission Complete - Press R to restart" : "Mission Failed - Press R to restart");
        }
    }
}
