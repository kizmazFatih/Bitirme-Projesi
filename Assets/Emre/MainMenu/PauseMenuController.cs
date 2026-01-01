using UnityEngine;
using UnityEngine.SceneManagement;

public class PauseMenuController : MonoBehaviour
{
    public GameObject pauseCanvas; // PauseCanvas

    [Header("Exit Behaviour")]
    public bool exitToMainMenu = true;
    public string mainMenuSceneName = "MainMenu";

    bool paused;

    void Start()
    {
        if (pauseCanvas) pauseCanvas.SetActive(false);
        ResumeTime(); // güvenlik
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (paused) Continue();
            else Pause();
        }
    }

    public void Pause()
    {
        paused = true;
        if (pauseCanvas) pauseCanvas.SetActive(true);
        Time.timeScale = 0f;

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    public void Continue()
    {
        paused = false;
        ResumeTime();
        if (pauseCanvas) pauseCanvas.SetActive(false);

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    public void OpenSettings()
    {
        Debug.Log("Settings opened (Pause)");
        // settings panel aç/kapat mantığını buraya bağlarsın
    }

    public void Exit()
    {
        ResumeTime(); // timeScale 0 ile sahne değiştirme riskini almayalım

        if (exitToMainMenu)
            SceneManager.LoadScene(mainMenuSceneName);
        else
            Application.Quit();
    }

    void ResumeTime()
    {
        Time.timeScale = 1f;
    }
}
