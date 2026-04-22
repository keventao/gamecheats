using System;
using System.IO;
using System.Linq;
using LordsAndVilleinsCheats.Util;
using Xunit;

namespace LordsAndVilleinsCheats.Tests
{
    public class SaveBackupTests : IDisposable
    {
        private readonly string _root;
        public SaveBackupTests()
        {
            _root = Path.Combine(Path.GetTempPath(), "lav-cheats-tests-" + Guid.NewGuid());
            Directory.CreateDirectory(_root);
        }
        public void Dispose() => Directory.Delete(_root, true);

        private string MakeSave(string name)
        {
            var path = Path.Combine(_root, name);
            File.WriteAllText(path, "fake save");
            return path;
        }

        [Fact]
        public void Run_CopiesAllSgzIntoTimestampedSubdir()
        {
            MakeSave("a.sgz");
            MakeSave("b.sgz");
            MakeSave("notes.txt");

            SaveBackup.Run(_root, maxKeep: 5);

            var backupDir = Directory.GetDirectories(Path.Combine(_root, "_modbackup")).Single();
            var files = Directory.GetFiles(backupDir).Select(Path.GetFileName).OrderBy(x => x).ToArray();
            Assert.Equal(new[] { "a.sgz", "b.sgz" }, files);
        }

        [Fact]
        public void Run_RotatesOldestOut_WhenExceedingMaxKeep()
        {
            MakeSave("a.sgz");
            for (int i = 0; i < 7; i++)
            {
                SaveBackup.Run(_root, maxKeep: 5);
                System.Threading.Thread.Sleep(20);
            }
            var backups = Directory.GetDirectories(Path.Combine(_root, "_modbackup"));
            Assert.Equal(5, backups.Length);
        }

        [Fact]
        public void Run_NoSaves_DoesNotCreateBackupDir()
        {
            SaveBackup.Run(_root, maxKeep: 5);
            Assert.False(Directory.Exists(Path.Combine(_root, "_modbackup")));
        }
    }
}
