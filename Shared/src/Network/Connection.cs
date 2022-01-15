using CardKartShared.Util;
using System;
using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Text;
using System.Threading;

namespace CardKartShared.Network
{
    public class Connection
    {
        private NetworkStream Stream;
        private static Encoding RawEncoding = Encoding.UTF8;

        public bool IsClosed;
        public delegate void OnCloseHander();
        public event OnCloseHander Closed;

        private const bool LogMessages = false;

        public EncryptionSuite EncryptionSuite { get; set; }

        public Connection(NetworkStream stream)
        {
            Stream = stream;
        }

        public RawMessage ReceiveMessage()
        {
            if (!IsClosed)
            {
                try
                {
                    var bodyLengthBuffer = new byte[sizeof(int)];
                    var bodyLengthReadCount = Stream.Read(bodyLengthBuffer, 0, bodyLengthBuffer.Length);
                    if (bodyLengthReadCount != bodyLengthBuffer.Length) { throw new Exception("Incorrect bodyLengthBuffer size."); }
                    var bodyLength = BitConverter.ToInt32(bodyLengthBuffer);
                    if (LogMessages) { Logging.Log(LogLevel.Debug, $"bodyLengthBuffer: {atos(bodyLengthBuffer)}."); }


                    var messageTypeBuffer = new byte[sizeof(int)];
                    var messageTypeReadCount = Stream.Read(messageTypeBuffer, 0, messageTypeBuffer.Length);
                    if (bodyLengthReadCount != messageTypeBuffer.Length) { throw new Exception("Incorrect messageTypeBuffer size."); }
                    var messageType = BitConverter.ToInt32(messageTypeBuffer);
                    if (LogMessages) { Logging.Log(LogLevel.Debug, $"messageTypeBuffer: {atos(messageTypeBuffer)}."); }


                    var bodyBuffer = new byte[bodyLength];
                    var bodyReadCount = Stream.Read(bodyBuffer, 0, bodyBuffer.Length);
                    if (bodyReadCount != bodyBuffer.Length) { throw new Exception("Incorrect bodyBuffer size."); }
                    if (EncryptionSuite != null) { bodyBuffer = EncryptionSuite.Decrypt(bodyBuffer); }
                    var bodyString = RawEncoding.GetString(bodyBuffer);
                    if (LogMessages) { Logging.Log(LogLevel.Debug, $"bodyBuffer: {atos(bodyBuffer)}."); }

                    if (LogMessages)
                    {
                        Logging.Log(LogLevel.Debug, $"Received message of type <{(MessageTypes)messageType}>.");
                    }

                    return new RawMessage((MessageTypes)messageType, bodyBuffer);
                }
                catch (Exception e)
                {
                    //Logging.Log(LogLevel.Error, $"{e}");
                    IsClosed = true;
                    Closed?.Invoke();
                }
            }
            else
            {
                Logging.Log(LogLevel.Warning, "Trying to receive message from closed connection.");
            }
            
            return new RawMessage(MessageTypes.None, null);
        }

        public void SendMessage(RawMessage message)
        {
            if (message.Bytes.Length == 0)
            {
                // Seding empty messages is buggy.
                // Just send a single byte and don't
                // decode it; e.g. JoinQueueRequest.
                Logging.Log(
                    LogLevel.Warning,
                    $"Seding empty message of type {message.MessageType}");
            }
            if (!IsClosed)
            {
                try
                {
                    var messageBodyBytes = message.Bytes;

                    if (EncryptionSuite != null) { messageBodyBytes = EncryptionSuite.Encrypt(messageBodyBytes);}

                    var bodyLengthBytes = BitConverter.GetBytes(messageBodyBytes.Length);
                    var messageTypeBytes = BitConverter.GetBytes((int)message.MessageType);

                    Stream.Write(bodyLengthBytes, 0, bodyLengthBytes.Length);
                    Stream.Write(messageTypeBytes, 0, messageTypeBytes.Length);
                    Stream.Write(messageBodyBytes, 0, messageBodyBytes.Length);

                    if (LogMessages)
                    { 
                        Logging.Log(LogLevel.Debug, $"Sent message of type <{message.MessageType}>.");
                        Logging.Log(LogLevel.Debug, $"bodyLengthBytes: {atos(bodyLengthBytes)}.");
                        Logging.Log(LogLevel.Debug, $"messageTypeBytes: {atos(messageTypeBytes)}.");
                        Logging.Log(LogLevel.Debug, $"messageBodyBytes: {atos(messageBodyBytes)}.");
                    }
                }
                catch (Exception)
                {
                    IsClosed = true;
                    Closed?.Invoke();
                }
            }
        }

        public void SendMessage(Message message)
        {
            SendMessage(message.Encode());
        }

        #region don't look pls

        private string atos(byte[] bs) 
        {
            var sb = new StringBuilder();
            sb.Append("[");
            foreach (var b in bs)
            {
                sb.Append(b);
                sb.Append(", ");
            }
            return sb.ToString();
        }

        #endregion
    }


    public class Server
    {
        private string Address { get; }
        private int Port { get; }
        public RSAParameters PublicKey { get; }

        public Server(string address, int port, RSAParameters publicKey)
        {
            Address = address;
            Port = port;
            PublicKey = publicKey;
        }

        public void Host(Action<Connection, TcpClient> connectedCallback)
        {
            var server = new TcpListener(IPAddress.Any, Port);
            server.Start();

            new Thread(() =>
            {
                while (true)
                {
                    TcpClient client = server.AcceptTcpClient();
                    connectedCallback(new Connection(client.GetStream()), client);
                }
            }).Start();
        }

        public Connection Connect()
        {
            TcpClient client = new TcpClient(Address, Port);
            return new Connection(client.GetStream());
        }
    }
}
