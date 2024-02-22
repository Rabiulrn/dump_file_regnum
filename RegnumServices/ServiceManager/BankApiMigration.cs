
using ApiFetcher.Models;
using Newtonsoft.Json;
using Oracle.ManagedDataAccess.Client;
using Oracle.ManagedDataAccess.Types;
using RegnumServices;
using System.ComponentModel;
using System.Data;
using System.Reflection.PortableExecutable;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace ApiFetcher
{
	public class BankApiMigration
	{
        static string connStringLocal = "";

        public void GetConStings()
        {
            ConfigSettings configSettingsA = new ConfigSettings();
            connStringLocal = configSettingsA.configSetting("RegnumLocal");
         
        }

        public async Task Sync(string urlFetch, string insertConString)
		{
			try
			{
				var datas = await Fetch(urlFetch);
				if (datas != null)
				{
					Insert(datas, "Temp_Registration_Vehicle");
				}
				//asdas();
			}
			catch (Exception E)
			{
				throw;

			}
		
			
		}

		private async Task<DataTable> Fetch(string urlFetch)
		{
            var dataTable = new DataTable();
			string apiUrl = urlFetch;

			using (HttpClient client = new HttpClient())
			{
				try
				{
					HttpResponseMessage response = await client.GetAsync(apiUrl);

					if (response.IsSuccessStatusCode)
					{
						var data = await response.Content.ReadAsStringAsync();
						if (data.Count()>0)
						{
                            var t=  JsonConvert.DeserializeObject<IList<RegistrationFetch>>(data);
                            dataTable =ToDataTable(t);

                        }
                        Console.WriteLine(data);
					}
					else
					{
						Console.WriteLine($"Error: {response.StatusCode} - {response.ReasonPhrase}");
					}
				}
				catch (HttpRequestException ex)
				{
					throw;
				}
			}
	
			if (dataTable.Rows.Count > 0)
			{
				LogWritter("Total  Count: " + dataTable.Rows.Count);
			}
          
            return dataTable;
		}
        private void ProcessTempData()
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

            var connection = new OracleConnection(connStringLocal);

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
        private void Insert(DataTable dataTable, string tablname) 
		{
			if (dataTable == null)
			{
				LogWritter(string.Format("Data Not found for {0} Table", tablname));

				return;
			}
			var connection = new OracleConnection(connStringLocal);
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
                throw;
            }
			finally
			{
				connection.Close();
			}
		}
		private void LogWritter(string message)
		{
			try
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
            catch (Exception EX)
			{

				throw;
			}
			
		}
        public DataTable ToDataTable<T>(IList<T> data)
        {
            PropertyDescriptorCollection props =
            TypeDescriptor.GetProperties(typeof(T));
            DataTable table = new DataTable();
            for (int i = 0; i < props.Count; i++)
            {
                PropertyDescriptor prop = props[i];
                table.Columns.Add(prop.Name, prop.PropertyType);
            }
            object[] values = new object[props.Count];
            foreach (T item in data)
            {
                for (int i = 0; i < values.Length; i++)
                {
                    values[i] = props[i].GetValue(item);
                }
                table.Rows.Add(values);
            }
            return table;
        }
  
    }
}
