
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using UnityEngine;

#if UNITY_EDITOR
using ParrelSync;
#endif

public class LobbyMatchmaking : MonoBehaviour {
    
    public int lobbySize;
    
    [Header("References")]
    [SerializeField] private GameObject lobbyMenu;
    [SerializeField] private GameObject startButton;
    [SerializeField] private GameObject matchmakeButton;
    
    private Lobby connectedLobby;
    private QueryResponse QueryResponse;
    private UnityTransport Transport;
    private const string JoinCodeKey = "FunnyLittleJoinCode";
    private string playerId;

    private void Awake() => Transport = FindObjectOfType<UnityTransport>();

    private void Start()
    {
        playerId = FindObjectOfType<LobbyManager>().playerId;
    }

    public async void CreateOrJoinLobby() {
        matchmakeButton.SetActive(false);
        connectedLobby = await QuickJoinLobby() ?? await CreateLobby();

        if (connectedLobby != null) matchmakeButton.SetActive(false);
    }
    

    private async Task<Lobby> QuickJoinLobby() {
        Debug.Log("Attempting Matchmaking");
        try {
            // Attempt to join a lobby in progress
            var lobby = await Lobbies.Instance.QuickJoinLobbyAsync();

            // If we found one, grab the relay allocation details
            var a = await RelayService.Instance.JoinAllocationAsync(lobby.Data[JoinCodeKey].Value);

            // Set the details to the transform
            SetTransformAsClient(a);

            // Join the game room as a client
            NetworkManager.Singleton.StartClient();
            lobbyMenu.SetActive(true);
            matchmakeButton.SetActive(true);
            return lobby;
        }
        catch (Exception exception) {
            Debug.Log($"No lobbies available via quick join" + exception);
            return null;
        }
    }

    private async Task<Lobby> CreateLobby() {
        try {

            // Create a relay allocation and generate a join code to share with the lobby
            var a = await RelayService.Instance.CreateAllocationAsync(lobbySize);
            var joinCode = await RelayService.Instance.GetJoinCodeAsync(a.AllocationId);

            // Create a lobby, adding the relay join code to the lobby data
            var options = new CreateLobbyOptions {
                Data = new Dictionary<string, DataObject> { { JoinCodeKey, new DataObject(DataObject.VisibilityOptions.Public, joinCode) } }
            };
            var lobby = await Lobbies.Instance.CreateLobbyAsync("Useless Lobby Name", lobbySize, options);

            // Send a heartbeat every 15 seconds to keep the room alive
            StartCoroutine(HeartbeatLobbyCoroutine(lobby.Id, 15));

            // Set the game room to use the relay allocation
            Transport.SetHostRelayData(a.RelayServer.IpV4, (ushort)a.RelayServer.Port, a.AllocationIdBytes, a.Key, a.ConnectionData);

            // Start the room
            NetworkManager.Singleton.StartHost();
            lobbyMenu.SetActive(true);
            matchmakeButton.SetActive(true);
            startButton.SetActive(true);
            return lobby;
        }
        catch (Exception exception) {
            print("Failed to create lobby" + exception);
            return null;
        }
    }

    private void SetTransformAsClient(JoinAllocation a) {
        Transport.SetClientRelayData(a.RelayServer.IpV4, (ushort)a.RelayServer.Port, a.AllocationIdBytes, a.Key, a.ConnectionData, a.HostConnectionData); 
        // sets the transport with all the basically default data (no idea what some of this is so dont touch)
    }

    private static IEnumerator HeartbeatLobbyCoroutine(string lobbyId, float waitTimeSeconds) { // pings the lobby every delay
        var delay = new WaitForSecondsRealtime(waitTimeSeconds);
        while (true) {
            Lobbies.Instance.SendHeartbeatPingAsync(lobbyId);
            yield return delay;
        }
    }

    private void OnDestroy() {
        try {
            StopAllCoroutines();
            if (connectedLobby != null) {
                if (connectedLobby.HostId == playerId) Lobbies.Instance.DeleteLobbyAsync(connectedLobby.Id); // damage reduction when this thing is killed
                else Lobbies.Instance.RemovePlayerAsync(connectedLobby.Id, playerId);
            }
        }
        catch (Exception exception) {
            Debug.Log($"Error shutting down lobby: {exception}");
        }
    }
}
