using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using UnityEngine;

public class NetworkTest : MonoBehaviour
{
    private Socket _socket;
    
    private void Start()
    {
        _socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            _socket.SendTo(Encoding.Default.GetBytes("Hello"), new IPEndPoint(IPAddress.Parse("127.0.0.1"), 9000));
        }
    }
}