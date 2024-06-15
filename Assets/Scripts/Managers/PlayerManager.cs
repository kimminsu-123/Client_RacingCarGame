using System.Collections.Generic;
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
    private readonly Dictionary<string, CarController> _carControllers = new(); 

    private void Start()
    {
        EventManager.Instance.AddListener(EventType.OnSyncPlayer, OnSyncPlayer);
        EventManager.Instance.AddListener(EventType.OnPlayerStatusChanged, OnPlayerStatusChanged);
    }

    private void OnSyncPlayer(EventType type, Component sender, object[] args)
    {
        TransformData data = args[0] as TransformData;

        if (_carControllers.ContainsKey(data.PlayerId))
        {
            RemoteCarController car = _carControllers[data.PlayerId] as RemoteCarController;
            if(car != null)
            {
                car.AddCoord(data.Position, data.Rotation);
            }
        }
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