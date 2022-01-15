
using CardKartShared.Network;

namespace CardKartShared.Network.Messages
{
    public class RegisterUserRequest : Message
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

            return new RawMessage(MessageTypes.RegisterRequestMessage, encoder.Bytes);
        }
    }
}
