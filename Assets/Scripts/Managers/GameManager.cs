using System.Collections.Generic;
using Unity.Services.Lobbies.Models;
using UnityEngine;

public enum GameType
{
    None,
    Wait,
    Playing,
    EndPlay,
}

public class GameManager : SingletonMonobehavior<GameManager>
{
    public GameType CurrentGameType { get; private set; } = GameType.None;
    
    public MainCanvas mainCanvas;
    public LobbyCanvas lobbyCanvas;
    public GameObject inGameGroup;

    private void Start()
    {
        EventManager.Instance.AddListener(EventType.OnEnterLobby, OnEnterLobby);
        EventManager.Instance.AddListener(EventType.OnLeaveLobby, OnLeaveLobby);
        EventManager.Instance.AddListener(EventType.OnConnected, OnConnected);
        EventManager.Instance.AddListener(EventType.OnDisconnected, OnDisconnected);
        EventManager.Instance.AddListener(EventType.OnStartingGame, OnStartingGame);
        EventManager.Instance.AddListener(EventType.OnStartGame, OnStartGame);
        EventManager.Instance.AddListener(EventType.OnEndGame, OnEndingGame);
        EventManager.Instance.AddListener(EventType.OnEndGame, OnEndGame);
        EventManager.Instance.AddListener(EventType.OnFailedNetworkTransfer, OnFailedNetworkTransfer);
        EventManager.Instance.AddListener(EventType.OnPlayerStatusChanged, OnPlayerStatusChanged);
    }
    
    private void OnEnterLobby(EventType type, Component sender, object[] args)
    {
        CurrentGameType = GameType.None;
        
        lobbyCanvas.gameObject.SetActive(true);
        mainCanvas.gameObject.SetActive(false);
        inGameGroup.SetActive(false);
    }

    private void OnLeaveLobby(EventType type, Component sender, object[] args)
    {
        CurrentGameType = GameType.None;
        
        lobbyCanvas.gameObject.SetActive(false);
        mainCanvas.gameObject.SetActive(true);
        inGameGroup.SetActive(false);
    }

    private void OnConnected(EventType type, Component sender, object[] args)
    {
        Dictionary<string, string> options = new Dictionary<string, string>()
        {
            { LobbyManager.Instance.ChangeStatusName, $"{(int)PlayerStatus.Connected}" }
        };

        LobbyManager.Instance.UpdatePlayerData(options, token =>
        {
            if (token.Type == CallbackType.Success)
            {
                EventManager.Instance.PostNotification(EventType.OnPlayerStatusChanged, this);
            }
        });
    }

    private void OnDisconnected(EventType type, Component sender, object[] args)
    {
        
    }

    private void OnStartingGame(EventType type, Component sender, object[] args)
    {        
        CurrentGameType = GameType.Wait;

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

        LobbyManager.Instance.UpdatePlayerData(options, token =>
        {
            if (token.Type == CallbackType.Success)
            {
                EventManager.Instance.PostNotification(EventType.OnPlayerStatusChanged, this);
            }
        });
    }

    private void OnStartGame(EventType type, Component sender, object[] args)
    {
        // 여기서 타이머 재서 시작하는 로직 만들기
        // UI에 콜백도 같이 넘겨서 콜백으로 이벤트 전달받아서 하기
        CurrentGameType = GameType.Playing;

        UIManager.Instance.HideLoading();
    }

    private void OnEndingGame(EventType type, Component sender, object[] args)
    {
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

    private void OnPlayerStatusChanged(EventType type, Component sender, object[] args)
    {
        if (LobbyManager.Instance.IsAllConnected)
        {
            Lobby currentLobby = LobbyManager.Instance.CurrentLobby;

            ConnectionData data = new ConnectionData();
            data.SessionId = currentLobby.Id;
            data.PlayerId = PlayerManager.Instance.LocalPlayer.Id;
            
            NetworkManager.Instance.SendStartGame(data);
        }
    }
}