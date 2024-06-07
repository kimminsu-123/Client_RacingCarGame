using System;
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
    public PlayerStatusType status;
    
    public void OnEnable()
    {
        switch (status)
        {
            case PlayerStatusType.Ready:
                bgImage.color = readyColor;
                break;
            case PlayerStatusType.UnReady:
                bgImage.color = isLocal ? localColor : remoteColor;
                break;
        }
    }
}