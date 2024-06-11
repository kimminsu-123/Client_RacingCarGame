using System;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Text;
using UnityEngine;

public class NetworkTest : MonoBehaviour
{
    private Socket _socket;
    
    private void Start()
    {
        _socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
    }

    private PacketHeaderSerializer HeaderSerializer = new PacketHeaderSerializer();
    
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            PacketHeader header;
            header.PacketType = PacketType.Connect;
            header.PacketId = 1;

            PacketInfo packetInfo = new PacketInfo();
            packetInfo.Buffer = Encoding.Default.GetBytes("Hello");
            packetInfo.Header = header;

            HeaderSerializer.Serialize(packetInfo.Header);
            byte[] headerBytes = HeaderSerializer.GetBuffer();

            byte[] sendBuffer = new byte[headerBytes.Length + packetInfo.Buffer.Length];
            int headerSize = Marshal.SizeOf(typeof(PacketHeader));
            Buffer.BlockCopy(headerBytes, 0, sendBuffer, 0, headerSize);
            Buffer.BlockCopy(packetInfo.Buffer, 0, sendBuffer, headerSize, packetInfo.Buffer.Length);
            
            _socket.SendTo(sendBuffer, new IPEndPoint(IPAddress.Parse("127.0.0.1"), 9000));
        }
    }
}