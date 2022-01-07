using CardKartShared.GameState;
using System;

namespace CardKartShared.Network.Messages
{
    public class GameEndedMessage : Message
    {
        public int GameID { get; set; }
        public int WinnerIndex { get; set; }
        public GameEndedReasons Reason { get; set; }

        public GameEndedMessage()
        {
        }

        public void Decode(RawMessage message)
        {
            var decoder = new ByteDecoder(message.Bytes);

            GameID = decoder.DecodeInt();
            WinnerIndex = decoder.DecodeInt();
            Reason = (GameEndedReasons)decoder.DecodeInt();
        }

        public RawMessage Encode()
        {
            var encoder = new ByteEncoder();

            encoder.EncodeInt(GameID);
            encoder.EncodeInt(WinnerIndex);
            encoder.EncodeInt((int)Reason);

            return new RawMessage(MessageTypes.GameEndedMessage, encoder.Bytes);
        }
    }
}
