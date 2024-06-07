using System;
using UnityEngine;

public class UIManager : SingletonMonobehavior<UIManager>
{
    private AlertCanvas _alertCanvas;
    private LoadingCanvas _loadingCanvas;

    private void Awake()
    {
        _alertCanvas = GetComponentInChildren<AlertCanvas>(true);
        _loadingCanvas = GetComponentInChildren<LoadingCanvas>(true);
        
        Debug.Assert(_alertCanvas, "Alert Canvas not found");
    }

    public void Alert(string title, string msg)
    {
        Debug.Log($"{title} : {msg}");
        
        _alertCanvas.Open(title, msg);
    }

    public void ShowLoading()
    {
        _loadingCanvas.gameObject.SetActive(true);
    }
    
    public void HideLoading()
    {
        _loadingCanvas.gameObject.SetActive(false);
    }
}