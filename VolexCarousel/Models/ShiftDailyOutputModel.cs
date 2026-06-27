using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VolexCarousel.Models
{
    public class ShiftDailyOutputModel
    {
        public string ShiftName { get; set; } = string.Empty;
        public int TargetOutput { get; set; } = 0;
        public int TotalOutput { get; set; } = 0;
    }
}
