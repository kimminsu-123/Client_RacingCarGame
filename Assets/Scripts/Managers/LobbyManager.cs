using System;
using System.Collections.Generic;
using Unity.Services.Authentication;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using UnityEngine;

public enum ResultLobbyType
{
    None,
    Failed,
    Success
}

public class ResultLobby
{
    public ResultLobbyType Type { get; set; }
    public string Msg { get; set; }
    public Lobby Lobby { get; set; }
}

public class LobbyManager : SingletonMonobehavior<LobbyManager>
{
    public bool IsInLobby => CurrentLobby != null;
    public Lobby CurrentLobby { get; private set; }
    
    private const int MAX_PLAYER = 4;
    
    public async void CreateLobby(string lobbyName, Action<ResultLobby> callback)
    {
        ResultLobby result = new ResultLobby();
        
        if (IsInLobby)
        {
            result.Type = ResultLobbyType.Failed;
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

            result.Type = ResultLobbyType.Success;
            result.Lobby = CurrentLobby;
        }
        catch (Exception err)
        {
            result.Type = ResultLobbyType.Failed;
            result.Msg = err.Message;
            result.Lobby = null;
        }
        finally
        {
            callback?.Invoke(result);
        }
    }

    public async void JoinLobby(string lobbyId, Action<ResultLobby> callback)
    {
        ResultLobby result = new ResultLobby();

        if (IsInLobby)
        {
            result.Type = ResultLobbyType.Failed;
            result.Msg = "already in lobby can't join lobby";
            result.Lobby = null;
            callback?.Invoke(result);
            return;
        }
        
        try
        {
            CurrentLobby = await LobbyService.Instance.JoinLobbyByIdAsync(lobbyId);
            
            result.Type = ResultLobbyType.Success;
            result.Lobby = CurrentLobby;
        }
        catch (Exception err)
        {
            result.Type = ResultLobbyType.Failed;
            result.Msg = err.Message;
            result.Lobby = null;
        }
        finally
        {
            callback?.Invoke(result);
        }
    }

    public async void LeaveLobby(Action<ResultLobby> callback)
    {
        ResultLobby result = new ResultLobby();
        
        if (!IsInLobby)
        {
            result.Type = ResultLobbyType.Failed;
            result.Msg = "you not in lobby can't leave lobby";
            result.Lobby = null;
            callback?.Invoke(result);
            return;
        }
        
        try
        {
            string playerId = AuthenticationService.Instance.PlayerId;
            await LobbyService.Instance.RemovePlayerAsync(CurrentLobby.Id, playerId);

            result.Type = ResultLobbyType.Success;
            result.Lobby = null;
            
            CurrentLobby = null;
        }
        catch (Exception err)
        {
            result.Type = ResultLobbyType.Failed;
            result.Msg = err.Message;
            result.Lobby = null;
        }
        finally
        {
            callback?.Invoke(result);
        }
    }
}