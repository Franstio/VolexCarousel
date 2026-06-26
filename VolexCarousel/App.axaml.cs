using System;
using System.IO;
using System.Linq;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Data.Core;
using Avalonia.Data.Core.Plugins;
using Avalonia.Markup.Xaml;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using Serilog.Events;
using VolexCarousel.Services;
using VolexCarousel.Store;
using VolexCarousel.ViewModels;
using VolexCarousel.Views;

namespace VolexCarousel
{
    public partial class App : Application
    {
        
        public override void Initialize()
        {
            AvaloniaXamlLoader.Load(this);
        }

        public override void OnFrameworkInitializationCompleted()
        {
            ServiceCollection services = new ServiceCollection();
            services.AddTransient<MainWindowViewModel>();
            services.AddTransient<DashboardViewModel>();
            services.AddTransient<LoginViewModel>();
            services.AddSingleton<UserStore>();
            services.AddSingleton<TcpService>();
            services.AddSingleton<InformationSpeedService>();
            services.AddSingleton<AppSettingService>();

            services.AddLogging(l =>
            {
                l.AddSerilog(new LoggerConfiguration()
                    .WriteTo.SQLite(Path.Combine(AppContext.BaseDirectory, "logs.db"),"tbl_log",restrictedToMinimumLevel:LogEventLevel.Debug,rollOver:true)
                    .CreateLogger());
            });
            var service = services.BuildServiceProvider();
            var vm = service.GetService<MainWindowViewModel>();
            if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                desktop.MainWindow = new MainWindow
                {
                    DataContext = vm//new MainWindowViewModel(),
                };
            }

            base.OnFrameworkInitializationCompleted();
        }
    }
}