using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.Services.Authentication;
using UnityEngine;

public class TransformSender : MonoBehaviour
{
    [SerializeField] private Transform _target;
    [SerializeField] private float _interval = 1f;
    [SerializeField] private bool _isRunning = false;

    private float _accumTime = 0f;

    public void SetTarget(Transform target)
    {
        _target = target;
    }

    public void SetInterval(float interval)
    {
        _interval = interval;
    }

    public void Run()
    {
        _accumTime = 0f;
        _isRunning = true;
    }

    public void Stop()
    {
        _isRunning = false;
    }

    private void Update()
    {
        if (!_isRunning)
        {
            return;
        }

        _accumTime += Time.deltaTime;
        if(_accumTime >= _interval)
        {
            _accumTime = 0f;
            Send(_target);
        }
    }

    public void Send(Transform target)
    {
        TransformData data = new TransformData();
        data.SessionId = LobbyManager.Instance.CurrentLobby.Id;
        data.PlayerId = AuthenticationService.Instance.PlayerId;
        data.Position = target.position;
        data.Rotation = target.rotation;

        NetworkManager.Instance.SendTransform(data);
    }

    public void Pause()
    {
        _isRunning = false;
    }

    public void Resume()
    {
        _isRunning = true;
    }
}
