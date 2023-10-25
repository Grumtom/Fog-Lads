using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Events;

public class PlacementSlot : NetworkBehaviour
{
    [Header("Settings")]
    public string requredItem = "Pickup"; 
    public bool rejectWrongItem = true;
    
    [SerializeField] private bool oneTimeEvent = true;
    [SerializeField] private bool continuousEvent = false;
    [SerializeField] private bool canReleaseCorrectItem = false;

    [Header("References")]
    public NetworkVariable<bool> hasItem = new NetworkVariable<bool>();
    public NetworkVariable<bool> hasCorrectItem = new NetworkVariable<bool>();
    [SerializeField] private Pickup slottedItem;
    public UnityEvent correctItemPlaced;

    private void FixedUpdate()
    {
        if (IsServer && continuousEvent && hasCorrectItem.Value)
        {
            correctItemPlaced.Invoke();
        }
    }

    public void recieveItem(GameObject item)
    {
        slottedItem = item.GetComponent<Pickup>();
        slottedItem.isSlotted.Value = true;
        hasItem.Value = true;
        slottedItem.holdingOffset = Vector3.zero;


        if (item.name == requredItem)
        {
            if (canReleaseCorrectItem)
            {
                slottedItem.isHeld.Value = false;
            }
            else
            {
                slottedItem.isHeld.Value = true;
            }
            
            
            hasCorrectItem.Value = true;

            if (oneTimeEvent)
            {
                correctItemPlaced.Invoke();
            }
        }
        else
        {
            slottedItem.isHeld.Value = false;
        }
    }
}
