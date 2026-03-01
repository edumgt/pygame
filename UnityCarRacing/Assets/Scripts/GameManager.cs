using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("References")]
    [SerializeField] private PlayerCarController player;
    [SerializeField] private ObstacleSpawner targetSpawner;
    [SerializeField] private UIController uiController;

    [Header("Game Rules")]
    [SerializeField] private int enemyDestroyGoal = 25;
    [SerializeField] private float playerStartHealth = 100f;
    [SerializeField] private int scorePerKill = 100;
    [SerializeField] private float passiveScoreTickSeconds = 1.25f;

    private int score;
    private int highScore;
    private int destroyedTargets;
    private int shotsFired;
    private int successfulHits;

    private float playerHealth;
    private bool isGameOver;
    private bool isVictory;

    private float scoreTimer;
    private float statusTimer;
    private string statusMessage = string.Empty;

    private const string HighScoreKey = "TankShooterHighScore";

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
        if (scoreTimer >= passiveScoreTickSeconds)
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
        playerHealth = Mathf.Max(1f, playerStartHealth);

        scoreTimer = 0f;
        statusTimer = 0f;
        statusMessage = "Open-field combat started";

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

    public void RegisterPlayerShot()
    {
        if (isGameOver)
        {
            return;
        }

        shotsFired++;
        SetStatus("CANNON FIRED");
    }

    public void RegisterPlayerHit()
    {
        if (isGameOver)
        {
            return;
        }

        successfulHits++;
        AddScore(15);
    }

    public void HandleEnemyDestroyed(Vector3 _position)
    {
        if (isGameOver)
        {
            return;
        }

        destroyedTargets++;
        AddScore(scorePerKill);
        SetStatus("ENEMY TANK DESTROYED");

        if (destroyedTargets >= enemyDestroyGoal)
        {
            Victory();
        }
    }

    public void HandlePlayerDamaged(float amount)
    {
        if (isGameOver)
        {
            return;
        }

        playerHealth = Mathf.Max(0f, playerHealth - Mathf.Max(0f, amount));
        SetStatus("PLAYER HIT");

        if (playerHealth <= 0f)
        {
            GameOver();
        }
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

        foreach (MissileProjectile projectile in FindObjectsByType<MissileProjectile>(FindObjectsSortMode.None))
        {
            Destroy(projectile.gameObject);
        }

        StartGame();
    }

    private void EnsureRuntimeSetup()
    {
        EnsureDisplaySettings();
        EnsureEnvironment();

        if (player == null)
        {
            player = FindAnyObjectByType<PlayerCarController>();
        }

        if (player == null)
        {
            player = CreateDefaultPlayer();
        }

        EnsureMainCamera(player);

        if (targetSpawner == null)
        {
            targetSpawner = FindAnyObjectByType<ObstacleSpawner>();
        }

        if (targetSpawner == null)
        {
            var spawnerGo = new GameObject("EnemySpawner");
            targetSpawner = spawnerGo.AddComponent<ObstacleSpawner>();
        }

        if (uiController == null)
        {
            uiController = FindAnyObjectByType<UIController>();
        }
    }

    private void RefreshUI()
    {
        bool shellReady = player != null && player.IsMissileReady;
        bool hasTargetLock = player != null && player.HasLockedTarget;
        int hullHp = Mathf.CeilToInt(playerHealth);
        int currentWave = targetSpawner != null ? targetSpawner.CurrentWave : 1;

        uiController?.UpdateCombat(
            score,
            highScore,
            destroyedTargets,
            enemyDestroyGoal,
            hullHp,
            shotsFired,
            successfulHits,
            shellReady,
            currentWave,
            hasTargetLock);
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

    private static void EnsureMainCamera(PlayerCarController followTarget)
    {
        Camera cam = Camera.main;
        if (cam == null)
        {
            var camGo = new GameObject("Main Camera");
            cam = camGo.AddComponent<Camera>();
            camGo.tag = "MainCamera";
        }

        cam.orthographic = false;
        cam.fieldOfView = 64f;
        cam.transform.position = new Vector3(0f, 5.2f, -7f);
        cam.transform.rotation = Quaternion.Euler(22f, 0f, 0f);
        cam.nearClipPlane = 0.1f;
        cam.farClipPlane = 260f;
        cam.backgroundColor = new Color(0.5f, 0.67f, 0.8f, 1f);

        TankCameraController followCam = cam.GetComponent<TankCameraController>();
        if (followCam == null)
        {
            followCam = cam.gameObject.AddComponent<TankCameraController>();
        }

        if (followTarget != null)
        {
            followCam.SetTarget(followTarget.transform);
        }
    }

    private static void EnsureDisplaySettings()
    {
        if (Application.isEditor)
        {
            return;
        }

        int width = Mathf.Max(1600, Screen.currentResolution.width);
        int height = Mathf.Max(900, Screen.currentResolution.height);
        Screen.fullScreenMode = FullScreenMode.FullScreenWindow;
        Screen.SetResolution(width, height, FullScreenMode.FullScreenWindow);
    }

    private static PlayerCarController CreateDefaultPlayer()
    {
        var go = RuntimeCarFactory.CreateTank(new Vector3(0f, 0.55f, 0f));
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

        float hitRate = shotsFired > 0 ? (float)successfulHits / shotsFired * 100f : 0f;
        int currentWave = targetSpawner != null ? targetSpawner.CurrentWave : 1;

        GUI.Label(new Rect(12, 8, 560, 24), $"Score: {score}   High Score: {highScore}");
        GUI.Label(new Rect(12, 28, 560, 24), $"Kills: {destroyedTargets}/{enemyDestroyGoal}   Wave: {currentWave}");
        GUI.Label(new Rect(12, 48, 560, 24), $"Hull: {Mathf.CeilToInt(playerHealth)}   Cannon: {(player != null && player.IsMissileReady ? "READY" : "RELOADING")}");
        GUI.Label(new Rect(12, 68, 560, 24), $"Shots: {shotsFired}   Hits: {successfulHits}   Accuracy: {hitRate:0}%");
        GUI.Label(new Rect(12, 88, 760, 24), "Controls: Up/Down move, Left/Right steer, Q/E turret turn, Space fire, R restart");

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
