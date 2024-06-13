using System;
using Cinemachine;
using UnityEngine;

public class FollowCam : MonoBehaviour
{
    private CinemachineVirtualCamera _cam;

    private void Awake()
    {
        _cam = GetComponent<CinemachineVirtualCamera>();
    }

    public void SetFollower(Transform target)
    {
        _cam.Follow = target;
        _cam.LookAt = target;
    }
}