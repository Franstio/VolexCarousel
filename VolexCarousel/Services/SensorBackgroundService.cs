using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using VolexCarousel.Core.Interfaces;
using VolexCarousel.Models;

namespace VolexCarousel.Services
{
    public class SensorBackgroundService : BackgroundService
    {
        private readonly ICheckItemService checkItemService;
        private readonly ChannelWriter<DateTime> channelWriter;
        private readonly ILogger<SensorBackgroundService> logger;
        private readonly string SENSOR_ADDRESS = "R003";
        private bool firstCheck = false,secondCheck=false;
        
        public SensorBackgroundService(ICheckItemService _checkItemService,
            [FromKeyedServices("sensorChannel")] Channel<DateTime> sensorChannel,
            ILogger<SensorBackgroundService> _logger)
        {
            checkItemService = _checkItemService;
            channelWriter = sensorChannel.Writer;
            logger = _logger;
        }
        public override Task StartAsync(CancellationToken cancellationToken)
        {
            checkItemService.Start();
            return base.StartAsync(cancellationToken);
        }
        public override Task StopAsync(CancellationToken cancellationToken)
        {
            checkItemService.Stop();
            return base.StopAsync(cancellationToken);
        }
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    if (!firstCheck)
                        firstCheck = (await checkItemService.CheckItemAsync(SENSOR_ADDRESS)) == "0";
                    else
                        secondCheck = (await checkItemService.CheckItemAsync(SENSOR_ADDRESS) == "1");

                    if (firstCheck && secondCheck)
                    {
                        if (channelWriter.TryWrite(DateTime.Now))
                        {
                            firstCheck = false;
                            secondCheck = false;
                        }
                    }
                }
                catch (Exception e)
                {
                    logger.LogError(e.Message);
                }
                finally
                {
                    await Task.Delay(TimeSpan.FromMilliseconds(50));
                }
            }
        }
    }
}
