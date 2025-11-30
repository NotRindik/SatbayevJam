using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuMover : MonoBehaviour
{
    /// <summary>
    /// Loads a scene by its name.
    /// </summary>
    /// <param name="sceneName">The name of the scene to load.</param>
    public void LoadScene(string sceneName)
    {
        PlayerPrefs.SetInt("IsGaming", 0);
        PlayerPrefs.Save();

        SceneManager.LoadScene(sceneName);
    }
    /// <summary>
    /// Closes the game application.
    /// </summary>
    public void QuitGame()
    {
        Application.Quit();
    }
    
}