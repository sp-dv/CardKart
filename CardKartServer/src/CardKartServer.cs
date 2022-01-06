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
        public static ServerConfiguration Config { get; private set; }

        static void Main(string[] args)
        {
            Logging.AddConsoleLogger();
            Logging.Log(LogLevel.Info, $"Running CardKart server version {Constants.Version}");

            Config = ServerConfiguration.Load();

            Database.Load();

            if (args .Length > 0)
            {
                HandleCommandLineCommand(args[0].ToLower(), args.Skip(1).ToArray());
                return;
            }


            ServerKey = JsonConvert.DeserializeObject<RSAParameters>(File.ReadAllText(Config.RSAKeyPath));

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

                        var publicKeyFileName = $"./public_new.keycode";
                        publicKeyString = publicKeyString.Replace("\"", "\\\""); // " -> \"
                        Logging.Log(LogLevel.Info, $"Writing {publicKeyFileName}");
                        File.WriteAllText(publicKeyFileName, 
                            $"private static RSAParameters NewKey => JsonConvert.DeserializeObject<RSAParameters>(\"{publicKeyString}\");"
                            );
                    } break;

                case "add-user":
                    {
                        if (arguments.Length != 2)
                        {
                            Logging.Log(LogLevel.Error, "Usage: add-user <username> <password>");
                            return;
                        }

                        var username = arguments[0];
                        var password = arguments[1];

                        LoginInfo.RegisterUser(username, password).Then(res => {
                            Logging.Log(LogLevel.Info, "User created successfully.");
                        }).Err(err => {
                            Logging.Log(LogLevel.Info, $"User creation failed: {err.ErrorMessage}");
                        });
                    } break;

                case "generate-config":
                    {
                        if (arguments.Length != 0)
                        {
                            Logging.Log(LogLevel.Error, "generate-config takes no arguments.");
                            return;
                        }

                        Console.WriteLine("Enter key path: ");
                        var keyPath = Console.ReadLine();

                        Console.WriteLine("Enter database file path: ");
                        var dbFilePath = Console.ReadLine();

                        CardKartServer.Config.RSAKeyPath = keyPath;
                        CardKartServer.Config.DBFilePath = dbFilePath;
                        CardKartServer.Config.Save();

                        Logging.Log(LogLevel.Info, "Config created successfully");

                    }
                    break;
            }
        }
    }
}
