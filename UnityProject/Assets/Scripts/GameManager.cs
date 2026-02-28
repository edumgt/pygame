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

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
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
        obstacleSpawner.enabled = false;
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
}
