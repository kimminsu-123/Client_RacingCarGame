using System;
using UnityEngine;

public abstract class CarController : MonoBehaviour
{
    public string PlayerId { get; private set; }

    private SpriteRenderer _renderer;

    private void Awake()
    {
        _renderer = GetComponent<SpriteRenderer>();
        OnAwake();
    }

    public void Initialize(string playerId, Color carColor)
    {
        PlayerId = playerId;
        _renderer.color = carColor;
    }
    
    protected virtual void OnAwake(){}
    public abstract void Acceleration(Vector2 force);
    public abstract void Steering(float angle);
}