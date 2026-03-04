using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Manages all HUD and menu UI elements.
/// Subscribes to GameManager events to keep UI in sync.
/// </summary>
public class UIManager : MonoBehaviour
{
    [Header("HUD")]
    [SerializeField] private TextMeshProUGUI scoreText;
    [SerializeField] private TextMeshProUGUI coinsText;
    [SerializeField] private TextMeshProUGUI livesText;
    [SerializeField] private GameObject hudPanel;

    [Header("Pause Menu")]
    [SerializeField] private GameObject pausePanel;
    [SerializeField] private Button resumeButton;
    [SerializeField] private Button restartButton;

    [Header("Game Over Screen")]
    [SerializeField] private GameObject gameOverPanel;
    [SerializeField] private TextMeshProUGUI finalScoreText;
    [SerializeField] private Button gameOverRestartButton;

    [Header("Level Complete Screen")]
    [SerializeField] private GameObject levelCompletePanel;
    [SerializeField] private TextMeshProUGUI levelScoreText;

    [Header("Animations")]
    [SerializeField] private Animator coinCollectAnimator;

    private static readonly int CollectTrigger = Animator.StringToHash("Collect");

    private void Start()
    {
        SetupButtons();
        SubscribeToGameManager();
        ShowHUD();
    }

    private void SetupButtons()
    {
        resumeButton?.onClick.AddListener(() => GameManager.Instance?.TogglePause());
        restartButton?.onClick.AddListener(() => GameManager.Instance?.RestartGame());
        gameOverRestartButton?.onClick.AddListener(() => GameManager.Instance?.RestartGame());
    }

    private void SubscribeToGameManager()
    {
        if (GameManager.Instance == null) return;

        GameManager.Instance.OnScoreChanged += UpdateScore;
        GameManager.Instance.OnCoinsChanged += UpdateCoins;
        GameManager.Instance.OnLivesChanged += UpdateLives;
        GameManager.Instance.OnGameOver += ShowGameOver;
        GameManager.Instance.OnLevelComplete += ShowLevelComplete;
        GameManager.Instance.OnGameStarted += ShowHUD;
    }

    private void OnDestroy()
    {
        if (GameManager.Instance == null) return;

        GameManager.Instance.OnScoreChanged -= UpdateScore;
        GameManager.Instance.OnCoinsChanged -= UpdateCoins;
        GameManager.Instance.OnLivesChanged -= UpdateLives;
        GameManager.Instance.OnGameOver -= ShowGameOver;
        GameManager.Instance.OnLevelComplete -= ShowLevelComplete;
        GameManager.Instance.OnGameStarted -= ShowHUD;
    }

    // --- HUD Updates ---

    private void UpdateScore(int newScore)
    {
        if (scoreText) scoreText.text = $"Score: {newScore:N0}";
    }

    private void UpdateCoins(int collected)
    {
        if (coinsText)
            coinsText.text = $"Coins: {collected} / {GameManager.Instance.TotalCoins}";

        // Trigger bounce animation
        coinCollectAnimator?.SetTrigger(CollectTrigger);
    }

    private void UpdateLives(int remaining)
    {
        if (livesText) livesText.text = $"Lives: {remaining}";
    }

    // --- Panel Management ---

    private void ShowHUD()
    {
        SetActive(hudPanel, true);
        SetActive(pausePanel, false);
        SetActive(gameOverPanel, false);
        SetActive(levelCompletePanel, false);

        // Refresh all HUD values
        if (GameManager.Instance != null)
        {
            UpdateScore(GameManager.Instance.Score);
            UpdateCoins(GameManager.Instance.CoinsCollected);
            UpdateLives(GameManager.Instance.Lives);
        }
    }

    private void ShowGameOver()
    {
        SetActive(hudPanel, false);
        SetActive(gameOverPanel, true);

        if (finalScoreText && GameManager.Instance != null)
            finalScoreText.text = $"Final Score: {GameManager.Instance.Score:N0}";
    }

    private void ShowLevelComplete()
    {
        SetActive(hudPanel, false);
        SetActive(levelCompletePanel, true);

        if (levelScoreText && GameManager.Instance != null)
            levelScoreText.text = $"Score: {GameManager.Instance.Score:N0}";
    }

    private static void SetActive(GameObject go, bool active)
    {
        if (go != null) go.SetActive(active);
    }
}
