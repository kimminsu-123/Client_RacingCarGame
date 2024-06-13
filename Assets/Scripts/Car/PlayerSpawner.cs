using System;
using System.Collections.Generic;
using Unity.Services.Lobbies.Models;
using UnityEngine;

public class PlayerSpawner : MonoBehaviour
{
    public Transform[] spawnPos;
    
    public GameObject localCarPrefab;
    public GameObject remoteCarPrefab;

    private CarController[] _spawnedCars;

    private void OnEnable()
    {
        Initialize();
        
        EventManager.Instance.AddListener(EventType.OnPlayerLeaved, OnPlayerLeaved);
    }

    private void Initialize()
    {
        _spawnedCars = new CarController[4];

        List<Player> players = LobbyManager.Instance.CurrentLobby.Players;
        string localPlayerId = PlayerManager.Instance.LocalPlayer.Id;
        for (int index = 0; index < players.Count; index++)
        {
            GameObject go;
            Player player = players[index];
            if (player.Id.Equals(localPlayerId))
            {
                go = Instantiate(localCarPrefab, spawnPos[index].position, spawnPos[index].rotation);
            }
            else
            {
                go = Instantiate(remoteCarPrefab, spawnPos[index].position, spawnPos[index].rotation);
            }

            CarController car = go.GetComponent<CarController>();
            PlayerManager.Instance.AddCar(car, player.Id, Color.white);

            _spawnedCars[index] = car;
        }
    }

    private void OnPlayerLeaved(EventType type, Component sender, object[] args)
    {
        List<int> indexes = args[0] as List<int>;
        if (indexes is not {Count: > 0})
        {
            return;
        }

        foreach (int index in indexes)
        {
            CarController car = _spawnedCars[index];
            _spawnedCars[index] = null;

            if (car != null)
            {
                PlayerManager.Instance.RemoveCar(car.PlayerId);
                Destroy(car.gameObject);
            }
        }
    }

    private void OnDisable()
    {
        foreach (CarController car in _spawnedCars)
        {
            if (car != null)
            {
                PlayerManager.Instance.RemoveCar(car.PlayerId);
                Destroy(car.gameObject);
            }
        }
        _spawnedCars = null;
        
        EventManager.Instance.RemoveListener(EventType.OnPlayerLeaved, OnPlayerLeaved);
    }
}