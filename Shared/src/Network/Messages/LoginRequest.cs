using System;
using System.Collections.Generic;
using System.Text;

namespace CardKartShared.Network.Messages
{
    public class LoginRequest : Message
    {
        public string Username { get; set; }
        public string Password { get; set; }

        public void Decode(RawMessage message)
        {
            var decoder = new ByteDecoder(message.Bytes);

            Username = decoder.DecodeString();
            Password = decoder.DecodeString();
        }

        public RawMessage Encode()
        {
            var encoder = new ByteEncoder();

            encoder.EncodeString(Username);
            encoder.EncodeString(Password);

            return new RawMessage(MessageTypes.LoginRequest, encoder.Bytes);
        }
    }
}
