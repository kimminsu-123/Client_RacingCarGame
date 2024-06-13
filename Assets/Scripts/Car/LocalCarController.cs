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

    private FollowCam _cam;
    private Rigidbody2D _cachedRigid2d;
    private PlayerInput _cachedPlayerInput;

    protected override void OnAwake()
    {
        _cam = FindObjectOfType<FollowCam>(true);
        _cachedRigid2d = GetComponent<Rigidbody2D>();
        _cachedPlayerInput = GetComponent<PlayerInput>();
    }

    private void Start()
    {
        _cam.SetFollower(transform);
    }

    private void FixedUpdate()
    {
        ApplyEnginForce();
        KillOrthogonalVelocity();
        ApplySteering();
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
