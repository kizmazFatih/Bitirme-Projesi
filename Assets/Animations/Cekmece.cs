using DG.Tweening;
using UnityEngine;

public class Cekmece : MonoBehaviour, IInteractable
{
    private bool isOpen = false;
    [SerializeField] private Vector3 openOffset = new Vector3(-2f, 0, 0);
    Vector3 closePosition;
    Vector3 openPosition;
    private void Start()
    {
        closePosition = transform.localPosition;
        openPosition = closePosition + openOffset;
    }
    public void Interact()
    {
        if (isOpen)
        {
            transform.DOLocalMove(closePosition, 1f);
            isOpen = false;
        }
        else
        {
            transform.DOLocalMove(openPosition, 1f);
            isOpen = true;
        }
    }
    public bool ShowMyUI()
    {
        return true;
    }

    public string GetInteractText()
    {
        return null;
    }
}
