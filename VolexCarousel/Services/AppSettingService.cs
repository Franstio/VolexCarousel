using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using VolexCarousel.Models;

namespace VolexCarousel.Services
{
    public class AppSettingService
    {
        private readonly static string basePath = AppDomain.CurrentDomain.BaseDirectory;
        private readonly static string settingName = "appsettings.json";
        private Func<string> GetPath = () => Path.Combine(basePath, settingName);
        public AppSettingsModel LoadSettings()
        {
            AppSettingsModel? settings;
            if (File.Exists(GetPath()))
            {
                var json = File.ReadAllText(GetPath());
                settings = System.Text.Json.JsonSerializer.Deserialize<Models.AppSettingsModel>(json);
                if (settings != null)
                {
                    return settings;
                }
            }

            settings = new AppSettingsModel();
            File.WriteAllText(GetPath(),JsonSerializer.Serialize(settings, new JsonSerializerOptions { WriteIndented=true}));
            return settings;
        }

        public void Save(AppSettingsModel settings)
        {
            File.WriteAllText(GetPath(), JsonSerializer.Serialize(settings, new JsonSerializerOptions { WriteIndented = true }));
        }
    }
}
