using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using MySql.Data.MySqlClient;
using System.IO;

namespace donyadasproject
{
    public partial class Form7 : Form
    {
        private string _showphonecus;
        public string phoneform7
        {
            get { return _showphonecus; }
            set { _showphonecus = value; 
                label1.Text = value;
                LoadOrders();
            }
        }
        public Form7()
        {
            InitializeComponent();
        }
        private string connectionString = "server=localhost;user id=root;password=;database=dearcookie;";

        private void LoadOrders()
        {
            string query = @"SELECT order_code, phone_customer, sale_datetime, SUM(amount * price) * 1.07 AS total_with_vat, receipt_position FROM sales_history WHERE phone_customer = @phone_customer GROUP BY order_code, phone_customer, sale_datetime ORDER BY sale_datetime DESC";
            using (MySqlConnection conn = new MySqlConnection(connectionString))
            {
                using (MySqlCommand cmd = new MySqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@phone_customer", label1.Text); // หรือ textbox.Text ที่มีเบอร์ลูกค้า

                    using (MySqlDataAdapter adapter = new MySqlDataAdapter(cmd))
                    {
                        DataTable dt = new DataTable();
                        adapter.Fill(dt);
                        dataGridView1.DataSource = dt;

                        // ตั้งชื่อหัวตาราง
                        dataGridView1.Columns["order_code"].HeaderText = "รหัสออเดอร์";
                        dataGridView1.Columns["phone_customer"].HeaderText = "เบอร์ลูกค้า";
                        dataGridView1.Columns["sale_datetime"].HeaderText = "วันที่ขาย";
                        dataGridView1.Columns["total_with_vat"].HeaderText = "ราคารวม";
                        dataGridView1.Columns["total_with_vat"].DefaultCellStyle.Format = "N2";
                        dataGridView1.Columns["receipt_position"].Visible = false;

                        // คำนวณราคารวมทั้งหมด
                        CalculateTotalSales(dataGridView1, "total_with_vat", label3);

                        // เพิ่มปุ่ม PDF
                        AddPdfButtonColumn();
                    }
                }
            }
            }
        private void LoadOrderDetails(string orderCode)
        {
            string query = @"SELECT product_name, amount, price, total, (total * 0.07) AS vat, (total * 1.07) AS totalandvat FROM sales_history WHERE order_code = @code";

            MySqlConnection conn = new MySqlConnection(connectionString);
            MySqlCommand cmd = new MySqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@code", orderCode);
            MySqlDataAdapter adapter = new MySqlDataAdapter(cmd);
            DataTable dt = new DataTable();
            adapter.Fill(dt);
            dataGridView2.DataSource = dt;

            // ตั้งชื่อหัวตาราง
            dataGridView2.Columns["product_name"].HeaderText = "สินค้า";
            dataGridView2.Columns["amount"].HeaderText = "จำนวน";
            dataGridView2.Columns["price"].HeaderText = "ราคา";
            dataGridView2.Columns["total"].HeaderText = "ราคารวม";
            dataGridView2.Columns["vat"].HeaderText = "vat 7%";
            dataGridView2.Columns["totalandvat"].Visible = false;

            dataGridView2.Columns["vat"].DefaultCellStyle.Format = "N2";
            dataGridView2.Columns["price"].DefaultCellStyle.Format = "N2";
            dataGridView2.Columns["total"].DefaultCellStyle.Format = "N2";
            CalculateTotalSales(dataGridView2, "totalandvat", label2);
        }
        private void dataGridView1_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0)
            {
                string selectedOrderCode = dataGridView1.Rows[e.RowIndex].Cells["order_code"].Value.ToString();
                LoadOrderDetails(selectedOrderCode);
            }
            if (e.RowIndex >= 0 && dataGridView1.Columns[e.ColumnIndex].Name == "btnViewPDF")
            {
                string receiptPath = dataGridView1.Rows[e.RowIndex].Cells["receipt_position"].Value.ToString();

                if (File.Exists(receiptPath))
                {
                    System.Diagnostics.Process.Start(receiptPath); // เปิด PDF
                }
                else
                {
                    MessageBox.Show("ไม่พบไฟล์ใบเสร็จ", "ข้อผิดพลาด", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }

        }
        private void AddPdfButtonColumn()
        {
            if (!dataGridView1.Columns.Contains("btnViewPDF"))
            {
                DataGridViewButtonColumn btn = new DataGridViewButtonColumn();
                btn.Name = "btnViewPDF";
                btn.HeaderText = "ใบเสร็จ";
                btn.Text = "เรียกดูใบเสร็จ";
                btn.UseColumnTextForButtonValue = true;
                dataGridView1.Columns.Add(btn);
            }
        }
        private void CalculateTotalSales(DataGridView dgv, string columnName, Label showLabel)
        {
            decimal sum = 0;
            foreach (DataGridViewRow row in dgv.Rows)
            {
                if (!row.IsNewRow && row.Cells[columnName].Value != null)
                {
                    if (decimal.TryParse(row.Cells[columnName].Value.ToString(), out decimal value))
                    {
                        sum += value;
                    }
                }
            }
            showLabel.Text = $"{sum:N2} บาท";
        }

        private void backtomb_Click(object sender, EventArgs e)
        {
            Form6 newForm = new Form6();
            newForm.phoneform6 = label1.Text;
            newForm.Show();

            this.Close();
        }

        private void Form7_Load(object sender, EventArgs e)
        {
            LoadOrders();
        }

        private void pictureBox1_Click(object sender, EventArgs e)
        {

        }
    }
}
