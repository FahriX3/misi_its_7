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
    public partial class FormTransaksiPenjualan : Form
    {
        public FormTransaksiPenjualan()
        {
            InitializeComponent();
        }

        private void HitungTotal()
        {
            decimal total = 0;
            foreach (DataGridViewRow row in dgvItem.Rows)
            {
                total += Convert.ToDecimal(row.Cells["Subtotal"].Value);
            }
            lblTotal.Text = $"Total: Rp {total:N0}";
        }

        private decimal GetHargaProduk(int produkId)
        {
            using (SqlConnection conn = Koneksi.GetConnection())
            {
                conn.Open();
                SqlCommand cmd = new SqlCommand("SELECT Harga FROM Produk WHERE Id =@id", conn);
                cmd.Parameters.AddWithValue("@id", produkId);
                return (decimal)cmd.ExecuteScalar();
            }
        }

        private void FormTransaksiPenjualan_Load(object sender, EventArgs e)
        {
            using (SqlConnection conn = Koneksi.GetConnection())
            {
                conn.Open();
                SqlCommand cmd = new SqlCommand("SELECT Id, NamaProduk FROM Produk",
                conn);
                SqlDataReader reader = cmd.ExecuteReader();
                Dictionary<int, string> produkDict = new Dictionary<int, string>();
                while (reader.Read())
                {
                    produkDict.Add((int)reader["Id"],
                    reader["NamaProduk"].ToString());
                }
                cmbProduk.DataSource = new BindingSource(produkDict, null);
                cmbProduk.DisplayMember = "Value";
                cmbProduk.ValueMember = "Key";
                cmbProduk.AutoCompleteMode = AutoCompleteMode.SuggestAppend;
                cmbProduk.AutoCompleteSource = AutoCompleteSource.ListItems;
                cmbProduk.DropDownStyle = ComboBoxStyle.DropDown;
            }
            // Setup kolom dgvItem
            dgvItem.Columns.Add("ProdukId", "ProdukId");
            dgvItem.Columns["ProdukId"].Visible = false;
            dgvItem.Columns.Add("NamaProduk", "Nama Produk");
            dgvItem.Columns.Add("Harga", "Harga");
            dgvItem.Columns.Add("Jumlah", "Jumlah");
            dgvItem.Columns.Add("Subtotal", "Subtotal");

            DataGridViewButtonColumn btnDelete = new DataGridViewButtonColumn();
            btnDelete.HeaderText = "Aksi";
            btnDelete.Text = "Hapus";
            btnDelete.Name = "btnDelete";
            btnDelete.UseColumnTextForButtonValue = true;
            dgvItem.Columns.Add(btnDelete);

            dgvItem.CellContentClick += dgvItem_CellContentClick;

        }

        private void dgvItem_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.ColumnIndex == dgvItem.Columns["btnDelete"].Index && e.RowIndex >= 0)
            {
                dgvItem.Rows.RemoveAt(e.RowIndex);
                HitungTotal();
            }
        }


        private void btnTambah_Click(object sender, EventArgs e)
        {
            if (cmbProduk.SelectedItem == null || !int.TryParse(txtJumlah.Text, out
            int jumlah) || jumlah <= 0)
            {
                MessageBox.Show("Pilih produk dan jumlah valid.");
                return;
            }
            var selected = (KeyValuePair<int, string>)cmbProduk.SelectedItem;
            int produkId = selected.Key;
            string namaProduk = selected.Value;
            decimal harga = GetHargaProduk(produkId);
            decimal subtotal = harga * jumlah;
            dgvItem.Rows.Add(produkId, namaProduk, harga, jumlah, subtotal);
            HitungTotal();
        }

        private void btnSimpan_Click(object sender, EventArgs e)
        {
            if (dgvItem.Rows.Count == 0)
            {
                MessageBox.Show("Belum ada item ditambahkan.");
                return;
            }

            using (SqlConnection conn = Koneksi.GetConnection())
            {
                conn.Open();
                SqlTransaction trx = conn.BeginTransaction();

                try
                {
                    decimal total = dgvItem.Rows.Cast<DataGridViewRow>()
                        .Sum(r => Convert.ToDecimal(r.Cells["Subtotal"].Value));

                    SqlCommand cmdPenjualan = new SqlCommand(
                        "INSERT INTO Penjualan (Tanggal, TotalHarga) VALUES (@tgl,@total); SELECT SCOPE_IDENTITY();",
                        conn, trx);
                    cmdPenjualan.Parameters.AddWithValue("@tgl", DateTime.Now);
                    cmdPenjualan.Parameters.AddWithValue("@total", total);
                    int penjualanId = Convert.ToInt32(cmdPenjualan.ExecuteScalar());

                    foreach (DataGridViewRow row in dgvItem.Rows)
                    {
                        int produkId = Convert.ToInt32(row.Cells["ProdukId"].Value);
                        int jumlah = Convert.ToInt32(row.Cells["Jumlah"].Value);
                        decimal subtotal = Convert.ToDecimal(row.Cells["Subtotal"].Value);

                        SqlCommand cmdCekStok = new SqlCommand(
                            "SELECT Stok FROM Produk WHERE Id = @id", conn, trx);
                        cmdCekStok.Parameters.AddWithValue("@id", produkId);
                        int stokSekarang = Convert.ToInt32(cmdCekStok.ExecuteScalar());

                        if (stokSekarang < jumlah)
                        {
                            MessageBox.Show($"Stok produk tidak mencukupi untuk {row.Cells["NamaProduk"].Value}! (Stok: {stokSekarang})");
                            trx.Rollback();
                            return;
                        }

                        SqlCommand cmdDetail = new SqlCommand(
                            @"INSERT INTO PenjualanDetail (PenjualanId, ProdukId, Jumlah, Subtotal)
                      VALUES (@pjId, @prodId, @jumlah, @subtotal)",
                            conn, trx);
                        cmdDetail.Parameters.AddWithValue("@pjId", penjualanId);
                        cmdDetail.Parameters.AddWithValue("@prodId", produkId);
                        cmdDetail.Parameters.AddWithValue("@jumlah", jumlah);
                        cmdDetail.Parameters.AddWithValue("@subtotal", subtotal);
                        cmdDetail.ExecuteNonQuery();

                        SqlCommand cmdUpdateStok = new SqlCommand(
                            "UPDATE Produk SET Stok = Stok - @jumlah WHERE Id = @id",
                            conn, trx);
                        cmdUpdateStok.Parameters.AddWithValue("@jumlah", jumlah);
                        cmdUpdateStok.Parameters.AddWithValue("@id", produkId);
                        cmdUpdateStok.ExecuteNonQuery();

                    }



                    trx.Commit();
                    MessageBox.Show("Transaksi berhasil disimpan dan stok diperbarui!");
                    dgvItem.Rows.Clear();
                    HitungTotal();
                }
                catch (Exception ex)
                {
                    trx.Rollback();
                    MessageBox.Show("Gagal menyimpan transaksi: " + ex.Message);
                }
            }
        }

    }
}
