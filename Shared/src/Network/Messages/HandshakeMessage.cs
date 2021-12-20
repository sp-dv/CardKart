namespace CardKartShared.Network.Messages
{
    public class HandshakeMessage : Message
    {
        public string VersionString;
        public int MagicNumber;

        public HandshakeMessage()
        {
        }

        public HandshakeMessage(string versionString, int magicNumber)
        {
            VersionString = versionString;
            MagicNumber = magicNumber;
        }

        public void Decode(RawMessage message)
        {
            var decoder = new ByteDecoder(message.Bytes);
            
            VersionString = decoder.DecodeString();
            MagicNumber = decoder.DecodeInt();
        }

        public RawMessage Encode()
        {
            var encoder = new ByteEncoder();
            
            encoder.EncodeString(VersionString);
            encoder.EncodeInt(MagicNumber);

            return new RawMessage(MessageTypes.HandshakeMessage, encoder.Bytes);
        }
    }
}
