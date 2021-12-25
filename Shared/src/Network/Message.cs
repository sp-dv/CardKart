using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CardKartShared.Network
{
    public interface Message
    {
        public abstract RawMessage Encode();
        public abstract void Decode(RawMessage message);
    }

    public class RawMessage
    {
        public MessageTypes MessageType { get; }
        public byte[] Bytes { get; }

        public bool AllowUnverified;
            
        public RawMessage(MessageTypes messageType, byte[] bytes)
        {
            MessageType = messageType;
            Bytes = bytes;

            AllowUnverified = 
                MessageType == MessageTypes.HandshakeMessage ||
                MessageType == MessageTypes.HandshakeResponseMessage ||
                MessageType == MessageTypes.LoginRequest ||
                MessageType == MessageTypes.GenericResponse;

        }
    }

    public enum MessageTypes
    {
        None,

        HandshakeMessage,
        HandshakeResponseMessage,

        LoginRequest,

        StartGameMessage,
        GameChoiceMessage,

        GenericResponse,

        JoinQueueRequest,
    }

    public class ByteEncoder
    {
        private List<byte> ByteList = new List<byte>();

        public byte[] Bytes => ByteList.ToArray();

        public void EncodeInt(int value)
        {
            ByteList.AddRange(BitConverter.GetBytes(value));
        }

        public void EncodeString(string value)
        {
            EncodeInt(value.Length);
            ByteList.AddRange(Encoding.UTF8.GetBytes(value));
        }

        public void EncodeBytes(IEnumerable<byte> bytes)
        {
            EncodeInt(bytes.Count());
            ByteList.AddRange(bytes);
        }
    }

    public class ByteDecoder
    {
        private byte[] Bytes;
        private int Counter;

        public ByteDecoder(byte[] bytes)
        {
            Bytes = bytes;
        }

        public int DecodeInt()
        {
            int value = BitConverter.ToInt32(Bytes, Counter);
            Counter += sizeof(int);
            return value;
        }

        public string DecodeString()
        {
            int stringLength = DecodeInt();
            var value = Encoding.UTF8.GetString(Bytes, Counter, stringLength);
            Counter += stringLength;
            return value;
        }

        public byte[] DecodeBytes()
        {
            var count = DecodeInt();
            var rt = new byte[count];
            for (int i = 0; i < count; i++)
            {
                rt[i] = Bytes[Counter++];
            }
            return rt;
        }

    }

    public class JsonEncoder
    {
        private List<string> JsonStrings = new List<string>();

        public byte[] Bytes => Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(JsonStrings));

        public void Encode(object obj)
        {
            JsonStrings.Add(JsonConvert.SerializeObject(obj));
        }
    }

    public class JsonDecoder
    {
        private List<string> JsonStrings;
        private int Counter;

        public JsonDecoder(byte[] bytes)
        {
            JsonStrings = JsonConvert.DeserializeObject<List<string>>(Encoding.UTF8.GetString(bytes));
        }

        public T Decode<T>()
        {
            return JsonConvert.DeserializeObject<T>(JsonStrings[Counter++]);
        }
    }
}
