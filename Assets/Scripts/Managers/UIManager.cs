using System;
using System.Threading;
using UnityEngine;

public class UIManager : SingletonMonobehavior<UIManager>
{
    private AlertCanvas _alertCanvas;
    private LoadingCanvas _loadingCanvas;
    private TimerCanvas _timerCanvas;

    private void Awake()
    {
        _alertCanvas = GetComponentInChildren<AlertCanvas>(true);
        _loadingCanvas = GetComponentInChildren<LoadingCanvas>(true);
        _timerCanvas = GetComponentInChildren<TimerCanvas>(true);
        
        Debug.Assert(_alertCanvas, "Alert Canvas not found");
        Debug.Assert(_loadingCanvas, "Loading Canvas not found");
        Debug.Assert(_timerCanvas, "Timer Canvas not found");
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

    public void ShowTimer(float time, Action callback)
    {
        _timerCanvas.SetTimer(time, callback);
        _timerCanvas.gameObject.SetActive(true);
    }
}