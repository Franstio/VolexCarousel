using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VolexCarousel.Models
{
    public class ShiftRowModel
    {
        public TimeOnly StartTime { get; set; }
        public TimeOnly EndTime { get; set; }

        public int TotalPcs { get; set; } = 0;

        public string TimeRange => $"{StartTime:HH:mm} - {EndTime:HH:mm}";
    }
}
