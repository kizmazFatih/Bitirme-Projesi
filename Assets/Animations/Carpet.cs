using DG.Tweening;
using UnityEngine;

public class Carpet : MonoBehaviour, IInteractable
{
    private bool bounce = true;

    [SerializeField] private Vector3 offset;
    private Vector3 normalPosition;
    private Vector3 nextPosition;


    private void Awake()
    {
        normalPosition = transform.localPosition;
        nextPosition = normalPosition + offset;
    }
    public void Interact()
    {
        if (bounce)
        {
            transform.DOLocalRotate(new Vector3(0, -12, 0), 1f);
            transform.DOLocalMove(nextPosition, 1f);
        }
        else
        {
            transform.DOLocalRotate(new Vector3(0, 12, 0), 1f);
            transform.DOLocalMove(normalPosition, 1f);
        }
        bounce = !bounce;
    }
    public bool ShowMyUI()
    {
        return false;
    }
    public string GetInteractText()
    {
        return null;
    }


}
