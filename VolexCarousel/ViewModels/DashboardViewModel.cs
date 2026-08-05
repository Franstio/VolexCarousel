using Avalonia.Logging;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using VolexCarousel.Core.Models;
using VolexCarousel.Core.Services;
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

        [ObservableProperty]
        string time = DateTime.Now.ToString("dd MMMM yyyy HH:mm:dd");

        private Func<AppSettingsModel> AppSettingService;
        private List<ShiftTransactionRecord> ShiftTransactionRecords = [];
        private DateTime startTime = DateTime.Now,endTime = DateTime.Now;
        private readonly InformationSpeedService _informationSpeedService;
        private readonly CarouselRepositoryService _carouselRepositoryService;
        private DispatcherTimer timerDate;
        private readonly ChannelReader<DateTime> boxTimeReader;
        private readonly ChannelReader<ShiftTransactionRecord> transactionChannelReader;
        private static CancellationTokenSource CancellationTokenSource = new CancellationTokenSource();
        ILogger<DashboardViewModel> logger;
        public DashboardViewModel(InformationSpeedService informationSpeedService, AppSettingService appSettingService, CarouselRepositoryService carouselRepositoryService,
            [FromKeyedServices("boxTimeChannel")] Channel<DateTime> boxChannel,
            [FromKeyedServices("itemChannel")] Channel<ShiftTransactionRecord> itemChannel,
            ILogger<DashboardViewModel> logger)
        {
            _informationSpeedService = informationSpeedService;
            _carouselRepositoryService = carouselRepositoryService;
            transactionChannelReader = itemChannel.Reader;
            boxTimeReader = boxChannel.Reader;
            this.logger = logger;
            AppSettingService = appSettingService.LoadSettings;
            
            timerDate = new DispatcherTimer()
            {
                Interval = TimeSpan.FromMilliseconds(30)
            };
            timerDate.Tick += (o, e) => {
                Time = DateTime.Now.ToString("dd MMMM yyyy HH:mm:ss");

                TimeSpan dt = endTime > startTime ? (endTime - startTime) : startTime - endTime;
                BoxByBox = dt.TotalSeconds.ToString("0.0");
                if (!string.IsNullOrEmpty(appSettingService.LoadSettings().Title))
                {
                    Title = appSettingService.LoadSettings().Title;
                }
            };

        }
        public void Initialization()
        {
            timerDate.Start();
            _ = Task.WhenAll([LoadInitData(),StartReadingTimeInput(), StartReadingTransaction()
//            ,StartInformationSpeedService()
            ]);
        }

        private Task LoadInitData()
        {
            return Dispatcher.UIThread.InvokeAsync(async () =>
            {


                ShiftRows = new ObservableCollection<ShiftDailyOutputModel>(await _carouselRepositoryService.GetDailyOutput());
                ShiftTransactionRecords.AddRange(await _carouselRepositoryService.GetTodayShiftRecord());
                PagiShiftRows = new ObservableCollection<ShiftRecordRowModel>(
                 await _carouselRepositoryService.GetTodayShiftDisplay("Shift 1",await _carouselRepositoryService.GetTodayShiftRecord("Shift 1")));
                SiangShiftRows = new ObservableCollection<ShiftRecordRowModel>(
                 await _carouselRepositoryService.GetTodayShiftDisplay("Shift 2", await _carouselRepositoryService.GetTodayShiftRecord("Shift 2")));
                MalamShiftRows = new ObservableCollection<ShiftRecordRowModel>(
                 await _carouselRepositoryService.GetTodayShiftDisplay("Shift 3", await _carouselRepositoryService.GetTodayShiftRecord("Shift 3")));

                TotalOutput = (await _carouselRepositoryService.GetTodayShiftRecord()).Count().ToString();
            });
        }
        public async Task SetDataShifts()
        {
            var rows = await _carouselRepositoryService.GetShift();
            ShiftTransactionRecords.Clear();
            ShiftTransactionRecords.AddRange(await _carouselRepositoryService.GetTodayShiftRecord());
            //            ShiftTransactionRecords.Add(record);

            var records = ShiftTransactionRecords.OrderBy(x => x.datetimeinput);
            var joinData = rows.GroupJoin(records, x => x.shiftname, z => z.shiftname, (x, y) => new { x, y }
                    ).SelectMany((x) => x.y.DefaultIfEmpty(), (x, y) =>
                    new ShiftTransactionRecord()
                    {
                        uid = y?.uid ?? Guid.Empty,
                        datetimeinput = y?.datetimeinput ?? default,
                        datetimeoutput = y?.datetimeoutput ?? default,
                        targetoutput = x.x.targetoutput,
                        targetdailyoutput = x.x.targetdailyoutput,
                        shiftname = x.x.shiftname
                    });

            PagiShiftRows = new ObservableCollection<ShiftRecordRowModel>(await _carouselRepositoryService.GetTodayShiftDisplay("Shift 1", records));
            SiangShiftRows = new ObservableCollection<ShiftRecordRowModel>(await _carouselRepositoryService.GetTodayShiftDisplay("Shift 2", records));
            MalamShiftRows = new ObservableCollection<ShiftRecordRowModel>(await _carouselRepositoryService.GetTodayShiftDisplay("Shift 3", records));

            Dispatcher.UIThread.Invoke(() =>
            {

                ShiftRows = new ObservableCollection<ShiftDailyOutputModel>(
                    joinData.GroupBy(x => x.shiftname).SelectMany(x => x.Select(y => new ShiftDailyOutputModel()
                    {
                        ShiftName = x.Key,
                        TargetOutput = y.targetdailyoutput,
                        TotalOutput = x.Count(z => z.datetimeoutput != default)
                    })).DistinctBy(x => x.ShiftName));
                TotalOutput = ShiftTransactionRecords.Count.ToString();
            });


        }
        public async Task StartInformationSpeedService()
        {
            var cancellationToken = CancellationTokenSource.Token;

            await foreach (var data in _informationSpeedService.ReadDataStreamAsync(cancellationToken))
            {
                Dispatcher.UIThread.Invoke(() =>
                {
                    InformationSpeedData = data;
                });
            }
        }
        public void StopServices()
        {
            timerDate.Stop();
            CancellationTokenSource.Cancel();
            CancellationTokenSource = new CancellationTokenSource();
        }

        public async Task StartReadingTimeInput()
        {
            var cancellationToken = CancellationTokenSource.Token;
            await foreach (var time in boxTimeReader.ReadAllAsync(cancellationToken))
            {
                    startTime = endTime;
                    endTime = time;
            }
        }
        public async Task StartReadingTransaction()
        {
            var cancellationToken = CancellationTokenSource.Token;

            await foreach (var record in transactionChannelReader.ReadAllAsync(cancellationToken))
            {
                await Dispatcher.UIThread.InvokeAsync(async () =>
                {
                    var lastRecord = ShiftTransactionRecords.OrderBy(x=>x.datetimeoutput).LastOrDefault();
                    if (lastRecord is not null)
                    {
                        TimeSpan diff = record.datetimeoutput - lastRecord.datetimeoutput;
                        InformationSpeedData = (AppSettingService().ModuleDistanceLength / diff.TotalSeconds).ToString("0.00");
                    }

                });
                await SetDataShifts();
            }
        }

    }
}