using System.Collections;
using UnityEngine;
using UnityEngine.Playables;

public class GameIntroCinematicController : MonoBehaviour
{
    [Header("Cinematic")]
    public PlayableDirector director;

    [Header("Gameplay")]
    public MonoBehaviour playerController;   // karakter kontrol scriptin (FPS/TP)
    public GameObject gameplayVCam;          // VCam_Gameplay (GameObject)

    [Header("Optional: traffic start")]
    public CarWaypointFollower[] carsToStart;

    // Eğer daha önce eklediysen, intro boyunca diğer kameraları "render etmesin" diye kullanılır (opsiyonel)
    public CameraRenderGate cameraGate;

    [Header("Intro Audio")]
    public AudioSource carEngineSource;      // hero car üzerindeki AudioSource
    public AudioSource cityAmbienceSource;   // ambience AudioSource

    [Header("Window Fade (Timeline seconds)")]
    public float windowFadeStart = 16f;
    public float windowFadeEnd = 22f;
    public float carVolumeAtWindow = 0.25f;   // 16-22 arası hedef volume
    public float cityVolumeAtWindow = 0.35f;  // 16-22 arası hedef volume

    [Header("End Fade Out")]
    public float endFadeOutTime = 0.6f;

    float _carBaseVol = 1f;
    float _cityBaseVol = 1f;

    void Awake()
    {
        if (director != null)
            director.stopped += OnCinematicStopped;

        if (carEngineSource) _carBaseVol = carEngineSource.volume;
        if (cityAmbienceSource) _cityBaseVol = cityAmbienceSource.volume;
    }

    void OnDestroy()
    {
        if (director != null)
            director.stopped -= OnCinematicStopped;
    }

    void Start()
    {
        // Menu’den geldiyse: GameLaunchContext ile karar verir
        // (Editor’da direkt Game scene açarsan default PlayIntro=true ise intro oynar)
        bool shouldPlayIntro = GameLaunchContext.PlayIntro && !GameLaunchContext.IsContinue;

        if (shouldPlayIntro) PlayFromSceneLoad();
        else SkipIntroToGameplay();
    }

    public void PlayFromSceneLoad()
    {
        if (cameraGate) cameraGate.EnterIntro();

        // Gameplay kapalı
        if (playerController) playerController.enabled = false;
        if (gameplayVCam) gameplayVCam.SetActive(false);

        // Trafik başlat
        if (carsToStart != null)
            foreach (var car in carsToStart)
                if (car != null) car.SetRunning(true);

        // Intro seslerini başlat
        if (carEngineSource)
        {
            if (_carBaseVol <= 0f) _carBaseVol = Mathf.Max(0.01f, carEngineSource.volume);
            carEngineSource.volume = _carBaseVol;
            if (!carEngineSource.isPlaying) carEngineSource.Play();
        }

        if (cityAmbienceSource)
        {
            if (_cityBaseVol <= 0f) _cityBaseVol = Mathf.Max(0.01f, cityAmbienceSource.volume);
            cityAmbienceSource.volume = _cityBaseVol;
            if (!cityAmbienceSource.isPlaying) cityAmbienceSource.Play();
        }

        if (director == null)
        {
            Debug.LogError("[INTRO] Director reference is NULL!");
            return;
        }

        // Timeline'ı başlat
        director.time = 0;
        director.Play();

        // 16-22 saniye arası pencereden içeri girerken sesleri azalt
        StopAllCoroutines();
        StartCoroutine(FadeIntroAudioDuringWindow());
    }

    public void SkipIntroToGameplay()
    {
        StopAllCoroutines();

        if (cameraGate) cameraGate.ExitIntroRestore();

        // Intro sesleri varsa kapat (continue'da genelde hiç başlamaz ama güvenli)
        StopIntroAudioImmediate();

        // Gameplay aç
        if (playerController) playerController.enabled = true;
        if (gameplayVCam) gameplayVCam.SetActive(true);
        Clocks.instance.timeStopped = false;


        // Trafik yine başlasın istersen
        if (carsToStart != null)
            foreach (var car in carsToStart)
                if (car != null) car.SetRunning(true);
    }

    void OnCinematicStopped(PlayableDirector d)
    {
        StopAllCoroutines();

        if (cameraGate) cameraGate.ExitIntroRestore();

        // Intro bitince sesleri fade-out ile kapat
        if (carEngineSource) StartCoroutine(FadeOutAndStop(carEngineSource, endFadeOutTime, _carBaseVol));
        if (cityAmbienceSource) StartCoroutine(FadeOutAndStop(cityAmbienceSource, endFadeOutTime, _cityBaseVol));

        // Gameplay devral
        if (playerController) playerController.enabled = true;
        if (gameplayVCam) gameplayVCam.SetActive(true);
        Clocks.instance.timeStopped = false;


        // Aynı session’da tekrar load olursa intro tekrar oynamasın diye
        GameLaunchContext.PlayIntro = false;
    }

    IEnumerator FadeIntroAudioDuringWindow()
    {
        if (director == null) yield break;

        float duration = Mathf.Max(0.01f, windowFadeEnd - windowFadeStart);

        // director.time 16 sn olana kadar bekle
        while (director.state == PlayState.Playing && director.time < windowFadeStart)
            yield return null;

        if (director.state != PlayState.Playing) yield break;

        float carStart = carEngineSource ? carEngineSource.volume : 0f;
        float cityStart = cityAmbienceSource ? cityAmbienceSource.volume : 0f;

        float t = 0f;
        while (director.state == PlayState.Playing && t < duration)
        {
            t += Time.deltaTime;
            float a = Mathf.Clamp01(t / duration);

            if (carEngineSource)
                carEngineSource.volume = Mathf.Lerp(carStart, carVolumeAtWindow, a);

            if (cityAmbienceSource)
                cityAmbienceSource.volume = Mathf.Lerp(cityStart, cityVolumeAtWindow, a);

            yield return null;
        }
    }

    IEnumerator FadeOutAndStop(AudioSource src, float t, float restoreVol)
    {
        if (src == null) yield break;

        float start = src.volume;
        float time = 0f;

        while (time < t)
        {
            time += Time.deltaTime;
            src.volume = Mathf.Lerp(start, 0f, time / t);
            yield return null;
        }

        src.Stop();
        src.volume = restoreVol; // testlerde tekrar düzgün başlasın diye
    }

    void StopIntroAudioImmediate()
    {
        if (carEngineSource)
        {
            carEngineSource.Stop();
            carEngineSource.volume = _carBaseVol;
        }

        if (cityAmbienceSource)
        {
            cityAmbienceSource.Stop();
            cityAmbienceSource.volume = _cityBaseVol;
        }
    }
}
