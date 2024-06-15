using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RemoteCarController : CarController
{
    private Vector2 _targetPos;
    private Quaternion _targetRot;

    private void Update()
    {
        if (_targetPos != Vector2.zero)
        {
            Vector2 pos = Vector2.Lerp(transform.position, _targetPos, Time.deltaTime / TransformSender.SEND_INTERVAL);
            Quaternion rot = Quaternion.Slerp(transform.rotation, _targetRot, Time.deltaTime / TransformSender.SEND_INTERVAL);

            if (Vector2.Distance(_targetPos, transform.position) <= 0.001f)
            {
                pos = _targetPos;
                rot = _targetRot;
            }

            transform.position = pos;
            transform.rotation = rot;
        }
    }

    public override void Acceleration(Vector2 force)
    {
        _targetPos = force;
    }

    public override void Steering(float angle)
    {
        _targetRot = Quaternion.Euler(0f, 0f, angle);
    }
}