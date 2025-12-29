using DG.Tweening;
using UnityEngine;

public class Cekmece : MonoBehaviour, IInteractable
{
    private bool isOpen = false;
    Vector3 closePosition;
    Vector3 openPosition;
    private void Start()
    {
        closePosition = transform.localPosition;
        openPosition = new Vector3(transform.localPosition.x - 2f, transform.localPosition.y, transform.localPosition.z);
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


}
