using BepInEx.Logging;

namespace SecurityDoorHologramOverhaul
{
    internal static class Logger
    {
        private static readonly ManualLogSource _logger;

        static Logger()
        {
            _logger = new ManualLogSource("SecDoorHolo");
            BepInEx.Logging.Logger.Sources.Add(_logger);
        }

        // Helper method for formatting messages, currently converts the provided
        // 'msg' object to a string, but it can be extended however you want
        private static string Format(object msg) => msg.ToString();

        // Helper methods for logging

        public static void Info(object data) => _logger.LogMessage(Format(data));
        public static void Verbose(object data)
        {
#if DEBUG
            _logger.LogDebug(Format(data));
#endif
        }
        public static void Debug(object data) => _logger.LogDebug(Format(data));
        public static void Error(object data) => _logger.LogError(Format(data));
    }
}
