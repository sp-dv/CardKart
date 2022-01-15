using CardKartShared.GameState;
using CardKartShared.Util;
using System;
using System.Collections.Generic;
using System.Text;

namespace CardKartShared.Network.Messages
{
    public class RipPackRequest : Message
    {
        public Packs Pack { get; set; }

        public void Decode(RawMessage message)
        {
            var decoder = new JsonDecoder(message.Bytes);

            Pack = decoder.Decode<Packs>();
        }

        public RawMessage Encode()
        {
            var encoder = new JsonEncoder();

            encoder.Encode(Pack);

            return new RawMessage(MessageTypes.RipPackRequest, encoder.Bytes);
        }
    }

    public class RipPackResponse : Message
    {
        public CardTemplates[] Templates { get; set; }

        public void Decode(RawMessage message)
        {
            var decoder = new JsonDecoder(message.Bytes);

            Templates = decoder.Decode<CardTemplates[]>();
        }

        public RawMessage Encode()
        {
            var encoder = new JsonEncoder();

            encoder.Encode(Templates);

            return new RawMessage(MessageTypes.RipPackResponse, encoder.Bytes);
        }
    }
}
