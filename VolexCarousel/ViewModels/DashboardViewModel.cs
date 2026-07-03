using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using VolexCarousel.Models;
using VolexCarousel.Services;

namespace VolexCarousel.ViewModels
{
    public partial class DashboardViewModel : ViewModelBase
    {
        public ObservableCollection<ShiftRecordRowModel> PagiShiftRows { get; set; } = new ObservableCollection<ShiftRecordRowModel>();
        public ObservableCollection<ShiftRecordRowModel> SiangShiftRows { get; set; } = new ObservableCollection<ShiftRecordRowModel>();
        public ObservableCollection<ShiftRecordRowModel> MalamShiftRows { get; set; } = new ObservableCollection<ShiftRecordRowModel>();
        public ObservableCollection<ShiftDailyOutputModel> ShiftRows { get; set; } = new ObservableCollection<ShiftDailyOutputModel>();

        [ObservableProperty]
        string informationSpeedData = string.Empty;

        [ObservableProperty]
        string title = "CAROUSEL MACHINE INFORMATION";
        private AppSettingService AppSettingService;

        private readonly InformationSpeedService _informationSpeedService;
        private readonly ItemCheckService _itemCheckService;

        private static CancellationTokenSource CancellationTokenSource = new CancellationTokenSource();
        public DashboardViewModel(InformationSpeedService informationSpeedService, AppSettingService appSettingService, ItemCheckService itemCheckService)
        {
            _informationSpeedService = informationSpeedService;
            _itemCheckService = itemCheckService;
            Random rnd = new Random();
            for (int i = 0; i < 100; i++)
            {
                PagiShiftRows.Add(new ShiftRecordRowModel { Timestamp = DateTime.Today.AddHours(rnd.Next(0, 13)).AddMinutes(rnd.Next(0, 61)), Output = rnd.Next(100, 2000), TargetOutput = 1000 });
            }
            for (int i = 0; i < 100; i++)
            {
                SiangShiftRows.Add(new ShiftRecordRowModel { Timestamp = DateTime.Today.AddHours(rnd.Next(12, 19)).AddMinutes(rnd.Next(0, 61)), Output = rnd.Next(100, 2000), TargetOutput = 1000 });
            }
            for (int i = 0; i < 100; i++)
            {
                MalamShiftRows.Add(new ShiftRecordRowModel { Timestamp = DateTime.Today.AddHours(rnd.Next(18, 25)).AddMinutes(rnd.Next(0, 61)), Output = rnd.Next(100, 2000), TargetOutput = 1000 });
            }

            ShiftRows.Add(new ShiftDailyOutputModel { ShiftName = "Day", TargetOutput = 1000, TotalOutput = PagiShiftRows.Sum(x => x.Output) });
            ShiftRows.Add(new ShiftDailyOutputModel { ShiftName = "Noon", TargetOutput = 1000, TotalOutput = SiangShiftRows.Sum(x => x.Output) });
            ShiftRows.Add(new ShiftDailyOutputModel { ShiftName = "Night", TargetOutput = 1000, TotalOutput = MalamShiftRows.Sum(x => x.Output) });
            PagiShiftRows = new ObservableCollection<ShiftRecordRowModel>(PagiShiftRows.OrderBy(x => x.Timestamp));
            SiangShiftRows = new ObservableCollection<ShiftRecordRowModel>(SiangShiftRows.OrderBy(x => x.Timestamp));
            MalamShiftRows = new ObservableCollection<ShiftRecordRowModel>(MalamShiftRows.OrderBy(x => x.Timestamp));
            AppSettingService = appSettingService;
            if (!string.IsNullOrEmpty(appSettingService.LoadSettings().Title))
            {
                Title = appSettingService.LoadSettings().Title;
            }

        }

        public void StartInformationSpeedService()
        {
            var cancellationToken = CancellationTokenSource.Token;
            _ = Task.Run(async () =>
            {
                await foreach (var data in _informationSpeedService.ReadDataStreamAsync(cancellationToken))
                {
                    Dispatcher.UIThread.Invoke(() =>
                    {
                        InformationSpeedData = data;
                    });
                }
            }, cancellationToken);
        }
        public void StopInformationSpeedService()
        {
            CancellationTokenSource.Cancel();
            CancellationTokenSource = new CancellationTokenSource();
        }   
    }
}