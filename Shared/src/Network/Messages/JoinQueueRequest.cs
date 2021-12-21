
namespace CardKartShared.Network.Messages
{
    public class JoinQueueRequest : Message
    {
        public void Decode(RawMessage message)
        {
        }

        public RawMessage Encode()
        {
            return new RawMessage(MessageTypes.JoinQueueRequest, new byte[1]);
        }
    }
}
