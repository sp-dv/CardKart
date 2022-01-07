using CardKartShared.Util;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace CardKartServer
{
    internal class ServerConfiguration
    {
        private const string ConfigFilePath = "./server.config";

        public string RSAKeyPath { get; set; }
        public string DBFilePath { get; set; }

        public void Save()
        {
            File.WriteAllText(ConfigFilePath, JsonConvert.SerializeObject(this));
        }

        public static ServerConfiguration Load()
        {
            try
            {
                var rt = JsonConvert.DeserializeObject<ServerConfiguration>(File.ReadAllText(ConfigFilePath));
                Logging.Log(LogLevel.Info, "Configuration loaded.");
                return rt;
            }
            catch
            {
                return new ServerConfiguration();
            }
        }
    }
}
