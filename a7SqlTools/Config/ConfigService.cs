using a7SqlTools.Config.Model;
using a7SqlTools.Utils;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace a7SqlTools.Config
{
    public class ConfigService
    {
        private static ConfigService _instance;
        public static ConfigService Instance
        {
            get
            {
                if (_instance == null)
                    _instance = new ConfigService();
                return _instance;
            }
        }
        private readonly string configFileName = "a7SqlToolsConfig.json";
        private string _configFileDictionaryPath;
        private string _configFileFullPath;

        public ConfigService()
        {
            var path = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location); //this get the embedded assemblies path in the installation folder
            path = Path.GetDirectoryName(path); //this gets the parent path, which is the installation folder with the version number
            _configFileDictionaryPath = Path.GetDirectoryName(path); //root folder of installation path
            _configFileFullPath = Path.Combine(_configFileDictionaryPath, configFileName);
            if (!Directory.Exists(_configFileDictionaryPath))
                Directory.CreateDirectory(_configFileDictionaryPath);
        }

        public LocalConfig GetConfiguration()
        {
            if (File.Exists(_configFileFullPath))
            {
                var jsonSerializerSettings = new JsonSerializerSettings()
                {
                    TypeNameHandling = TypeNameHandling.All
                };
                var serialized = File.ReadAllText(_configFileFullPath);
                var cm = JsonConvert.DeserializeObject<LocalConfig>(serialized, jsonSerializerSettings);
                return cm;
            }
            else
            {
                var config = new LocalConfig();
                saveConfiguration(config);
                return config;
            }
        }

        private void saveConfiguration(LocalConfig config)
        {
            var jsonSerializerSettings = new JsonSerializerSettings()
            {
                TypeNameHandling = TypeNameHandling.All
            };
            var json = JsonConvert.SerializeObject(config, jsonSerializerSettings);
            File.WriteAllText(this._configFileFullPath, json);
        }

        public void AddConnection(ConnectionItem connectionItem)
        {
            var config = this.GetConfiguration();
            config.Connections.Add(connectionItem);
            saveConfiguration(config);
        }

        public void UpdateConnection(ConnectionItem connectionItem)
        {
            var config = this.GetConfiguration();
            var existing = config.Connections.FirstOrDefault(c => c.ConnectionData.Name == connectionItem.ConnectionData.Name);
            var index = config.Connections.IndexOf(existing);
            config.Connections.RemoveAt(index);
            config.Connections.Insert(index, connectionItem);
            saveConfiguration(config);
        }

        public void RemoveConnection(string connectionString)
        {
            var config = this.GetConfiguration();
            config.Connections = config.Connections.Where(c => c.ConnectionData != null).ToList();
            var item = config.Connections.FirstOrDefault(c => ConnectionStringGenerator.Get(c.ConnectionData) == connectionString);
            if (item != null)
                config.Connections.Remove(item);
            saveConfiguration(config);
        }
    }
}
