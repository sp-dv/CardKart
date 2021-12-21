using System;
using System.Threading;

namespace CardKartShared.GameState
{
    public class PublicSaxophone<T>
    {
        private ManualResetEvent ResetEvent = new ManualResetEvent(false);
        private T Payload;

        public void Play(T tune)
        {
            Payload = tune;
            ResetEvent.Set();
        }

        public T Listen()
        {
            ResetEvent.WaitOne();
            var rt = Payload;
            ResetEvent.Reset();
            return rt;
        }

        public T Listen(Func<T, bool> filter)
        {
            while (true)
            {
                var rt = Listen();
                if (filter(rt)) { return rt; }
            }
        }
    }
}
