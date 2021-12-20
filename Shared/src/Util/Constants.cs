using CardKartShared.Network;

namespace CardKartShared.Util
{
    public static class Constants
    {
        public const string Version = "0.0.0";
        public static bool IsDevVersion = true;

        public static readonly Server DebugServer = new Server(4444, "localhost");
    }
}
