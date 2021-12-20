using System;
using System.Collections.Generic;
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

        public RawMessage(MessageTypes messageType, byte[] bytes)
        {
            MessageType = messageType;
            Bytes = bytes;
        }
    }

    public enum MessageTypes
    {
        None,

        HandshakeMessage,

        GenericResponse,
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
    }
}
