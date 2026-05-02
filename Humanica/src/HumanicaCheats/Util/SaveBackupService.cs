using System;
using System.IO;
using MelonLoader;

namespace HumanicaCheats.Util
{
    internal static class SaveBackupService
    {
        public static bool BackupSaves(string reason)
        {
            try
            {
                string source = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                    "..",
                    "LocalLow",
                    "Panfachdev",
                    "Humanica",
                    "Saves");
                source = Path.GetFullPath(source);

                if (!Directory.Exists(source))
                {
                    MelonLogger.Warning($"[SaveBackupService] Saves folder not found: {source}");
                    return false;
                }

                string timestamp = DateTime.Now.ToString("yyyyMMdd-HHmmss");
                string safeReason = Sanitize(reason);
                string destination = Path.Combine(
                    MelonLoader.Utils.MelonEnvironment.UserDataDirectory,
                    "HumanicaCheats",
                    "SaveBackups",
                    $"{timestamp}-{safeReason}");

                Directory.CreateDirectory(destination);
                foreach (string file in Directory.GetFiles(source, "*.save", SearchOption.TopDirectoryOnly))
                {
                    string target = Path.Combine(destination, Path.GetFileName(file));
                    File.Copy(file, target, overwrite: false);
                }

                MelonLogger.Msg($"[SaveBackupService] Backed up saves to {destination}");
                return true;
            }
            catch (Exception ex)
            {
                MelonLogger.Error($"[SaveBackupService] Backup failed: {ex.Message}");
                return false;
            }
        }

        private static string Sanitize(string value)
        {
            foreach (char invalid in Path.GetInvalidFileNameChars())
            {
                value = value.Replace(invalid, '-');
            }
            return string.IsNullOrWhiteSpace(value) ? "backup" : value.Trim();
        }
    }
}
