using System;

namespace EFEM.DataCenter
{
    public interface IConnectorConfig
    {
        string Comport { get; set; }
        int Baud { get; set; }
        int Parity { get; set; }
        int StopBits { get; set; }
        int DataBits { get; set; }

        string Ip { get; set; }
        string Port { get; set; }

        event EventHandler ModifyValueChangedEvent;
    }
}