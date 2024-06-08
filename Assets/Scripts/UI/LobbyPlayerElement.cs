using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LobbyPlayerElement : MonoBehaviour
{
    public Color localColor;
    public Color remoteColor;
    public Color readyColor;
    
    public TMP_Text playerIdText;
    public Image carColorImg;
    public Image bgImage;

    public bool isLocal;
    public PlayerStatus status;
    
    private void OnEnable()
    {
        ApplyUpdated();
    }

    public void ApplyUpdated()
    {
        switch (status)
        {
            case PlayerStatus.Ready:
                bgImage.color = readyColor;
                break;
            case PlayerStatus.UnReady:
                bgImage.color = isLocal ? localColor : remoteColor;
                break;
        }
    }
}