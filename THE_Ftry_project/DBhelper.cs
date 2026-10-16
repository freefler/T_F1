using System;
using System.Data;
using System.Data.SqlClient;


namespace THE_Ftry_project
{
    internal class DBhelper
    {
        private const string connectionString =
            "Data Source=DESKTOP-MD6I2MI\\SQLEXPRESS;" +
                "Initial Catalog=SCADA_Demo;" +
            "Integrated Security=True;TrustServerCertificate=True";
        public static DataTable QueryHistorty(int N)
        {
            var dt = new DataTable();
            using (var con = new SqlConnection(connectionString))
            using (var cmd = new SqlCommand
                ($"SELECT TOP {N} * FROM [SCADA_Demo].[dbo].[HistoryData] ", con))
            {
                con.Open();
                using (var reader = cmd.ExecuteReader())
                {
                    dt.Load(reader);
                }
            }

            return dt;
        }

        public static int InsertHistoryData(string tagName, int tagvalue, DateTime CollectTime)
        {
            using (var con = new SqlConnection(connectionString))
            using (var cmd = new SqlCommand
                ("INSERT INTO [SCADA_Demo].[dbo].[HistoryData] ( tagName, tagvalue, CollectTime) VALUES (@Id, @Value, @Timestamp)", con))
            {
                cmd.Parameters.AddWithValue("@TagName", tagName);
                cmd.Parameters.AddWithValue("@TagValue", tagvalue);
                cmd.Parameters.AddWithValue("@CollectTime", CollectTime);
                con.Open();
                return cmd.ExecuteNonQuery();
            }
        }

       

        ///防止注入command.Parameters.AddWithValue("",n);
        //对sql直接操作command.ExecuteNonQuery();
        //对内存操作command.ExecuteReader();









    }
}
