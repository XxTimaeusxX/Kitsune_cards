using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LoaderSceneManager : MonoBehaviour
{
    void Start()
    {
        // Start the coroutine to load the next scene after a brief delay
        StartCoroutine(LoadNextSceneRoutine());
    }

    IEnumerator LoadNextSceneRoutine()
    {
        // Wait for one frame (or a very short time like 0.1s)
        // to ensure the scene is fully rendered and the input block UI is active
        yield return null;

        // Load your Main Menu scene
        SceneManager.LoadScene("MainMenu"); // Replace "MainMenuSceneName" with your actual scene name
    }
}
