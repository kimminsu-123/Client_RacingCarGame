using System;
using System.Linq;
using UnityEngine;

public class NetworkPaletteSender : MonoBehaviour, INetworkSender
{
    private PaletteColorButton[] _colorButtons;
    
    private int _selectedButtonIndex;
    
    private void Awake()
    {
        _colorButtons = GetComponentsInChildren<PaletteColorButton>(true);
    }

    private void Start()
    {
        for (int i = 0; i < _colorButtons.Length; i++)
        {
            _colorButtons[i].onClickColor.AddListener(SendColorToOthers);
        }
    }

    private void SendColorToOthers(Color color)
    {
        _selectedButtonIndex = _colorButtons
                                    .ToList()
                                    .FindIndex(x => x.color.Equals(color));
    }

    public void Send()
    {   
        // _selectedButtonIndex 여기서 로비에 있는 다른 플레이어에게 전송
    }
}