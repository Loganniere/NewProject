using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Central game state manager. Handles score, lives, game states,
/// level completion and transitions.
/// </summary>
public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Game Settings")]
    [SerializeField] private int totalCoins = 10;
    [SerializeField] private int startingLives = 3;
    [SerializeField] private float respawnDelay = 2f;
    [SerializeField] private float levelCompleteDelay = 2f;

    [Header("Respawn")]
    [SerializeField] private Transform respawnPoint;

    // State
    private int score;
    private int coinsCollected;
    private int lives;
    private GameState currentState;

    public bool IsPlaying => currentState == GameState.Playing;
    public int Score => score;
    public int CoinsCollected => coinsCollected;
    public int TotalCoins => totalCoins;
    public int Lives => lives;

    public event System.Action<int> OnScoreChanged;
    public event System.Action<int> OnCoinsChanged;
    public event System.Action<int> OnLivesChanged;
    public event System.Action OnGameOver;
    public event System.Action OnLevelComplete;
    public event System.Action OnGameStarted;

    private enum GameState { Menu, Playing, Paused, GameOver, LevelComplete }

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
        StartGame();
    }

    private void Update()
    {
        if (currentState == GameState.Playing && Input.GetKeyDown(KeyCode.P))
        {
            TogglePause();
        }
    }

    public void StartGame()
    {
        score = 0;
        coinsCollected = 0;
        lives = startingLives;
        currentState = GameState.Playing;
        Time.timeScale = 1f;

        OnScoreChanged?.Invoke(score);
        OnCoinsChanged?.Invoke(coinsCollected);
        OnLivesChanged?.Invoke(lives);
        OnGameStarted?.Invoke();
    }

    public void AddScore(int points)
    {
        if (!IsPlaying) return;
        score += points;
        OnScoreChanged?.Invoke(score);
    }

    public void CollectCoin(int value = 100)
    {
        if (!IsPlaying) return;

        coinsCollected++;
        AddScore(value);
        OnCoinsChanged?.Invoke(coinsCollected);

        if (coinsCollected >= totalCoins)
        {
            StartCoroutine(LevelCompleteRoutine());
        }
    }

    public void OnPlayerDied()
    {
        if (currentState != GameState.Playing) return;

        lives--;
        OnLivesChanged?.Invoke(lives);

        if (lives <= 0)
        {
            StartCoroutine(GameOverRoutine());
        }
        else
        {
            StartCoroutine(RespawnRoutine());
        }
    }

    private IEnumerator RespawnRoutine()
    {
        yield return new WaitForSeconds(respawnDelay);
        RespawnPlayer();
    }

    private void RespawnPlayer()
    {
        PlayerController player = PlayerController.Instance;
        if (player == null || respawnPoint == null) return;

        player.transform.position = respawnPoint.position;
        player.transform.rotation = respawnPoint.rotation;

        Rigidbody rb = player.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.velocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            rb.isKinematic = false;
        }

        // Reset dead flag via reflection (keeps PlayerController clean)
        var field = typeof(PlayerController).GetField("isDead",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        field?.SetValue(player, false);
    }

    private IEnumerator LevelCompleteRoutine()
    {
        currentState = GameState.LevelComplete;
        OnLevelComplete?.Invoke();
        yield return new WaitForSeconds(levelCompleteDelay);
        // Reload scene or go to next level
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    private IEnumerator GameOverRoutine()
    {
        currentState = GameState.GameOver;
        OnGameOver?.Invoke();
        yield return new WaitForSeconds(3f);
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void TogglePause()
    {
        if (currentState == GameState.Playing)
        {
            currentState = GameState.Paused;
            Time.timeScale = 0f;
        }
        else if (currentState == GameState.Paused)
        {
            currentState = GameState.Playing;
            Time.timeScale = 1f;
        }
    }

    public void RestartGame()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}
