using System;
using System.Collections.Generic;
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

        ConnectionData data = new ConnectionData();
        data.PlayerId = PlayerManager.Instance.LocalPlayer.Id;
        data.SessionId = LobbyManager.Instance.CurrentLobby.Id;
        NetworkManager.Instance.ConnectServer(data);
        
        Dictionary<string, string> options = new Dictionary<string, string>()
        {
            { LobbyManager.Instance.ChangeStatusName, $"{(int)PlayerStatus.Connecting}" }
        };

        LobbyManager.Instance.UpdatePlayerData(options, _ =>
        {
            EventManager.Instance.PostNotification(EventType.OnPlayerStatusChanged, this);
        });
        
        // 인게임이 로드되면 플레이어 생성
        // 연결이 되면 Connect로 속성 변경
        // 모든 플레이어가 Connect가 되면 게임 시작 타이머
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