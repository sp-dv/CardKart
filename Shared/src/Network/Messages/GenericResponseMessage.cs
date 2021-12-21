namespace CardKartShared.Network.Messages
{
    public class GenericResponseMessage : Message
    {
        public Codes Code;
        public string Info = "";

        public GenericResponseMessage()
        {
        }

        public GenericResponseMessage(Codes code, string info)
        {
            Code = code;
            Info = info;
        }

        public void Decode(RawMessage message)
        {
            var decoder = new ByteDecoder(message.Bytes);

            Code = (Codes)decoder.DecodeInt();
            Info = decoder.DecodeString();
        }

        public RawMessage Encode()
        {
            var encoder = new ByteEncoder();

            encoder.EncodeInt((int)Code);
            encoder.EncodeString(Info);

            return new RawMessage(MessageTypes.GenericResponse, encoder.Bytes);
        }

        public enum Codes
        {
            None,

            OK,
            Error,
        }
    }
}
