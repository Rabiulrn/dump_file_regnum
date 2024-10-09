using Oracle.ManagedDataAccess.Client;
using RegnumServices.Entities.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RegnumServices.ServiceManager
{
    public class DBBackUpModule
    {
        protected static string connStringHQ = "";

        public async Task SyncDBBackups(string getConString, QuerySettingDTO settingDTO)
        {
            try
            {
                connStringHQ = getConString;
             
                SchemaBackUp(settingDTO);
                // Clean up old .dmp files after backup
              
                LogWritter("BackUpSuccessfull");

            }
            catch (Exception ex)
            {

                LogWritter("BackUp error:" + ex.Message);

            }

        }
        //public void SchemaBackUp(QuerySettingDTO settingDTO)
        //{

        //    using (OracleConnection connection = new OracleConnection(connStringHQ))
        //    {
        //        try
        //        {
        //            connection.Open();

        //            using (OracleCommand command = connection.CreateCommand())
        //            {

        //                command.CommandText = $@"DECLARE
        //                  h1 NUMBER;
        //                BEGIN
        //                  h1 := DBMS_DATAPUMP.OPEN(operation => 'EXPORT', job_mode => 'SCHEMA', job_name => '{settingDTO.JobName}');
        //                  DBMS_DATAPUMP.ADD_FILE(handle => h1, filename => '{settingDTO.DMPFileName}.dmp', directory => '{settingDTO.DirectoryName}', filetype => dbms_datapump.ku$_file_type_dump_file);
        //                  DBMS_DATAPUMP.ADD_FILE(handle => h1, filename => '{settingDTO.LogFileName}.log', directory => '{settingDTO.DirectoryName}', filetype => dbms_datapump.ku$_file_type_log_file);
        //                  DBMS_DATAPUMP.METADATA_FILTER(handle => h1, name => 'SCHEMA_EXPR', value => '= ''{settingDTO.DBUserName}''');
        //                  DBMS_DATAPUMP.START_JOB(handle => h1);
        //                  DBMS_DATAPUMP.DETACH(handle => h1);
        //                END;";
        //                command.ExecuteNonQuery();
        //            }
        //        }
        //        catch (Exception ex)
        //        {
        //            throw;

        //        }
        //        finally
        //        {
        //            connection.Close();
        //        }
        //    }
        //}
        public void SchemaBackUp(QuerySettingDTO settingDTO)
        {
            using (OracleConnection connection = new OracleConnection(connStringHQ))
            {
                try
                {
                    connection.Open();

                    using (OracleCommand command = connection.CreateCommand())
                    {
                        // Create timestamp with correct format
                        string timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");

                        // Ensure the correct file name format, including uppercase and proper structure
                        string dmpFileName = $"{settingDTO.DMPFileName}.DMP";
                        string logFileName = $"{settingDTO.LogFileName}_BACKUP{timestamp}.LOG";

                        // Use the corrected file names in the DBMS_DATAPUMP command
                        command.CommandText = $@"DECLARE
                  h1 NUMBER;
                BEGIN
                  h1 := DBMS_DATAPUMP.OPEN(operation => 'EXPORT', job_mode => 'SCHEMA', job_name => '{settingDTO.JobName}_{timestamp}');
                  DBMS_DATAPUMP.ADD_FILE(handle => h1, filename => '{dmpFileName}', directory => '{settingDTO.DirectoryName}', filetype => dbms_datapump.ku$_file_type_dump_file);
                  DBMS_DATAPUMP.ADD_FILE(handle => h1, filename => '{logFileName}', directory => '{settingDTO.DirectoryName}', filetype => dbms_datapump.ku$_file_type_log_file);
                  DBMS_DATAPUMP.METADATA_FILTER(handle => h1, name => 'SCHEMA_EXPR', value => '= ''{settingDTO.DBUserName}''');
                  DBMS_DATAPUMP.START_JOB(handle => h1);
                  DBMS_DATAPUMP.DETACH(handle => h1);
                END;";
                        command.ExecuteNonQuery();

                        CleanOldDumpFiles(settingDTO.DirectoryPath);

                    }
                }
                catch (Exception ex)
                {
                    throw;
                }
                finally
                {
                    connection.Close();
                }
            }
        }



        public void CleanOldDumpFiles(string directoryPath)
        {
            try
            {
                // Hardcoded directory (no file prefix needed)
               // string directory = @"D:\TESTDMP"; // Replace with your actual directory path

                DirectoryInfo dirInfo = new DirectoryInfo(directoryPath);

                // Get all .dmp files in the specified directory
                FileInfo[] dmpFiles = dirInfo.GetFiles("*.dmp")
                                             .OrderByDescending(f => f.CreationTime)
                                             .ToArray();

                // Get all .log files in the specified directory
                FileInfo[] logFiles = dirInfo.GetFiles("*.log")
                                             .OrderByDescending(f => f.CreationTime)
                                             .ToArray();

                // Delete all .dmp files except the 3 most recent ones
                foreach (FileInfo file in dmpFiles.Skip(3)) // Skips the 3 most recent .dmp files
                {
                    file.Delete(); // Deletes older .dmp files
                }

                // Delete all .log files except the 3 most recent ones
                foreach (FileInfo file in logFiles.Skip(3)) // Skips the 3 most recent .log files
                {
                    file.Delete(); // Deletes older .log files
                }
            }
            catch (Exception ex)
            {
                // Log the exception if there's any error
                LogWritter("Error cleaning old dump and log files: " + ex.Message);
            }
        }







        public void LogWritter(string message)
        {
            string filename = DateTime.Now.ToString("yyyy_MM_dd") + "_DBBackUpLog.log";
            string appPath = $@"D:\\demolog";
            int index = appPath.LastIndexOf("\\");
            appPath = appPath.Remove(index);
            string path = Path.Combine(appPath, "Log" + DateTime.Now.Year.ToString());
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }

            File.AppendAllText(path + "\\" + filename, "\r\n" + DateTime.Now.ToString("dd HH:mm:ss.f") + "\t" + message);

        }
    }
}

