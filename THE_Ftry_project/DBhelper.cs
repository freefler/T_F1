using System.Data;
using System.Data.SqlClient;

namespace THE_Ftry_project
{
    internal class DBhelper
    {
        static string select_ALL = "select * from dbo.HistoryData";
        SqlConnection con = new SqlConnection();
        private string connectionString = "Data Source=DESKTOP-MD6I2MI\\SQLEXPRESS;" +
                "Initial Catalog=SCADA_Demo;Integrated Security=True;TrustServerCertificate=True";
        public void OpenSqlConnection()
        {
            using (con=new SqlConnection(connectionString))
                con.Open();
        }
        public DataTable Method_Read()
        {
            var dt = new DataTable();
            OpenSqlConnection();
            using (SqlCommand com = new SqlCommand(select_ALL, con))
            {
                using (var reader = com.ExecuteReader())
                { dt.Load(reader); }
            }
            
                

            ///防止注入command.Parameters.AddWithValue("",n);
            //对sql直接操作command.ExecuteNonQuery();
            //对内存操作command.ExecuteReader();
            return dt;



        }
        



    }
}
