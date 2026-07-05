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
        private TimeSpan startTime = TimeSpan.Zero;
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

                TotalOutput = (await _carouselRepositoryService.GetTodayShiftRecord()).Count().ToString();
            });
        }
        private async Task SetDataShifts(ShiftTransactionRecord record)
        {
            var rows = await _carouselRepositoryService.GetShift();
            ShiftTransactionRecords.Add(record);

            var records = ShiftTransactionRecords.Where(x => x.shiftname == record.shiftname).OrderBy(x => x.datetimeinput);
            var joinData = rows.GroupJoin(records, x => x.shiftname, z => z.shiftname, (x, y) => new { x, y }
                    ).SelectMany((x) => x.y.DefaultIfEmpty(), (x, y) =>
                    new ShiftTransactionRecord()
                    {
                        uid = y?.uid ?? Guid.Empty,
                        datetimeinput = y?.datetimeinput ?? default,
                        datetimeoutput = y?.datetimeoutput ?? default,
                        targetoutput = x.x.targetoutput,
                        shiftname = x.x.shiftname
                    });

            if (record.shiftname == "Day")
            {
                PagiShiftRows = new ObservableCollection<ShiftRecordRowModel>(_carouselRepositoryService.GetTodayShiftDisplay(joinData));
                
            }
            else if (record.shiftname == "Noon")
            {
                SiangShiftRows = new ObservableCollection<ShiftRecordRowModel>(_carouselRepositoryService.GetTodayShiftDisplay(joinData));
            }
            else if (record.shiftname == "Night")
            {
                MalamShiftRows = new ObservableCollection<ShiftRecordRowModel>(_carouselRepositoryService.GetTodayShiftDisplay(joinData));
            }
            Dispatcher.UIThread.Invoke(() =>
            {
               
                ShiftRows = new ObservableCollection<ShiftDailyOutputModel>(
                    joinData.GroupBy(x=>x.shiftname).SelectMany(x => x.Select(y=>new ShiftDailyOutputModel()
                    {
                         ShiftName = x.Key,
                         TargetOutput = y.targetoutput,
                         TotalOutput = x.Count(z=>z.datetimeoutput != default)
                    })).DistinctBy(x=>x.ShiftName) );
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
                    TimeSpan dt = (DateTime.Now.TimeOfDay - startTime);
                    startTime = DateTime.Now.TimeOfDay;
                    Dispatcher.UIThread.Invoke(() =>
                    {
                        BoxByBox = dt.TotalSeconds.ToString("0.0");
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
                        await _carouselRepositoryService.RecordItemInput(record);
                    });
                }
            }, cancellationToken);
        }

    }
}