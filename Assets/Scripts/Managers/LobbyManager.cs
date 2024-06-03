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

public class LobbyCallbackToken
{
    public CallbackType Type { get; set; }
    public string Msg { get; set; }
    public Lobby Lobby { get; set; }
}

public class LobbyManager : SingletonMonobehavior<LobbyManager>
{
    public bool InLobby => CurrentLobby != null;
    public bool Initialized { get; private set; }
    public Lobby CurrentLobby { get; private set; }

    public float heartbeatInterval = 3f;
    
    private const int MAX_PLAYER = 4;
    private const string ENVIRONMENT = "development";

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

        return hasLobby;
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

        callbacks.PlayerDataAdded += OnPlayerDataAdded;
        callbacks.PlayerDataChanged += OnPlayerDataChanged;
        callbacks.PlayerDataRemoved += OnPlayerDataRemoved;
        callbacks.PlayerJoined += OnPlayerJoined;
        callbacks.PlayerLeft += OnPlayerLeft;

        await LobbyService.Instance.SubscribeToLobbyEventsAsync(CurrentLobby.Id, callbacks);
    }

    private void OnPlayerDataAdded(Dictionary<int, Dictionary<string, ChangedOrRemovedLobbyValue<PlayerDataObject>>> players)
    {
        Debug.Log($"Added {string.Join(", ", players.Select(x => x.Key))}");
    }

    private void OnPlayerDataChanged(Dictionary<int, Dictionary<string, ChangedOrRemovedLobbyValue<PlayerDataObject>>> players)
    {
        Debug.Log($"Changed {string.Join(", ", players.Select(x => x.Key))}");
    }

    private void OnPlayerDataRemoved(Dictionary<int, Dictionary<string, ChangedOrRemovedLobbyValue<PlayerDataObject>>> players)
    {
        Debug.Log($"Removed {string.Join(", ", players.Select(x => x.Key))}");
    }

    private void OnPlayerJoined(List<LobbyPlayerJoined> others)
    {
        Debug.Log($"Joined {string.Join(", ", others.Select(x => x.PlayerIndex))}");
    }

    private void OnPlayerLeft(List<int> others)
    {
        Debug.Log($"Left {string.Join(", ", others)}");
    }

    public async void Initialize(Action<LobbyCallbackToken> callback)
    {
        LobbyCallbackToken result = new LobbyCallbackToken();
        
        try
        {
            InitializationOptions options = new InitializationOptions();
            options.SetOption("Environment", ENVIRONMENT);
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
        
        CreateLobbyOptions options = new CreateLobbyOptions();
        options.IsPrivate = false;
        
        try
        {
            CurrentLobby = await LobbyService.Instance.CreateLobbyAsync(lobbyName, MAX_PLAYER, options);
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
        
        try
        {
            CurrentLobby = await LobbyService.Instance.JoinLobbyByCodeAsync(lobbyCode);
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
}