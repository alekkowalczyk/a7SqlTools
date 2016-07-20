using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Security.AccessControl;
using System.Text;
using System.Threading.Tasks;

namespace a7SqlTools.Utils
{
    class a7Utils
    {
        public static string GetExePath()
        {
            return Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly().Location);
        }

        public static string GetFilePathFromExePath(string fileName)
        {
            return Path.Combine(GetExePath(), fileName);
        }

        public static void ExtractZipToDirectory(string zipPath, string dstPath)
        {
            if (!Directory.Exists(dstPath))
                Directory.CreateDirectory(dstPath);
            ZipArchive zipArchive = ZipFile.OpenRead(zipPath);
            foreach (ZipArchiveEntry entry in zipArchive.Entries)
            {
                var path = Path.Combine(dstPath, entry.FullName);
                var directory = Path.GetDirectoryName(path);
                if (!Directory.Exists(directory))
                    Directory.CreateDirectory(directory);
                entry.ExtractToFile(path, true);
            }
            zipArchive.Dispose();
        }

        public static bool ProcessIsRunning(string procName)
        {
            var pro = Process.GetProcessesByName(procName);
            return pro.Count() > 0;
        }

        public static void SetFullAccessToFolder(string userName, string folderPath)
        {
            if (!Directory.Exists(folderPath))
                Directory.CreateDirectory(folderPath);
            DirectoryInfo dirInfo = new DirectoryInfo(folderPath);
            DirectorySecurity dirSecurity = dirInfo.GetAccessControl();

            dirSecurity.AddAccessRule(new FileSystemAccessRule(userName, FileSystemRights.FullControl, InheritanceFlags.ContainerInherit | InheritanceFlags.ObjectInherit, PropagationFlags.InheritOnly, AccessControlType.Allow));


            dirInfo.SetAccessControl(dirSecurity);
        }
    }
}
