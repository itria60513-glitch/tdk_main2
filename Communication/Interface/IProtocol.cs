using System;

namespace Communication.Interface
{
    public delegate void LogEventHandler(int category, string msg);
    public interface IProtocol
    {
        int AddOutFrameInfo(ref byte[] byteArray, int intSize);
        int AddOutFrameInfoWithFakeHeader(ref byte[] byteArray, int intSize);
        int Pop(ref byte[] byteArray);
        void Purge();
        int Push(byte[] byteArray, int intSize);
        int BufferSize
        {
            get;
        }
        (bool, byte[]) VerifyInFrameStructure(byte[] byteArray, int intSize);
        event LogEventHandler LoggingRequest;

    }
}