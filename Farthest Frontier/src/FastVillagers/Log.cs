using System.Collections.Generic;
using MelonLoader;

namespace FarthestFrontier.FastVillagers
{
    internal static class Log
    {
        private static readonly HashSet<string> WarnedKeys = new HashSet<string>();

        public static void Msg(string message)
        {
            MelonLogger.Msg("[KK Fast Villagers] " + message);
        }

        public static void MsgOnce(string key, string message)
        {
            if (!WarnedKeys.Add("msg:" + key))
            {
                return;
            }

            Msg(message);
        }

        public static void Warn(string message)
        {
            MelonLogger.Msg("[KK Fast Villagers] WARN: " + message);
        }

        public static void WarnOnce(string key, string message)
        {
            if (!WarnedKeys.Add(key))
            {
                return;
            }

            Warn(message);
        }
    }
}
