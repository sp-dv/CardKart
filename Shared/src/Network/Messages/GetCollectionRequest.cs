using CardKartShared.GameState;
using CardKartShared.Util;
using System;
using System.Collections.Generic;
using System.Text;

namespace CardKartShared.Network.Messages
{
    public class GetCollectionRequest : Message
    {
        public void Decode(RawMessage message)
        {
        }

        public RawMessage Encode()
        {
            return new RawMessage(MessageTypes.GetCollectionRequest, new byte[1]);
        }
    }

    public class GetCollectionResponse : Message
    {
        public double Galds { get; set; }
        public Dictionary<CardTemplates, int> OwnedCards { get; set; } = new Dictionary<CardTemplates, int>();
        public Dictionary<Packs, int> OwnedPacks { get; set; } = new Dictionary<Packs, int>();

        public void Decode(RawMessage message)
        {
            var decoder = new JsonDecoder(message.Bytes);

            Galds = decoder.Decode<double>();
            OwnedCards = decoder.Decode<Dictionary<CardTemplates, int>>();
            OwnedPacks = decoder.Decode<Dictionary<Packs, int>>();
        }

        public RawMessage Encode()
        {
            var encoder = new JsonEncoder();

            encoder.Encode(Galds);
            encoder.Encode(OwnedCards);
            encoder.Encode(OwnedPacks);

            return new RawMessage(MessageTypes.GetCollectionRequest, encoder.Bytes);
        }
    }
}
