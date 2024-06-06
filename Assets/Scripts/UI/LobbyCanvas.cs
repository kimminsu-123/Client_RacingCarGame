using System;
using TMPro;
using Unity.Services.Lobbies.Models;
using Unity.VisualScripting;
using UnityEngine;

public class LobbyCanvas : MonoBehaviour
{
    public LobbyPlayerElement[] playerElements;
    public TMP_Text lobbyIdText;

    private void Start()
    {
        EventManager.Instance.AddListener(EventType.OnPlayerJoined, OnPlayerJoined);
        EventManager.Instance.AddListener(EventType.OnPlayerLeaved, OnPlayerLeaved);
    }

    private void OnEnable()
    {
        lobbyIdText.text = LobbyManager.Instance.CurrentLobby.LobbyCode;

        UpdatePlayerList();
    }

    public void LeaveLobby()
    {
        LobbyManager.Instance.LeaveLobby(OnLeaveLobby);
    }

    private void OnLeaveLobby(LobbyCallbackToken token)
    {
        switch (token.Type)
        {
            case CallbackType.Failed:
                UIManager.Instance.Alert("Failed Leave Lobby", token.Msg);
                break;
            case CallbackType.Success:
                EventManager.Instance.PostNotification(EventType.OnLeaveLobby, this);
                break;
        }
    }

    private void UpdatePlayerList()
    {
        Lobby lobby = LobbyManager.Instance.CurrentLobby;

        int lastIndex = 0;
        foreach (Player player in lobby.Players)
        {
            playerElements[lastIndex].playerIdText.text = player.Id;
            playerElements[lastIndex].gameObject.SetActive(true);
            lastIndex++;
        }

        for (; lastIndex < playerElements.Length; lastIndex++)
        {
            playerElements[lastIndex].gameObject.SetActive(false);
        }
    }
    
    private void OnPlayerJoined(EventType type, Component sender, object[] args)
    {
        UpdatePlayerList();
    }

    private void OnPlayerLeaved(EventType type, Component sender, object[] args)
    {
        UpdatePlayerList();
    }
}