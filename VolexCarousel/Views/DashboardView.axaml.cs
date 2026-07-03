using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using VolexCarousel.ViewModels;

namespace VolexCarousel.Views;

public partial class DashboardView : UserControl
{
    public DashboardView()
    {
        InitializeComponent();
    }
    protected override void OnInitialized()
    {
        if (DataContext is DashboardViewModel viewModel)
        {
            viewModel.Initialization();
        }
    }
    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        if (DataContext is DashboardViewModel viewModel)
        {
            viewModel.StopServices();
        }
    }
}