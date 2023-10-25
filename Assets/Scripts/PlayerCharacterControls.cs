using System;
using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using Unity.Mathematics;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class PlayerCharacterControls : NetworkBehaviour
{
    [Header("Stats")] 
    [SerializeField] private float moveSpeed = 0.1f;
    [SerializeField] private float lookSensitivity = 0.5f;
    [SerializeField] private float interactRange;
    
    [Header("References")] 
    [SerializeField] private GameObject handSlot;

    [SerializeField] private GameObject camStuffPrefab;
    [SerializeField] private CinemachineFreeLook freeLookCam;
    [SerializeField] private Camera cam;

    [SerializeField] private GameObject pingSelfPrefab;
    [SerializeField] private GameObject pingWorldPrefab;

    [SerializeField] private GameObject pingSpawnPoint;
    [SerializeField] private GameObject pingSpawnDir;
    
    public Player Player;

    private GameObject heldItem;
    private bool isHolding = false;
    
    private GameObject cameraTargetObject;
    private Vector3 cameraTarget;

    private CharacterController Controller;
    private PlayerInput PlayerInput;

    
    private Vector2 move;
    private Vector2 look;

    public float turnSmoothTime = 0.1f;
    private float turnSmoothVelocity;

    public override void OnNetworkSpawn()
    {
        if (!IsOwner)
        {
            enabled = false;
            return;
        }
        print("Rigging player character");
        gameObject.name = "Local Player Character";

        Instantiate(camStuffPrefab, transform);
        
        Controller = GetComponent<CharacterController>();
        PlayerInput = GetComponent<PlayerInput>();
        freeLookCam = GetComponentInChildren<CinemachineFreeLook>();
        cam = GetComponentInChildren<Camera>();

        freeLookCam.Follow = transform;
        
        cam.enabled = true;
        freeLookCam.enabled = true;
        Controller.enabled = true;
        PlayerInput.enabled = true;
        
        Cursor.lockState = CursorLockMode.Locked;
    }
    
    private void Update()
    {
        ReadInputs();
    }

    private void FixedUpdate()
    {
        Movement();
        checkForInteractable();
    }

    void ReadInputs()
    {
        move = PlayerInput.actions.FindAction("Move").ReadValue<Vector2>();
        look = PlayerInput.actions.FindAction("Look").ReadValue<Vector2>();
    }

    void Movement()
    {
        if (move.magnitude > 0.1)
        {
            //Controller.Move(new Vector3(move.x, -1, move.y) * moveSpeed);
            
            float targetLookAngle = Mathf.Atan2(move.x, move.y) * Mathf.Rad2Deg + cam.transform.eulerAngles.y;
            
            float angle = Mathf.SmoothDampAngle(transform.rotation.eulerAngles.y, targetLookAngle, ref turnSmoothVelocity, turnSmoothTime);
            
            transform.rotation = Quaternion.Euler(0f, angle, 0f); // looks in the direction
            
            Vector3 moveVector3 = Quaternion.Euler(0f, targetLookAngle, 0f) * Vector3.forward; // adds the camera direction to the movement
            moveVector3 += -Vector3.up.normalized;
            moveVector3 *= moveSpeed;

            Controller.Move(moveVector3);
        }

        float sensitivity = lookSensitivity;
        if (PlayerInput.currentControlScheme == "Gamepad")
        {
            sensitivity *= 25;
        }

        freeLookCam.m_YAxis.m_InputAxisValue = look.y * sensitivity;
        freeLookCam.m_XAxis.m_InputAxisValue = look.x * sensitivity;

    }

    void checkForInteractable()
    {
        RaycastHit hit;
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition); // raycasts to get the mouse pos in game

        if (Physics.Raycast(ray, out hit))
        {
            if((hit.point - transform.position).magnitude < interactRange)
            cameraTarget = hit.point;
            cameraTargetObject = hit.collider.gameObject;
            
            switch (hit.collider.tag)
            {
                case "Pickup":
                    
                    break;
                case "Interactable":
                    
                    break;
                case "Placement Slot":
                    
                    break;
            }
        }
        else
        {
            cameraTarget = Vector3.zero;
        }
    }

    public void interact(InputAction.CallbackContext context)
    {
        if (context.started)
        {
            print("Interact button pressed");
            if (cameraTarget != Vector3.zero)
            {
                print(cameraTargetObject.name);
                //if looking at a colider
                switch (cameraTargetObject.tag)
                {
                    case "Pickup": // if looking at a pickup, try pick it up if you have space
                        if (isHolding)
                        {
                            dropObject(); // drops
                        }
                        else
                        {
                            print("Picking up");
                            Pickup pickup = cameraTargetObject.GetComponent<Pickup>();
                            if (!pickup.isHeld.Value)
                            {
                                pickupServerRpc(pickup.GetComponent<NetworkObject>().NetworkObjectId);
                                pickup.holdingOffset = handSlot.transform.localPosition;
                                heldItem = pickup.gameObject;
                                isHolding = true;
                            }
                            else
                            {
                                print("Cant pick that up, tis held");
                            }
                        }
                        break;
                    
                    
                    case "Interactable": // if looking at an interactable, try interact
                        print("Interacting");
                        interactServerRpc(cameraTargetObject.GetComponent<NetworkObject>().NetworkObjectId);
                        break;
                    
                    
                    case "Placement Slot": // if looking at a placement slot, try place your item
                        PlacementSlot placementSlot = cameraTargetObject.GetComponent<PlacementSlot>();
                        if (isHolding)
                        {
                            
                            if (!placementSlot.hasItem.Value)
                            {
                                if (placementSlot.rejectWrongItem)
                                {
                                    if (heldItem.name == placementSlot.requredItem)
                                    {
                                        placeServerRpc(cameraTargetObject.GetComponent<NetworkObject>().NetworkObjectId);
                                        isHolding = false;
                                        heldItem = null;
                                    }
                                    else
                                    {
                                        print("Cant place that, it's not the right shape");
                                    }
                                }
                                else
                                {
                                    placeServerRpc(cameraTargetObject.GetComponent<NetworkObject>().NetworkObjectId);
                                    isHolding = false;
                                    heldItem = null;
                                }
                            }
                            else
                            {
                                print("Placement slot full");
                                dropObject();
                            }
                        }
                        else
                        {
                            print("Dont have anything to place");
                        }
                        break;
                    
                    
                    default:
                        print("Not looking at anything of interest, attempting drop");
                        if (isHolding)
                        {
                            dropObject(); // drops
                        }
                        break;
                }
            }
            else
            {
                print("Not looking at anything");
                if (isHolding)
                {
                    dropObject(); // drops
                }
                // if looking at the sky, simply attempt to drop stuff
            }
        }
    }

    [ServerRpc]
    void interactServerRpc(ulong id)
    {
        print(GetNetworkObject(id).name);
        Interactable interactable = GetNetworkObject(id).GetComponent<Interactable>();
        interactable.triggerEvent();
        print("Interacted");
    }

    [ServerRpc]
    void pickupServerRpc(ulong id)
    {
        Pickup pickup = GetNetworkObject(id).GetComponent<Pickup>();
        print(gameObject.name + " is trying to pick up " + pickup.gameObject.name);
        if (pickup.GetComponent<NetworkObject>().TrySetParent(gameObject, false))
        {
            pickup.GetComponent<NetworkObject>().ChangeOwnership(this.OwnerClientId);
            pickup.isHeld.Value = true;
            heldItem = pickup.gameObject;
        }
        else
        {
            print("Parenting failed");
        }
    }

    [ServerRpc]
    public void placeServerRpc(ulong id)
    {
        PlacementSlot placementSlot = GetNetworkObject(id).GetComponent<PlacementSlot>();
        print(gameObject.name + " is placing " + heldItem.name + " in " + placementSlot.gameObject.name);
        if (heldItem.GetComponent<NetworkObject>().TrySetParent(placementSlot.transform))
        {
            heldItem.GetComponent<NetworkObject>().ChangeOwnership(NetworkManager.LocalClientId);
            heldItem.GetComponent<Pickup>().holdingOffset = Vector3.zero;
            placementSlot.recieveItem(heldItem);
            isHolding = false;
            heldItem = null;
        }
        else
        {
            print("Parenting failed");
        }
    }

    [ServerRpc]
    void dropServerRpc()
    {
        heldItem.GetComponent<NetworkObject>().TryRemoveParent();
        heldItem.GetComponent<Pickup>().isHeld.Value = false;
    }

    void dropObject()
    {
        print("dropping");
        heldItem.GetComponent<Pickup>().drop();
        dropServerRpc();
        heldItem = null;
        isHolding = false;
    }

    public void pingSelf(InputAction.CallbackContext context)
    {
        if (context.started)
        {
            pingSelfServerRpc();
        }
    }

    [ServerRpc]
    private void pingSelfServerRpc()
    {
        GameObject PS = Instantiate(pingSelfPrefab, pingSpawnPoint.transform);
        PS.GetComponent<NetworkObject>().Spawn();
    }
    
    
    public void pingWorld(InputAction.CallbackContext context)
    {
        if (context.started)
        {
            pingWorldServerRpc(freeLookCam.transform.position);
        }
    }

    [ServerRpc]
    private void pingWorldServerRpc(Vector3 camPos)
    {
        pingSpawnPoint.transform.LookAt(camPos);
        GameObject PW = Instantiate(pingWorldPrefab, pingSpawnPoint.transform);
        PW.GetComponent<Rigidbody>().isKinematic = true;
        PW.transform.rotation = pingSpawnPoint.transform.rotation;
        PW.transform.Rotate(180,0,0, Space.Self);
        PW.GetComponent<NetworkObject>().Spawn();
        
    }
}