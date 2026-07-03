using Avalonia.Threading;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace VolexCarousel.Services
{
    public class InformationSpeedService : TcpService
    {
        private readonly ILogger<InformationSpeedService> _logger;
        private readonly ILogger<TcpService> _tcpLogger;
        private readonly AppSettingService _appSettingService;

        public InformationSpeedService(ILogger<TcpService> tcpLogger, ILogger<InformationSpeedService> logger, AppSettingService appSettingService) : base(tcpLogger)
        {
            _tcpLogger = tcpLogger;
            _logger = logger;
            _appSettingService = appSettingService;
        }


        public async IAsyncEnumerable<string> ReadDataStreamAsync([EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                if (!IsConnected)
                {
                    try
                    {
                        IPEndPoint endpoint = IPEndPoint.Parse(_appSettingService.LoadSettings().InformationSpeedPort);
                        Start(endpoint);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Failed to start TCP service");
                        await Task.Delay(5000, cancellationToken); 
                        continue;
                    }
                }
                var data = await ReadData();
                yield return data;
            }
            Stop();
        }
    }
}
