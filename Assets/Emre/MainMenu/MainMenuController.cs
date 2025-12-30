using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuController : MonoBehaviour
{
    public string gameSceneName = "Game";

    public void StartGame()
    {
        SceneManager.LoadScene(gameSceneName);
    }

    public void ContinueGame()
    {
        // burada save sistemine göre load yaparsın
        SceneManager.LoadScene(gameSceneName);
    }

    public void OpenSettings()
    {
        // Settings panelini aç/kapat mantığı
        Debug.Log("Settings opened");
    }

    public void ExitGame()
    {
        Application.Quit();
        Debug.Log("Quit (Editor'da çalışmaz)");
    }
}
