using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameplayMenuController : MonoBehaviour
{
    public GameObject pauseMenu;
    public GameObject OptionsMenu;
    // Start is called before the first frame update
    void Start()
    {
        Time.timeScale = 1f;
       pauseMenu.SetActive(false);
       OptionsMenu.SetActive(false);
    }


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
}
