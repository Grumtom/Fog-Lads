using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class Player : NetworkBehaviour
{
    [SerializeField] private GameObject characterPrefab;

    void spawnPlayer(Vector3 spawnPos)
    {
        GameObject character = Instantiate(characterPrefab, spawnPos, Quaternion.identity);
        character.GetComponent<NetworkObject>().SpawnWithOwnership(this.OwnerClientId);
        activateControlsClientRpc(character.GetComponent<NetworkObject>().NetworkObjectId);
    }

    [ClientRpc]
    public void activateControlsClientRpc(ulong id)
    {
        if (IsOwner)
        {
            PlayerCharacterControls controls = GetNetworkObject(id).GetComponent<PlayerCharacterControls>();
            controls.enabled = true;
            controls.Player = this;
        }
    }
}
