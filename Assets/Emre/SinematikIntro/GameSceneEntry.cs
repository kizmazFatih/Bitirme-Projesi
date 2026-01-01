using UnityEngine;

public class GameSceneEntry : MonoBehaviour
{
    public GameIntroCinematicController introController;

    void Start()
    {
        // Continue ise veya intro istemiyorsak direkt gameplay
        if (!GameLaunchContext.PlayIntro || GameLaunchContext.IsContinue)
        {
            introController.SkipIntroToGameplay();
            return;
        }

        // New game: intro oyna
        introController.PlayFromSceneLoad();
    }
}
