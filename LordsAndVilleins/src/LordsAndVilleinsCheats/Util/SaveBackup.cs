using System;
using System.IO;
using System.Linq;

namespace LordsAndVilleinsCheats.Util
{
    public static class SaveBackup
    {
        private const string BackupRoot = "_modbackup";

        public static void Run(string saveDir, int maxKeep)
        {
            if (!Directory.Exists(saveDir)) return;
            var saves = Directory.GetFiles(saveDir, "*.sgz", SearchOption.TopDirectoryOnly);
            if (saves.Length == 0) return;

            var backupParent = Path.Combine(saveDir, BackupRoot);
            Directory.CreateDirectory(backupParent);

            var stamp = DateTime.Now.ToString("yyyyMMdd-HHmmss-fff");
            var thisRun = Path.Combine(backupParent, stamp);
            Directory.CreateDirectory(thisRun);

            foreach (var src in saves)
            {
                var dest = Path.Combine(thisRun, Path.GetFileName(src));
                File.Copy(src, dest, overwrite: true);
            }

            var existing = Directory.GetDirectories(backupParent)
                                    .OrderByDescending(d => d)
                                    .ToList();
            foreach (var oldDir in existing.Skip(maxKeep))
            {
                try { Directory.Delete(oldDir, recursive: true); }
                catch { }
            }
        }
    }
}
