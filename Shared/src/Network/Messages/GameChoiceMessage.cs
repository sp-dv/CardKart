using CardKartShared.Util;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace CardKartShared.Network.Messages
{
    public class GameChoiceMessage : Message
    {
        public int GameID;
        public GameChoice Choices;

        public void Decode(RawMessage message)
        {
            var decoder = new ByteDecoder(message.Bytes);

            GameID = decoder.DecodeInt();

            var jsonString = decoder.DecodeString();
            Choices = JsonConvert.DeserializeObject<GameChoice>(jsonString);
        }

        public RawMessage Encode()
        {
            var encoder = new ByteEncoder();

            var jsonString = JsonConvert.SerializeObject(Choices);

            encoder.EncodeInt(GameID);
            encoder.EncodeString(jsonString);

            return new RawMessage(MessageTypes.GameChoiceMessage, encoder.Bytes);
        }
    }

    public class GameChoice
    {
        public Dictionary<string, int> Singletons { get; } 
            = new Dictionary<string, int>();
        public Dictionary<string, int[]> Arrays { get; } 
            = new Dictionary<string, int[]>();
    }
}
