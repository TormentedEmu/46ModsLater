using NLog;

namespace FortySixModsLater
{
    // external access to Patch Scripts
    public static class LogEx
    {
        private static Logger _log = LogManager.GetCurrentClassLogger();

        public static void Info(string msg)
        {
            _log.Info(msg);
        }

        public static void Warn(string msg)
        {
            _log.Warn(msg);
        }

        public static void Error(string msg)
        {
            _log.Error(msg);
        }

        public static void Debug(string msg)
        {
            _log.Debug(msg);
        }

        public static void Trace(string msg)
        {
            _log.Trace(msg);
        }

    }
}
