using System;
using System.Linq;
using Unity.Services.Authentication;
using Unity.Services.Lobbies.Models;
using UnityEngine;

public class PlayerManager : SingletonMonobehavior<PlayerManager>
{
    public Player LocalPlayer => LobbyManager.Instance.CurrentLobby
                                                        .Players
                                                        .FirstOrDefault(
                                                            p => p.Id.Equals(AuthenticationService.Instance.PlayerId)
                                                        );
    
    private void Start()
    {
        EventManager.Instance.AddListener(EventType.OnPlayerStatusChanged, OnPlayerStatusChanged);
    }

    private void OnPlayerStatusChanged(EventType type, Component sender, object[] args)
    {
        if (!LobbyManager.Instance.IsAllReady)
        {
            return;
        }
        
        GameManager.Instance.lobbyCanvas.gameObject.SetActive(false);
        UIManager.Instance.ShowLoading();
        
        // 만약 모든 플레이어가 준비 완료이면
            // Lobby UI 끄기
            // Loading UI 켜기
            // 서버랑 연결하기
            // 로비 옵션에 ConnectedServer 옵션을 True 로 바꾸기
            // 만약 모든 플레이어가 True가 되면 게임 시작하기
                // 타이머 시작
    }
}