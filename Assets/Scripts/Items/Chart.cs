using UnityEngine;

public class Chart : MonoBehaviour, IInteractable, IResetable
{
  private Rigidbody rb;
  bool isEnable = true;

  private void Start()
  {
    rb = GetComponent<Rigidbody>();
  }

  public void Interact()
  {
    if (isEnable == false) return;
    rb.isKinematic = false;
  }

  public void ResetOnLoop()
  {
    rb.isKinematic = true;
    isEnable = true;
  }

  public bool ShowMyUI()
  {
    return false;
  }
}
