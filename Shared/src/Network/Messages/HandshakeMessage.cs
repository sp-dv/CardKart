using System;
using System.Security.Cryptography;

namespace CardKartShared.Network.Messages
{
    public class HandshakeMessage : Message
    {
        public string VersionString;
        public byte[] MagicBytes;


        public void Decode(RawMessage message)
        {
            var decoder = new ByteDecoder(message.Bytes);
            
            VersionString = decoder.DecodeString();
            MagicBytes = decoder.DecodeBytes();
        }

        public RawMessage Encode()
        {
            var encoder = new ByteEncoder();
            
            encoder.EncodeString(VersionString);
            encoder.EncodeBytes(MagicBytes);

            return new RawMessage(MessageTypes.HandshakeMessage, encoder.Bytes);
        }
    }

    public class Magic
    {
        public AesParams AesParams;
        public byte[] Nonce;

        public Magic()
        {
            var rand = new Random();
            Nonce = new byte[64];
            rand.NextBytes(Nonce);

            var aes = Aes.Create();
            AesParams = new AesParams(aes);
        }
    }

    public class HandshakeResponse : Message
    {
        public byte[] MagicBytes;
        public string Error;

        public void Decode(RawMessage message)
        {
            var decoder = new JsonDecoder(message.Bytes);

            MagicBytes = decoder.Decode<byte[]>();
            Error = decoder.Decode<string>();
        }

        public RawMessage Encode()
        {
            var encoder = new JsonEncoder();
            encoder.Encode(MagicBytes);
            encoder.Encode(Error);

            return new RawMessage(MessageTypes.HandshakeResponseMessage, encoder.Bytes);
        }
    }

}
