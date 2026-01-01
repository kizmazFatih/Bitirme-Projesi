using JetBrains.Annotations;
using UnityEngine;

[CreateAssetMenu(fileName = "New Item", menuName = "ScriptableObjects/Item")]
public class SOItem : ScriptableObject
{

    public bool isPlaceable;
    public Texture my_image;
    public GameObject my_prefab;
    public int my_amount;
    public int max_stack;
    public string my_tooltip;

    [Header("Elde Tutma Ayarları")]
    public Vector3 heldPosition; // Elimizdeki lokal pozisyonu
    public Vector3 heldRotation; // Elimizdeki lokal rotasyonu
    public Vector3 heldScale = Vector3.one; // Elimizdeki ölçeği (Örn: Anahtar için 5,5,5 gibi)

}
