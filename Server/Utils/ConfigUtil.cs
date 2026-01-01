using DansDevTools.Configuration;
using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.Helpers;
using System.Reflection;
using System.Runtime.Serialization.Json;

namespace DansDevTools.Utils
{
    [Injectable(InjectionType.Singleton)]
    public class ConfigUtil
    {
        private const string FILENAME = "config.json";

        public ModConfig CurrentConfig { get; private set; }

        public bool IsModEnabled => CurrentConfig.Enabled;

        public ConfigUtil(ModHelper modHelper)
        {
            string pathToMod = modHelper.GetAbsolutePathToModFolder(Assembly.GetExecutingAssembly());
            string fileText = File.ReadAllText(Path.Combine(pathToMod, FILENAME));
            CurrentConfig = Deserialize(fileText);
        }

        public static string Serialize(ModConfig config)
        {
            using (MemoryStream memoryStream = new MemoryStream())
            using (StreamReader reader = new StreamReader(memoryStream))
            {
                DataContractJsonSerializer serializer = new DataContractJsonSerializer(typeof(ModConfig));
                serializer.WriteObject(memoryStream, config);
                memoryStream.Position = 0;
                return reader.ReadToEnd();
            }
        }

        public static ModConfig Deserialize(string configFileText)
        {
            using (Stream stream = new MemoryStream())
            {
                byte[] data = System.Text.Encoding.UTF8.GetBytes(configFileText);
                stream.Write(data, 0, data.Length);
                stream.Position = 0;
                DataContractJsonSerializer deserializer = new DataContractJsonSerializer(typeof(ModConfig));
                object? obj = deserializer.ReadObject(stream);
                if (obj == null)
                {
                    throw new InvalidOperationException("Could not deserialize config file");
                }

                return (ModConfig)obj;
            }
        }
    }
}
