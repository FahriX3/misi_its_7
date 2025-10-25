using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Drawing.Printing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;

namespace sql_projek
{
    public partial class FormLaporan : Form
    {
        public FormLaporan()
        {
            InitializeComponent();
        }

        private void TampilkanPenjualanPerHari(DateTime dari, DateTime sampai)
        {
            chartPenjualan.Series.Clear();
            chartPenjualan.ChartAreas.Clear();
            ChartArea area = new ChartArea("AreaUtama");
            chartPenjualan.ChartAreas.Add(area);
            Series series = new Series("Penjualan Per Hari");
            series.ChartType = SeriesChartType.Column;
            series.XValueType = ChartValueType.Date;
            chartPenjualan.Series.Add(series);

            decimal totalKeseluruhan = 0;

            using (SqlConnection conn = Koneksi.GetConnection())
            {
                conn.Open();
                string query = @"SELECT CAST(Tanggal AS DATE) AS Tgl, SUM(TotalHarga) AS Total
                         FROM Penjualan
                         WHERE Tanggal BETWEEN @Dari AND @Sampai
                         GROUP BY CAST(Tanggal AS DATE)
                         ORDER BY Tgl";
                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@Dari", dari);
                cmd.Parameters.AddWithValue("@Sampai", sampai);

                SqlDataReader reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    DateTime tanggal = Convert.ToDateTime(reader["Tgl"]);
                    decimal total = Convert.ToDecimal(reader["Total"]);
                    totalKeseluruhan += total;
                    series.Points.AddXY(tanggal.ToString("dd-MM"), total);
                }
                reader.Close();
            }

            chartPenjualan.Titles.Clear();
            chartPenjualan.Titles.Add($"Grafik Penjualan per Hari ({dari:dd/MM/yyyy} - {sampai:dd/MM/yyyy})");

            lblTotal.Text = $"Total Penjualan: Rp {totalKeseluruhan:N0}";
        }



        private void TampilkanPenjualanPerMinggu(DateTime dari, DateTime sampai)
        {
            chartPenjualan.Series.Clear();
            chartPenjualan.ChartAreas.Clear();
            ChartArea area = new ChartArea("AreaUtama");
            chartPenjualan.ChartAreas.Add(area);
            Series series = new Series("Penjualan Per Minggu");
            series.ChartType = SeriesChartType.Column;
            series.XValueType = ChartValueType.String;
            chartPenjualan.Series.Add(series);

            decimal totalKeseluruhan = 0;

            using (SqlConnection conn = Koneksi.GetConnection())
            {
                conn.Open();
                string query = @"
            SELECT 
                DATEPART(YEAR, Tanggal) AS Tahun,
                DATEPART(WEEK, Tanggal) AS MingguKe,
                SUM(TotalHarga) AS Total
            FROM Penjualan
            WHERE Tanggal BETWEEN @Dari AND @Sampai
            GROUP BY DATEPART(YEAR, Tanggal), DATEPART(WEEK, Tanggal)
            ORDER BY DATEPART(YEAR, Tanggal), DATEPART(WEEK, Tanggal)";
                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@Dari", dari);
                cmd.Parameters.AddWithValue("@Sampai", sampai);

                SqlDataReader reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    int tahun = Convert.ToInt32(reader["Tahun"]);
                    int minggu = Convert.ToInt32(reader["MingguKe"]);
                    decimal total = Convert.ToDecimal(reader["Total"]);
                    totalKeseluruhan += total;

                    string label = $"Minggu {minggu} ({tahun})";
                    series.Points.AddXY(label, total);
                }
                reader.Close();
            }

            chartPenjualan.Titles.Clear();
            chartPenjualan.Titles.Add($"Grafik Penjualan per Minggu ({dari:dd/MM} - {sampai:dd/MM})");

            lblTotal.Text = $"Total Penjualan: Rp {totalKeseluruhan:N0}";
        }


        private void TampilkanPenjualanPerBulan(DateTime dari, DateTime sampai)
        {
            chartPenjualan.Series.Clear();
            chartPenjualan.ChartAreas.Clear();
            ChartArea area = new ChartArea("AreaUtama");
            chartPenjualan.ChartAreas.Add(area);
            Series series = new Series("Penjualan Per Bulan");
            series.ChartType = SeriesChartType.Column;
            series.XValueType = ChartValueType.String;
            chartPenjualan.Series.Add(series);

            decimal totalKeseluruhan = 0;

            using (SqlConnection conn = Koneksi.GetConnection())
            {
                conn.Open();
                string query = @"
            SELECT 
                DATEPART(YEAR, Tanggal) AS Tahun,
                MONTH(Tanggal) AS Bulan,
                SUM(TotalHarga) AS Total
            FROM Penjualan
            WHERE Tanggal BETWEEN @Dari AND @Sampai
            GROUP BY DATEPART(YEAR, Tanggal), MONTH(Tanggal)
            ORDER BY DATEPART(YEAR, Tanggal), MONTH(Tanggal)";
                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@Dari", dari);
                cmd.Parameters.AddWithValue("@Sampai", sampai);

                SqlDataReader reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    int tahun = Convert.ToInt32(reader["Tahun"]);
                    int bulan = Convert.ToInt32(reader["Bulan"]);
                    decimal total = Convert.ToDecimal(reader["Total"]);
                    totalKeseluruhan += total;
                    string namaBulan = new DateTime(tahun, bulan, 1).ToString("MMMM");
                    string label = $"{namaBulan} {tahun}";
                    series.Points.AddXY(label, total);
                }
                reader.Close();
            }

            chartPenjualan.Titles.Clear();
            chartPenjualan.Titles.Add($"Grafik Penjualan per Bulan ({dari:dd/MM/yyyy} - {sampai:dd/MM/yyyy})");

            lblTotal.Text = $"Total Penjualan: Rp {totalKeseluruhan:N0}";
        }


        private void TampilkanPenjualanPerKategori(DateTime dari, DateTime sampai)
        {
            chartPenjualan.Series.Clear();
            chartPenjualan.ChartAreas.Clear();
            ChartArea area = new ChartArea("AreaKategori");
            chartPenjualan.ChartAreas.Add(area);
            Series series = new Series("Penjualan per Kategori");
            series.ChartType = SeriesChartType.Pie;
            chartPenjualan.Series.Add(series);

            using (SqlConnection conn = Koneksi.GetConnection())
            {
                conn.Open();
                string query = @"
            SELECT k.NamaKategori, SUM(pd.Subtotal) AS Total
            FROM PenjualanDetail pd
            JOIN Produk p ON p.Id = pd.ProdukId
            JOIN Kategori k ON k.Id = p.KategoriId
            JOIN Penjualan j ON j.Id = pd.PenjualanId
            WHERE j.Tanggal BETWEEN @Dari AND @Sampai
            GROUP BY k.NamaKategori";
                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@Dari", dari);
                cmd.Parameters.AddWithValue("@Sampai", sampai);

                SqlDataReader reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    string kategori = reader["NamaKategori"].ToString();
                    decimal total = Convert.ToDecimal(reader["Total"]);
                    series.Points.AddXY(kategori, total);
                }
                reader.Close();
            }

            chartPenjualan.Titles.Clear();
            chartPenjualan.Titles.Add($"Grafik Penjualan per Kategori ({dari:dd/MM/yyyy} - {sampai:dd/MM/yyyy})");
        }


        private void FormLaporan_Load(object sender, EventArgs e)
        {
            dtpDari.Value = DateTime.Now.AddMonths(-1);
            dtpSampai.Value = DateTime.Now;

            TampilkanPenjualanPerHari(dtpDari.Value, dtpSampai.Value);
        }


        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (cmbTipeLaporan.SelectedItem == null) return;

            string tipe = cmbTipeLaporan.SelectedItem.ToString();

            if (tipe == "Harian")
                TampilkanPenjualanPerHari(dtpDari.Value, dtpSampai.Value);
            else if (tipe == "Mingguan")
                TampilkanPenjualanPerMinggu(dtpDari.Value, dtpSampai.Value);
            else if (tipe == "Bulanan")
                TampilkanPenjualanPerBulan(dtpDari.Value, dtpSampai.Value);
            else
                TampilkanPenjualanPerKategori(dtpDari.Value, dtpSampai.Value);
        }

        private void chartPenjualan_Click(object sender, EventArgs e)
        {

        }

        private void printDocument1_PrintPage(object sender, PrintPageEventArgs e)
        {
            try
            {
                using (Bitmap bmp = new Bitmap(800, 500))
                {
                    chartPenjualan.DrawToBitmap(bmp, new Rectangle(0, 0, bmp.Width, bmp.Height));

                    if (chartPenjualan.Titles.Count > 0)
                    {
                        e.Graphics.DrawString(chartPenjualan.Titles[0].Text,
                            new Font("Arial", 16, FontStyle.Bold),
                            Brushes.Black, new PointF(50, 50));
                    }

                    e.Graphics.DrawImage(bmp, 50, 100, 700, 400);

                    if (lblTotal != null && !string.IsNullOrEmpty(lblTotal.Text))
                    {
                        e.Graphics.DrawString(lblTotal.Text,
                            new Font("Arial", 12, FontStyle.Regular),
                            Brushes.Black, new PointF(50, 520));
                    }
                }

                e.HasMorePages = false;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Gagal mencetak: " + ex.Message);
            }
        }


        private void lblTotal_Click(object sender, EventArgs e)
        {

        }

        private void btnPrint_Click(object sender, EventArgs e)
        {
            PrintDocument pd = new PrintDocument();
            pd.PrintPage += new PrintPageEventHandler(printDocument1_PrintPage);

            PrintPreviewDialog preview = new PrintPreviewDialog();
            preview.Document = pd;
            preview.ShowDialog();
        }

    }
}
