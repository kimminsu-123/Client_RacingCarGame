using System;
using UnityEngine;

public class UIManager : SingletonMonobehavior<UIManager>
{
    private AlertCanvas _alertCanvas;

    private void Awake()
    {
        _alertCanvas = GetComponentInChildren<AlertCanvas>(true);
        
        Debug.Assert(_alertCanvas, "Alert Canvas not found");
    }

    public void Alert(string title, string msg)
    {
        Debug.Log($"{title} : {msg}");
        
        _alertCanvas.Open(title, msg);
    }
}