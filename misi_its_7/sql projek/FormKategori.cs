using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Design;
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
    public partial class FormKategori : Form
    {
        private void LoadDataKategori()
        {
            dgvKategori.Rows.Clear();
            dgvKategori.Columns.Clear();
            using (SqlConnection conn = Koneksi.GetConnection())
            {
                conn.Open();
                string query = "SELECT Id, NamaKategori FROM Kategori";
                SqlCommand cmd = new SqlCommand(query, conn);
                SqlDataReader reader = cmd.ExecuteReader();
                dgvKategori.Columns.Add("Id", "ID");
                dgvKategori.Columns.Add("NamaKategori", "Nama Kategori");
                while (reader.Read())
                {
                    dgvKategori.Rows.Add(reader["Id"], reader["NamaKategori"]);
                }
                reader.Close();
            }

            btnEdit.Enabled = false;
            btnHapus.Enabled = false;
        }

        public FormKategori()
        {
            InitializeComponent();
        }



        private void FormKategori_Load(object sender, EventArgs e)
        {
            LoadDataKategori();
            dgvKategori.ClearSelection();

            btnEdit.Enabled = false;
            btnHapus.Enabled = false;
        }

        private void btnTambah_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtNamaKategori.Text))
            {
                MessageBox.Show("Nama kategori tidak boleh kosong.");
                return;
            } else if (txtNamaKategori.Text.Length < 3)
            {
                MessageBox.Show("Nama kategori minimal 3 karakter.");
                return;
            }
                using (SqlConnection conn = Koneksi.GetConnection())
                {
                    conn.Open();
                    string query = "INSERT INTO Kategori (NamaKategori) VALUES (@nama)";
                    SqlCommand cmd = new SqlCommand(query, conn);
                    cmd.Parameters.AddWithValue("@nama", txtNamaKategori.Text.Trim());
                    cmd.ExecuteNonQuery();
                    MessageBox.Show("Kategori berhasil ditambahkan!");
                    txtNamaKategori.Clear();
                    LoadDataKategori();
                }
        }

        private void btnEdit_Click(object sender, EventArgs e)
        {
            if (dgvKategori.SelectedRows.Count == 0)
            {
                MessageBox.Show("Pilih kategori terlebih dahulu.");
                return;
            }
            int id = Convert.ToInt32(dgvKategori.SelectedRows[0].Cells["Id"].Value);
            string nama = txtNamaKategori.Text.Trim();
            if (string.IsNullOrWhiteSpace(nama))
            {
                MessageBox.Show("Nama kategori tidak boleh kosong.");
                return;
            } else if (txtNamaKategori.Text.Length < 3)
            {
                MessageBox.Show("Nama kategori minimal 3 karakter.");
                return;
            }
            using (SqlConnection conn = Koneksi.GetConnection())
            {
                conn.Open();
                string query = "UPDATE Kategori SET NamaKategori = @nama WHERE Id = @id";
            SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@nama", nama);
                cmd.Parameters.AddWithValue("@id", id);
                cmd.ExecuteNonQuery();
                MessageBox.Show("Kategori berhasil diubah.");
                txtNamaKategori.Clear();
                LoadDataKategori();
            }
        }

        private void btnHapus_Click(object sender, EventArgs e)
        {
            if (dgvKategori.SelectedRows.Count == 0)
            {
                MessageBox.Show("Pilih kategori yang ingin dihapus.");
                return;
            }
            int id = Convert.ToInt32(dgvKategori.SelectedRows[0].Cells["Id"].Value);
            DialogResult confirm = MessageBox.Show(
            "Yakin ingin menghapus kategori ini?",
            "Konfirmasi",
            MessageBoxButtons.YesNo,
            MessageBoxIcon.Warning
            );
            if (confirm == DialogResult.Yes)
            {
                using (SqlConnection conn = Koneksi.GetConnection())
                {
                    conn.Open();
                    string query = "DELETE FROM Kategori WHERE Id = @id";
                    SqlCommand cmd = new SqlCommand(query, conn);
                    cmd.Parameters.AddWithValue("@id", id);
                    cmd.ExecuteNonQuery();
                    MessageBox.Show("Kategori berhasil dihapus.");
                    LoadDataKategori();
                }
            }
        }

        private void dgvKategori_SelectionChanged(object sender, EventArgs e)
        {
            if (dgvKategori.SelectedRows.Count == 0 || dgvKategori.Columns.Count == 0)
            {
                btnEdit.Enabled = false;
                btnHapus.Enabled = false;
                txtNamaKategori.Clear();
                dgvProdukTerkait.Rows.Clear();
                dgvProdukTerkait.Columns.Clear();
                label_jumlahProduk.Text = "Jumlah Produk: 0";
                return;
            }

            var selectedRow = dgvKategori.SelectedRows[0];
            if (selectedRow.Cells["Id"].Value == null || selectedRow.Cells["NamaKategori"].Value == null)
            {
                btnEdit.Enabled = false;
                btnHapus.Enabled = false;
                txtNamaKategori.Clear();
                return;
            }


            btnEdit.Enabled = true;
            btnHapus.Enabled = true;


            if (dgvKategori.SelectedRows.Count == 0) return;
            int kategoriId =
            Convert.ToInt32(dgvKategori.SelectedRows[0].Cells["Id"].Value);
            txtNamaKategori.Text =
            dgvKategori.SelectedRows[0].Cells["NamaKategori"].Value.ToString();
            dgvProdukTerkait.Rows.Clear();
            dgvProdukTerkait.Columns.Clear();
            using (SqlConnection conn = Koneksi.GetConnection())
            {
                conn.Open();
                string query = "SELECT NamaProduk, Harga, Stok, Deskripsi FROM Produk WHERE KategoriId = @kategoriId";
                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@kategoriId", kategoriId);
                SqlDataReader reader = cmd.ExecuteReader();

                dgvProdukTerkait.Columns.Add("NamaProduk", "Nama Produk");
                dgvProdukTerkait.Columns.Add("Harga", "Harga");
                dgvProdukTerkait.Columns.Add("Stok", "Stok");
                dgvProdukTerkait.Columns.Add("Deskripsi", "Deskripsi");
                while (reader.Read())
            {
                string hargaFormatted = "";
                if (decimal.TryParse(reader["Harga"].ToString(), out decimal harga))
                {
                    hargaFormatted = $"Rp. {harga:N0}";
                }
                else
                {
                    hargaFormatted = "Rp. 0";
                }
                
                dgvProdukTerkait.Rows.Add(
                    reader["NamaProduk"],
                    hargaFormatted,
                    reader["Stok"],
                    reader["Deskripsi"]
                );
            }
                reader.Close();
                string countQuery = "SELECT COUNT(*) FROM Produk WHERE KategoriId = @kategoriId";
                SqlCommand countCmd = new SqlCommand(countQuery, conn);
                countCmd.Parameters.AddWithValue("@kategoriId", kategoriId);
                int jumlahProduk = (int)countCmd.ExecuteScalar();

                label_jumlahProduk.Text = $"Jumlah Produk: {jumlahProduk}";
            }
        }

    }
}

