using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
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

    private readonly Dictionary<string, CarController> _carControllers = new(); 

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

        EventManager.Instance.PostNotification(EventType.OnStartingGame, this);
    }

    public void AddCar(CarController car, string playerId, Color carColor)
    {
        if (_carControllers.TryAdd(playerId, car))
        {
            car.Initialize(playerId, carColor);
        }
    }

    public void RemoveCar(string playerId)
    {
        if (_carControllers.ContainsKey(playerId))
        {
            _carControllers.Remove(playerId);
        }
    }
}