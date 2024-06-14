using System;
using TMPro;
using UnityEngine;

public class TimerCanvas : MonoBehaviour
{
    public TMP_Text timerText;

    private float _time;
    private Action _callback;
    
    public void SetTimer(float time, Action callback)
    {
        _time = time;
        _callback = callback;
        timerText.text = $"{time}";
    }

    private void Update()
    {
        _time -= Time.deltaTime;

        if (_time <= 0f)
        {
            _callback?.Invoke();
            gameObject.SetActive(false);
        }

        timerText.text = $"{_time:F1}";
    }
}