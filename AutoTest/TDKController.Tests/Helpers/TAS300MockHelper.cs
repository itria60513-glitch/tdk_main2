using System;
using System.Threading;
using System.Text;
using Communication.Interface;
using Communication.Connector.Enum;

namespace TDKController.Tests.Helpers
{
    /// <summary>
    /// Mock helper that simulates TAS300 response sequences via IConnector.DataReceived.
    /// Intercepts Send() calls to trigger pre-configured response patterns.
    /// </summary>
    public class TAS300MockHelper : IConnector
    {
        /// <summary>Response data to return for ACK responses.</summary>
        private string _ackResponseData;

        /// <summary>Response data to return for INF responses.</summary>
        private string _infResponseData;

        /// <summary>Delay in ms before sending ACK response.</summary>
        private int _ackDelay;

        /// <summary>Delay in ms before sending INF response.</summary>
        private int _infDelay;

        /// <summary>Type of ACK response (ACK or NAK).</summary>
        private string _ackType = "ACK";

        /// <summary>Type of INF response (INF or ABS).</summary>
        private string _infType = "INF";

        /// <summary>If true, skip sending ACK (simulate ACK timeout).</summary>
        private bool _suppressAck;

        /// <summary>If true, skip sending INF (simulate INF timeout).</summary>
        private bool _suppressInf;

        /// <summary>The last command sent via Send().</summary>
        public string LastSentCommand { get; private set; }

        /// <summary>Count of Send() calls.</summary>
        public int SendCount { get; private set; }

        /// <inheritdoc />
        public event ReceivedDataEventHandler DataReceived;

        public IProtocol Protocol { get; set; }
        public bool IsConnected { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        /// <summary>
        /// Configure a successful ACK + INF response sequence (Operation Command).
        /// </summary>
        /// <param name="ackData">Optional data in ACK response.</param>
        /// <param name="infData">Optional data in INF response.</param>
        public void SetupSuccessResponse(string ackData = null, string infData = null)
        {
            _ackType = "ACK";
            _infType = "INF";
            _ackResponseData = ackData;
            _infResponseData = infData;
            _ackDelay = 0;
            _infDelay = 0;
            _suppressAck = false;
            _suppressInf = false;
        }

        /// <summary>
        /// Configure a successful ACK-only response (Quick Command).
        /// </summary>
        /// <param name="responseData">Data payload in the ACK response.</param>
        public void SetupQuickSuccessResponse(string responseData = null)
        {
            _ackType = "ACK";
            _ackResponseData = responseData;
            _ackDelay = 0;
            _suppressAck = false;
            _suppressInf = true;
        }

        /// <summary>
        /// Configure a NAK response.
        /// </summary>
        public void SetupNakResponse()
        {
            _ackType = "NAK";
            _ackDelay = 0;
            _suppressAck = false;
            _suppressInf = true;
        }

        /// <summary>
        /// Configure ABS (abort) completion response.
        /// </summary>
        public void SetupAbsResponse()
        {
            _ackType = "ACK";
            _infType = "ABS";
            _ackDelay = 0;
            _infDelay = 0;
            _suppressAck = false;
            _suppressInf = false;
        }

        /// <summary>
        /// Configure ACK timeout (no response sent).
        /// </summary>
        public void SetupAckTimeout()
        {
            _suppressAck = true;
            _suppressInf = true;
        }

        /// <summary>
        /// Configure INF timeout (ACK sent, INF not sent).
        /// </summary>
        public void SetupInfTimeout()
        {
            _ackType = "ACK";
            _ackDelay = 0;
            _suppressAck = false;
            _suppressInf = true;
        }

        /// <inheritdoc />
        public HRESULT Send(byte[] byPtBuf, int length)
        {
            LastSentCommand = Encoding.ASCII.GetString(byPtBuf ?? Array.Empty<byte>(), 0, Math.Max(0, length));
            SendCount++;

            // Fire responses asynchronously to simulate I/O thread behavior
            ThreadPool.QueueUserWorkItem(_ =>
            {
                try
                {
                    // Phase 1: ACK/NAK response
                    if (!_suppressAck)
                    {
                        if (_ackDelay > 0) Thread.Sleep(_ackDelay);
                        string ackResponse = _ackType;
                        if (!string.IsNullOrEmpty(_ackResponseData))
                        {
                            ackResponse = _ackType + ":" + _ackResponseData;
                        }
                        var ackBytes = Encoding.ASCII.GetBytes(ackResponse);
                        DataReceived?.Invoke(ackBytes, ackBytes.Length);
                    }

                    // Phase 2: INF/ABS response
                    if (!_suppressInf)
                    {
                        if (_infDelay > 0) Thread.Sleep(_infDelay);
                        string infResponse = _infType;
                        if (!string.IsNullOrEmpty(_infResponseData))
                        {
                            infResponse = _infType + ":" + _infResponseData;
                        }
                        var infBytes = Encoding.ASCII.GetBytes(infResponse);
                        DataReceived?.Invoke(infBytes, infBytes.Length);
                    }
                }
                catch
                {
                    // Swallow exceptions in mock helper
                }
            });

            return null;
        }

        /// <summary>
        /// Directly inject a FOUP event response via DataReceived.
        /// </summary>
        /// <param name="eventName">FOUP event name: PODOF, PODON, SMTON, ABNST.</param>
        public void InjectFoupEvent(string eventName)
        {
            var evtBytes = Encoding.ASCII.GetBytes("EVT:" + eventName);
            DataReceived?.Invoke(evtBytes, evtBytes.Length);
        }

        public HRESULT Connect()
        {
            return null;
        }

        public void Disconnect()
        {
        }

        /// <inheritdoc />
        public void Dispose()
        {
            // No resources to clean up in mock
        }
    }
}
