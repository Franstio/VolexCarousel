using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VolexCarousel.Models
{
    public class AppSettingsModel
    {
        public string InformationSpeedPort { get; set; } = string.Empty;
        public string PLCPort { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public double CarouselLength { get; set; } = 1.9;
        public string CarouselDb { get; set; } = $"data source={Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "carousel.db")};default timeout=3000";

    }
}
