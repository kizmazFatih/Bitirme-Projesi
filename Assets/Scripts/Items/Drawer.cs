using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Drawer : MonoBehaviour , IInteractable
{
    private Animator animator;
    private bool isOpen ;
    public bool inverse;
    void Start()
    {
        animator = transform.GetComponent<Animator>();
        if(inverse)
        {
          animator.SetBool("isOpen",true);
          animator.SetBool("isOpen",true);
        }
    }

    public void Interact()
    {
      

      isOpen = !isOpen;
      if(inverse)
      animator.SetBool("isOpen",!isOpen);
      else
      animator.SetBool("isOpen",isOpen);
    }
    public bool ShowMyUI()
    {
        return true;
    }
}
