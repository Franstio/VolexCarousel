using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VolexCarousel.Models
{
    public class ShiftTransactionRecord
    {
        public Guid uid { get; set; } = Guid.NewGuid(); 
        public DateTime datetimeinput { get; set; } = DateTime.Now;
        public DateTime datetimeoutput { get; set; } = DateTime.Now;
        public string shiftname { get; set; } = string.Empty;
        public int targetoutput { get; set; } = 0;
    }
}
