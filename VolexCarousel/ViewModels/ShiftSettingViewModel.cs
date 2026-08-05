using Avalonia.Media;
using Avalonia.Threading;
using AvaloniaDialogs.Views;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using VolexCarousel.Core.Services;
using VolexCarousel.Models;
using VolexCarousel.Services;

namespace VolexCarousel.ViewModels
{
    public partial class ShiftSettingViewModel: ViewModelBase
    {
        private readonly AppSettingService AppSettingService = null!;
        private readonly CarouselRepositoryService CarouselRepositoryService = null!;
        [ObservableProperty]
        private string title = string.Empty;

        [ObservableProperty]
        public TimeSpan? dayShiftTimeStart = TimeSpan.FromHours(1);

        [ObservableProperty]
        public TimeSpan? noonShiftTimeStart = TimeSpan.FromHours(1);
        [ObservableProperty]
        public TimeSpan? nightShiftTimeStart = TimeSpan.FromHours(1);

        [ObservableProperty]
        public TimeSpan? dayShiftTimeEnd = TimeSpan.FromHours(1);

        [ObservableProperty]
        public TimeSpan? noonShiftTimeEnd = TimeSpan.FromHours(1);
        [ObservableProperty]
        public TimeSpan? nightShiftTimeEnd = TimeSpan.FromHours(1);

        [ObservableProperty]
        public ObservableCollection<LogModel> logs = new ObservableCollection<LogModel>();

        [ObservableProperty]
        public int targetOutputShift = 0;
        [ObservableProperty]
        public int targetOutputDaily = 0;

        [ObservableProperty]
        public string logState = "Resume";

        [ObservableProperty]
        public string? selectedShift = "Shift 1";

        [ObservableProperty]
        public ObservableCollection<string> resetShifts = new ObservableCollection<string>([
            "Shift 1",
            "Shift 2",
            "Shift 3"
            ]);

        private readonly TCPPLCService tcpPLCService;

        public ShiftSettingViewModel(AppSettingService appSettingService, CarouselRepositoryService carouselRepositoryService, TCPPLCService tcpPLCService)
        {
            AppSettingService = appSettingService;
            CarouselRepositoryService = carouselRepositoryService;
            appSettingService.LoadSettings();
            this.tcpPLCService = tcpPLCService;
        }
        public void Init()
        {
            this.tcpPLCService.OnResponse += acceptLog;
        }
        public void Close()
        {
            this.tcpPLCService.OnResponse -= acceptLog;
        }
        private void acceptLog(string log)
        {
            Dispatcher.UIThread.Invoke(() =>
            {
                Logs.Insert(0,new LogModel()
                {
                    log = log,
                    time = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")
                });
            });
        }
        [RelayCommand]
        public void ClearLog() => Logs.Clear();

        [RelayCommand]
        public void ToggleLogState()
        {
            if (LogState == "Pause")
            {
                Close();
                LogState = "Resume";
            }
            else if (LogState == "Resume")
            {
                Init();
                LogState = "Pause";
            }
        }
        [RelayCommand]
        public async Task ResetShift()
        {
            if (SelectedShift is not null)
            {
                await CarouselRepositoryService.DeleteShiftRecordToday(SelectedShift);
                await Dispatcher.UIThread.InvokeAsync(async () =>
                {
                    SingleActionDialog dialog = new()
                    {
                        Message = $"Sucessfully Resetting Today {SelectedShift} Shift",
                        Background = Brush.Parse("#17367F"),
                        ButtonText = "Ok"
                    };
                    await dialog.ShowAsync();
                });
            }
        }
        public async Task LoadSettings()
        {
            var setting = AppSettingService.LoadSettings();
            Title = setting.Title;
            var shifts = await CarouselRepositoryService.GetShift();
            if (shifts.Any())
            {
                TargetOutputShift = shifts.First().targetoutput;
                TargetOutputDaily = shifts.First().targetdailyoutput;
            }
            foreach (var shift in shifts)
            {
                switch (shift.shiftname)
                {
                    case "Shift 1":
                        DayShiftTimeStart = shift.shiftstart;
                        DayShiftTimeEnd = shift.shiftend;
                        break;
                    case "Shift 2":
                        NoonShiftTimeStart = shift.shiftstart;
                        NoonShiftTimeEnd = shift.shiftend;
                        break;
                    case "Shift 3":
                        NightShiftTimeStart = shift.shiftstart;
                        NightShiftTimeEnd = shift.shiftend;
                        break;
                }
            }
        }

        public async Task SetTitle()
        {
            var setting = AppSettingService.LoadSettings();
            setting.Title = Title;
            AppSettingService.Save(setting);
        }

        public async Task SetOutput()
        {
            await CarouselRepositoryService.UpdateTargetOutput(TargetOutputShift);
            await CarouselRepositoryService.UpdateTargetDailyOutput(TargetOutputDaily);
        }
        [RelayCommand]
        public async Task SetAppinfo()
        {
            await SetTitle();
            await SetOutput();
            await Dispatcher.UIThread.InvokeAsync(async () =>
            {
                SingleActionDialog dialog = new()
                {
                    Message = "Settings saved",
                    Background = Brush.Parse("#17367F"),
                    ButtonText = "Ok"
                };
                await dialog.ShowAsync();
            });
        }

        public async Task SetDayShift(string shiftName,TimeSpan shiftStart,TimeSpan shiftEnd)
        {
            var shiftData =  await CarouselRepositoryService.GetShift(shiftName);
            var shift = shiftData.FirstOrDefault() ?? new Models.ShiftMasterRecord() { shiftname=shiftName};
            shift.shiftstart = shiftStart;
            shift.shiftend = shiftEnd;
            if (shiftData.Any())
                await CarouselRepositoryService.UpdateShiftMaster(shiftName, shift);
            else
                await CarouselRepositoryService.AddShift(shift);
        }


        [RelayCommand]
        public async Task SetAllShift()
        {
            string[] shifts = ["Shift 1", "Shift 2", "Shift 3"];   
            for (int i = 0; i < shifts.Length; i++)
            {
                var shiftName = shifts[i];
                TimeSpan shiftStart;
                TimeSpan shiftEnd;
                switch (shiftName)
                {
                    case "Shift 1":
                        shiftStart = DayShiftTimeStart!.Value;
                        shiftEnd = DayShiftTimeEnd!.Value;
                        break;
                    case "Shift 2":
                        shiftStart = NoonShiftTimeStart!.Value;
                        shiftEnd = NoonShiftTimeEnd!.Value;
                        break;
                    case "Shift 3":
                        shiftStart = NightShiftTimeStart!.Value;
                        shiftEnd = NightShiftTimeEnd!.Value;
                        break;
                    default:
                        throw new ArgumentException($"Invalid shift name: {shiftName}");
                }
                await SetDayShift(shiftName, shiftStart, shiftEnd);
            }
            await Dispatcher.UIThread.InvokeAsync(async () =>
            {
                SingleActionDialog dialog = new()
                {
                    Message = "Settings saved",
                    Background = Brush.Parse("#17367F"),
                    ButtonText = "Ok"
                };
                await dialog.ShowAsync();
            });
        }

    }
}