public interface INetworkSender
{
    public void Send();
}

public interface INetworkReceiver<T>
{
    public void Receive(T packet);
}

public interface IPacket<T>
{
    T Data { get; set; }
    byte[] Serialize();
    T Deserialize(byte[] bytes);
}

public interface ICompress
{
    byte[] Compress(byte[] bytes);
    byte[] Decompress(byte[] bytes);
}