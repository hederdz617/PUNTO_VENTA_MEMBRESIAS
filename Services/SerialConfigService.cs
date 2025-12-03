using System;
using System.IO;
using System.Text.Json;

namespace NuevoAPPwindowsforms.Services
{
    public class SerialConfig
    {
        public string PuertoSerial { get; set; } = "COM9";
        public int Baudrate { get; set; } = 9600;
    }

    public static class SerialConfigService
    {
        private static readonly string ConfigPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "serialconfig.json");

        public static SerialConfig Load()
        {
            if (!File.Exists(ConfigPath))
                return new SerialConfig();
            var json = File.ReadAllText(ConfigPath);
            return JsonSerializer.Deserialize<SerialConfig>(json) ?? new SerialConfig();
        }

        public static void Save(SerialConfig config)
        {
            var json = JsonSerializer.Serialize(config, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(ConfigPath, json);
        }
    }
}
