using Avalonia;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace VolexCarousel.Services
{
    public class DesktopAppService : BackgroundService
    {
        private readonly IHostApplicationLifetime hostApplicationLifetime;
        
        public DesktopAppService(IHostApplicationLifetime hostApplicationLifetime)
        {
            this.hostApplicationLifetime = hostApplicationLifetime;
        }
        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            Program.BuildAvaloniaApp().StartWithClassicDesktopLifetime(Environment.GetCommandLineArgs());
            this.hostApplicationLifetime.StopApplication();
            return Task.CompletedTask;
        }
    }
}
