using CommunityToolkit.Mvvm.Input;
using DialogHostAvalonia;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VolexCarousel.Store;

namespace VolexCarousel.ViewModels
{
    public partial class LoginViewModel : ViewModelBase
    {
        private UserStore store;
        public LoginViewModel(UserStore store)
        {
            this.store = store;
        }
        [RelayCommand]
        public void Login()
        {
            store.User = new Models.User
            {
                Username = "John Doe",
            };
            DialogHost.Close("Root_Dialog");
        }
    }
}
