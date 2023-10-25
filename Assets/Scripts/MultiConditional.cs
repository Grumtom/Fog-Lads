using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Events;

public class MultiConditional : MonoBehaviour
{
    [SerializeField] private GameObject[] conditions;
    [SerializeField] private bool[] met;

    [Header("Settings")] 
    [SerializeField] private bool conditionsMet = false;
    [SerializeField] private bool oneTimeTrigger = true;
    [SerializeField] private bool canTriggerAgain;
    [SerializeField] private bool continuousTrigger = false;

    [SerializeField] private UnityEvent singleTrigger;
    [SerializeField] private UnityEvent repeatTrigger;

    private NetworkManager NetworkManager;
    private bool firstTimeTriggered = false;

    private void Start()
    {
        NetworkManager = FindObjectOfType<NetworkManager>();
        met = new bool[conditions.Length];
    }

    private void FixedUpdate()
    {
        if (NetworkManager.IsServer)
        {
            int metCount = 0;
            
            for (int i = 0; i < conditions.Length; i++)
            {

                if (conditions[i].TryGetComponent<Interactable>(out Interactable interactable))
                {
                    if (interactable.isActive)
                    {
                        met[i] = true;
                        metCount++;
                    }
                    else
                    {
                        met[i] = false;
                    }
                }
                
                if (conditions[i].TryGetComponent<PlacementSlot>(out PlacementSlot placementSlot))
                {
                    if(placementSlot.hasCorrectItem.Value)
                    {
                        met[i] = true;
                        metCount++;
                    }
                    else
                    {
                        met[i] = false;
                    }
                }
            }

            if (metCount == conditions.Length)
            {
                if (oneTimeTrigger && !firstTimeTriggered)
                {
                    singleTrigger.Invoke();
                    firstTimeTriggered = true;
                }

                if (continuousTrigger)
                {
                    repeatTrigger.Invoke();
                }
            }
            else
            {
                if (canTriggerAgain)
                {
                    firstTimeTriggered = false;
                }
            }
        }
    }
}
