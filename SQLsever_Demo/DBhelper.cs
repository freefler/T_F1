using System;
using System.Data;
using System.Data.SqlClient;
using System.Windows.Forms;

namespace Com_sql
{
    internal class DBhelper
    {
        public string connStr { get; set; }
        public DBhelper(string connectionStr)
        {
            connStr = connectionStr;
        }
        public bool TestConnection()
        {
            try
            {
                using (var con = new SqlConnection(connStr))
                {
                    con.Open();
                    return true;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("连接数据库失败: " + ex.Message);
                return false;
            }
        }

        public DataTable ConnectToData()
        {
            var dataTable = new DataTable();
            using (var con = new SqlConnection(connStr))
            using (var com = new SqlCommand("SELECT * FROM [SCADA_Demo].[dbo].[HistoryData]", con))
            {
                con.Open();
                using (var reader = com.ExecuteReader())
                {
                    dataTable.Load(reader);
                   
                }
            }
            return dataTable;
        }

        public int Inserdata(string tagName, double tagValue)
        {
            using (var con = new SqlConnection(connStr))
            using (var com = new SqlCommand("INSERT INTO [SCADA_Demo].[dbo].[HistoryData] " +
                "(TagName, TagValue, CollectTime) VALUES (@TagName, @TagValue, @CollectTime)", con))
            {
                con.Open();
                com.Parameters.AddWithValue("@TagName", tagName);
                com.Parameters.AddWithValue("@TagValue", tagValue);
                com.Parameters.AddWithValue("@CollectTime", DateTime.Now);
                return com.ExecuteNonQuery();
            }

        }

        public int UpdateData( int ID,string tagName, double tagValue)
        {
            using (var con = new SqlConnection(connStr))
            using (var com = new SqlCommand("UPDATE [SCADA_Demo].[dbo].[HistoryData] " +
                "SET TagValue = @TagValue, TagName=@TagName,CollectTime = @CollectTime WHERE Id = @Id", con))
            {
                con.Open();
                com.Parameters.AddWithValue("@Id", ID);
                com.Parameters.AddWithValue("@TagName", tagName);
                com.Parameters.AddWithValue("@TagValue", tagValue);
                com.Parameters.AddWithValue("@CollectTime", DateTime.Now);
                return com.ExecuteNonQuery();
            }
        }


        public int DeleteData(int ID)
        {
            using (var con = new SqlConnection(connStr))
            using (var com = new SqlCommand("DELETE FROM [SCADA_Demo].[dbo].[HistoryData] WHERE Id = @Id", con))
            {
                con.Open();
                com.Parameters.AddWithValue("@Id", ID);
                return com.ExecuteNonQuery();
            }
        }
    }
}
