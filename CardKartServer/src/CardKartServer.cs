using CardKartServer.Schemas;
using CardKartShared.Network;
using CardKartShared.Util;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace CardKartServer
{
    internal class CardKartServer
    {
        public static ConnectionHandler ConnectionHandler { get; private set; }
        public static ClientHandler ClientHandler { get; private set; }
        public static GameCoordinator GameCoordinator { get; private set; }
        public static RSAParameters ServerKey { get; private set; }


        static void Main(string[] args)
        {
            Logging.AddConsoleLogger();
            Logging.Log(LogLevel.Info, $"Running CardKart server version {Constants.Version}.");

            if (args .Length > 0)
            {
                HandleCommandLineCommand(args[0].ToLower(), args.Skip(1).ToArray());
                return;
            }


            
            Database.Load();

            ServerKey = JsonConvert.DeserializeObject<RSAParameters>(File.ReadAllText("server.key"));

            ClientHandler = new ClientHandler();
            
            GameCoordinator = new GameCoordinator();
            
            ConnectionHandler = new ConnectionHandler();
            ConnectionHandler.Start();
        }

        static void HandleCommandLineCommand(string command, string[] arguments)
        {
            switch (command)
            {
                case "generate-keys":
                    {
                        if (arguments.Length != 0)
                        {
                            Logging.Log(LogLevel.Error, "generate-keys takes no arguments");
                            return;
                        }

                        Logging.Log(LogLevel.Info, "Generating keys...");

                        var keypair = RSAEncryption.GenerateKeys();
                        var privateKeyString = keypair.Item1;
                        var publicKeyString = keypair.Item2;
                        

                        var privateKeyFileName = $"./server_new.key";
                        Logging.Log(LogLevel.Info, $"Writing {privateKeyFileName}");
                        File.WriteAllText(privateKeyFileName, privateKeyString);

                        var publicKeyFileName = $"./public.keycode";
                        publicKeyString = publicKeyString.Replace("\"", "\\\""); // " -> \"
                        Logging.Log(LogLevel.Info, $"Writing {publicKeyFileName}");
                        File.WriteAllText(publicKeyFileName, 
                            $"private static RSAParameters NewKey => JsonConvert.DeserializeObject<RSAParameters>(\"{publicKeyString}\");"
                            );
                    }break;
            }
        }
    }
}
