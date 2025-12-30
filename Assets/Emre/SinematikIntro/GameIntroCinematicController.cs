using UnityEngine;
using UnityEngine.Playables;

public class GameIntroCinematicController : MonoBehaviour
{
    [Header("Cinematic")]
    public PlayableDirector director;

    [Header("Gameplay")]
    public MonoBehaviour playerController; // karakter kontrol scriptin
    public GameObject gameplayVCam;        // VCam_Gameplay

    [Header("Optional: traffic start")]
    public CarWaypointFollower[] carsToStart;
    public Camera playerRealCamera; // player üstündeki gerçek Camera varsa
    
	public CameraRenderGate cameraGate;


    void Awake()
    {
        if (director != null)
            director.stopped += OnCinematicStopped;
    }

    void Start()
    {
        // DEBUG: gerçekten buraya giriyor mu?
        Debug.Log($"[INTRO] Game loaded. PlayIntro={GameLaunchContext.PlayIntro}, IsContinue={GameLaunchContext.IsContinue}");

        bool shouldPlayIntro = GameLaunchContext.PlayIntro && !GameLaunchContext.IsContinue;

        if (shouldPlayIntro) PlayFromSceneLoad();
        else SkipIntroToGameplay();
    }

    public void PlayFromSceneLoad()
    {
        Debug.Log("[INTRO] Playing intro timeline...");

		if (cameraGate) cameraGate.EnterIntro();
		if (playerRealCamera) playerRealCamera.enabled = false;
        if (playerController) playerController.enabled = false;
        if (gameplayVCam) gameplayVCam.SetActive(false);

        if (carsToStart != null)
            foreach (var car in carsToStart)
                if (car != null) car.SetRunning(true);

        if (director == null)
        {
            Debug.LogError("[INTRO] Director reference is NULL!");
            return;
        }

        director.time = 0;
        director.Play();
    }

    public void SkipIntroToGameplay()
    {
        Debug.Log("[INTRO] Skipping intro -> gameplay");
		if (cameraGate) cameraGate.ExitIntroRestore();

		if (playerRealCamera) playerRealCamera.enabled = true;
        if (playerController) playerController.enabled = true;
        if (gameplayVCam) gameplayVCam.SetActive(true);

        if (carsToStart != null)
            foreach (var car in carsToStart)
                if (car != null) car.SetRunning(true);
    }

    void OnCinematicStopped(PlayableDirector d)
    {
        Debug.Log("[INTRO] Intro finished -> gameplay");
		if (cameraGate) cameraGate.ExitIntroRestore();

        if (playerController) playerController.enabled = true;
        if (gameplayVCam) gameplayVCam.SetActive(true);

        // Aynı session içinde tekrar yüklenirse intro tekrarlanmasın
        GameLaunchContext.PlayIntro = false;
    }

}
