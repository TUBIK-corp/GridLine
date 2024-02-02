using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace GridLine_IDE.Helpers
{
    public static class ConfigHelper
    {
        public static string ConfigPath { get; set; } = "config.json";

        public static Config Config { get; set; } = new Config();

        public static void WriteConfig()
        {
            var fullPath = Path.Combine(Environment.CurrentDirectory, ConfigPath);
            if(!File.Exists(fullPath))
                File.Create(fullPath).Close();

            var json = JsonConvert.SerializeObject(Config, Formatting.Indented);
            File.WriteAllText(fullPath, json);
        }

        public static Config ReadConfig()
        {
            var fullPath = Path.Combine(Environment.CurrentDirectory, ConfigPath);
            if (!File.Exists(fullPath))
            {
                File.Create(fullPath).Close();
                WriteConfig();
            }

            try
            {
                var config = JsonConvert.DeserializeObject<Config>(File.ReadAllText(fullPath));
                Config = config ?? new Config();
                return Config; 
            } catch
            {
                MessageBox.Show("Конфигурационный файл повреждён.");
                return Config;
            }
        }

        public static string PackConfig(Config config)
        {
            return JsonConvert.SerializeObject(config);
        }
        public static Config UnpackConfig(string config_json)
        {
            return JsonConvert.DeserializeObject<Config>(config_json) ?? new Config();
        }
    }
    public class Config
    {
        public string SkinName { get; set; } = "UserBlueRobotImage";
        public string IconName { get; set; } = "Синий пылесос";
        public int StartX { get; set; } = 0;
        public int StartY { get; set; } = 0;
        public int Width { get; set; } = 21;
        public int Height { get; set; } = 21;
        public string ConnectSQLite { get; set; } = "";
        public int LimitNesting { get; set; } = 3;
        public int LimitMaxValue { get; set; } = 1000;
    }
}
