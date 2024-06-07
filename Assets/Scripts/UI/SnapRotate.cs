using System;
using UnityEngine;
using UnityEngine.UIElements;

public class SnapRotate : MonoBehaviour
{
    public float interval;
    public float snapAngle;
    public bool turnRight;

    private float _accumTime;

    private void OnEnable()
    {
        transform.rotation = Quaternion.identity;
        _accumTime = 0f;
    }

    private void Update()
    {
        _accumTime += Time.deltaTime;

        if (_accumTime >= interval)
        {
            Vector3 rotation = Vector3.forward * (snapAngle * (turnRight ? -1f : 1f));
            transform.Rotate(rotation);
            _accumTime = 0f;
        }
    }
}