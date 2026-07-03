using System;
using System.Data;
using System.IO;
using System.Linq;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Data.Core;
using Avalonia.Data.Core.Plugins;
using Avalonia.Markup.Xaml;
using Dapper;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using Serilog.Events;
using VolexCarousel.Interfaces;
using VolexCarousel.Mappers;
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

        public override async void OnFrameworkInitializationCompleted()
        {
            ServiceCollection services = new ServiceCollection();

            services.AddSingleton<AppSettingService>();
            services.AddTransient<IDbConnection>((sp) =>
            {

                var setting = sp.GetRequiredService<AppSettingService>();
                return new SqliteConnection(setting.LoadSettings().CarouselDb);
            });
            SqlMapper.AddTypeHandler(new SQLTImespanHandler());
            services.AddTransient<CarouselRepositoryService>();
            services.AddTransient<MainWindowViewModel>();
            services.AddTransient<DashboardViewModel>();
            services.AddTransient<LoginViewModel>();
            services.AddTransient<ShiftSettingViewModel>();
            services.AddSingleton<UserStore>();
            services.AddSingleton<InformationSpeedService>();
            services.AddSingleton<TCPPLCService>();
            services.AddSingleton<ICheckItemService>(sp => sp.GetRequiredService<TCPPLCService>());
            services.AddSingleton<ItemCheckService>();

            services.AddLogging(l =>
            {
                l.AddSerilog(new LoggerConfiguration()
                    .WriteTo.SQLite(Path.Combine(AppContext.BaseDirectory, "logs.db"),"tbl_log",restrictedToMinimumLevel:LogEventLevel.Debug,rollOver:true)
                    .CreateLogger());
            });
            var service = services.BuildServiceProvider();
            var carouselRepo = service.GetRequiredService<CarouselRepositoryService>();
            await carouselRepo.Initialization();
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