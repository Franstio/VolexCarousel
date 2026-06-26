using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace VolexCarousel.Services
{
    public class InformationSpeedService
    {
        private readonly TcpService _tcpService;
        private readonly ILogger<InformationSpeedService> _logger;
        private readonly AppSettingService _appSettingService;

        public InformationSpeedService(TcpService tcpService, ILogger<InformationSpeedService> logger, AppSettingService appSettingService)
        {
            _tcpService = tcpService;
            _logger = logger;
            _appSettingService = appSettingService;
        }


        public async IAsyncEnumerable<string> ReadDataStreamAsync([EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                if (!_tcpService.IsConnected)
                {
                    try
                    {
                        _tcpService.Start();
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Failed to start TCP service");
                        await Task.Delay(5000, cancellationToken); 
                        continue;
                    }
                }
                var data = await _tcpService.ReadData();
                yield return data;
            }
        }
    }
}
