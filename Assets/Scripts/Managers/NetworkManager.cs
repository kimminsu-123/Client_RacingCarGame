using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Threading;
using Baracuda.Threading;
using Unity.Services.Authentication;
using UnityEngine;

public class NetworkManager : SingletonMonobehavior<NetworkManager>
{
    public const string OperationServerIp = "127.0.0.1";
    public const int OperationServerPort = 9000;

    public bool IsConnected { get; private set; }
    
    private Network _network;
    private bool _isQuit;
    
    private void Start()
    {
        _isQuit = false;
        IsConnected = false;
        
        Application.wantsToQuit += OnApplicationWantsToQuit;

        _network = new Network(OperationServerIp, OperationServerPort);
        _network.RegisterReceiveCallback(PacketType.Connect, OnConnect);
        _network.RegisterReceiveCallback(PacketType.Disconnect, OnDisconnect);
        _network.RegisterReceiveCallback(PacketType.SyncTransform, OnSyncTransform);
        _network.RegisterReceiveCallback(PacketType.GoalLine, OnGoalLine);
        _network.Initialize();
    }

    private void OnConnect(PacketInfo info)
    {
        Debug.Log("On Connected");
        
        if (info.Header.ResultType == ResultType.Success)
        {
            IsConnected = true;
        }
        else
        {
            IsConnected = false;
        }
    }

    private void OnDisconnect(PacketInfo info)
    {
        Debug.Log("On Disconnected");

        if (info.Header.ResultType == ResultType.Success)
        {
            IsConnected = false;
        }
        else
        {
            IsConnected = true;
        }

        if (_isQuit)
        {
            Application.Quit();
        }
    }

    private void OnSyncTransform(PacketInfo info)
    {
    }

    private void OnGoalLine(PacketInfo info)
    {
    }

    private bool OnApplicationWantsToQuit()
    {
        bool isConnected = IsConnected;
        
        if (isConnected)
        {
            ConnectionData data = new ConnectionData();
            data.PlayerId = AuthenticationService.Instance.PlayerId;
            data.SessionId = LobbyManager.Instance.CurrentLobby.Id;
            
            DisconnectServer(data);
        }

        return !isConnected;
    }

    public void ConnectServer(ConnectionData data)
    {
        ConnectionPacket packet = new ConnectionPacket(data);
        
        _network.EnqueueSendPacket(PacketType.Connect, packet);
    }

    public void DisconnectServer(ConnectionData data)
    {
        ConnectionPacket packet = new ConnectionPacket(data);
        
        _network.EnqueueSendPacket(PacketType.Disconnect, packet);
    }

    public void SendTransform(TransformData data)
    {
        TransformPacket packet = new TransformPacket(data);
        
        _network.EnqueueSendPacket(PacketType.GoalLine, packet);
    }

    public void SendGoal(ConnectionData data)
    {
        ConnectionPacket packet = new ConnectionPacket(data);
        
        _network.EnqueueSendPacket(PacketType.GoalLine, packet);
    }

}

public class Network : IDisposable
{
    public delegate void OnReceiveEvent(PacketInfo info);
    
    private Socket _socket;
    private string _serverIp;
    private int _serverPort;

    private readonly ConcurrentQueue<PacketInfo> _sendQueue;
    private ConcurrentDictionary<PacketType, List<OnReceiveEvent>> _receiveListeners;

    private volatile bool _isRunning;
    private volatile PacketHeaderSerializer _headerSerializer;
    private volatile List<Thread> _workerThreads;
    private volatile EndPoint _serverEp;

    private const int ReceiveIntervalMS = 100;
    private const int SendIntervalMS = 100;
    private const int BufferSize = 512;
    
    public Network(string ip, int port)
    {
        _socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);

        _serverIp = ip;
        _serverPort = port;

        _sendQueue = new ConcurrentQueue<PacketInfo>();
        _receiveListeners = new ConcurrentDictionary<PacketType, List<OnReceiveEvent>>();

        _isRunning = false;
        _headerSerializer = new PacketHeaderSerializer();
        _workerThreads = new List<Thread>();
        _serverEp = new IPEndPoint(IPAddress.Parse(ip), port);
    }

    public void RegisterReceiveCallback(PacketType type, OnReceiveEvent callback)
    {
        if (!_receiveListeners.ContainsKey(type))
        {
            _receiveListeners.TryAdd(type, new List<OnReceiveEvent>());
        }
        
        _receiveListeners[type].Add(callback);
    }
    
    public void UnRegisterReceiveCallback(PacketType type, OnReceiveEvent callback)
    {
        if (!_receiveListeners.ContainsKey(type))
        {
            return;
        }
        
        _receiveListeners[type].Remove(callback);
    }

    public void PostNotificationOnReceive(PacketType type, PacketInfo info)
    {
        if (!_receiveListeners.ContainsKey(type))
        {
            return;
        }

        Dispatcher.Invoke(() =>
        {
            foreach (OnReceiveEvent callback in _receiveListeners[type])
            {
                callback?.Invoke(info);
            }
        });
    }
    
    public void Initialize()
    {
        if (_isRunning)
        {
            return;
        }
        
        _isRunning = true;

        Thread sendThread = new Thread(LoopSend);
        Thread receiveThread = new Thread(LoopReceive);

        sendThread.IsBackground = true;
        receiveThread.IsBackground = true;
        
        sendThread.Start();
        receiveThread.Start();
        
        _workerThreads.Add(sendThread);
        _workerThreads.Add(receiveThread);
    }

    public void Dispose()
    {
        _isRunning = false;

        foreach (Thread thread in _workerThreads)
        {
            thread.Join();
        }
        
        _workerThreads.Clear();
        _sendQueue.Clear();
        _socket?.Dispose();
    }

    public void EnqueueSendPacket<T>(PacketType type, IPacket<T> packet)
    {
        PacketHeader header;
        header.PacketType = type;
        header.PacketId = 0;
        header.ResultType = ResultType.Success;
        
        PacketInfo packetInfo = new PacketInfo();
        packetInfo.Header = header; 
        packetInfo.Buffer = packet.GetBytes();
        
        _sendQueue.Enqueue(packetInfo);
    }

    private void LoopSend()
    {
        while (_isRunning)
        {
            Thread.Sleep(SendIntervalMS);

            if(_sendQueue.IsEmpty) continue;

            _sendQueue.TryDequeue(out PacketInfo packetInfo);

            if (packetInfo == null) continue;

            bool ret = _headerSerializer.Serialize(packetInfo.Header);
            if (!ret)
            {
                Dispatcher.Invoke(() =>
                {
                    EventManager.Instance.PostNotification(EventType.OnFailedNetworkTransfer, null, "Failed Packet Header Serialize");     
                });
                return;
            }

            byte[] headerBytes = _headerSerializer.GetBuffer();
            byte[] sendBuffer = new byte[headerBytes.Length + packetInfo.Buffer.Length];
            int headerSize = Marshal.SizeOf(typeof(PacketHeader));
            Buffer.BlockCopy(headerBytes, 0, sendBuffer, 0, headerSize);
            Buffer.BlockCopy(packetInfo.Buffer, 0, sendBuffer, headerSize, packetInfo.Buffer.Length);

            try
            {
                SocketAsyncEventArgs receiveArgs = new SocketAsyncEventArgs();
                receiveArgs.SetBuffer(sendBuffer, 0, sendBuffer.Length);
                receiveArgs.RemoteEndPoint = _serverEp;
                receiveArgs.Completed += SendCompleted;

                _socket.SendToAsync(receiveArgs);   
            }
            catch (Exception err)
            {
                Dispatcher.Invoke(() =>
                {
                    EventManager.Instance.PostNotification(EventType.OnFailedNetworkTransfer, null, err.Message);     
                });
            }
        }
    }

    private void SendCompleted(object sender, SocketAsyncEventArgs e)
    {
        Dispatcher.Invoke(() =>
        {
            if (e.SocketError != SocketError.Success)
            {
                EventManager.Instance.PostNotification(EventType.OnFailedNetworkTransfer, null, e.SocketError.ToString());
            }     
        });
    }

    private void LoopReceive()
    {
        while (_isRunning)
        {
            try
            {
                Thread.Sleep(ReceiveIntervalMS);
    
                SocketAsyncEventArgs receiveArgs = new SocketAsyncEventArgs();
                receiveArgs.SetBuffer(new byte[BufferSize], 0, BufferSize);
                receiveArgs.RemoteEndPoint = new IPEndPoint(IPAddress.None, 0);
                receiveArgs.Completed += ReceiveCompleted;

                _socket.ReceiveFromAsync(receiveArgs);   
            }
            catch (Exception err)
            {
                Dispatcher.Invoke(() =>
                {
                    EventManager.Instance.PostNotification(EventType.OnFailedNetworkTransfer, null, err.Message);     
                });     
            }
        }
    }

    private void ReceiveCompleted(object sender, SocketAsyncEventArgs e)
    {
        PacketHeader header = default;
        byte[] received = new byte[e.BytesTransferred];
        Buffer.BlockCopy(e.Buffer, 0, received, 0, received.Length);
        bool ret = _headerSerializer.Deserialize(received, ref header);

        if (!ret)
        {
            Dispatcher.Invoke(() =>
            {
                EventManager.Instance.PostNotification(EventType.OnFailedNetworkTransfer, null, "Failed Packet Header Deserialize");     
            });
            return;
        }

        int headerSize = Marshal.SizeOf(typeof(PacketHeader));
        byte[] packetData = new byte[received.Length - headerSize];
        Buffer.BlockCopy(received, headerSize, packetData, 0, packetData.Length);

        PacketInfo packetInfo = new PacketInfo
        {
            Header = header,
            Buffer = packetData,
            ClientEndPoint = e.RemoteEndPoint
        };

        PostNotificationOnReceive(header.PacketType, packetInfo);
    }
}