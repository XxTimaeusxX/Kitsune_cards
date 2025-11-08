using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuController : MonoBehaviour
{

    public GameObject pauseMenu;
    public GameObject OptionsMenu;
    public void Start()
    {
        Time.timeScale = 1;
        pauseMenu.SetActive(false);
        OptionsMenu.SetActive(false);
    }
    public void LoadRegularmode()
    {
        Time.timeScale = 1;
        SceneManager.LoadScene("RegularModeScene");
    }
    public void LoadBuffOnlyMode()
    {
        Time.timeScale = 1;
        GameModeConfig.SetMode(GameMode.BuffAndDebuff);
        // Use the same gameplay scene unless you have a dedicated BuffOnly scene
        SceneManager.LoadScene("RegularModeScene");
    }
    public void LoadBossOnlyMode()
    {
        Time.timeScale = 1;
        GameModeConfig.SetMode(GameMode.BossOnly);
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
    /////////////// Pause Menu Functions //////////////
    public void PauseGame()
    {
        pauseMenu.SetActive(true);
        OptionsMenu.SetActive(false);
        Time.timeScale = 0f; // Pause the game
    }
    public void ResumeGame()
    {
        pauseMenu.SetActive(false);
        Time.timeScale = 1; // Resume the game
    }
    public void LoadOptions()
    {
        pauseMenu.SetActive(false);
        OptionsMenu.SetActive(true);
    }
    // Optional: Quit button
    public void QuitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}
