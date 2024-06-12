using System;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Text;
using UnityEngine;

public class NetworkTest : MonoBehaviour
{
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            ConnectionData data = new ConnectionData();
            data.PlayerId = "123";
            data.SessionId = "123123";
            
            NetworkManager.Instance.ConnectServer(data);
        }
        
        if (Input.GetKeyDown(KeyCode.Backspace))
        {
            ConnectionData data = new ConnectionData();
            data.PlayerId = "123";
            data.SessionId = "123123";
            
            NetworkManager.Instance.DisconnectServer(data);
        }
    }
}