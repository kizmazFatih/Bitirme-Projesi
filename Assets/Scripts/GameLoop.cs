using UnityEngine;

public class GameLoop : MonoBehaviour
{
    public enum GameState
    {
        FirstGame,
        SecondSection,
        ThirdSection,
        LastSection
    }

    public bool isBoxUsed = false;
    public bool allKeysCollected = false;

    

    public void GoLoop()
    {
        
    }
}
