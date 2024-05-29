using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerInput : MonoBehaviour
{
    private static readonly string AccelerationInputName = "Vertical";
    private static readonly string SteeringInputName = "Horizontal";

    public float Accel { get; private set; }
    public float Steering { get; private set; } 
    
    private void Update()
    {
        Accel = Input.GetAxis(AccelerationInputName);
        Steering = Input.GetAxis(SteeringInputName);
    }
}
