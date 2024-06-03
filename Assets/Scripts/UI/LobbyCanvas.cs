using System;
using TMPro;
using Unity.Services.Lobbies.Models;
using UnityEngine;

public class LobbyCanvas : MonoBehaviour
{
    public TMP_Text lobbyIdText;
    
    private void OnEnable()
    {
        lobbyIdText.text = LobbyManager.Instance.CurrentLobby.LobbyCode;
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            LobbyManager.Instance.LeaveLobby(x =>
            {
                switch (x.Type)
                {
                    case CallbackType.Failed:
                        UIManager.Instance.Alert("Leave Lobby", x.Msg);
                        break;
                    case CallbackType.Success:
                        EventManager.Instance.PostNotification(EventType.OnLeaveLobby, this);
                        break;
                }
            });
        }
    }
}