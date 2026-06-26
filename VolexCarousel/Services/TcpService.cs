using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace VolexCarousel.Services
{
    public class TcpService
    {
        private readonly ILogger<TcpService> _logger;
        private readonly AppSettingService _appSettingService;
        private readonly TcpClient _tcpClient;
        private NetworkStream? _networkStream = null;
        private readonly SemaphoreSlim semaphore = new SemaphoreSlim(1, 1);
        public bool IsConnected => _tcpClient.Connected;
        public TcpService(ILogger<TcpService> logger, AppSettingService appSettingService)
        {
            _logger = logger;
            _appSettingService = appSettingService;
            _tcpClient = new TcpClient();
        }
        public void Start()
        {
            try
            {
                if (_tcpClient.Connected)
                    return;
                var AppSettings = _appSettingService.LoadSettings();
                string data = AppSettings.InformationSpeedPort;
                string ip = data.Split(':')[0];
                int port = int.Parse(data.Split(':')[1]);
                _tcpClient.Connect(ip, port);
                _networkStream = _tcpClient.GetStream();
                _logger.LogInformation($"Connected to Information Speed at {ip}:{port}");
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
            try
            {
                await semaphore.WaitAsync();
                if (_networkStream == null)
                {
                    _logger.LogWarning("Network stream is null. Cannot read data.");
                    throw new Exception("Network stream is null. Cannot read data.");
                }
                byte[] buffer = new byte[1024];
                var data = await _networkStream.ReadAsync(buffer, 0, buffer.Length);
                return Encoding.ASCII.GetString(buffer, 0, data);
            }
            catch (Exception e)
            {
                _logger.LogError(e.Message + " | " + e.StackTrace);
                return string.Empty;
            }
            finally
            {
                semaphore.Release();
            }
        }

        public async Task<string> WriteData(string message)
        {
            try
            {
                await semaphore.WaitAsync();
                if (_networkStream == null)
                {
                    _logger.LogWarning("Network stream is null. Cannot write data.");
                    throw new Exception("Network stream is null. Cannot write data.");
                }
                byte[] buffer = Encoding.ASCII.GetBytes(message);
                await _networkStream.WriteAsync(buffer, 0, buffer.Length);
                return await ReadData();
            }
            catch (Exception e)
            {
                _logger.LogError(e.Message + " | " + e.StackTrace);
                return string.Empty;
            }
            finally
            {
                semaphore.Release();
            }
        }
    }
}
