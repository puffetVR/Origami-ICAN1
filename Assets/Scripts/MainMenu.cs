using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    public void LoadLevel(int scene)
    {
        Debug.Log("Loading game...");
        SceneManager.LoadScene(scene);
    }

    public void QuitGame()
    {
        Debug.Log("Quitting...");
        Application.Quit();
    }
}
