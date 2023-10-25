using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class Pickup : NetworkBehaviour
{
    public NetworkVariable<bool> isSlotted = new NetworkVariable<bool>();
    public NetworkVariable<bool> isHeld = new NetworkVariable<bool>();
    public Vector3 holdingOffset = Vector3.zero;
    
    private Rigidbody rb;
    private Collider col;
    private float dropTime = 0.3f;
    private float dropTimer = 0;
    public override void OnNetworkSpawn()
    {
        rb = GetComponent<Rigidbody>();
        col = GetComponent<Collider>();
    }

    public void drop()
    {
        dropTimer = dropTime;
    }

    private void FixedUpdate()
    {
        if (IsOwner)
        {
            if (dropTimer > 0)
            {
                dropTimer -= Time.fixedDeltaTime;
            }
            else
            {
                if (isHeld.Value)
                {
                    transform.localPosition = holdingOffset;
                    transform.localRotation = Quaternion.identity;
                    col.enabled = false;
                    rb.isKinematic = true;
                }
                else
                {
                    col.enabled = true;
                    rb.isKinematic = false;
                } 
            }
           
        }
    }
}

