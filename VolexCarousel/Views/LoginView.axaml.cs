using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using VolexCarousel.ViewModels;

namespace VolexCarousel.Views;

public partial class LoginView : UserControl
{
    public LoginView()
    {
        InitializeComponent();
    }
    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);
        if (this.DataContext is LoginViewModel lvm)
        {
            lvm.Init();
        }
    }
}