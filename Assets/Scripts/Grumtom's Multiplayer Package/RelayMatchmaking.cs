using System;
using System.Threading.Tasks;
using TMPro;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using UnityEngine;

public class RelayMatchmaking : MonoBehaviour
{
    public int lobbySize;
    
    [Header("References")]
    [SerializeField] private TMP_Text JoinCodeText;
    [SerializeField] private TMP_Text errorMessageText;
    [SerializeField] private TMP_InputField JoinInput;
    [SerializeField] private GameObject buttons;
    [SerializeField] private GameObject lobbyMenu;
    [SerializeField] private GameObject startButton;

    private UnityTransport Transport;
   

    private void Awake() {
        Transport = FindObjectOfType<UnityTransport>(); // finds the transport and authenticates
    }

    public async void CreateGame() {
        buttons.SetActive(false);
        
        Allocation a = await RelayService.Instance.CreateAllocationAsync(lobbySize);
        JoinCodeText.text = await RelayService.Instance.GetJoinCodeAsync(a.AllocationId);

        Transport.SetHostRelayData(a.RelayServer.IpV4, (ushort)a.RelayServer.Port, a.AllocationIdBytes, a.Key, a.ConnectionData); // creates a game to host
        lobbyMenu.SetActive(true);
        startButton.SetActive(true);
        JoinCodeText.gameObject.SetActive(true);
        NetworkManager.Singleton.StartHost();
    }
    
    public async void JoinGame() {
        buttons.SetActive(false);
        print(JoinInput.text);
        if (JoinInput.text == "")
        {
            print("No Code?");
            errorMessageText.text = "Please Enter A Code"; // complains if the code is empty
            errorMessageText.gameObject.SetActive(true);
            buttons.SetActive(true);
            return;
        }

        JoinAllocation a;
        try
        {
            a = await RelayService.Instance.JoinAllocationAsync(JoinInput.text); // complains if the code is wrong
        }
        catch (Exception exception)
        {
            print("invalid Code" + exception.ToString());
            errorMessageText.text = "Invalid Code";
            errorMessageText.gameObject.SetActive(true);
            buttons.SetActive(true);
            return;
        }
        
        JoinCodeText.text = JoinInput.text;
        lobbyMenu.SetActive(true);
        JoinCodeText.gameObject.SetActive(true);

        Transport.SetClientRelayData(a.RelayServer.IpV4, (ushort)a.RelayServer.Port, a.AllocationIdBytes, a.Key, a.ConnectionData, a.HostConnectionData); // starts the client
        NetworkManager.Singleton.StartClient();
        buttons.SetActive(true);
    }
}
