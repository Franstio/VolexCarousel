using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VolexCarousel.Interfaces
{
    public interface ICheckItemService
    {
        Task<string> CheckItemAsync(object id);
        void Start();
        void Stop();
    }
}
