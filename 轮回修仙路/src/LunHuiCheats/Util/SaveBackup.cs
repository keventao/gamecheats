using System;
using System.IO;
using System.Linq;

namespace LunHuiCheats.Util
{
    /// <summary>
    /// Simple save backup utility. Copies the entire save directory before making changes.
    /// </summary>
    public static class SaveBackup
    {
        public static void Run(string saveDir, int maxKeep = 5)
        {
            if (!Directory.Exists(saveDir))
            {
                Plugin.LogSrc?.LogWarning($"[SaveBackup] Directory not found: {saveDir}");
                return;
            }

            var timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
            var backupDir = Path.Combine(saveDir, $"backup_{timestamp}");

            try
            {
                Directory.CreateDirectory(backupDir);
                foreach (var file in Directory.GetFiles(saveDir, "*.txt", SearchOption.TopDirectoryOnly))
                {
                    var dest = Path.Combine(backupDir, Path.GetFileName(file));
                    File.Copy(file, dest, overwrite: true);
                }
                Plugin.LogSrc?.LogInfo($"[SaveBackup] Created backup: {backupDir}");
            }
            catch (Exception ex)
            {
                Plugin.LogSrc?.LogError($"[SaveBackup] Failed to create backup: {ex.Message}");
                return;
            }

            // Prune old backups
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
                    Plugin.LogSrc?.LogInfo($"[SaveBackup] Pruned old backup: {old}");
                }
            }
            catch (Exception ex)
            {
                Plugin.LogSrc?.LogWarning($"[SaveBackup] Prune failed: {ex.Message}");
            }
        }
    }
}
