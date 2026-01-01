using UnityEngine;

public class PuzzleManager : MonoBehaviour, IResetable
{
    public static PuzzleManager instance;
    public PuzzleSlot[] allSlots; // Sahnendeki 9 slotu buraya sürükle
    public int correctCount;
    [SerializeField] private Door door;


    private void Awake() { instance = this; }

    public void CheckCompletion()
    {


        if (correctCount >= 9)
        {
            Debug.Log("COMPLETED");
            door.mainDoor = false;
            door.isLocked = false;
            door.Interact();
        }
    }

    public void ResetOnLoop()
    {
        correctCount = 0;
        foreach (var slot in allSlots)
        {
            slot.currentPiece = null;
        }
    }
}