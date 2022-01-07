using System;
using System.Collections.Generic;
using System.Threading;

namespace CardKartShared.GameState
{
    public class PrivateSaxophone<T>
    {
        private ManualResetEvent ResetEvent = new ManualResetEvent(false);
        //private T Payload;
        private Queue<T> Q = new Queue<T>();

        public void Play(T tune)
        {
            lock (Q)
            {
                Q.Enqueue(tune);
            }
            ResetEvent.Set();
        }

        public T Listen()
        {
            bool set = false;
            T rt = default(T);

            lock (Q)
            {
                if (Q.Count > 0)
                {
                    rt = Q.Dequeue();
                    set = true;
                }
            }

            if (!set)
            {
                ResetEvent.WaitOne();
                lock (Q)
                {
                    rt = Q.Dequeue();
                }
            }
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
