using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.Services.Authentication;
using UnityEngine;

public class TransformSender : MonoBehaviour
{
    public const float SEND_INTERVAL = 0.1f;

    [SerializeField] private Transform _target;
    [SerializeField] private bool _isRunning = false;

    private bool _isPause = false;
    private float _accumTime = 0f;

    public void SetTarget(Transform target)
    {
        _target = target;
    }

    public void Run()
    {
        _accumTime = 0f;
        _isRunning = true;
        _isPause = false;
    }

    public void Stop()
    {
        _isRunning = false;
        _isPause = false;
    }

    private void Update()
    {
        if (!_isRunning || _isPause)
        {
            return;
        }

        _accumTime += Time.deltaTime;
        if(_accumTime >= SEND_INTERVAL)
        {
            _accumTime = 0f;
            Send(_target);
        }
    }

    public void Send(Transform target)
    {
        TransformData data = new TransformData();
        data.SessionId = LobbyManager.Instance.CurrentLobby.Id;
        data.PlayerId = PlayerManager.Instance.LocalPlayer.Id;
        data.Position = target.position;
        data.Rotation = target.rotation;

        NetworkManager.Instance.SendTransform(data);
    }

    public void Pause()
    {
        _isPause = true;
    }

    public void Resume()
    {
        _isPause = false;
    }
}
