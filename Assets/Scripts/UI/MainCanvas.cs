using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MainCanvas : MonoBehaviour
{
    public TMP_InputField lobbyNameInputField;
    public Button createLobbyButton;
    public Button joinLobbyButton;

    private void Start()
    {
        createLobbyButton.onClick.AddListener(CreateLobby);
        joinLobbyButton.onClick.AddListener(JoinLobby);
    }

    private void OnEnable()
    {
        createLobbyButton.interactable = LobbyManager.Instance.Initialized;
        joinLobbyButton.interactable = LobbyManager.Instance.Initialized;
        
        if (!LobbyManager.Instance.Initialized)
        {
            LobbyManager.Instance.Initialize(OnInitialized);
        }
    }

    private void CreateLobby()
    {
        string lobbyName = lobbyNameInputField.text;

        if (string.IsNullOrEmpty(lobbyName))
        {
            UIManager.Instance.Alert("Create Lobby", "Cannot be null or empty lobby name");
            return;
        }
        
        createLobbyButton.interactable = false;
        joinLobbyButton.interactable = false;
        
        LobbyManager.Instance.CreateLobby(lobbyName, OnCreateLobby);
    }
    
    private void JoinLobby()
    {
        string lobbyCode = lobbyNameInputField.text;

        if (string.IsNullOrEmpty(lobbyCode))
        {
            UIManager.Instance.Alert("Create Lobby", "Cannot be null or empty lobby name");
            return;
        }

        createLobbyButton.interactable = false;
        joinLobbyButton.interactable = false;
        
        LobbyManager.Instance.JoinLobby(lobbyCode, OnJoinLobby);   
    }

    private void OnInitialized(LobbyCallbackToken token)
    {
        switch (token.Type)
        {
            case CallbackType.Failed:
                createLobbyButton.interactable = false;
                joinLobbyButton.interactable = false;
                break;
            case CallbackType.Success:
                createLobbyButton.interactable = true;
                joinLobbyButton.interactable = true;
                break;
        }
    }
    
    private void OnCreateLobby(LobbyCallbackToken token)
    {
        createLobbyButton.interactable = true;
        joinLobbyButton.interactable = true;
        
        switch (token.Type)
        {
            case CallbackType.Failed:
                UIManager.Instance.Alert("Failed Create Lobby", token.Msg);
                break;
            case CallbackType.Success:
                EventManager.Instance.PostNotification(EventType.OnEnterLobby, this);
                break;
        }
    }

    private void OnJoinLobby(LobbyCallbackToken token)
    {   
        createLobbyButton.interactable = true;
        joinLobbyButton.interactable = true;
        
        switch (token.Type)
        {
            case CallbackType.Failed:
                UIManager.Instance.Alert("Failed Join Lobby", token.Msg);
                break;
            case CallbackType.Success:
                EventManager.Instance.PostNotification(EventType.OnEnterLobby, this);
                break;
        }
    }
}