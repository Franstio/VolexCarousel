using System;
using System.Drawing;
using System.Globalization;
using Avalonia.Data.Converters;
using Avalonia.Media;
using VolexCarousel.Models;

namespace VolexCarousel.Converters
{
    internal class PCStoColorIndicatorConverter : IValueConverter
    {
        public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            bool isGreen = false, isYellow = false, isRed = false;
            if (value is int)
            {
                isGreen = value is int pcsValue && pcsValue >= 2000;
                isYellow = value is int pcsValue2 && pcsValue2 < 2000 && pcsValue2 >= 1000;
                isRed = value is int pcsValue3 && pcsValue3 < 1000;
            }
            else if (value is ShiftRecordRowModel)
            {
                return Brushes.White;
                //isGreen = value is ShiftRecordRowModel pcsValue && pcsValue.Output >= pcsValue.TargetOutput;
                //isYellow = value is ShiftRecordRowModel pcsValue2 && pcsValue2.Output == pcsValue2.TargetOutput;
                //isRed = value is ShiftRecordRowModel pcsValue3 && pcsValue3.Output < pcsValue3.TargetOutput;
            }
            else if (value is ShiftDailyOutputModel)
            {

                isGreen = value is ShiftDailyOutputModel pcsValue && pcsValue.TotalOutput >= pcsValue.TargetOutput;
                isYellow = value is ShiftDailyOutputModel pcsValue2 && pcsValue2.TotalOutput == pcsValue2.TargetOutput;
                isRed = value is ShiftDailyOutputModel pcsValue3 && pcsValue3.TotalOutput < pcsValue3.TargetOutput;
            }
            IBrush color = isGreen ? Brushes.LimeGreen : isYellow ? Brushes.Yellow : isRed ? Brushes.IndianRed : Brushes.White;
            return color;
        }

        public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            return new Avalonia.Data.BindingNotification(value);
        }
    }
}
