using UnityEngine;

public class PuzzleManager : MonoBehaviour
{
    public static PuzzleManager instance;
    public PuzzleSlot[] allSlots; // Sahnendeki 9 slotu buraya sürükle
    public int correctCount;


    private void Awake() { instance = this; }

    public void CheckCompletion()
    {

        foreach (var slot in allSlots)
        {
            if (slot.currentPiece != null)
            {
                // Parçanın içindeki correctIndex ile slotun slotIndex'i uyuşuyor mu?
                if (slot.currentPiece.itemData.correctIndex == slot.slotIndex)
                {
                    correctCount++;

                }
            }
        }

        if (correctCount == 9)
        {
            Debug.Log("COMPLETED");
            // Buraya kapı açılma veya ses çalma kodlarını ekleyebilirsin.
        }
    }
}