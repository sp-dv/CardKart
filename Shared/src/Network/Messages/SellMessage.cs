using CardKartShared.GameState;

namespace CardKartShared.Network.Messages
{
    public class SellCardRequest : Message
    {
        public CardTemplates CardTemplate { get; set; }
        public bool IsQuickSellOrder { get; set; }
        public double StrikePrice { get; set; }

        public void Decode(RawMessage message)
        {
            var decoder = new JsonDecoder(message.Bytes);

            CardTemplate = decoder.Decode<CardTemplates>();
            IsQuickSellOrder = decoder.Decode<bool>();
            StrikePrice = decoder.Decode<double>();
        }

        public RawMessage Encode()
        {
            var encoder = new JsonEncoder();

            encoder.Encode(CardTemplate);
            encoder.Encode(IsQuickSellOrder);
            encoder.Encode(StrikePrice);

            return new RawMessage(MessageTypes.SellCardRequest, encoder.Bytes);
        }
    }

    public class SellCardResponse : Message
    {
        public bool Success { get; set; }
        public double NewBalance { get; set; }
        public string Error { get; set; }

        public void Decode(RawMessage message)
        {
            var decoder = new JsonDecoder(message.Bytes);

            Success = decoder.Decode<bool>();
            NewBalance = decoder.Decode<double>();
            Error = decoder.Decode<string>();
        }

        public RawMessage Encode()
        {
            var encoder = new JsonEncoder();

            encoder.Encode(Success);
            encoder.Encode(NewBalance);
            encoder.Encode(Error);

            return new RawMessage(MessageTypes.SellCardResponse, encoder.Bytes);
        }
    }
}
