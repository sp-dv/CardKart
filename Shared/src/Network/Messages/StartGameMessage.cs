using System;

namespace CardKartShared.Network.Messages
{
    public class StartGameMessage : Message
    {
        public int GameID;
        public int PlayerIndex;

        public void Decode(RawMessage message)
        {
            var decoder = new ByteDecoder(message.Bytes);

            GameID = decoder.DecodeInt();
            PlayerIndex = decoder.DecodeInt();
        }

        public RawMessage Encode()
        {
            var encoder = new ByteEncoder();

            encoder.EncodeInt(GameID);
            encoder.EncodeInt(PlayerIndex);

            return new RawMessage(MessageTypes.StartGameMessage, encoder.Bytes);
        }
    }
}
