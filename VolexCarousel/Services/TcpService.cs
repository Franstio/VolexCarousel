using Avalonia.Vulkan;
using Microsoft.Extensions.Logging;
using Microsoft.VisualBasic;
using SkiaSharp;
using SQLitePCL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace VolexCarousel.Services
{
    public abstract class TcpService
    {
        private readonly ILogger<TcpService> _logger;
        private  TcpClient _tcpClient;
        private NetworkStream? _networkStream = null;
        private readonly SemaphoreSlim semaphore = new SemaphoreSlim(1, 1);
        public bool IsConnected => _tcpClient.Connected;
        private IPEndPoint? _endpoint = null;
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
            }
            catch (Exception ex)
            {
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
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to disconnect from Information Speed");
                throw;
            }
        }

        public async Task<string> ReadData()
        {
            CancellationTokenSource source = new CancellationTokenSource();
            try
            {
                source.Token.ThrowIfCancellationRequested();
                source.CancelAfter(TimeSpan.FromSeconds(2));
                DateTime dt = DateTime.Now;
                await semaphore.WaitAsync();
                Reconnect();
                if (_networkStream == null)
                {
                    _logger.LogWarning("Network stream is null. Cannot read data.");
                    throw new Exception("Network stream is null. Cannot read data.");
                }
                byte[] buffer = new byte[1024];
                var data = await _networkStream.ReadAsync(buffer, 0, buffer.Length,source.Token);
                semaphore.Release();
                return Encoding.ASCII.GetString(buffer, 0, data).Trim();
            }
            catch (Exception e)
            {
                semaphore.Release();
                _logger.LogError(e.Message + " | " + e.StackTrace);
                throw;
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
                semaphore.Release();
            }
            catch (Exception e)
            {
                semaphore.Release();
                _logger.LogError(e.Message + " | " + e.StackTrace);
                throw;
            }
            return await ReadData();
        }
    }
}
