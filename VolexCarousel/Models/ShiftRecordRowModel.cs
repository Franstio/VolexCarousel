using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VolexCarousel.Models
{
    public class ShiftRecordRowModel
    {
        public DateTime Timestamp { get; set; }
        public int TargetOutput { get; set; }
        public int Output { get; set; }
    }
}
