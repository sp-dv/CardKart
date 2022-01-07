using System;

namespace CardKartShared.Network.Messages
{
    public class SurrenderGameMessage : Message
    {
        public int GameID { get; set; }

        public void Decode(RawMessage message)
        {
            var decoder = new ByteDecoder(message.Bytes);

            GameID = decoder.DecodeInt();
        }

        public RawMessage Encode()
        {
            var encoder = new ByteEncoder();

            encoder.EncodeInt(GameID);

            return new RawMessage(MessageTypes.SurrenderGameMessage, encoder.Bytes);
        }
    }
}
