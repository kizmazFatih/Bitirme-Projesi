using DG.Tweening;
using UnityEngine;

public class Carpet : MonoBehaviour, IInteractable
{
    private bool bounce = true;
    public void Interact()
    {
        if (bounce)
        {
            transform.DORotate(new Vector3(0, -12, 0), 1f);
            transform.DOMove(transform.position + transform.right * 2f, 1f);
        }
        else
        {
            transform.DORotate(new Vector3(0, 12, 0), 1f);
            transform.DOMove(transform.position + transform.right * -2f, 1f);
        }
        bounce = !bounce;
    }
    public bool ShowMyUI()
    {
        return false;
    }


}
