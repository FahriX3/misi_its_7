using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Data.SqlClient; 

namespace sql_projek
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            using (SqlConnection sqlConnection = Koneksi.GetConnection())
            {
                sqlConnection.Open();

                SqlCommand cmd = new SqlCommand("SELECT * FROM Kategori", sqlConnection);
                SqlDataReader reader = cmd.ExecuteReader();

                if (reader.HasRows)
                {
                    cmbKategori.Items.Clear();
                    while (reader.Read())
                    {
                        cmbKategori.Items.Add(reader["NamaKategori"].ToString());
                    }
                }

                reader.Close();
            }
        }

        private void cmbKategori_SelectedIndexChanged(object sender, EventArgs e)
        {
            MessageBox.Show("Anda memilih kategori: " +
            cmbKategori.SelectedItem.ToString());
        }

        private void label1_Click(object sender, EventArgs e)
        {

        }
    }
}
