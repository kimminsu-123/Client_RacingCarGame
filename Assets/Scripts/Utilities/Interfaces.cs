public interface IPacket<T>
{
    byte[] GetBytes();
    T GetData();
}