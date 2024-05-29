using UnityEngine;

public abstract class CarController : MonoBehaviour
{ 
    public abstract void Acceleration(Vector2 force);
    public abstract void Steering(float angle);
}