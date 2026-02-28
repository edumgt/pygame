using TMPro;
using UnityEngine;

public class UIController : MonoBehaviour
{
    [SerializeField] private TMP_Text scoreText;
    [SerializeField] private TMP_Text highScoreText;
    [SerializeField] private TMP_Text shieldText;
    [SerializeField] private GameObject gameOverPanel;

    public void UpdateScore(int score, int highScore)
    {
        if (scoreText != null)
        {
            scoreText.text = $"Score: {score}";
        }

        if (highScoreText != null)
        {
            highScoreText.text = $"High Score: {highScore}";
        }
    }

    public void UpdateShield(bool hasShield)
    {
        if (shieldText != null)
        {
            shieldText.text = hasShield ? "Shield: READY" : "Shield: NONE";
        }
    }

    public void ShowGameOver(bool show)
    {
        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(show);
        }
    }
}
