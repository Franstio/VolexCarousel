using Avalonia;
using Avalonia.Controls.Primitives;
using System.Collections.ObjectModel;
using VolexCarousel.Models;

namespace VolexCarousel.Controls;

public class ShiftItemTable : TemplatedControl
{

    public static readonly StyledProperty<string> TitleProperty =
        AvaloniaProperty.Register<ShiftItemTable, string>(
            name: nameof(Title),
            defaultValue: "");
    public static readonly StyledProperty<ObservableCollection<ShiftRecordRowModel>> ShiftItemsProperty =
        AvaloniaProperty.Register<ShiftItemTable, ObservableCollection<ShiftRecordRowModel>>(
            name: nameof(ShiftItems),
            defaultValue: new ObservableCollection<ShiftRecordRowModel>());

    public ObservableCollection<ShiftRecordRowModel> ShiftItems
    {
        get => GetValue(ShiftItemsProperty);
        set => SetValue(ShiftItemsProperty, value);
    }
    public string Title
    {
        get => GetValue(TitleProperty);
        set => SetValue(TitleProperty, value);
    }
}