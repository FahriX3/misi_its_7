using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

namespace sql_projek
{
    public partial class FormProdukDetail : Form
    {
        public int? ProdukId { get; set; } = null;

        private void LoadDataProduk()
        {
            if (ProdukId == null) return;
            using (SqlConnection conn = Koneksi.GetConnection())
            {
                try
                {
                    conn.Open();
                    string query = @"SELECT NamaProduk, Harga, Stok, KategoriId, Deskripsi
                    FROM Produk WHERE Id = @id";
                    SqlCommand cmd = new SqlCommand(query, conn);
                    cmd.Parameters.AddWithValue("@id", ProdukId);
                    SqlDataReader reader = cmd.ExecuteReader();
                    if (reader.Read())
                    {
                        txtNamaProduk.Text = reader["NamaProduk"].ToString();
                        txtHarga.Text = reader["Harga"].ToString();
                        txtStok.Text = reader["Stok"].ToString();
                        txtDeskripsi.Text = reader["Deskripsi"].ToString();
                        cmbKategori.SelectedValue =
                        Convert.ToInt32(reader["KategoriId"]);
                    }
                    reader.Close();
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Gagal memuat data produk: " + ex.Message);
                }
            }
        }

        public FormProdukDetail()
        {
            InitializeComponent();
        }

        private void label4_Click(object sender, EventArgs e)
        {

        }

        private void FormProdukDetail_Load(object sender, EventArgs e)
        {
            using (SqlConnection conn = Koneksi.GetConnection())
            {
                try
                {
                    conn.Open();
                    string query = "SELECT Id, NamaKategori FROM Kategori";
                    SqlCommand cmd = new SqlCommand(query, conn);
                    SqlDataReader reader = cmd.ExecuteReader();
                    Dictionary<int, string> kategoriDict = new Dictionary<int,
                    string>();
                    while (reader.Read())
                    {
                        kategoriDict.Add((int)reader["Id"],
                        reader["NamaKategori"].ToString());
                    }

                    cmbKategori.DataSource = new BindingSource(kategoriDict, null);
                    cmbKategori.DisplayMember = "Value";
                    cmbKategori.ValueMember = "Key";
                    reader.Close();
                    }
                catch (Exception ex)
                {
                    MessageBox.Show("Gagal memuat kategori: " + ex.Message);
                }
            }

            if (ProdukId.HasValue)
            {
                LoadDataProduk();
                this.Text = "Edit Produk";
            }
            else
            {
                this.Text = "Tambah Produk";
            }
        }

        private void btnSimpan_Click(object sender, EventArgs e)
        {
            using (SqlConnection conn = Koneksi.GetConnection())
            {
                try
                {
                    if (string.IsNullOrWhiteSpace(txtNamaProduk.Text))
                    {
                        MessageBox.Show("Nama produk tidak boleh kosong!");
                        return;
                    }

                    if (!decimal.TryParse(txtHarga.Text, out decimal harga))
                    {
                        MessageBox.Show("Harga harus berupa angka!");
                        return;
                    }

                    if (!int.TryParse(txtStok.Text, out int stok))
                    {
                        MessageBox.Show("Stok harus berupa angka!");
                        return;
                    }

                    if (cmbKategori.SelectedItem == null)
                    {
                        MessageBox.Show("Pilih kategori terlebih dahulu!");
                        return;
                    }

                    conn.Open();

                    string query;

                    if (ProdukId.HasValue)
                    {
                        query = @"UPDATE Produk
                          SET NamaProduk = @nama, Harga = @harga, Stok = @stok, KategoriId = @kategori, Deskripsi = @deskripsi
                          WHERE Id = @id";
                    }
                    else
                    {
                        query = @"INSERT INTO Produk (NamaProduk, Harga, Stok, KategoriId, Deskripsi)
                          VALUES (@nama, @harga, @stok, @kategori, @deskripsi)";
                    }

                    SqlCommand cmd = new SqlCommand(query, conn);
                    cmd.Parameters.AddWithValue("@nama", txtNamaProduk.Text);
                    cmd.Parameters.AddWithValue("@harga", harga);
                    cmd.Parameters.AddWithValue("@stok", stok);
                    cmd.Parameters.AddWithValue("@kategori", ((KeyValuePair<int, string>)cmbKategori.SelectedItem).Key);
                    cmd.Parameters.AddWithValue("@deskripsi", txtDeskripsi.Text);

                    if (ProdukId.HasValue)
                    {
                        cmd.Parameters.AddWithValue("@id", ProdukId);
                    }

                    cmd.ExecuteNonQuery();

                    if (ProdukId.HasValue)
                        MessageBox.Show("Produk berhasil diperbarui!");
                    else
                        MessageBox.Show("Produk berhasil ditambahkan!");

                    DialogResult = DialogResult.OK;
                    Close();
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Gagal menyimpan produk: " + ex.Message);
                }
            }
        }


        private void btnBatal_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void label5_Click(object sender, EventArgs e)
        {

        }

        private void txtDeskripsi_TextChanged(object sender, EventArgs e)
        {

        }
    }
}
