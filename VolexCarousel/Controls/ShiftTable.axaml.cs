using Avalonia;
using Avalonia.Controls.Primitives;
using System.Collections.ObjectModel;
using VolexCarousel.Controls;
using VolexCarousel.Models;

namespace VolexCarousel;

public class ShiftTable : TemplatedControl
{
    public static readonly StyledProperty<ObservableCollection<ShiftDailyOutputModel>> ShiftDataProperty =
        AvaloniaProperty.Register<ShiftTable, ObservableCollection<ShiftDailyOutputModel>>(
            name: nameof(ShiftData),
            defaultValue: new ObservableCollection<ShiftDailyOutputModel>());

    public ObservableCollection<ShiftDailyOutputModel> ShiftData
    {
        get => GetValue(ShiftDataProperty);
        set => SetValue(ShiftDataProperty, value);
    }
}