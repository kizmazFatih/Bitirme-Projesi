using UnityEngine;

[CreateAssetMenu(fileName = "New Puzzle Item", menuName = "Inventory/Puzzle Item")]
public class PuzzleItem : SOItem
{
    [Header("Görsel Ayar")]
    public Material pieceMaterial; // Bu parçaya özel (resmin o kısmı) materyal
    [Header("Puzzle Ayarı")]
    public int correctIndex; // Bu parçanın girmesi gereken doğru yuva (0-8)
}