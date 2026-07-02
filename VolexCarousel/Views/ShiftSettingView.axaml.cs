using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using VolexCarousel.ViewModels;

namespace VolexCarousel.Views;

public partial class ShiftSettingView : UserControl
{
    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);
        if (this.DataContext is ShiftSettingViewModel viewModel)
        {
            _ = viewModel.LoadSettings();
        }
    }
    public ShiftSettingView()
    {
        InitializeComponent();
    }
}