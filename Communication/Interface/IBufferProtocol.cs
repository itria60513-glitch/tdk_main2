namespace Communication.Interface
{
    public interface IBufferProtocol
    {
        void ReturnBufferToPool(byte[] byteArray);
    }
}