using Avalonia;
using Avalonia.Controls.Primitives;
using System.Collections.ObjectModel;
using VolexCarousel.Controls;
using VolexCarousel.Models;

namespace VolexCarousel;

public class ShiftTable : TemplatedControl
{
    public static readonly StyledProperty<ObservableCollection<ShiftRowModel>> ShiftDataProperty =
        AvaloniaProperty.Register<ShiftTable, ObservableCollection<ShiftRowModel>>(
            name: nameof(ShiftData),
            defaultValue: new ObservableCollection<ShiftRowModel>());

    public ObservableCollection<ShiftRowModel> ShiftData
    {
        get => GetValue(ShiftDataProperty);
        set => SetValue(ShiftDataProperty, value);
    }
}