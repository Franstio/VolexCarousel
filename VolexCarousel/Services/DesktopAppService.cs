using Avalonia;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using VolexCarousel.Core.Services;

namespace VolexCarousel.Services
{
    public class DesktopAppService : BackgroundService
    {
        private readonly IHostApplicationLifetime hostApplicationLifetime;
        private readonly TCPPLCService plcService;
        public DesktopAppService(IHostApplicationLifetime hostApplicationLifetime,TCPPLCService plcService)
        {
            this.hostApplicationLifetime = hostApplicationLifetime;
            this.plcService = plcService;
        }
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            await plcService.WriteCommand("MR701","1");
            Program.BuildAvaloniaApp().StartWithClassicDesktopLifetime(Environment.GetCommandLineArgs());
            this.hostApplicationLifetime.StopApplication();
        }
    }
}
