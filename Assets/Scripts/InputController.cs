using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class InputController : MonoBehaviour
{
    public static InputController instance;


    public PlayerInputs playerInputs;
    [HideInInspector] public InputActionMap playerActions;
    [HideInInspector] public InputActionMap interactionActions;

    private List<InputActionMap> inputActions = new List<InputActionMap>();

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else if (instance != this)
        {
            Destroy(gameObject);
        }
        
        playerInputs = new PlayerInputs();
        playerActions = playerInputs.Movement;
        interactionActions = playerInputs.Interaction;

        inputActions.Add(interactionActions);
        inputActions.Add(playerActions);
     
    }

    void OnEnable()
    {
        playerInputs.Enable();
    }
    void OnDisable()
    {
        playerInputs.Disable();
    }


    public void Activation(InputActionMap inputActionMap)
    {
        if(inputActionMap == null) return;
        inputActionMap.Enable();
    }

    public void DeActivate(InputActionMap inputActionMap)
    {
        if (inputActionMap == null) return;
        inputActionMap.Disable();
    }




}
