using Microsoft.Extensions.Logging;
using Serilog;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace VolexCarousel.Core.Services
{
    public abstract class TcpService
    {
        private readonly ILogger<TcpService> _logger;
        private  TcpClient _tcpClient;
        private NetworkStream? _networkStream = null;
        private readonly SemaphoreSlim semaphore = new SemaphoreSlim(1, 1);
        private readonly SemaphoreSlim semaphoreRead = new SemaphoreSlim(1, 1);
        public bool IsConnected => _tcpClient.Connected;
        private IPEndPoint? _endpoint = null;
        public event Action<string>? OnResponse;
        public TcpService(ILogger<TcpService> logger)
        {
            _logger = logger;
            _tcpClient = new TcpClient();
            _tcpClient.ReceiveTimeout = 3000;
            _tcpClient.SendTimeout = 3000;
        }
        public void Reconnect()
        {
            if (_endpoint is null || _tcpClient.Connected) return;
            Start(_endpoint);
        }
        public void Start(IPEndPoint endpoint)
        {
            try
            {
                if (_tcpClient.Connected)
                    return;
                _endpoint = endpoint;
                _tcpClient = new TcpClient();
                _tcpClient.Connect(endpoint);
                _networkStream = _tcpClient.GetStream();
                _networkStream.ReadTimeout = 3000;
                _networkStream.WriteTimeout = 3000;
                _logger.LogInformation($"Connected to Information Speed at {endpoint.Address}:{endpoint.Port}");
                OnResponse?.Invoke("Connected");
            }
            catch (Exception ex)
            {
                OnResponse?.Invoke("Error: "+ex.Message);
                _logger.LogError(ex, "Failed to connect to Information Speed");
                throw;
            }
        }

        public void Stop()
        {
            try
            {
                if (!_tcpClient.Connected)
                    return;
                _tcpClient.Close();
                _logger.LogInformation("Disconnected from Information Speed");
                OnResponse?.Invoke("Closing tcp");

            }
            catch (Exception ex)
            {
                OnResponse?.Invoke("Error closing");

                _logger.LogError(ex, "Failed to disconnect from Information Speed");
                throw;
            }
        }

        public async Task<string> ReadData()
        {
            CancellationTokenSource source = new CancellationTokenSource();
            try
            {
                DateTime dt = DateTime.Now;
                await semaphoreRead.WaitAsync();
                source.Token.ThrowIfCancellationRequested();
                source.CancelAfter(TimeSpan.FromSeconds(5));
                Reconnect();
                if (_networkStream == null)
                {
                    _logger.LogWarning("Network stream is null. Cannot read data.");
                    throw new Exception("Network stream is null. Cannot read data.");
                }
                byte[] buffer = new byte[1024];
                var data = await _networkStream.ReadAsync(buffer, 0, buffer.Length,source.Token);
                var res = Encoding.ASCII.GetString(buffer, 0, data).Trim();
                return res;
            }
            catch (Exception e)
            {
                this.Stop();
                Reconnect();
                OnResponse?.Invoke("Error Reading: "+e.Message );

                _logger.LogError(e.Message + " | " + e.StackTrace);
                throw;
            }
            finally
            {
                semaphoreRead.Release();

            }
        }

        public async Task<string> WriteData(string message)
        {
            try
            {
                DateTime dt = DateTime.Now;
                await semaphore.WaitAsync();
                Reconnect();
                if (_networkStream == null)
                {
                    _logger.LogWarning("Network stream is null. Cannot write data.");
                    throw new Exception("Network stream is null. Cannot write data.");
                }
                byte[] buffer = Encoding.ASCII.GetBytes(message);
                await _networkStream.WriteAsync(buffer, 0, buffer.Length);

                var res = await ReadData();
                if (string.IsNullOrEmpty(res))
                {
                    Stop();
                    Reconnect();
                    throw new NoNullAllowedException("PLC Return is null or empty");
                }
                OnResponse?.Invoke("Sucess write: " + message);
                OnResponse?.Invoke($"Result {message} : {res}");
                return res;
            }
            catch (Exception e)
            {
                OnResponse?.Invoke("Error Write: "+e.Message);

                _logger.LogError(e.Message + " | " + e.StackTrace);
                throw;
            }
            finally
            {
                semaphore.Release();
            }
        }
    }
}
