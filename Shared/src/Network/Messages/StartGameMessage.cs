using System;

namespace CardKartShared.Network.Messages
{
    public class StartGameMessage : Message
    {
        public int GameID;
        public int PlayerIndex;
        public int RNGSeed;

        public void Decode(RawMessage message)
        {
            var decoder = new ByteDecoder(message.Bytes);

            GameID = decoder.DecodeInt();
            PlayerIndex = decoder.DecodeInt();
            RNGSeed = decoder.DecodeInt();
        }

        public RawMessage Encode()
        {
            var encoder = new ByteEncoder();

            encoder.EncodeInt(GameID);
            encoder.EncodeInt(PlayerIndex);
            encoder.EncodeInt(RNGSeed);

            return new RawMessage(MessageTypes.StartGameMessage, encoder.Bytes);
        }
    }
}
