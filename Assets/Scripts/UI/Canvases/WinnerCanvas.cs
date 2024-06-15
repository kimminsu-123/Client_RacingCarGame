using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class WinnerCanvas : MonoBehaviour
{
    public TMP_Text winnerIdText;
    public Button exitButton;

    public void ShowWinner(string winnerId)
    {
        winnerIdText.text = winnerId;
    }

    public void Hide()
    {
        ConnectionData data = new ConnectionData();
        data.SessionId = LobbyManager.Instance.CurrentLobby.Id;
        data.PlayerId = PlayerManager.Instance.LocalPlayer.Id;
        NetworkManager.Instance.DisconnectServer(data);

        EventManager.Instance.PostNotification(EventType.OnEnterLobby, this);

        gameObject.SetActive(false);
    }

    private void OnEnable()
    {
        Invoke("ActiveExitButton", 3f);
    }

    private void OnDisable()
    {
        exitButton.interactable = false;
    }

    private void ActiveExitButton()
    {
        exitButton.interactable = true;
    }
}
