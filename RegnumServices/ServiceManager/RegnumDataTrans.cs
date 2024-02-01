using Microsoft.Extensions.Configuration;

using Oracle.ManagedDataAccess.Client;
using RegnumServices;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Mail;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Text.RegularExpressions;


namespace ServiceManager
{
    public class RegnumDataTrans
    {

        static string connStringLocal = "";
        static string connStringHQ = "";

		public void GetConStings()
		{
			ConfigSettings configSettingsA = new ConfigSettings();
			connStringLocal = configSettingsA.configSetting("RegnumLocal");
			ConfigSettings configSettingsB = new ConfigSettings();
			connStringHQ = configSettingsB.configSetting("RegnumHeadOffice");
		}
       
        public void SyncData()
        {

            try
            {
				GetConStings();


				LogWritter("Data Pulling from Regnum Local");
                var data = GetDataLocal();
				if (data.Rows.Count > 0)
				{
					LogWritter("Total  Count: " + data.Rows.Count);
				}

				BulkInsert(data, "temp_bridges");
				ProcessTempDataABX();
				LogWritter("Data Synced");

			}
            catch (Exception ex)
            {

                LogWritter("Data Pulling error:" + ex.Message);

            }

        }

        private DataTable GetDataLocal()
		{// adapter.Fill(dt);
			var dataTable = new DataTable();
            string query = $@"select b.id,b.full_name,b.bridge_name,b.location,b.status,b.created_by,b.updated_by,b.created_at,b.updated_at from bridges b ";
			using (OracleConnection connection = new OracleConnection(connStringLocal))
			{
				connection.Open();

				using (OracleCommand cmd = new OracleCommand(query, connection))
				{
					using (OracleDataReader reader = cmd.ExecuteReader())
					{
						if (reader.HasRows)
						{
							// Add columns to DataTable based on reader's schema
							for (int i = 0; i < reader.FieldCount; i++)
							{
								dataTable.Columns.Add(reader.GetName(i), reader.GetFieldType(i));
							}

							// Read data and add rows to DataTable
							while (reader.Read())
							{
								DataRow dataRow = dataTable.NewRow();

								for (int i = 0; i < reader.FieldCount; i++)
								{
									dataRow[i] = reader[i];
								}

								dataTable.Rows.Add(dataRow);
							}
						}
					}
				}
			}
			return dataTable;
        }
		private void ProcessTempDataABX()
		{
			string query = @"MERGE INTO bridges dest
	USING (
    SELECT 
        ID as id,
        full_name AS full_name,
        bridge_name AS bridge_name,
        location AS location,
        status AS status,
        created_by AS created_by,
        updated_by AS updated_by,
        created_at AS created_at,
        updated_at AS updated_at
    FROM temp_bridges
) src
ON (dest.id = src.id)
WHEN MATCHED THEN
    UPDATE SET
        dest.full_name = src.full_name,
        dest.bridge_name = src.bridge_name,
        dest.location = src.location,
        dest.status = src.status,
        dest.created_by = src.created_by,
        dest.updated_by = src.updated_by,
        dest.created_at = src.created_at,
        dest.updated_at = src.updated_at
WHEN NOT MATCHED THEN
    INSERT (
        full_name, 
        bridge_name,
        location,
        status,
        created_by,
        updated_by,
        created_at,
        updated_at
    ) VALUES (
		src.full_name,
        src.bridge_name,
        src.location,
        src.status,
        src.created_by,
        src.updated_by,
        src.created_at,
        src.updated_at
    )";

			var connection = new OracleConnection(connStringHQ);

			try
			{
				//LogWritter(string.Format("Total Records : {0} for {1} Table", "AbxCustomerDetails"));

				connection.Open();
				var comm = new OracleCommand();
				comm.Connection = connection;
				comm.CommandText = query;
				comm.CommandType = CommandType.Text;
				comm.ExecuteNonQuery();

			}
			catch (Exception ex)
			{

				throw ex;
			}
			finally
			{
				connection.Close();
			}

		}
		public void BulkInsert(DataTable dataTable, string tablname)
		{
			if (dataTable == null)
			{
				LogWritter(string.Format("Data Not found for {0} Table", tablname));

				return;
			}
			var connection = new OracleConnection(connStringHQ);
			try
			{
				LogWritter(string.Format("Total Records : {0} for {1} Table", dataTable.Rows.Count, tablname));

				connection.Open();
				var comm = new OracleCommand();
				comm.Connection = connection;
				comm.CommandText = "Delete from " + tablname;
				comm.CommandType = CommandType.Text;
				comm.ExecuteNonQuery();
				comm.CommandTimeout = 1000 * 60 * 5;

				using (var bulk = new OracleBulkCopy(connection))
				{
					bulk.DestinationTableName = tablname;
					bulk.BulkCopyTimeout = 300000;
					bulk.WriteToServer(dataTable);

				}

			}
			catch (Exception ex)
			{

				LogWritter(string.Format("{1} Table: {0}", ex.Message, tablname));

			}
			finally
			{
				connection.Close();
			}
		}
		
		public void LogWritter(string message)
        {

            string filename = DateTime.Now.ToString("yyyy_MM_dd") + "_serviceEquipment.log";
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
