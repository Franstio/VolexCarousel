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

        [ObservableProperty] ObservableCollection<ShiftRecordRowModel> pagiShiftRows = new ObservableCollection<ShiftRecordRowModel>();

        [ObservableProperty] ObservableCollection<ShiftRecordRowModel> siangShiftRows = new ObservableCollection<ShiftRecordRowModel>();

        [ObservableProperty] ObservableCollection<ShiftRecordRowModel> malamShiftRows = new ObservableCollection<ShiftRecordRowModel>();

        [ObservableProperty] ObservableCollection<ShiftDailyOutputModel> shiftRows = new ObservableCollection<ShiftDailyOutputModel>();
        

        [ObservableProperty]
        string title = "CAROUSEL MACHINE INFORMATION";
        [ObservableProperty]
        string boxByBox = TimeSpan.Zero.TotalSeconds.ToString();

        [ObservableProperty]
        string informationSpeedData = "0";

        [ObservableProperty]
        string totalOutput = "0";

        private AppSettingService AppSettingService;
        private List<ShiftTransactionRecord> ShiftTransactionRecords = [];
        private DateTime startTime = DateTime.Now;
        private readonly InformationSpeedService _informationSpeedService;
        private readonly ItemCheckService _itemCheckService;
        private readonly CarouselRepositoryService _carouselRepositoryService;

        private static CancellationTokenSource CancellationTokenSource = new CancellationTokenSource();
        public DashboardViewModel(InformationSpeedService informationSpeedService, AppSettingService appSettingService, ItemCheckService itemCheckService, CarouselRepositoryService carouselRepositoryService)
        {
            _informationSpeedService = informationSpeedService;
            _itemCheckService = itemCheckService;
            _carouselRepositoryService = carouselRepositoryService;

            AppSettingService = appSettingService;
            if (!string.IsNullOrEmpty(appSettingService.LoadSettings().Title))
            {
                Title = appSettingService.LoadSettings().Title;
            }

        }
        public async void Initialization()
        {
            await LoadInitData();
            StartItemCheckInputService();
            StartItemCheckOutputService();
            StartInformationSpeedService();
        }

        private async Task LoadInitData()
        {
            await Dispatcher.UIThread.InvokeAsync(async () =>
            {


                ShiftRows = new ObservableCollection<ShiftDailyOutputModel>(await _carouselRepositoryService.GetDailyOutput());
                ShiftTransactionRecords.AddRange(await _carouselRepositoryService.GetTodayShiftRecord());
                PagiShiftRows = new ObservableCollection<ShiftRecordRowModel>(
                 _carouselRepositoryService.GetTodayShiftDisplay(await _carouselRepositoryService.GetTodayShiftRecord("Day")));
                SiangShiftRows = new ObservableCollection<ShiftRecordRowModel>(
                 _carouselRepositoryService.GetTodayShiftDisplay(await _carouselRepositoryService.GetTodayShiftRecord("Noon")));
                MalamShiftRows = new ObservableCollection<ShiftRecordRowModel>(
                 _carouselRepositoryService.GetTodayShiftDisplay(await _carouselRepositoryService.GetTodayShiftRecord("Night")));
            });
        }
        private async Task SetDataShifts(ShiftTransactionRecord record)
        {
            ShiftTransactionRecords.Add(record);
            var records = ShiftTransactionRecords.Where(x => x.shiftname == record.shiftname).OrderBy(x => x.datetimeinput);
            if (record.shiftname == "Day")
            {
                PagiShiftRows = new ObservableCollection<ShiftRecordRowModel>(_carouselRepositoryService.GetTodayShiftDisplay(records));
                
            }
            else if (record.shiftname == "Noon")
            {
                SiangShiftRows = new ObservableCollection<ShiftRecordRowModel>(_carouselRepositoryService.GetTodayShiftDisplay(records));
            }
            else if (record.shiftname == "Night")
            {
                MalamShiftRows = new ObservableCollection<ShiftRecordRowModel>(_carouselRepositoryService.GetTodayShiftDisplay(records));
            }
            Dispatcher.UIThread.Invoke(() =>
            {
                ShiftRows = new ObservableCollection<ShiftDailyOutputModel>(
                    records.GroupBy(x=>x.shiftname).SelectMany(x => x.Select(y=>new ShiftDailyOutputModel()
                    {
                         ShiftName = x.Key,
                         TargetOutput = y.targetoutput,
                         TotalOutput = x.Count(z=>z.datetimeoutput != default)
                    })));
                TotalOutput = ShiftTransactionRecords.Count.ToString();
            });


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
        public void StopServices()
        {
            CancellationTokenSource.Cancel();
            CancellationTokenSource = new CancellationTokenSource();
            _itemCheckService.Stop();
        }
        
        public void StartItemCheckInputService()
        {
            var cancellationToken = CancellationTokenSource.Token;
            _ = Task.Run(async () =>
            {
                await foreach (var record in _itemCheckService.RunCheckInput(cancellationToken))
                {

                    TimeSpan dt = (record.datetimeinput - startTime);
                    await Dispatcher.UIThread.InvokeAsync(async () =>
                    {
                        BoxByBox = dt.TotalSeconds.ToString();
                        startTime= record.datetimeinput;
                    });
                }
            }, cancellationToken);
        }
        public void StartItemCheckOutputService()
        {
            var cancellationToken = CancellationTokenSource.Token;
            _ = Task.Run(async () =>
            {
                await foreach (var record in _itemCheckService.RunCheckOutput(cancellationToken))
                {
                    await Dispatcher.UIThread.InvokeAsync(async () =>
                    {
                        await SetDataShifts(record);
                        
                    });
                }
            }, cancellationToken);
        }

    }
}