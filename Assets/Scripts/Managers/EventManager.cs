using System.Collections.Generic;
using UnityEngine;

public enum EventType
{
    OnEnterLobby,
    OnLeaveLobby,
    
    OnPlayerJoined,
    OnPlayerLeaved,
    
    OnApplicationWantsToQuit,
}

public class EventManager : SingletonMonobehavior<EventManager>
{
    public delegate void OnEvent(EventType type, Component sender, params object[] args);

    private Dictionary<EventType, List<OnEvent>> _listeners = new();

    public void PostNotification(EventType type, Component sender, params object[] args)
    {
        if (!_listeners.ContainsKey(type)) return;

        foreach (OnEvent listener in _listeners[type])
        {
            listener?.Invoke(type, sender, args);
        }
    }

    public void AddListener(EventType type, OnEvent action)
    {
        if (!_listeners.ContainsKey(type))
        {
            _listeners.Add(type, new List<OnEvent>());
        }

        _listeners[type].Add(action);
    }
    
    public void RemoveListener(EventType type, OnEvent action)
    {
        if (!_listeners.ContainsKey(type)) return;

        _listeners[type].Remove(action);
    }
}