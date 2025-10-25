using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.SqlClient;

namespace sql_projek
{
    internal class Koneksi
    {
        public static string connectionString = @"Data Source=RPL2-C2\SQLEXPRESS;Initial Catalog=dbtoko;Integrated Security=True;Encrypt=False;";
        //public static string connectionString = @"Data Source=DESKTOP-3GOI6IH\SQLEXPRESS;Initial Catalog=TokoDB;Integrated Security=True;Encrypt=False;";

        public static SqlConnection GetConnection()
        {
            return new SqlConnection(connectionString);
        }
    }
}
