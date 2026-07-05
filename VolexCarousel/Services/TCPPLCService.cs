using Microsoft.Extensions.Logging;
using NModbus;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using VolexCarousel.Interfaces;

namespace VolexCarousel.Services
{
    public class TCPPLCService : TcpService,ICheckItemService
    {
        private readonly ILogger<TCPPLCService> _logger;
        private readonly AppSettingService _appSettingService;
        private readonly SemaphoreSlim semaphoreSlim = new SemaphoreSlim(1, 1);
        public TCPPLCService(ILogger<TCPPLCService> logger, AppSettingService appSettingService,ILogger<TcpService> tcpLogger) : base(tcpLogger)
        {
            _logger = logger;
            _appSettingService = appSettingService;
        }
        public void Start()
        {
            try
            {
                semaphoreSlim.Wait();
                if (IsConnected)
                    return;
                IPEndPoint endpoint = IPEndPoint.Parse(_appSettingService.LoadSettings().PLCPort);
                Start(endpoint);
            }

            catch (Exception e)
            {
                _logger.LogError(e.Message + " | " + e.StackTrace);
            }
            finally
            {
                semaphoreSlim.Release();
            }
        }


        public async Task<string> WriteCommand(string command,string value)
        {
            string result = await WriteData($"WR {command} {value}\r\n");
            _logger.LogDebug(result);
            return result;
        }

        public async Task<string> ReadCommand(string command)
        {
            string result = await WriteData($"RD {command}\r\n");
            _logger.LogDebug(result);
            return result;
        }

        public async Task<IEnumerable<string>> PushCommand(string command,TimeSpan interval,params string[] values)
        {
            List<string> results = [];
            for (int i = 0; i < values.Length; i++)
            {
                string result = await WriteData($"WR {command} {values[i]}\r\n");
                _logger.LogDebug(result);
                results.Add(result);
                await Task.Delay(interval);
            }
            return results;
        }

        public async Task<string> CheckItemAsync(object id)
        {
            return await ReadCommand(id.ToString()!);
        }
    }
}
