using System.IO;
using LunHuiCheats.Util;
using Xunit;

namespace LunHuiCheats.Tests
{
    public class SaveBackupTests
    {
        [Fact]
        public void Run_CreatesBackupDirectory()
        {
            var tempDir = Path.Combine(Path.GetTempPath(), "lunhui-test-saves");
            Directory.CreateDirectory(tempDir);
            File.WriteAllText(Path.Combine(tempDir, "playerData.txt"), "test");

            try
            {
                SaveBackup.Run(tempDir, maxKeep: 3);
                var backups = Directory.GetDirectories(tempDir, "backup_*");
                Assert.Single(backups);
            }
            finally
            {
                Directory.Delete(tempDir, recursive: true);
            }
        }

        [Fact]
        public void Run_PrunesOldBackups()
        {
            var tempDir = Path.Combine(Path.GetTempPath(), "lunhui-test-saves-prune");
            Directory.CreateDirectory(tempDir);
            File.WriteAllText(Path.Combine(tempDir, "playerData.txt"), "test");

            try
            {
                for (int i = 0; i < 5; i++)
                {
                    SaveBackup.Run(tempDir, maxKeep: 2);
                    System.Threading.Thread.Sleep(50);
                }
                var backups = Directory.GetDirectories(tempDir, "backup_*");
                Assert.True(backups.Length <= 2, $"Expected <=2 backups, found {backups.Length}");
            }
            finally
            {
                Directory.Delete(tempDir, recursive: true);
            }
        }
    }
}
