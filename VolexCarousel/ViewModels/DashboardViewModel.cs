using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using VolexCarousel.Models;

namespace VolexCarousel.ViewModels
{
    partial class DashboardViewModel: ViewModelBase
    {
        public ObservableCollection<ShiftRecordRowModel> PagiShiftRows { get; set; } = new ObservableCollection<ShiftRecordRowModel>();
        public ObservableCollection<ShiftRecordRowModel> SiangShiftRows { get; set; } = new ObservableCollection<ShiftRecordRowModel>();
        public ObservableCollection<ShiftRecordRowModel> MalamShiftRows { get; set; } = new ObservableCollection<ShiftRecordRowModel>();
        public ObservableCollection<ShiftRowModel> ShiftRows { get; set; } = new ObservableCollection<ShiftRowModel>();

        public DashboardViewModel()
        {
            Random rnd = new Random();
            for (int i = 0; i < 24; i = i + 8)
            {
                ShiftRows.Add(new ShiftRowModel { StartTime = TimeOnly.FromDateTime(DateTime.Today.AddHours(i+7)), EndTime = TimeOnly.FromDateTime(DateTime.Today.AddHours(i +7 + 8)), TotalPcs = rnd.Next(100, 5000) });
            }
            for (int i = 0; i < 100; i++)
            {
                PagiShiftRows.Add(new ShiftRecordRowModel { Timestamp = DateTime.Today.AddHours(rnd.Next(0,13)).AddMinutes(rnd.Next(0, 61)), Output = rnd.Next(100, 2000) });
            }
            for (int i = 0; i < 100; i++)
            {
                SiangShiftRows.Add(new ShiftRecordRowModel { Timestamp = DateTime.Today.AddHours(rnd.Next(12,19)).AddMinutes(rnd.Next(0, 61)), Output = rnd.Next(100, 2000) });
            }
            for (int i = 0; i < 100; i++)
            {
                MalamShiftRows.Add(new ShiftRecordRowModel { Timestamp = DateTime.Today.AddHours(rnd.Next(18, 25)).AddMinutes(rnd.Next(0, 61)), Output = rnd.Next(100, 2000) });
            }
            PagiShiftRows = new ObservableCollection<ShiftRecordRowModel>(PagiShiftRows.OrderBy(x=>x.Timestamp));
            SiangShiftRows = new ObservableCollection<ShiftRecordRowModel>(SiangShiftRows.OrderBy(x=>x.Timestamp));
            MalamShiftRows = new ObservableCollection<ShiftRecordRowModel>(MalamShiftRows.OrderBy(x=>x.Timestamp));
        }
    }
}