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
        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            Program.BuildAvaloniaApp().StartWithClassicDesktopLifetime(Environment.GetCommandLineArgs());
            return Task.CompletedTask;
        }
    }
}
