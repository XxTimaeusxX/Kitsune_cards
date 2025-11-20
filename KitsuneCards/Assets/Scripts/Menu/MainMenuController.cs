using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuController : MonoBehaviour
{
    public void Start()
    {
        Time.timeScale = 1;
        // No direct PlayMenuMusic here; AudioManager will handle it on scene load
    }

    public void LoadRegularmode()
    {
        Time.timeScale = 1;
        GameModeConfig.SetMode(GameMode.Regular);
        GameModeConfig.SetUpgradesEnabled(false);
        SceneManager.LoadScene("RegularModeScene");
    }

    public void LoadBuffOnlyMode()
    {
        Time.timeScale = 1;
        GameModeConfig.SetMode(GameMode.RogueLike);
        GameModeConfig.SetUpgradesEnabled(true);
        SceneManager.LoadScene("RegularModeScene");
    }

    public void LoadBossOnlyMode()
    {
        Time.timeScale = 1;
        GameModeConfig.SetMode(GameMode.BossOnly);
        GameModeConfig.SetUpgradesEnabled(false);
        SceneManager.LoadScene("RegularModeScene");
    }

    public void LoadCredits()
    {
        Time.timeScale = 1;
        SceneManager.LoadScene("Credits");
    }

    public void LoadMainMenu()
    {
        Time.timeScale = 1;
        SceneManager.LoadScene("MainMenu");
    }

    public void QuitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}
