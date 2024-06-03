using System;
using Unity.Services.Lobbies.Models;
using UnityEngine;

public class GameManager : SingletonMonobehavior<GameManager>
{
    public MainCanvas mainCanvas;
    public LobbyCanvas lobbyCanvas;

    private void Start()
    {
        EventManager.Instance.AddListener(EventType.OnEnterLobby, OnEnterLobby);
        EventManager.Instance.AddListener(EventType.OnLeaveLobby, OnLeaveLobby);
    }

    private void OnEnterLobby(EventType type, Component sender, object[] args)
    {
        lobbyCanvas.gameObject.SetActive(true);
        mainCanvas.gameObject.SetActive(false);
    }

    private void OnLeaveLobby(EventType type, Component sender, object[] args)
    {
        lobbyCanvas.gameObject.SetActive(false);
        mainCanvas.gameObject.SetActive(true);
    }

    private void OnApplicationQuit()
    {
        EventManager.Instance.PostNotification(EventType.OnApplicationWantsToQuit, this);
    }
}