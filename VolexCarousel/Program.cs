using Avalonia;
using Dapper;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using ModbusProxy;
using Serilog;
using Serilog.Events;
using System;
using System.Data;
using System.IO;
using System.Threading.Channels;
using VolexCarousel.Core.Interfaces;
using VolexCarousel.Core.Services;
using VolexCarousel.Core.Models;
using VolexCarousel.Mappers;
using VolexCarousel.Models;
using VolexCarousel.Services;
using VolexCarousel.Store;
using VolexCarousel.ViewModels;

namespace VolexCarousel
{
    internal sealed class Program
    {
        public static IHost HostApp { get; private set; } = null!;
        // Initialization code. Don't use any Avalonia, third-party APIs or any
        // SynchronizationContext-reliant code before AppMain is called: things aren't initialized
        // yet and stuff might break.
        [STAThread]
        public static void Main(string[] args)
        {
            
            var builder = Host.CreateApplicationBuilder();
            IServiceCollection services = builder.Services;
            services.AddSingleton<AppSettingService>();
            services.AddTransient<IDbConnection>((sp) =>
            {

                var setting = (sp.GetRequiredService<AppSettingService>()).LoadSettings();
                return new SqliteConnection(setting.CarouselDb);
            });
            var channelOption = new BoundedChannelOptions(1)
            {
                FullMode = BoundedChannelFullMode.DropOldest,
                SingleReader = true,
                SingleWriter = true,
            };
            Channel<DateTime> sensorChannel = Channel.CreateBounded<DateTime>(channelOption);

            Channel<DateTime> boxChannel = Channel.CreateBounded<DateTime>(channelOption);

            Channel<ShiftTransactionRecord> transactionChannel = Channel.CreateBounded<ShiftTransactionRecord>(channelOption);


            SqlMapper.AddTypeHandler(new SQLTImespanHandler());
            services.AddKeyedSingleton("sensorChannel", sensorChannel);
            services.AddKeyedSingleton("boxTimeChannel", boxChannel);
            services.AddKeyedSingleton("itemChannel", transactionChannel);
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
            services.AddSingleton<SensorBackgroundService>();
            services.AddHostedService(sp => sp.GetRequiredService<SensorBackgroundService>());
            services.AddHostedService(sp => sp.GetRequiredService<ItemCheckService>());
            services.AddLogging(l =>
            {
                l.AddSerilog(new LoggerConfiguration().Enrich.FromLogContext()
                    .WriteTo.SQLite(Path.Combine(AppContext.BaseDirectory, "logs.db"), "tbl_log", restrictedToMinimumLevel: LogEventLevel.Debug, rollOver: true)
                    .CreateLogger());
            });
            //using ModbusProxy, remove this code if volexcarousel need to be standalone without modbusproxy
            builder.Services.AddSingleton<ModbusProxyService>();
            builder.Services.AddHostedService(sp => sp.GetRequiredService<ModbusProxyService>());
            builder.Services.AddHostedService<ModbusWorkService>();

            //


            services.AddHostedService<DesktopAppService>();
            HostApp = builder.Build();
            var carouselRepo = HostApp.Services.GetRequiredService<CarouselRepositoryService>();
            _ = carouselRepo.Initialization();
            HostApp.Run();
        }

        // Avalonia configuration, don't remove; also used by visual designer.
        public static AppBuilder BuildAvaloniaApp()
            => AppBuilder.Configure<App>()
                .UsePlatformDetect()
                .WithInterFont()
                .LogToTrace();
    }
}
