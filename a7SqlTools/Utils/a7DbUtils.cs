using Microsoft.SqlServer.Management.Common;
using Microsoft.SqlServer.Management.Smo;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace a7SqlTools.Utils
{
    class a7DbUtils
    {
        public static async Task<List<string>> GetDbNames(ServerConnection connection)
        {
            Server sqlServer = new Server(connection);
            List<string> dbList = new List<string>();
            await Task.Run(() =>
            {
                foreach (var db in sqlServer.Databases)
                {
                    var database = db as Database;
                    dbList.Add(database.Name);
                }
            });
            return dbList;
        }

        public static bool BackupDatabase(ServerConnection connection, string dataBaseName, string destinationPath, Action<string> log)
        {
            //ServerConnection connection = new ServerConnection(csb.DataSource, csb.UserID, csb.Password);
            Server sqlServer = new Server(connection);
            log("Backup of file '" + destinationPath + "' as database '" + dataBaseName + "'");
            Backup bkpDatabase = new Backup();
            bkpDatabase.Action = BackupActionType.Database;
            bkpDatabase.Database = dataBaseName;
            BackupDeviceItem bkpDevice = new BackupDeviceItem(destinationPath, DeviceType.File);
            bkpDatabase.Devices.Add(bkpDevice);
            bkpDatabase.SqlBackup(sqlServer);
            return File.Exists(destinationPath);
            //connection.Disconnect();

        }

        public static bool RestoreDatabase(ServerConnection connection, string databaseName, string backUpFile, Action<string> log, bool asReadOnly = false)//, String serverName, String userName, String password)
        {
            var path = Path.GetDirectoryName(backUpFile);
            Server sqlServer = new Server(connection);
            Restore rstDatabase = new Restore();
            rstDatabase.Action = RestoreActionType.Database;
            rstDatabase.Database = databaseName;
            BackupDeviceItem bkpDevice = new BackupDeviceItem(backUpFile, DeviceType.File);
            rstDatabase.Devices.Add(bkpDevice);
            rstDatabase.ReplaceDatabase = true;
            System.Data.DataTable logicalRestoreFiles = rstDatabase.ReadFileList(sqlServer);
            var mdfFileName = logicalRestoreFiles.Rows[0][0].ToString();
            var ldfFileName = logicalRestoreFiles.Rows[1][0].ToString();
            var mdfFileFullPath = logicalRestoreFiles.Rows[0][1].ToString();
            var ldfFileFullPath = logicalRestoreFiles.Rows[1][1].ToString();
            var mdfFilePath = Path.GetDirectoryName(mdfFileFullPath);
            var ldfFilePath = Path.GetDirectoryName(ldfFileFullPath);
            if (path != "" && path != null)
            {
                if (!Directory.Exists(path))
                    Directory.CreateDirectory(path);
                mdfFilePath = path;
                ldfFilePath = path;
            }
            var newMdfFileFullPath = Path.Combine(mdfFilePath, databaseName + ".mdf");
            var newLdfFileFullPath = Path.Combine(ldfFilePath, databaseName + "_Log.ldf");
            rstDatabase.RelocateFiles.Add(new RelocateFile(mdfFileName, newMdfFileFullPath));
            rstDatabase.RelocateFiles.Add(new RelocateFile(ldfFileName, newLdfFileFullPath));
            rstDatabase.SqlRestore(sqlServer);
            var restoredDb = sqlServer.Databases[databaseName];
            if (restoredDb != null)
            {
                //a7DbUtils.SetUserForDataBase(databaseName, a7Config.a7ServiceUserName, log);
                //if (asReadOnly)
                //{
                //    restoredDb.DatabaseOptions.ReadOnly = true;
                //    restoredDb.ReadOnly = true;
                //}
                return true;
            }
            else
            {
                return false;
            }
        }


        private static bool backupDbToZip(ServerConnection connection, string dbName, string backupPath, Action<string> log)
        {
            var dataBakFilePath = Path.Combine(backupPath, dbName, dbName + ".bak");
            BackupDatabase(connection, dbName, dataBakFilePath, log);
            if (!File.Exists(dataBakFilePath))
                return false;
            log("Ziping file '" + backupPath + "'");
            var dataZipFilePath = Path.Combine(backupPath, dbName + ".zip");
            if (File.Exists(dataZipFilePath))
                File.Delete(dataZipFilePath);
            ZipFile.CreateFromDirectory(Path.GetDirectoryName(dataBakFilePath), dataZipFilePath);
            if (!File.Exists(dataZipFilePath))
                return false;
            return true;
        }

        public static void SetUserForDataBase(ServerConnection connection, string databaseName, string userName, Action<string> log)
        {
            Server server = new Server(connection);
            if (userName.IndexOf(@"\") == -1)
                userName = System.Environment.MachineName + @"\" + userName;
            var db = server.Databases[databaseName];
            if (db == null)
                throw new Exception("a7DbUtils.SetUserForDataBase: database '" + databaseName + "' doesn't exist!");
            if (!server.Logins.Contains(userName))
            {
                log("DB Server doesn't have login for user '" + userName + "' - creating");
                Login login = new Login(server, userName);
                login.LoginType = LoginType.WindowsUser;
                login.Create();
            }
            if (!db.Users.Contains(userName))
            {
                log("DB '" + databaseName + "' doesn't have user '" + userName + "' - creating");
                var user = new User(db, userName);
                user.Login = userName;
                user.Create();
                db.Roles["db_datareader"].AddMember(userName);
                db.Roles["db_datawriter"].AddMember(userName);
            }
        }

        public static bool CheckDatabaseExists(ServerConnection connection, string databaseName)
        {
            Server server = new Server(connection);
            var db = server.Databases[databaseName];
            return db != null;
        }

        public static void DeleteDatabase(ServerConnection connection, string databaseName, Action<string> log)
        {
            Server server = new Server(connection);
            log("Deleting database:" + databaseName);
            server.KillDatabase(databaseName);
        }

        public static bool MoveDatabase(ServerConnection connection, string sourceDbName, string destinationDbName, Action<string> log)
        {
            log("Moving database '" + sourceDbName + "' to '" + destinationDbName + "'");
            var oldConfigFilePath = a7Utils.GetFilePathFromExePath(destinationDbName);
            if (File.Exists(oldConfigFilePath))
                File.Delete(oldConfigFilePath);
            if (a7DbUtils.BackupDatabase(connection, sourceDbName, oldConfigFilePath, log))
            {
                a7DbUtils.DeleteDatabase(connection, sourceDbName, log);
                a7DbUtils.RestoreDatabase(connection, destinationDbName, oldConfigFilePath, log);
                return a7DbUtils.CheckDatabaseExists(connection, destinationDbName);
            }
            else
            {
                return false;
            }
        }
    }
}
