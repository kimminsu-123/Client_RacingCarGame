using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RemoteCoordinate
{
    public Vector2 position;
    public Quaternion rotation;
}

public class RemoteCarController : CarController
{
    private Queue<RemoteCoordinate> _coordQueue;
    private RemoteCoordinate _currentCoord;
    private RemoteCoordinate _prevCoord;
    private float _t = 0f;

    private void Awake()
    {
        _coordQueue = new Queue<RemoteCoordinate>();
    }

    private void OnEnable()
    {
        GameManager.Instance.OnChangedGameState += OnChangedGameState;
    }

    private void OnDisable()
    {
        GameManager.Instance.OnChangedGameState -= OnChangedGameState;
    }

    private void OnChangedGameState(GameStateType type)
    {
        switch (type)
        {
            case GameStateType.Playing:
                break;
        }
    }

    private void Start()
    {
        _currentCoord = new RemoteCoordinate()
        {
            position = transform.position,
            rotation = transform.rotation,
        };
        _t = 1f;
    }

    public void AddCoord(Vector2 pos, Quaternion rot)
    {
        _coordQueue.Enqueue(new RemoteCoordinate()
        {
            position = pos,
            rotation = rot,
        });
    }

    private void Update()
    {
        if (GameManager.Instance.CurrentGameType != GameStateType.Playing)
        {
            return;
        }
        
        if (_coordQueue.Count <= 0)
        {
            return;
        }
        
        if (Vector2.Distance(_currentCoord.position, transform.position) <= 0.0001f)
        {
            Acceleration(_currentCoord.position);
            Steering(_currentCoord.rotation.eulerAngles.z);

            _prevCoord = _currentCoord;
            _currentCoord = _coordQueue.Dequeue();
            _t = 0f;
        }

        _t += Time.deltaTime / TransformSender.SEND_INTERVAL;

        Vector2 pos = Vector2.Lerp(_prevCoord.position, _currentCoord.position, _t);
        Quaternion rot = Quaternion.Slerp (_currentCoord.rotation, _currentCoord.rotation, _t);

        Acceleration(pos);
        Steering(rot.eulerAngles.z);
    }

    public override void Acceleration(Vector2 force)
    {
        transform.position = force;
    }

    public override void Steering(float angle)
    {
        transform.rotation = Quaternion.Euler(0f, 0f, angle);
    }
}