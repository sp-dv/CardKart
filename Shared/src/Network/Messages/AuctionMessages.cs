using CardKartShared.GameState;

namespace CardKartShared.Network.Messages
{
    public class GetQuoteRequest : Message
    {
        public CardTemplates Template { get; set; }

        public void Decode(RawMessage message)
        {
            var decoder = new JsonDecoder(message.Bytes);

            Template = decoder.Decode<CardTemplates>();
        }

        public RawMessage Encode()
        {
            var encoder = new JsonEncoder();

            encoder.Encode(Template);

            return new RawMessage(MessageTypes.CardQuoteRequest, encoder.Bytes);
        }
    }

    public class GetQuoteResponse : Message
    {
        public Quote Quote { get; set; }

        public void Decode(RawMessage message)
        {
            var decoder = new JsonDecoder(message.Bytes);

            Quote = decoder.Decode<Quote>();
        }

        public RawMessage Encode()
        {
            var encoder = new JsonEncoder();

            encoder.Encode(Quote);

            return new RawMessage(MessageTypes.CardQuoteRequest, encoder.Bytes);
        }
    }

    public class Quote
    {
        public double Bid { get; set; }
        public double Ask { get; set; }
    }
}
