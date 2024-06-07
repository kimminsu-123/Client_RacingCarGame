using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using UnityEngine;
using Task = System.Threading.Tasks.Task;

public enum CallbackType
{
    None,
    Failed,
    Success
}

public enum PlayerStatusType
{
    UnReady = 0,
    Ready,
}

public class LobbyCallbackToken
{
    public CallbackType Type { get; set; }
    public string Msg { get; set; }
    public Lobby Lobby { get; set; }
}

public class LobbyManager : SingletonMonobehavior<LobbyManager>
{
    public readonly string ChangeColorName = "C";
    public readonly string ChangeStatusName = "R";
    
    public bool InLobby => CurrentLobby != null;
    public bool Initialized { get; private set; }
    public Lobby CurrentLobby { get; private set; }

    public float heartbeatInterval = 3f;
    
    private const int MaxPlayer = 4;
    private const string Environment = "development";
    
    private readonly PlayerDataObject _colorObject = new(PlayerDataObject.VisibilityOptions.Member, "0");
    private readonly PlayerDataObject _statusObject = new(PlayerDataObject.VisibilityOptions.Member, "0");
    
    private void Start()
    {
        Application.wantsToQuit += OnApplicationWantsToQuit;
    }

    private bool OnApplicationWantsToQuit()
    {
        bool hasLobby = InLobby;
        
        if (hasLobby)
        {
            LeaveLobby(null, true);
        }

        return !hasLobby;
    }

    private IEnumerator HeartbeatLobbyCoroutine()
    {
        WaitForSecondsRealtime delay = new WaitForSecondsRealtime(heartbeatInterval);

        while (InLobby)
        {
            LobbyService.Instance.SendHeartbeatPingAsync(CurrentLobby.Id);
            yield return delay;
        }
    }
    
    private async Task AddEventCallbacks()
    {
        LobbyEventCallbacks callbacks = new LobbyEventCallbacks();

        callbacks.LobbyChanged += OnLobbyChanged;
        callbacks.PlayerJoined += OnPlayerJoined;
        callbacks.PlayerLeft += OnPlayerLeft;
        callbacks.PlayerDataAdded += OnHandlePlayerData;
        callbacks.PlayerDataChanged += OnHandlePlayerData;
        callbacks.PlayerDataRemoved += OnHandlePlayerData;
        
        await LobbyService.Instance.SubscribeToLobbyEventsAsync(CurrentLobby.Id, callbacks);
    }

    private void OnLobbyChanged(ILobbyChanges obj)
    {
        obj.ApplyToLobby(CurrentLobby);
    }

    private void OnPlayerJoined(List<LobbyPlayerJoined> others)
    {
        EventManager.Instance.PostNotification(EventType.OnPlayerJoined, this, others);
    }

    private void OnPlayerLeft(List<int> others)
    {
        EventManager.Instance.PostNotification(EventType.OnPlayerJoined, this, others);
    }

    private void OnHandlePlayerData(Dictionary<int, Dictionary<string, ChangedOrRemovedLobbyValue<PlayerDataObject>>> data)
    {
        foreach (var playerData in data)
        {
            var player = CurrentLobby.Players[playerData.Key];
            if(player == null) continue;
            
            var playerChanges = playerData.Value;
            foreach (var playerChange in playerChanges)
            {
                var changedValue = playerChange.Value;
                var playerDataObject = changedValue.Value;

                switch (playerChange.Key)
                {
                    case "C":
                        EventManager.Instance.PostNotification(EventType.OnPlayerColorChanged, this, player, playerDataObject.Value);
                        break;
                    case "R":
                        EventManager.Instance.PostNotification(EventType.OnPlayerStatusChanged, this, player, playerDataObject.Value);
                        break;
                }
            }
        }
    }

    public async void Initialize(Action<LobbyCallbackToken> callback)
    {
        LobbyCallbackToken result = new LobbyCallbackToken();
        
        try
        {
            InitializationOptions options = new InitializationOptions();
            options.SetOption("Environment", Environment);
            await UnityServices.InitializeAsync(options);
            await AuthenticationService.Instance.SignInAnonymouslyAsync();

            result.Type = CallbackType.Success;
            Initialized = true;
        }
        catch (Exception err)
        {
            result.Type = CallbackType.Failed;
            result.Msg = err.Message;
            
            Initialized = false;
        }
        finally
        {
            callback?.Invoke(result);
        }
    }

    public async void CreateLobby(string lobbyName, Action<LobbyCallbackToken> callback)
    {
        LobbyCallbackToken result = new LobbyCallbackToken();
        
        if (InLobby)
        {
            result.Type = CallbackType.Failed;
            result.Msg = "already in lobby can't create lobby";
            result.Lobby = null;
            callback?.Invoke(result);
            return;
        }
        
        Dictionary<string, PlayerDataObject> data = new Dictionary<string, PlayerDataObject>
        {
            { ChangeColorName, _colorObject },
            { ChangeStatusName, _statusObject }
        };
        
        CreateLobbyOptions options = new CreateLobbyOptions
        {
            IsPrivate = false,
            Player = new Player(id: AuthenticationService.Instance.PlayerId, data: data)
        };

        try
        {
            CurrentLobby = await LobbyService.Instance.CreateLobbyAsync(lobbyName, MaxPlayer, options);
            await AddEventCallbacks();
            StartCoroutine(HeartbeatLobbyCoroutine());

            result.Type = CallbackType.Success;
            result.Lobby = CurrentLobby;
        }
        catch (Exception err)
        {
            result.Type = CallbackType.Failed;
            result.Msg = err.Message;
            result.Lobby = null;
            CurrentLobby = null;
        }
        finally
        {
            callback?.Invoke(result);
        }
    }

    public async void JoinLobby(string lobbyCode, Action<LobbyCallbackToken> callback)
    {
        LobbyCallbackToken result = new LobbyCallbackToken();

        if (InLobby)
        {
            result.Type = CallbackType.Failed;
            result.Msg = "already in lobby can't join lobby";
            result.Lobby = null;
            callback?.Invoke(result);
            return;
        }
        
        Dictionary<string, PlayerDataObject> data = new Dictionary<string, PlayerDataObject>
        {
            { ChangeColorName, _colorObject },
            { ChangeStatusName, _statusObject }
        };
        
        JoinLobbyByCodeOptions options = new JoinLobbyByCodeOptions
        {
            Player = new Player(id: AuthenticationService.Instance.PlayerId, data: data)
        };
        
        try
        {
            CurrentLobby = await LobbyService.Instance.JoinLobbyByCodeAsync(lobbyCode, options);
            await AddEventCallbacks();
            StartCoroutine(HeartbeatLobbyCoroutine());
            
            result.Type = CallbackType.Success;
            result.Lobby = CurrentLobby;
        }
        catch (Exception err)
        {
            result.Type = CallbackType.Failed;
            result.Msg = err.Message;
            result.Lobby = null;
            CurrentLobby = null;
        }
        finally
        {
            callback?.Invoke(result);
        }
    }

    public async void LeaveLobby(Action<LobbyCallbackToken> callback, bool isQuit = false)
    {
        LobbyCallbackToken result = new LobbyCallbackToken();
        
        if (!InLobby)
        {
            result.Type = CallbackType.Failed;
            result.Msg = "you not in lobby can't leave lobby";
            result.Lobby = null;
            callback?.Invoke(result);
            return;
        }
        
        try
        {
            string playerId = AuthenticationService.Instance.PlayerId;
            await LobbyService.Instance.RemovePlayerAsync(CurrentLobby.Id, playerId);

            result.Type = CallbackType.Success;
            result.Lobby = null;
            
            CurrentLobby = null;
        }
        catch (Exception err)
        {
            result.Type = CallbackType.Failed;
            result.Msg = err.Message;
            result.Lobby = null;
        }
        finally
        {
            callback?.Invoke(result);
        }

        if (isQuit)
        {
            Application.Quit();
        }
    }

    public async void UpdatePlayerData(Dictionary<string, string> options)
    {
        if (!InLobby) return;
        
        string localPlayerId = PlayerManager.Instance.LocalPlayer.Id;
        Dictionary<string, PlayerDataObject> dataCurr = new Dictionary<string, PlayerDataObject>();
        
        foreach (var dataNew in options)
        {
            PlayerDataObject dataObj = new PlayerDataObject(PlayerDataObject.VisibilityOptions.Member, dataNew.Value);
            if (dataCurr.ContainsKey(dataNew.Key))
                dataCurr[dataNew.Key] = dataObj;
            else
                dataCurr.Add(dataNew.Key, dataObj);
        }

        UpdatePlayerOptions updateOptions = new UpdatePlayerOptions
        {
            Data = dataCurr,
            AllocationId = null,
            ConnectionInfo = null
        };
        
        CurrentLobby = await LobbyService.Instance.UpdatePlayerAsync(CurrentLobby.Id, localPlayerId, updateOptions);
    }
}