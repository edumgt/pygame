using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("References")]
    [SerializeField] private PlayerCarController player;
    [SerializeField] private ObstacleSpawner obstacleSpawner;
    [SerializeField] private UIController uiController;

    [Header("Score")]
    [SerializeField] private float scoreTickSeconds = 1f;

    private int score;
    private int highScore;
    private bool isGameOver;
    private float scoreTimer;

    public bool HasShield { get; private set; }
    public bool IsGameOver => isGameOver;

    private const string HighScoreKey = "HighScore";

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
            return;
        }

        scoreTimer += Time.deltaTime;
        if (scoreTimer >= scoreTickSeconds)
        {
            scoreTimer = 0f;
            AddScore(1);
        }
    }

    public void StartGame()
    {
        isGameOver = false;
        score = 0;
        scoreTimer = 0f;
        HasShield = false;

        if (player != null)
        {
            player.ResetPlayer();
        }

        if (obstacleSpawner != null)
        {
            obstacleSpawner.ResetSpawner();
            obstacleSpawner.enabled = true;
        }

        uiController?.UpdateScore(score, highScore);
        uiController?.UpdateShield(HasShield);
        uiController?.ShowGameOver(false);
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

        uiController?.UpdateScore(score, highScore);
    }

    public void AcquireShield()
    {
        if (isGameOver)
        {
            return;
        }

        HasShield = true;
        uiController?.UpdateShield(HasShield);
    }

    public void HandleObstacleCollision()
    {
        if (isGameOver)
        {
            return;
        }

        if (HasShield)
        {
            HasShield = false;
            uiController?.UpdateShield(HasShield);
            return;
        }

        GameOver();
    }

    public void GameOver()
    {
        if (isGameOver)
        {
            return;
        }

        isGameOver = true;
        if (obstacleSpawner != null)
        {
            obstacleSpawner.enabled = false;
        }
        uiController?.ShowGameOver(true);
    }

    private void RestartGame()
    {
        foreach (ObstacleMover obstacle in FindObjectsByType<ObstacleMover>(FindObjectsSortMode.None))
        {
            Destroy(obstacle.gameObject);
        }

        foreach (ShieldPickup shield in FindObjectsByType<ShieldPickup>(FindObjectsSortMode.None))
        {
            Destroy(shield.gameObject);
        }

        StartGame();
    }

    private void EnsureRuntimeSetup()
    {
        EnsureMainCamera();
        if (player == null)
        {
            player = FindAnyObjectByType<PlayerCarController>();
        }
        if (player == null)
        {
            player = CreateDefaultPlayer();
        }

        if (obstacleSpawner == null)
        {
            obstacleSpawner = FindAnyObjectByType<ObstacleSpawner>();
        }
        if (obstacleSpawner == null)
        {
            var spawnerGo = new GameObject("ObstacleSpawner");
            obstacleSpawner = spawnerGo.AddComponent<ObstacleSpawner>();
        }

        if (uiController == null)
        {
            uiController = FindAnyObjectByType<UIController>();
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

        cam.orthographic = true;
        cam.orthographicSize = 6f;
        cam.transform.position = new Vector3(0f, 0f, -10f);
        cam.backgroundColor = new Color(0.08f, 0.1f, 0.12f, 1f);
    }

    private static PlayerCarController CreateDefaultPlayer()
    {
        var go = new GameObject("Player");
        go.transform.position = new Vector3(0f, -4f, 0f);

        var renderer = go.AddComponent<SpriteRenderer>();
        renderer.sprite = RuntimeSpriteFactory.GetSquareSprite();
        renderer.color = new Color(0.2f, 0.8f, 1f, 1f);
        go.transform.localScale = new Vector3(0.8f, 1.2f, 1f);

        var collider = go.AddComponent<BoxCollider2D>();
        collider.isTrigger = true;

        var rb = go.AddComponent<Rigidbody2D>();
        rb.gravityScale = 0f;
        rb.bodyType = RigidbodyType2D.Kinematic;
        rb.freezeRotation = true;

        return go.AddComponent<PlayerCarController>();
    }

    private void OnGUI()
    {
        if (uiController != null)
        {
            return;
        }

        GUI.Label(new Rect(12, 8, 300, 24), $"Score: {score}");
        GUI.Label(new Rect(12, 28, 300, 24), $"High Score: {highScore}");
        GUI.Label(new Rect(12, 48, 300, 24), HasShield ? "Shield: READY" : "Shield: NONE");
        if (isGameOver)
        {
            GUI.Label(new Rect(12, 72, 420, 24), "Game Over - Press R to restart");
        }
    }
}
