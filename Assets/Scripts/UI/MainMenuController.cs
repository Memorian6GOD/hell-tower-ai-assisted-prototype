
using UnityEngine;
using UnityEngine.SceneManagement;

[DisallowMultipleComponent]
public sealed class MainMenuController : MonoBehaviour
{
    [Header("Scene Navigation")]
    [SerializeField] private string gameSceneName = "Map01_Training";

    private bool isLoadingScene;

    public void PlayGame()
    {
        // Prevent multiple clicks during scene loading.
        if (isLoadingScene)
        {
            return;
        }

        // Check that the scene is included in Build Profiles.
        if (string.IsNullOrWhiteSpace(gameSceneName) ||
            !Application.CanStreamedLevelBeLoaded(gameSceneName))
        {
            Debug.LogError(
                "MainMenuController: Game scene not found in Build Profiles: " +
                gameSceneName,
                this
            );

            return;
        }

        isLoadingScene = true;

        // A new run must always start with normal game time.
        Time.timeScale = 1f;

        Debug.Log(
            "MainMenuController: Starting scene " + gameSceneName,
            this
        );

        SceneManager.LoadScene(
            gameSceneName,
            LoadSceneMode.Single
        );
    }
}
