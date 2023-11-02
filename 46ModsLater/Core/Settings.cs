using NLog;
using Newtonsoft.Json;

namespace FortySixModsLater
{
    internal class SettingsData
    {
        public string ModsPath { get; set; } = @"C:\MyMods";
        public string GamePath { get; set; } = @"C:\MyGame";
        public bool IsLoggingToFile { get; set; } = false; // not currently in use
        public Dictionary<string, bool> Mods { get; set; } = new Dictionary<string, bool>();

        public SettingsData()
        {
        }

        public override string ToString()
        {
            return JsonConvert.SerializeObject(this, Formatting.Indented);
        }
    }

    internal class Settings
    {
        private static Logger _log = LogManager.GetCurrentClassLogger();

        private SettingsData _settingsData = new SettingsData();
        private string gameManagedPath = string.Empty;

        public string SettingsFilename = "Settings.json";

        public string ModsPath
        {
            get => _settingsData.ModsPath;
            set => _settingsData.ModsPath = value;
        }

        public string GamePath
        {
            get => _settingsData.GamePath;
            set => _settingsData.GamePath = value;
        }

        public bool IsLogging
        {
            get => _settingsData.IsLoggingToFile;
            set => _settingsData.IsLoggingToFile = value;
        }

        public Dictionary<string, bool> Mods
        {
            get => _settingsData.Mods;
            set => _settingsData.Mods = value;
        }

        public string GameManagedPath { get => gameManagedPath; set => gameManagedPath = value; }


        public bool this[string key]
        {
            get
            {
                bool enabled = true;
                if (!_settingsData.Mods.TryGetValue(key, out enabled))
                {
                    _settingsData.Mods.Add(key, enabled);
                }
                return enabled;
            }
            set
            {
                bool enabled = true;
                if (!_settingsData.Mods.TryGetValue(key, out enabled))
                    _settingsData.Mods.Add(key, enabled);
                else
                    _settingsData.Mods[key] = value;
            }
        }

        public Settings()
        {
        }

        public void Init(string path)
        {
            SettingsFilename = Path.Combine(path, "Settings.json");

            if (!File.Exists(SettingsFilename))
            {
                _log.Error($"Failed to find the settings json file: {SettingsFilename}");
                _log.Info("Using default settings.");
                return;
            }

            string file = string.Empty;
            try
            {
                file = File.ReadAllText(SettingsFilename);
            }
            catch (Exception ex)
            {
                _log.Error($"Settings::Init - caught exception: {ex}");
                return;
            }

            _settingsData = JsonConvert.DeserializeObject<SettingsData>(file);

            if (_settingsData == null)
            {
                _log.Error("Failed to deserialize the Settings.json file. Using default settings.");
                _settingsData = new SettingsData();
            }

            GameManagedPath = Utils.FindManagedFolder(GamePath);
            _log.Info("Current Settings:\n" + _settingsData.ToString());
        }

        public void Save()
        {
            try
            {
                foreach (var item in _settingsData.Mods.Where(kvp => string.IsNullOrEmpty(kvp.Key)).ToList())
                {
                    _settingsData.Mods.Remove(item.Key);
                }

                File.WriteAllText(SettingsFilename, JsonConvert.SerializeObject(_settingsData, Formatting.Indented));
            }
            catch (Exception ex)
            {
                _log.Error(ex.ToString());
            }
        }
    }
}
