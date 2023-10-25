using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Events;

public class Interactable : NetworkBehaviour
{
    [Header("Settings")] 
    [SerializeField] private bool continuousTrigger = false;
    public bool oneTimeTrigger = true;
    public bool canTurnOff = false;
    [SerializeField] private bool animationBoolControl;
    [SerializeField] private float resetTime = 0;
    [SerializeField] private float resetTimer;

    [Header("References")]
    public UnityEvent interact;
    public UnityEvent cancel;
    public bool isActive = false;
    [SerializeField] private Animator Animator;

    private void FixedUpdate()
    {
        if (IsServer)
        {
            if (resetTime > 0)
            {
                if (resetTimer > 0)
                {
                    resetTimer-= Time.fixedDeltaTime;
                    if (resetTimer <= 0)
                    {
                        isActive = false;
                    }
                }
            }
            
            if (continuousTrigger)
            {
                if (isActive)
                {
                    interact.Invoke();
                }
                else
                {
                    cancel.Invoke();
                }
            }


            if (animationBoolControl)
            {
                if (isActive)
                {
                    Animator.SetBool("on",true);
                }
                else
                {
                    Animator.SetBool("on",false);
                }
            }
        }
    }

    public void triggerEvent()
    {
        resetTimer = resetTime;
        if (oneTimeTrigger)
        {
            interact.Invoke();
        }

        if (continuousTrigger)
        {
            if (canTurnOff)
            {
                isActive = !isActive;
            }
            else
            {
                isActive = true;
            }
        }
    }
}
