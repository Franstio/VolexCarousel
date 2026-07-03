using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace VolexCarousel.Services
{
    public class TCPPLCService
    {
        private readonly TcpService _tcpService;
        private readonly ILogger<TCPPLCService> _logger;
        private readonly AppSettingService _appSettingService;
        public TCPPLCService(TcpService tcpService, ILogger<TCPPLCService> logger, AppSettingService appSettingService)
        {
            _tcpService = tcpService;
            _logger = logger;
            _appSettingService = appSettingService;
        }

        public async Task<string> WriteCommand(string command,string value)
        {
            string result = await _tcpService.WriteData($"WR {command} {value}\r\n");
            _logger.LogDebug(result);
            return result;
        }

        public async Task<string> ReadCommand(string command)
        {
            string result = await _tcpService.WriteData($"RD {command}\r\n");
            _logger.LogDebug(result);
            return result;
        }

        public async Task<IEnumerable<string>> PushCommand(string command,TimeSpan interval,params string[] values)
        {
            List<string> results = [];
            for (int i = 0; i < values.Length; i++)
            {
                string result = await _tcpService.WriteData($"WR {command} {values[i]}\r\n");
                _logger.LogDebug(result);
                results.Add(result);
                await Task.Delay(interval);
            }
            return results;
        }
    }
}
