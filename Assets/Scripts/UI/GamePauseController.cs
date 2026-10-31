
using UnityEngine;
using UnityEngine.SceneManagement;

[DisallowMultipleComponent]
public sealed class GamePauseController : MonoBehaviour
{
    [Header("Pause UI")]
    [SerializeField] private GameObject pausePanel;

    [Header("Game References")]
    [SerializeField] private GameManager gameManager;

    [Header("Scene Navigation")]
    [SerializeField] private string mainMenuSceneName = "MainMenu";

    private bool isPaused;
    private bool isExiting;
    private float timeScaleBeforePause = 1f;

    public bool IsPaused => isPaused;

    private void Awake()
    {
        if (gameManager == null)
        {
            gameManager = GetComponent<GameManager>();
        }

        if (pausePanel != null)
        {
            pausePanel.SetActive(false);
        }
    }

    public void PauseGame()
    {
        if (isPaused || isExiting)
        {
            return;
        }

        if (gameManager != null && gameManager.IsGameOver)
        {
            return;
        }

        // Do not interfere with other pause systems,
        // such as upgrade selection or tower restoration.
        if (Time.timeScale <= 0f)
        {
            return;
        }

        if (pausePanel == null)
        {
            Debug.LogError(
                "GamePauseController: Pause Panel is not assigned.",
                this
            );

            return;
        }

        timeScaleBeforePause = Time.timeScale;
        isPaused = true;

        Time.timeScale = 0f;
        pausePanel.SetActive(true);

        Debug.Log("GamePauseController: Game paused.", this);
    }

    public void ResumeGame()
    {
        if (!isPaused || isExiting)
        {
            return;
        }

        isPaused = false;

        if (pausePanel != null)
        {
            pausePanel.SetActive(false);
        }

        if (gameManager != null && gameManager.IsGameOver)
        {
            return;
        }

        Time.timeScale = timeScaleBeforePause;

        Debug.Log("GamePauseController: Game resumed.", this);
    }

    public void ExitToMainMenu()
    {
        if (!isPaused || isExiting)
        {
            return;
        }

        if (string.IsNullOrWhiteSpace(mainMenuSceneName) ||
            !Application.CanStreamedLevelBeLoaded(mainMenuSceneName))
        {
            Debug.LogError(
                "GamePauseController: Main menu scene not found in Build Profiles: " +
                mainMenuSceneName,
                this
            );

            return;
        }

        isExiting = true;
        isPaused = false;

        // Restore game time before changing scenes.
        Time.timeScale = 1f;

        Debug.Log(
            "GamePauseController: Returning to " + mainMenuSceneName,
            this
        );

        SceneManager.LoadScene(
            mainMenuSceneName,
            LoadSceneMode.Single
        );
    }

    private void OnDisable()
    {
        if (!isPaused || isExiting)
        {
            return;
        }

        ResumeGame();
    }
}
