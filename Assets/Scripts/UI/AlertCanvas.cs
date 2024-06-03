using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class AlertCanvas : MonoBehaviour
{
    public TMP_Text titleText;
    public TMP_Text msgText;

    public Button closeButton;

    private void Awake()
    {
        closeButton.onClick.AddListener(Close);
    }

    public void Open(string title, string msg)
    {
        titleText.text = title;
        msgText.text = msg;
        
        gameObject.SetActive(true);   
    }

    public void Close()
    {
        gameObject.SetActive(false);
    }
}