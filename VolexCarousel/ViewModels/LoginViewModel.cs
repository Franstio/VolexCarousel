using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DialogHostAvalonia;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VolexCarousel.Services;
using VolexCarousel.Store;

namespace VolexCarousel.ViewModels
{
    public partial class LoginViewModel : ViewModelBase
    {
        private readonly UserStore store;
        private readonly CarouselRepositoryService dbService;

        [ObservableProperty]
        private string username = "";
        [ObservableProperty]
        private string password = "";
        public LoginViewModel(UserStore store,CarouselRepositoryService dbService)
        {
            this.store = store;
            this.dbService = dbService;
        }
        [RelayCommand]
        public async Task Login()
        {
            var usr = await dbService.GetUser(Username);
            if (usr is null || !usr.Any())
            {
                DialogHost.Close("Root_Dialog", false);
                return;
            }
            if (usr.First().Password != Password)
            {
                DialogHost.Close("Root_Dialog", false);
                return;
            }
            store.User = usr.First();
            DialogHost.Close("Root_Dialog", true);

        }
    }
}
