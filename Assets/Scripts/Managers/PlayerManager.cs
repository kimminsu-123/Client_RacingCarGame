using System;
using System.Linq;
using Unity.Services.Authentication;
using Unity.Services.Lobbies.Models;
using UnityEngine;

public class PlayerManager : SingletonMonobehavior<PlayerManager>
{
    public Player LocalPlayer => LobbyManager.Instance.CurrentLobby
                                                        .Players
                                                        .FirstOrDefault(
                                                            p => p.Id.Equals(AuthenticationService.Instance.PlayerId)
                                                        );
    
    private void Start()
    {
        EventManager.Instance.AddListener(EventType.OnPlayerStatusChanged, OnPlayerStatusChanged);
    }

    private void OnPlayerStatusChanged(EventType type, Component sender, object[] args)
    {
        if (!LobbyManager.Instance.IsAllReady)
        {
            return;
        }

        EventManager.Instance.PostNotification(EventType.OnBeginningGame, this);
    }
}