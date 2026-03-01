using TMPro;
using UnityEngine;

public class UIController : MonoBehaviour
{
    [SerializeField] private TMP_Text scoreText;
    [SerializeField] private TMP_Text highScoreText;
    [SerializeField] private TMP_Text objectiveText;
    [SerializeField] private TMP_Text livesText;
    [SerializeField] private TMP_Text fireText;
    [SerializeField] private TMP_Text lockText;
    [SerializeField] private TMP_Text accuracyText;
    [SerializeField] private TMP_Text messageText;
    [SerializeField] private GameObject gameOverPanel;
    [SerializeField] private TMP_Text resultText;

    public void UpdateCombat(
        int score,
        int highScore,
        int destroyed,
        int targetGoal,
        int lives,
        int shots,
        int hits,
        bool hasLock,
        bool missileReady)
    {
        if (scoreText != null)
        {
            scoreText.text = $"Score: {score}";
        }

        if (highScoreText != null)
        {
            highScoreText.text = $"High Score: {highScore}";
        }

        if (objectiveText != null)
        {
            objectiveText.text = $"Targets: {destroyed}/{targetGoal}";
        }

        if (livesText != null)
        {
            livesText.text = $"Lives: {lives}";
        }

        if (fireText != null)
        {
            fireText.text = $"Missile: {(missileReady ? "READY" : "COOLDOWN")}";
        }

        if (lockText != null)
        {
            lockText.text = $"Lock: {(hasLock ? "ON" : "OFF")}";
        }

        if (accuracyText != null)
        {
            float accuracy = shots > 0 ? (float)hits / shots * 100f : 0f;
            accuracyText.text = $"Accuracy: {accuracy:0}% ({hits}/{shots})";
        }
    }

    public void SetTransientMessage(string message)
    {
        if (messageText != null)
        {
            messageText.text = message;
        }
    }

    public void ShowGameOver(bool show, bool victory)
    {
        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(show);
        }

        if (resultText != null)
        {
            resultText.text = show
                ? (victory ? "Mission Complete! Press R to restart" : "Mission Failed! Press R to restart")
                : string.Empty;
        }
    }
}
