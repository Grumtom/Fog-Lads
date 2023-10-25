
using System;
using System.Collections;
using System.Collections.Generic;
using System.Net.Security;
using System.Threading.Tasks;
using TMPro;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

#if UNITY_EDITOR
using ParrelSync;
#endif

public class LobbyManager : NetworkBehaviour
{
    private UnityTransport transport;
    public string gameScene;
    public string playerId;
    
    [SerializeField] private GameObject mainMenu;
    [SerializeField] private TMP_Text lobbyPlayerList;
    [SerializeField] private GameObject buttons;

    public NetworkVariable<int> playerCount = new NetworkVariable<int>();


    public override void OnNetworkSpawn()
    {
        playerCount.OnValueChanged += playerCountChanged; // subscribes to the playercount event
    }

    public void playerCountChanged(int previous, int current) // this is run on everyones computer once the playercount is changed by anything
    {
        lobbyPlayerList.text = "Players: " + current;
    }

    private void Update()
    {
        if(!IsServer) return;
        if (NetworkManager.Singleton.ConnectedClients != playerCount)
        {
            playerCount.Value = NetworkManager.Singleton.ConnectedClients.Count; // saves the playercount if it ever changes
        }
    }

    private async void Awake()
    {
        transport = FindObjectOfType<UnityTransport>(); // finds the transport and authenticates
        await Authenticate();
        mainMenu.SetActive(true);
    }

    private async Task Authenticate() {
        Debug.Log("Attempting authenticaton");
        var options = new InitializationOptions();
        
#if UNITY_EDITOR
        // Remove this if you don't have ParrelSync installed. 
        // It's used to differentiate the clients, otherwise lobby will count them as the same
        options.SetProfile(ClonesManager.IsClone() ? ClonesManager.GetArgument() : "Primary");
#endif

        await UnityServices.InitializeAsync(options);

        await AuthenticationService.Instance.SignInAnonymouslyAsync();
        playerId = AuthenticationService.Instance.PlayerId;
    }

    public void loadScene()
    {
        buttons.SetActive(false);
        NetworkManager.Singleton.SceneManager.LoadScene(gameScene,LoadSceneMode.Single); // loads a scene
    }
    
    public void QuitGame()
    {
        buttons.SetActive(false);
        Application.Quit();
    }

    public void leaveGame()
    {
        NetworkManager.Singleton.Shutdown(); // to leave a lobby
        print(IsHost + "\v" + IsClient);
    }
}
