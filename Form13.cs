using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using iTextSharp.text;
using MySql.Data.MySqlClient;
using Org.BouncyCastle.Asn1.Cmp;
using System.IO;

namespace donyadasproject
{
    public partial class Form13 : Form
    {
        private string _shownamestaff;

        public string namestaff13
        {
            get { return _shownamestaff; }
            set { _shownamestaff = value; label3.Text = value; } 
        }
        public Form13()
        {
            InitializeComponent();
        }

       
        private void Form13_Load(object sender, EventArgs e)
        {
           
            
            yearly.Items.Add("");

           
            for (int year = DateTime.Now.Year; year >= DateTime.Now.Year - 10; year--)
                yearly.Items.Add(year.ToString());  

            
            mothly.Items.Add("");

           
            for (int month = 1; month <= 12; month++)
                mothly.Items.Add(month.ToString());  
            
            daily.Items.Add("");

           
            for (int day = 1; day <= 31; day++)
                daily.Items.Add(day.ToString()); 

           
            yearly.SelectedIndex = 0;
            mothly.SelectedIndex = 0;
            daily.SelectedIndex = 0;

           
            LoadOrders();
        }

        private string connectionString = "server=localhost;user id=root;password=;database=dearcookie;";

        private void LoadOrders()
        {
            MySqlConnection conn = new MySqlConnection(connectionString);

           
            string query = @"SELECT order_code, phone_customer, sale_datetime, 
                            SUM(amount * price) * 1.07 AS total_with_vat, 
                            receipt_position 
                     FROM sales_history 
                     GROUP BY order_code, phone_customer, sale_datetime 
                     ORDER BY sale_datetime DESC";

            MySqlDataAdapter adapter = new MySqlDataAdapter(query, conn);
            DataTable dt = new DataTable(); 
            adapter.Fill(dt); 
            dataGridView1.DataSource = dt; 

            // ตั้งชื่อหัวตาราง
            dataGridView1.Columns["order_code"].HeaderText = "รหัสออเดอร์";
            dataGridView1.Columns["phone_customer"].HeaderText = "เบอร์ลูกค้า";
            dataGridView1.Columns["sale_datetime"].HeaderText = "วันที่ขาย";
            dataGridView1.Columns["sale_datetime"].DefaultCellStyle.Format = "dd/MM/yyyy"; 
            dataGridView1.Columns["sale_datetime"].DefaultCellStyle.FormatProvider = System.Globalization.CultureInfo.InvariantCulture;
            dataGridView1.Columns["total_with_vat"].HeaderText = "ราคารวม";
            dataGridView1.Columns["total_with_vat"].DefaultCellStyle.Format = "N2"; 
            dataGridView1.Columns["receipt_position"].Visible = false;

            CalculateTotalSales(dataGridView1, "total_with_vat", label1);

            AddPdfButtonColumn(); 

        }

        private void dataGridView2_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {

        }
       
        private void LoadOrderDetails(string orderCode)
        {
            
            string query = @"SELECT product_name, amount, price, total, 
                     (total * 0.07) AS vat, 
                     (total * 1.07) AS totalandvat 
                     FROM sales_history 
                     WHERE order_code = @code";

            
            MySqlConnection conn = new MySqlConnection(connectionString);

          
            MySqlCommand cmd = new MySqlCommand(query, conn);

           
            cmd.Parameters.AddWithValue("@code", orderCode);

           
            MySqlDataAdapter adapter = new MySqlDataAdapter(cmd);

          
            DataTable dt = new DataTable();

           
            adapter.Fill(dt);

            
            dataGridView2.DataSource = dt;

          
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



        private void backtostaff_Click(object sender, EventArgs e)
        {
            Form11 newForm = new Form11
            {
                namestaff11 = label3.Text
            };
            newForm.Show();

            this.Hide();
        }

        private void daily_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        
        private void bttnsearch_Click(object sender, EventArgs e)
        {
          
            string selectedDay = daily.SelectedItem?.ToString();

            
            string selectedMonth = mothly.SelectedItem?.ToString();

           
            string selectedYear = yearly.SelectedItem?.ToString();

            
            if (string.IsNullOrEmpty(selectedYear))
            {
              
                MessageBox.Show("กรุณาเลือกปี");

               
                return;
            }

            



        List<string> conditions = new List<string>(); 
            if (!string.IsNullOrEmpty(selectedYear))
                conditions.Add("YEAR(sale_datetime) = @year");
            if (!string.IsNullOrEmpty(selectedMonth))
                conditions.Add("MONTH(sale_datetime) = @month");


            if (!string.IsNullOrEmpty(selectedDay))
                conditions.Add("DAY(sale_datetime) = @day");

            string whereClause = string.Join(" AND ", conditions);

            

            string query = $@"
   SELECT 
    order_code,                          
    phone_customer,                     
    sale_datetime,                      
    SUM(amount * price) * 1.07 AS total_with_vat, // ยอดรวม (จำนวน * ราคา) คูณด้วย 1.07 เพื่อรวม VAT
    receipt_position                     
FROM sales_history 
WHERE {whereClause}                     
GROUP BY order_code, phone_customer, sale_datetime, receipt_position // รวมข้อมูลตามกลุ่มนี้
ORDER BY sale_datetime DESC";           

            
            using (MySqlConnection conn = new MySqlConnection(connectionString))
            {
                try
                {
                    conn.Open(); 

                    
                    MySqlCommand cmd = new MySqlCommand(query, conn);

                    
                    cmd.Parameters.AddWithValue("@year", selectedYear);

                    
                    if (!string.IsNullOrEmpty(selectedMonth))
                        cmd.Parameters.AddWithValue("@month", selectedMonth);

                    
                    if (!string.IsNullOrEmpty(selectedDay))
                        cmd.Parameters.AddWithValue("@day", selectedDay);

                    
                    MySqlDataAdapter adapter = new MySqlDataAdapter(cmd);

                   
                    DataTable dt = new DataTable();
                    adapter.Fill(dt); 

                    
                    dataGridView1.DataSource = dt;

                    
                    CalculateTotalSales(dataGridView1, "total_with_vat", label1);
                }
                catch (Exception ex) 
                {
                    MessageBox.Show("เกิดข้อผิดพลาด: " + ex.Message); 

                }
            }
        }

        private void dataGridView1_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {

        }

        private void pictureBox1_Click(object sender, EventArgs e)
        {

        }


    }
}