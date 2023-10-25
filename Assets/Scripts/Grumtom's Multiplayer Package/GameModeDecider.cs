using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GameModeDecider : MonoBehaviour
{
    [SerializeField] private LobbyManager lobbyManager;
    [SerializeField] private LobbyMatchmaking lobbyMatchmaking;
    [SerializeField] private RelayMatchmaking relayMatchmaking;

    [SerializeField] private TMP_Dropdown gameModeDropdown;

    [SerializeField] private string[] sceneNames;
    [SerializeField] private int[] lobbySize;
    private void Awake()
    {
        lobbyManager = FindObjectOfType<LobbyManager>();
        lobbyMatchmaking = FindObjectOfType<LobbyMatchmaking>();
        relayMatchmaking = FindObjectOfType<RelayMatchmaking>();
        updateGameMode();
    }

    public void updateGameMode()
    {
        int index = gameModeDropdown.value;
        lobbyManager.gameScene = sceneNames[index];
        lobbyMatchmaking.lobbySize = lobbySize[index];
        relayMatchmaking.lobbySize = lobbySize[index];
    }
}
