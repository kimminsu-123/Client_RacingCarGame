public interface INetworkSender
{
    public void Send();
}

public interface INetworkReceiver<T>
{
    public void Receive(T packet);
}