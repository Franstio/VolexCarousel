using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using VolexCarousel.Services;

namespace VolexCarousel.ViewModels
{
    public partial class ShiftSetttingViewModel: ViewModelBase
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
        public int targetOutputShift = 0;

        public ShiftSetttingViewModel(AppSettingService appSettingService, CarouselRepositoryService carouselRepositoryService)
        {
            AppSettingService = appSettingService;
            CarouselRepositoryService = carouselRepositoryService;
            appSettingService.LoadSettings();
        }
        public async Task LoadSettings()
        {
            var setting = AppSettingService.LoadSettings();
            Title = setting.Title;
            var shifts = await CarouselRepositoryService.GetShift();
            if (shifts.Any())
                TargetOutputShift = shifts.First().targetoutput;
            foreach (var shift in shifts)
            {
                switch (shift.shiftname)
                {
                    case "Day":
                        DayShiftTimeStart = shift.shiftstart;
                        DayShiftTimeEnd = shift.shiftend;
                        break;
                    case "Noon":
                        NoonShiftTimeStart = shift.shiftstart;
                        NoonShiftTimeEnd = shift.shiftend;
                        break;
                    case "Night":
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
        }
        [RelayCommand]
        public async Task SetAppinfo()
        {
            await SetTitle();
            await SetOutput();
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
            string[] shifts = ["Day", "Noon", "Night"];   
            for (int i = 0; i < shifts.Length; i++)
            {
                var shiftName = shifts[i];
                TimeSpan shiftStart;
                TimeSpan shiftEnd;
                switch (shiftName)
                {
                    case "Day":
                        shiftStart = DayShiftTimeStart!.Value;
                        shiftEnd = DayShiftTimeEnd!.Value;
                        break;
                    case "Noon":
                        shiftStart = NoonShiftTimeStart!.Value;
                        shiftEnd = NoonShiftTimeEnd!.Value;
                        break;
                    case "Night":
                        shiftStart = NightShiftTimeStart!.Value;
                        shiftEnd = NightShiftTimeEnd!.Value;
                        break;
                    default:
                        throw new ArgumentException($"Invalid shift name: {shiftName}");
                }
                await SetDayShift(shiftName, shiftStart, shiftEnd);
            }
        }

    }
}