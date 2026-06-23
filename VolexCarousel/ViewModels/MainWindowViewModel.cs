using Avalonia.Threading;
using AvaloniaDialogs.Views;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DialogHostAvalonia;
using VolexCarousel.Controls;
using VolexCarousel.Store;
using VolexCarousel.Views;

namespace VolexCarousel.ViewModels
{
    public partial class MainWindowViewModel : ViewModelBase
    {
        private readonly LoginViewModel _loginViewModel;
        private readonly UserStore _userStore;
        public MainWindowViewModel(LoginViewModel loginViewModel,UserStore userStore)
        {
            _loginViewModel = loginViewModel;
            _userStore = userStore;
        }
        [ObservableProperty]
        private ViewModelBase _currentViewModel = new DashboardViewModel();


        [RelayCommand]
        public void ShowShiftsView()
        {
            DialogHost.Show(_loginViewModel, new DialogClosingEventHandler(async (obj, e) =>
            {
                var usr = _userStore.User;
                if (usr is not null)
                {
                    await Dispatcher.UIThread.InvokeAsync(async () =>
                    {
                        await DialogHost.Show(new ShiftSettingView());
                    });
                }
            }));
        }
    }
}
