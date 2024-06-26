using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Services.Authentication;
using Unity.Services.Lobbies.Models;
using UnityEngine;

public enum GameStateType
{
    None,
    Wait,
    Playing,
    EndPlay,
}

public class GameManager : SingletonMonobehavior<GameManager>
{
    public Action<GameStateType> OnChangedGameState;

    private GameStateType _currentGameType = GameStateType.None;
    public GameStateType CurrentGameType 
    {
        get => _currentGameType;
        private set
        {
            _currentGameType = value;
            OnChangedGameState?.Invoke(_currentGameType);
        }
    }
    
    public MainCanvas mainCanvas;
    public LobbyCanvas lobbyCanvas;
    public GameObject inGameGroup;

    public float waitTime;
    public float goalWaitTime = 5f;

    private bool _wasGoal;

    private void Start()
    {
        EventManager.Instance.AddListener(EventType.OnEnterLobby, OnEnterLobby);
        EventManager.Instance.AddListener(EventType.OnLeaveLobby, OnLeaveLobby);
        EventManager.Instance.AddListener(EventType.OnConnected, OnConnected);
        EventManager.Instance.AddListener(EventType.OnStartingGame, OnStartingGame);
        EventManager.Instance.AddListener(EventType.OnStartGame, OnStartGame);
        EventManager.Instance.AddListener(EventType.OnGoalLine, OnGoalLine);
        EventManager.Instance.AddListener(EventType.OnEndingGame, OnEndingGame);
        EventManager.Instance.AddListener(EventType.OnEndGame, OnEndGame);
        EventManager.Instance.AddListener(EventType.OnFailedNetworkTransfer, OnFailedNetworkTransfer);
        EventManager.Instance.AddListener(EventType.OnPlayerStatusChanged, OnPlayerStatusChanged);
    }

    private void OnEnterLobby(EventType type, Component sender, object[] args)
    {
        CurrentGameType = GameStateType.None;
        
        lobbyCanvas.gameObject.SetActive(true);
        mainCanvas.gameObject.SetActive(false);
        inGameGroup.SetActive(false);

        _wasGoal = false;
    }

    private void OnLeaveLobby(EventType type, Component sender, object[] args)
    {
        CurrentGameType = GameStateType.None;
        
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
            else
            {
                UIManager.Instance.Alert("OnConnect", token.Msg);
            }
        });
    }

    private void OnStartingGame(EventType type, Component sender, object[] args)
    {        
        CurrentGameType = GameStateType.Wait;

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

    private void OnGoalLine(EventType type, Component sender, object[] args)
    {
        GoalData goal = args[0] as GoalData;
        if(goal == null)
        {
            return;
        }

        if (_wasGoal)
        {
            return;
        }

        _wasGoal = true;

        UIManager.Instance.ShowTimer(goalWaitTime, () =>
        {
            ConnectionData data = new ConnectionData();
            data.SessionId = LobbyManager.Instance.CurrentLobby.Id;
            data.PlayerId = PlayerManager.Instance.LocalPlayer.Id;
            NetworkManager.Instance.SendEndGame(data);

            EventManager.Instance.PostNotification(EventType.OnEndingGame, this);
        });
    }

    private void OnStartGame(EventType type, Component sender, object[] args)
    {
        UIManager.Instance.HideLoading();

        UIManager.Instance.ShowTimer(waitTime, () =>
        {
            CurrentGameType = GameStateType.Playing;
        });
    }

    private void OnEndingGame(EventType type, Component sender, object[] args)
    {
        LobbyManager.Instance.UpdateLobbyData(null, null, true);

        Dictionary<string, string> options = new Dictionary<string, string>()
        {
            { LobbyManager.Instance.ChangeStatusName, $"{(int)PlayerStatus.UnReady}" }
        };
        LobbyManager.Instance.UpdatePlayerData(options, null);
    }
    
    private void OnEndGame(EventType type, Component sender, object[] args)
    {
        CurrentGameType = GameStateType.EndPlay;

        ConnectionData data = args[0] as ConnectionData;
        if(data == null)
        {
            return;
        }

        UIManager.Instance.ShowWinner(data.PlayerId);
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