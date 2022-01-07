using System;

namespace CardKartShared
{
    public class ThisShouldNeverHappen : Exception
    {
        public ThisShouldNeverHappen()
        {
        }

        public ThisShouldNeverHappen(string message) : base(message)
        {
        }
    }
}
