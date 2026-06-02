using System;
using System.IO;
using System.Linq;
using MelonLoader;

namespace ClanfolkCheats.Util
{
    public static class SaveBackup
    {
        public static void Run(string saveDir, int maxKeep = 5)
        {
            if (!Directory.Exists(saveDir))
            {
                MelonLogger.Warning($"[SaveBackup] Directory not found: {saveDir}");
                return;
            }

            var timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
            var backupDir = Path.Combine(saveDir, $"backup_{timestamp}");

            try
            {
                CopyDirectory(saveDir, backupDir);
                MelonLogger.Msg($"[SaveBackup] Created backup: {backupDir}");
            }
            catch (Exception ex)
            {
                MelonLogger.Error($"[SaveBackup] Failed: {ex.Message}");
                return;
            }

            try
            {
                var backups = Directory.GetDirectories(saveDir, "backup_*")
                    .OrderByDescending(d => d)
                    .ToList();

                while (backups.Count > maxKeep)
                {
                    var old = backups[^1];
                    Directory.Delete(old, recursive: true);
                    backups.RemoveAt(backups.Count - 1);
                    MelonLogger.Msg($"[SaveBackup] Pruned old backup: {old}");
                }
            }
            catch (Exception ex)
            {
                MelonLogger.Warning($"[SaveBackup] Prune failed: {ex.Message}");
            }
        }

        private static void CopyDirectory(string sourceDir, string destDir)
        {
            Directory.CreateDirectory(destDir);
            foreach (var file in Directory.GetFiles(sourceDir))
            {
                var name = Path.GetFileName(file);
                if (name.StartsWith("backup_")) continue;
                File.Copy(file, Path.Combine(destDir, name), overwrite: true);
            }
            foreach (var dir in Directory.GetDirectories(sourceDir))
            {
                var name = Path.GetFileName(dir);
                if (name.StartsWith("backup_")) continue;
                CopyDirectory(dir, Path.Combine(destDir, name));
            }
        }
    }
}
