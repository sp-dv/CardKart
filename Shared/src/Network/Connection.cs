using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace CardKartShared.Network
{
    public class Connection
    {
        private NetworkStream Stream;
        private static Encoding RawEncoding = Encoding.UTF8;

        public Connection(NetworkStream stream)
        {
            Stream = stream;
        }

        public RawMessage ReceiveMessage()
        {
            var bodyLengthBuffer = new byte[sizeof(int)];
            var bodyLengthReadCount = Stream.Read(bodyLengthBuffer, 0, bodyLengthBuffer.Length);
            if (bodyLengthReadCount != bodyLengthBuffer.Length) { throw new Exception(); }
            var bodyLength = BitConverter.ToInt32(bodyLengthBuffer);

            var messageTypeBuffer = new byte[sizeof(int)];
            var messageTypeReadCount = Stream.Read(messageTypeBuffer, 0, messageTypeBuffer.Length);
            if (bodyLengthReadCount != messageTypeBuffer.Length) { throw new Exception(); }
            var messageType = BitConverter.ToInt32(messageTypeBuffer);

            var bodyBuffer = new byte[bodyLength];
            var bodyReadCount = Stream.Read(bodyBuffer, 0, bodyBuffer.Length);
            if (bodyReadCount != bodyBuffer.Length) { throw new Exception(); }
            var bodyString = RawEncoding.GetString(bodyBuffer);

            return new RawMessage((MessageTypes)messageType, bodyBuffer);
        }

        public void SendMessage(RawMessage message)
        {
            var bodyLengthBytes = BitConverter.GetBytes(message.Bytes.Length);
            var messageTypeBytes = BitConverter.GetBytes((int)message.MessageType);
            var messageBodyBytes = message.Bytes;

            Stream.Write(bodyLengthBytes, 0, bodyLengthBytes.Length);
            Stream.Write(messageTypeBytes, 0, messageTypeBytes.Length);
            Stream.Write(messageBodyBytes, 0, messageBodyBytes.Length);
        }
    }


    public class Server
    {
        private int Port;
        private string Address;

        public Server(int port, string address)
        {
            Port = port;
            Address = address;
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
