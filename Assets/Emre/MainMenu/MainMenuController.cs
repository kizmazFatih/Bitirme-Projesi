using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuController : MonoBehaviour
{
    public string gameSceneName = "Game";

    public void StartGame()
{
    GameLaunchContext.PlayIntro = true;
    GameLaunchContext.IsContinue = false;

    Debug.Log($"[MENU] StartGame clicked. PlayIntro={GameLaunchContext.PlayIntro}, IsContinue={GameLaunchContext.IsContinue}");

    SceneManager.LoadScene(gameSceneName);
}


    public void ContinueGame()
    {
        // genelde continue'da intro oynatmayız
        GameLaunchContext.PlayIntro = false;
        GameLaunchContext.IsContinue = true;

        SceneManager.LoadScene(gameSceneName);
    }

    public void OpenSettings()
    {
        Debug.Log("Settings opened");
    }

    public void ExitGame()
    {
        Application.Quit();
        Debug.Log("Quit (Editor'da çalışmaz)");
    }
}
