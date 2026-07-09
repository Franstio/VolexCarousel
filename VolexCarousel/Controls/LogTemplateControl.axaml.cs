using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using System.Collections.ObjectModel;
using VolexCarousel.Models;
using System.Reactive;
using System.Collections.Specialized;
using System.Linq;

namespace VolexCarousel.Controls;

public class LogTemplateControl : TemplatedControl
{
    public static readonly StyledProperty<ObservableCollection<LogModel>> LogsProperty = AvaloniaProperty.Register<LogTemplateControl,ObservableCollection<LogModel>>(
    name: nameof(Logs),
    defaultValue: []
    );
    public ObservableCollection<LogModel> Logs
    {
        get => GetValue(LogsProperty);
        set
        {
            SetValue(LogsProperty, value);
        }
    }

    public LogTemplateControl()
    {
    }
}