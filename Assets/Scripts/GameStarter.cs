using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class GameStarter : NetworkBehaviour
{
    [SerializeField] private GameObject playerPrefab;
    public override void OnNetworkSpawn()
    {
        if (IsServer)
        {
            for (int i = 0; i < NetworkManager.ConnectedClients.Count; i++)
            {
                GameObject newPlayer = Instantiate(playerPrefab);
                newPlayer.GetComponent<NetworkObject>().SpawnWithOwnership(NetworkManager.ConnectedClientsIds[i]);
            }
        }
    }
}
