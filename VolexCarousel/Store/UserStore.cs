using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VolexCarousel.Models;

namespace VolexCarousel.Store
{
    public class UserStore
    {

        private User? _user;
        public User? User
        {
            get => _user;
            set
            {
                _user = value;
                UserChanged?.Invoke(_user);
            }
        }

        public Action<User?>? UserChanged { get; set; }


    }
}
