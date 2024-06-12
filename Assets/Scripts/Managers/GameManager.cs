using System;
using Unity.Services.Lobbies.Models;
using UnityEngine;

public class GameManager : SingletonMonobehavior<GameManager>
{
    public MainCanvas mainCanvas;
    public LobbyCanvas lobbyCanvas;
    public GameObject inGameGroup;

    private void Start()
    {
        EventManager.Instance.AddListener(EventType.OnEnterLobby, OnEnterLobby);
        EventManager.Instance.AddListener(EventType.OnLeaveLobby, OnLeaveLobby);
        EventManager.Instance.AddListener(EventType.OnBeginningGame, OnBeginningGame);
        EventManager.Instance.AddListener(EventType.OnEndGame, OnEndGame);
        EventManager.Instance.AddListener(EventType.OnFailedNetworkTransfer, OnFailedNetworkTransfer);
    }

    private void OnEnterLobby(EventType type, Component sender, object[] args)
    {
        lobbyCanvas.gameObject.SetActive(true);
        mainCanvas.gameObject.SetActive(false);
        inGameGroup.SetActive(false);
    }

    private void OnLeaveLobby(EventType type, Component sender, object[] args)
    {
        lobbyCanvas.gameObject.SetActive(false);
        mainCanvas.gameObject.SetActive(true);
        inGameGroup.SetActive(false);
    }

    private void OnBeginningGame(EventType type, Component sender, object[] args)
    {
        mainCanvas.gameObject.SetActive(false);
        lobbyCanvas.gameObject.SetActive(false);
        
        inGameGroup.SetActive(true);

        UIManager.Instance.ShowLoading();
        
        // 게임 서버와 연결하기
    }

    private void OnEndGame(EventType type, Component sender, object[] args)
    {
        OnEnterLobby(type, sender, args);
    }
    
    private void OnFailedNetworkTransfer(EventType type, Component sender, object[] args)
    {
        string msg = args[0] as string;
        
        UIManager.Instance.Alert("Network Error", msg);
    }
}