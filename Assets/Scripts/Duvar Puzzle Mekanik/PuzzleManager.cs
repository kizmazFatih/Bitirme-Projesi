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

        /*foreach (var slot in allSlots)
        {
            if (slot.currentPiece != null)
            {
                // Parçanın içindeki correctIndex ile slotun slotIndex'i uyuşuyor mu?
                if (slot.currentPiece.itemData.correctIndex == slot.slotIndex)
                {
                    correctCount++;
                    Debug.Log("correctCount: " + correctCount);
                }
            }
        }*/

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