using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VolexCarousel.Models
{
    public class ShiftTransactionRecord
    {
        public DateTime InputTime { get; set; } = DateTime.Now;
        public DateTime OutputTime { get; set; } = DateTime.Now;
        public string RecordedShift { get; set; } = string.Empty;
    }
}
