#nullable enable
using Communication.Interface;
using System;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using TDKTestSocketServer.Config;

namespace TDKTestSocketServer.Module
{
    public delegate void ShowMessageEventHandler(string serverName, string msg);
    internal class SocketTestServer
    {

        private readonly int _port;
        private TcpListener? _listener;
        private CancellationTokenSource? _cts;
        private readonly string _serverName = "";
        private readonly TDKTestServerConfig _serverConfig;
        public event ShowMessageEventHandler ShowMessageEventHandler;
        private Task? _serverTask;
        private readonly IProtocol _protocol;

        public SocketTestServer(string serverName, TDKTestServerConfig serverConfig, IProtocol protocol)
        {
            _port = int.TryParse(serverConfig.Port,out var tmpPort) ? tmpPort : 8888;
            _serverName = serverName;
            _protocol = protocol;
            _serverConfig = serverConfig;
        }

        private void OnShowMessageEvent(string message)
        {
            ShowMessageEventHandler?.Invoke(_serverName, message);
        }

        public void Start()
        {
            if (_serverTask != null && !_serverTask.IsCompleted)
                return; 

            _cts = new CancellationTokenSource();
            _serverTask = Task.Run(() => StartAsync());
        }


        private async Task StartAsync()
        {
            _listener = new TcpListener(IPAddress.Any, _port);
            _listener.Start();

            OnShowMessageEvent($@"Socket Server {_serverName} started on port {_port}.");

            try
            {
                while (!_cts.Token.IsCancellationRequested)
                {
                    var client = await _listener.AcceptTcpClientAsync();
                    _ = HandleClientAsync(client, _cts.Token);
                }
            }
            catch (ObjectDisposedException)
            {
                // Listener stopped
            }
        }

        public async Task Stop()
        {
            OnShowMessageEvent($@"Socket Server {_serverName} stop.");

            if (_cts == null)
                return;

            _cts.Cancel();

            _listener?.Stop();
            if (_serverTask != null)
                await _serverTask; 

            _cts.Dispose();
            _cts = null;

        }


        private async Task HandleClientAsync(TcpClient client, CancellationToken token)
        {
            try
            {
                using (client)
                using (var stream = client.GetStream())
                {
                    var buffer = new byte[1024];

                    while (!token.IsCancellationRequested)
                    {
                        int bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length, token);

                        if (bytesRead == 0)
                            break;




                        var payload = new byte[bytesRead];
                        Buffer.BlockCopy(buffer, 0, payload, 0, bytesRead);
                        byte[] responseBytes;

                        try
                        {
                            var responseVerify = _protocol.VerifyInFrameStructure(payload, bytesRead);

                            switch (_serverName)
                            {
                                case "Loadport":
                                    responseBytes = responseVerify.Item1
                                        ? LoadportResponse(responseVerify.Item2, responseVerify.Item2.Length)
                                        : LoadportVerifyErrorResponse();

                                    break;
                                default:
                                    responseBytes = new byte[] {0};

                                    break;
                            }

                            var respText = Encoding.UTF8.GetString(responseBytes, 0, responseBytes.Length);

                            if (respText.Contains("FAKE"))
                                _protocol.AddOutFrameInfoWithFakeHeader(ref responseBytes, responseBytes.Length);
                            else
                                _protocol.AddOutFrameInfo(ref responseBytes, responseBytes.Length);

                        }
                        catch (Exception ex)
                        {

                            OnShowMessageEvent($"HandleClient descript error：{ex}");
                            responseBytes = LoadportVerifyErrorResponse();
                            _protocol.AddOutFrameInfo(ref responseBytes, responseBytes.Length);

                        }


                        await stream.WriteAsync(responseBytes, 0, responseBytes.Length, token);
                    }
                }
            }

            catch (OperationCanceledException)
            {
            }
            catch (Exception ex)
            {
                OnShowMessageEvent($"HandleClientAsync UnHandle Exception：{ex}");
            }

        }
        #region LoadportFunction

        public byte[] LoadportResponse(byte[] requestBuffer,int bytesRead)
        {
            if (bytesRead <= 0)
            {
                OnShowMessageEvent($"Receive request error:NAK:TOO_SHORT");
                return Encoding.UTF8.GetBytes("NAK:TOO_SHORT");

            }
            string requestStr = Encoding.UTF8.GetString(requestBuffer, 0, requestBuffer.Length);
            OnShowMessageEvent($"Receive request:{requestStr}");

            requestStr = requestStr.Replace(";", "");

            string responseString;
            var cmd = _serverConfig.Command
                                   .FirstOrDefault(x => x.Item1 == requestStr);

            if (cmd != default)
            {
                responseString = cmd.Item2 ?? "";
            }
            else
            {
                responseString = $"NAK:{requestStr}";
                OnShowMessageEvent($"Receive request error.Response command: NAK:{requestStr}");
            }

            return Encoding.UTF8.GetBytes(responseString);

        }

        public byte[] LoadportVerifyErrorResponse()
        {
            OnShowMessageEvent($"Receive request error:NAK:CKSUM");
            return Encoding.UTF8.GetBytes(@"NAK:CKSUM");
        }
        #endregion LoadportFunction
    }
}
