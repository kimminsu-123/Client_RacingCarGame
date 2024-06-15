using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D), typeof(PlayerInput))]
public class LocalCarController : CarController
{
    [Header("settings stopping drags")]
    public float withoutInputDrag = 3f;
    public float dragDelta = 3f;
    
    [Header("settings acceleration")]
    public float accelerationFactor = 30f;
    public float maxSpeed = 30f;
    
    [Header("settings drift")]
    public float driftFactor = 0.95f;
    public float turnMultiplierFactor = 8f;
    public float turnFactor = 3.5f;

    private float _rotationAngle = 0f;
    private bool _canMove = false;

    private FollowCam _cam;
    private Rigidbody2D _cachedRigid2d;
    private PlayerInput _cachedPlayerInput;
    private TransformSender _networkCoordSender;
    

    protected override void OnAwake()
    {
        _cam = FindObjectOfType<FollowCam>(true);
        _cachedRigid2d = GetComponent<Rigidbody2D>();
        _cachedPlayerInput = GetComponent<PlayerInput>();
        _networkCoordSender = GetComponent<TransformSender>();
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
            case GameStateType.None:
                break;
            case GameStateType.Wait:
                _canMove = false;
                break;
            case GameStateType.Playing:
                _canMove = true;
                if(LobbyManager.Instance.CurrentLobby.Players.Count > 1)
                {
                    _networkCoordSender.Run();
                }
                break;
            case GameStateType.EndPlay:
                _canMove = false;
                _networkCoordSender.Stop();
                break;
        }
    }

    private void Start()
    {
        _cam.SetFollower(transform);
        _networkCoordSender.SetTarget(transform);
    }

    private void FixedUpdate()
    {
        if (!_canMove)
        {
            return;
        }

        ApplyEnginForce();
        KillOrthogonalVelocity();
        ApplySteering();

        if (_cachedRigid2d.velocity != Vector2.zero)
        {
            _networkCoordSender.Resume();
        }
        else
        {
            _networkCoordSender.Pause();
        }
    }

    private void ApplyEnginForce()
    {
        float upSpeed = Vector2.Dot(transform.up, _cachedRigid2d.velocity);
        if (upSpeed > maxSpeed && _cachedPlayerInput.Accel > 0f) return;
        if (upSpeed < -maxSpeed * 0.5f && _cachedPlayerInput.Accel < 0f) return;
        if ((_cachedRigid2d.velocity.sqrMagnitude > (maxSpeed * maxSpeed)) && _cachedPlayerInput.Accel > 0f) return;
        
        _cachedRigid2d.drag = _cachedPlayerInput.Accel != 0f ? 0f : Mathf.Lerp(_cachedRigid2d.drag, withoutInputDrag, Time.fixedDeltaTime * dragDelta);
        
        Vector2 enginForceVector = transform.up * (accelerationFactor * _cachedPlayerInput.Accel);

        Acceleration(enginForceVector);    
    }

    private void ApplySteering()
    {
        float factor = _cachedRigid2d.velocity.sqrMagnitude / (turnMultiplierFactor * turnMultiplierFactor);
        float minSpeedBeforeAllowTurningFactor = Mathf.Clamp01(factor);
        
        _rotationAngle -= _cachedPlayerInput.Steering * turnFactor * minSpeedBeforeAllowTurningFactor;

        Steering(_rotationAngle);
    }

    private void KillOrthogonalVelocity()
    {
        Vector2 forwardVelocity = transform.up * Vector2.Dot(_cachedRigid2d.velocity, transform.up);
        Vector2 rightVelocity = transform.right * Vector2.Dot(_cachedRigid2d.velocity, transform.right);

        _cachedRigid2d.velocity = forwardVelocity + rightVelocity * driftFactor;
    }

    public override void Acceleration(Vector2 force)
    {
        _cachedRigid2d.AddForce(force, ForceMode2D.Force);
    }

    public override void Steering(float angle)
    {
        _cachedRigid2d.MoveRotation(_rotationAngle);   
    }
}
