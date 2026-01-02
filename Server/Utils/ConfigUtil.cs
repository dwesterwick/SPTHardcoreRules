using HardcoreRules.Configuration;
using HardcoreRules.Helpers;
using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.Helpers;
using System.Reflection;

namespace HardcoreRules.Utils
{
    [Injectable(InjectionType.Singleton)]
    public class ConfigUtil
    {
        private const string FILENAME_CONFIG = "config.json";
        private const string FILENAME_TRANSLATIONS = "translations.json";

        private string _serverModDirectory = null!;
        public string ServerModDirectory
        {
            get
            {
                if (_serverModDirectory == null)
                {
                    _serverModDirectory = GetServerModDirectory();
                }

                return _serverModDirectory;
            }
        }

        private ModConfig _currentConfig = null!;
        public ModConfig CurrentConfig
        {
            get
            {
                if (_currentConfig == null)
                {
                    _currentConfig = GetObject<ModConfig>(FILENAME_CONFIG);
                }

                return _currentConfig;
            }
        }

        private Dictionary<string, Dictionary<string, string>> _translations = null!;
        public Dictionary<string, Dictionary<string, string>> Translations
        {
            get
            {
                if (_translations == null)
                {
                    _translations = GetObject<Dictionary<string, Dictionary<string, string>>>(FILENAME_TRANSLATIONS);
                }

                return _translations;
            }
        }

        private ModHelper _modHelper;

        public ConfigUtil(ModHelper modHelper)
        {
            _modHelper = modHelper;
        }

        private string GetServerModDirectory()
        {
            return _modHelper.GetAbsolutePathToModFolder(Assembly.GetExecutingAssembly());
        }

        private T GetObject<T>(string filename)
        {
            string fileText = File.ReadAllText(Path.Combine(_serverModDirectory, filename));
            T? obj = ConfigHelpers.Deserialize<T>(fileText);
            if (obj == null)
            {
                throw new InvalidOperationException($"Could not deserialize {filename}");
            }

            return obj;
        }
    }
}
