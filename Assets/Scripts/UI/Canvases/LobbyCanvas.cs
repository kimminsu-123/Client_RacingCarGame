using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using Unity.Services.Lobbies.Models;
using UnityEngine;
using UnityEngine.UI;

public class LobbyCanvas : MonoBehaviour
{
    public LobbyPlayerElement[] playerElements;
    public PaletteColorButton[] paletteColorButtons;
    public Image carPreviewImage;
    public TMP_Text lobbyIdText;

    private void Start()
    {
        EventManager.Instance.AddListener(EventType.OnPlayerJoined, OnPlayerUpdated);
        EventManager.Instance.AddListener(EventType.OnPlayerLeaved, OnPlayerUpdated);
        EventManager.Instance.AddListener(EventType.OnPlayerColorChanged, OnPlayerUpdated);
        EventManager.Instance.AddListener(EventType.OnPlayerStatusChanged, OnPlayerUpdated);

        foreach (PaletteColorButton colorButton in paletteColorButtons)
        {
            colorButton.onClickColor.AddListener(OnChangedColor);
        }
    }

    private void OnEnable()
    {
        lobbyIdText.text = LobbyManager.Instance.CurrentLobby.LobbyCode;

        UpdatePlayerList();
    }

    private void OnChangedColor(PaletteColorButton button)
    {
        carPreviewImage.color = button.color;

        foreach (var b in paletteColorButtons)
        {
            b.ApplyInteractInterval();
        }

        int index = paletteColorButtons.ToList().IndexOf(button);

        Dictionary<string, string> option = new Dictionary<string, string>
        {
            { LobbyManager.Instance.ChangeColorName, index.ToString() }
        };

        LobbyManager.Instance.UpdatePlayerData(option, token =>
        {
            if (token.Type == CallbackType.Success)
            {
                EventManager.Instance.PostNotification(EventType.OnPlayerColorChanged, this);
            }
        });
    }

    public void LeaveLobby()
    {
        LobbyManager.Instance.LeaveLobby(OnLeaveLobby);
    }

    public Color GetColor(int index)
    {
        if (index < 0 || paletteColorButtons.Length <= index)
        {
            return Color.white;
        }

        return paletteColorButtons[index].color;
    }

    public void ReadyPlayer(bool flag)
    {
        PlayerStatus status = flag ? PlayerStatus.Ready : PlayerStatus.UnReady;
        
        Dictionary<string, string> options = new Dictionary<string, string>()
        {
            { LobbyManager.Instance.ChangeStatusName, $"{(int)status}" }
        };

        LobbyManager.Instance.UpdatePlayerData(options, token =>
        {
            if (token.Type == CallbackType.Success)
            {
                EventManager.Instance.PostNotification(EventType.OnPlayerStatusChanged, this);
            }
        });
    }
    
    private void OnLeaveLobby(LobbyCallbackToken token)
    {
        switch (token.Type)
        {
            case CallbackType.Failed:
                UIManager.Instance.Alert("Failed Leave Lobby", token.Msg);
                break;
            case CallbackType.Success:
                EventManager.Instance.PostNotification(EventType.OnLeaveLobby, this);
                break;
        }
    }

    private void UpdatePlayerList()
    {
        Lobby lobby = LobbyManager.Instance.CurrentLobby;
        Player localPlayer = PlayerManager.Instance.LocalPlayer;

        for (int index = 0; index < playerElements.Length; index++)
        {
            playerElements[index].gameObject.SetActive(false);
        }

        for (int index = 0; index < lobby.Players.Count; index++)
        {
            Player player = lobby.Players[index];
            ConfigurePlayerElement(index, player, localPlayer);
        }
    }

    private void OnPlayerUpdated(EventType type, Component sender, object[] args)
    {
        UpdatePlayerList();
    }

    private void ConfigurePlayerElement(int lastIndex, Player curPlayer, Player localPlayer)
    {
        var colorIndex = int.Parse(curPlayer.Data[LobbyManager.Instance.ChangeColorName].Value);
        var statusFlag = curPlayer.Data[LobbyManager.Instance.ChangeStatusName].Value;

        playerElements[lastIndex].carColorImg.color = paletteColorButtons[colorIndex].color;
        playerElements[lastIndex].isLocal = curPlayer.Id.Equals(localPlayer.Id);
        playerElements[lastIndex].status = (PlayerStatus)int.Parse(statusFlag);
        playerElements[lastIndex].playerIdText.text = curPlayer.Id;
        playerElements[lastIndex].gameObject.SetActive(true);
    }
}