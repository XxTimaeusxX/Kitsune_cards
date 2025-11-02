using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuController : MonoBehaviour
{
    public void LoadRegularmode()
    {
        SceneManager.LoadScene("RegularModeScene");
    }
    public void LoadBuffOnlyMode()
    {
        GameModeConfig.SetMode(GameMode.BuffAndDebuff);
        // Use the same gameplay scene unless you have a dedicated BuffOnly scene
        SceneManager.LoadScene("RegularModeScene");
    }
    public void LoadBossOnlyMode()
    {
        GameModeConfig.SetMode(GameMode.BossOnly);
        SceneManager.LoadScene("RegularModeScene");
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
