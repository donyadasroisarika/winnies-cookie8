using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml.Linq;
using MySql.Data.MySqlClient;

using MySql.Data.MySqlClient; // เพิ่มการใช้ MySQL

namespace donyadasproject
{
    public partial class Form14 : Form
    {

        private string connectionString = "Server=127.0.0.1;Database=dearcookie;Uid=root;Pwd=;";

        private string _shownamestaff;
        public string namestaff14
        {
            get { return _shownamestaff; }
            set { _shownamestaff = value; label3.Text = value; }
        }
        public Form14()
        {
            InitializeComponent();


        }


        private void LoadCookieData()
        {

            using (MySqlConnection conn = new MySqlConnection(connectionString))
            {
                try
                {
                    conn.Open(); // เปิดการเชื่อมต่อฐานข้อมูล
                    string query = "SELECT id, pic_cookie, name_cookie, amount, price FROM menu_cookie";
                    MySqlDataAdapter adapter = new MySqlDataAdapter(query, conn);
                    DataTable dt = new DataTable();
                    adapter.Fill(dt);

                    datarestcok.DataSource = dt;


                    datarestcok.Columns["id"].Visible = false;
                    datarestcok.Columns["pic_cookie"].Visible = false;
                    datarestcok.Columns["amount"].Width = 50; 
                    datarestcok.Columns["amount"].DefaultCellStyle.Format = "N0"; 
                    datarestcok.Columns["price"].DefaultCellStyle.Format = "N2";
                    conn.Close();
                }
                catch (Exception ex)
                {
                    MessageBox.Show("เชื่อมต่อฐานข้อมูลไม่ได้: " + ex.Message);
                }
            }
        }

        private bool isImageFromFile = false;

        private void dataGridView1_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            // เช็คว่าไม่ได้คลิกแถว header
            if (e.RowIndex >= 0)
            {
                DataGridViewRow row = datarestcok.Rows[e.RowIndex];
                label_id.Text = row.Cells["id"].Value.ToString();
                namepd.Text = row.Cells["name_cookie"].Value.ToString();

                decimal amount = Convert.ToDecimal(row.Cells["amount"].Value);
                amountpd.Text = amount.ToString("N0");

                decimal price = Convert.ToDecimal(row.Cells["price"].Value);
                pricepd.Text = price.ToString("N2");

                if (row.Cells["pic_cookie"].Value != DBNull.Value)
                {
                    try
                    {
                        byte[] imageBytes = (byte[])row.Cells["pic_cookie"].Value; // แปลงข้อมูลภาพเป็น byte[]
                        using (MemoryStream ms = new MemoryStream(imageBytes))
                        {
                            picboxrestock.Image = Image.FromStream(ms); // แสดงภาพ
                            picboxrestock.SizeMode = PictureBoxSizeMode.Zoom; // ปรับขนาดภาพอัตโนมัติ
                            isImageFromFile = false; // รูปภาพมาจากฐานข้อมูล
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("ไม่สามารถแปลงรูปภาพ: " + ex.Message);
                        picboxrestock.Image = null;
                    }
                }
                else
                {
                    picboxrestock.Image = null; // ถ้าไม่มีภาพ
                }
            }
        }

        private void bttn_delst_Click(object sender, EventArgs e) //ปุ่มลบรายการในสต็อก
        {
            // เช็คว่ามีการเลือกรายการก่อนลบหรือไม่
            if (string.IsNullOrEmpty(label_id.Text))
            {
                MessageBox.Show("กรุณาเลือกสินค้าที่ต้องการลบ");
                return;
            }

            // แสดงกล่องยืนยันก่อนลบ
            DialogResult result = MessageBox.Show("คุณต้องการลบสินค้านี้ใช่หรือไม่?", "ยืนยันการลบ", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
            if (result == DialogResult.Yes)
            {
                try
                {
                    using (MySqlConnection conn = new MySqlConnection(connectionString))
                    {
                        conn.Open();
                        string sql = "DELETE FROM menu_cookie WHERE id = @id";

                        using (MySqlCommand cmd = new MySqlCommand(sql, conn))
                        {
                            cmd.Parameters.AddWithValue("@id", label_id.Text);
                            int rowsAffected = cmd.ExecuteNonQuery();

                            if (rowsAffected > 0)
                            {
                                MessageBox.Show("ลบสินค้าสำเร็จ");
                                LoadCookieData();


                                label_id.Text = "";
                                namepd.Clear();
                                amountpd.Clear();
                                pricepd.Clear();
                                picboxrestock.Image = null;
                            }
                            else
                            {
                                MessageBox.Show("ไม่พบสินค้าที่จะลบ");
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("เกิดข้อผิดพลาด: " + ex.Message);
                }
            }
        }

        private void bttn_redone_Click(object sender, EventArgs e)
        {

            if (string.IsNullOrWhiteSpace(label_id.Text))
            {
                MessageBox.Show("กรุณาเลือกรายการที่ต้องการอัปเดตก่อน");
                return;
            }

            try
            {
                using (MySqlConnection conn = new MySqlConnection(connectionString))
                {
                    conn.Open();
                    string sql;


                    if (isImageFromFile)
                    {

                        sql = "UPDATE menu_cookie SET name_cookie = @name, amount = @amount, price = @price, pic_cookie = @image WHERE id = @id";
                    }
                    else
                    {

                        sql = "UPDATE menu_cookie SET name_cookie = @name, amount = @amount, price = @price WHERE id = @id";
                    }

                    using (MySqlCommand cmd = new MySqlCommand(sql, conn))
                    {

                        cmd.Parameters.AddWithValue("@id", label_id.Text);
                        cmd.Parameters.AddWithValue("@name", namepd.Text);
                        cmd.Parameters.AddWithValue("@amount", amountpd.Text);
                        cmd.Parameters.AddWithValue("@price", pricepd.Text);

                        if (isImageFromFile) // ถ้าเลือกใช้รูปจากไฟล์
                        {
                            // แปลงรูปภาพจาก PictureBox เป็น byte[]
                            byte[] imageBytes = ImageToByteArray(picboxrestock.Image);

                            cmd.Parameters.AddWithValue("@image", imageBytes);
                        }

                        int rowsAffected = cmd.ExecuteNonQuery();

                        if (rowsAffected > 0)
                        {
                            MessageBox.Show("อัปเดตข้อมูลเรียบร้อยแล้ว");


                            label_id.Text = "";
                            namepd.Clear();
                            amountpd.Clear();
                            pricepd.Clear();
                            picboxrestock.Image = null;
                        }
                        else
                        {
                            MessageBox.Show("ไม่พบข้อมูลที่จะอัปเดต");
                        }
                    }
                }
            }
            catch (Exception ex) // 
            {
                MessageBox.Show("เกิดข้อผิดพลาด: " + ex.Message);
            }
        }


        // แปลงภาพ (Image) เป็นอาเรย์ของไบต์ (byte array)
        private byte[] ImageToByteArray(Image image)
        {

            using (MemoryStream ms = new MemoryStream())
            {
                //  PNG, JPEG)
                image.Save(ms, image.RawFormat);


                return ms.ToArray();
            }
        }


        private void picboxrestock_Click(object sender, EventArgs e)
        {

            using (OpenFileDialog openFileDialog = new OpenFileDialog())
            {
                openFileDialog.Title = "เลือกรูปภาพ";
                openFileDialog.Filter = "Image Files|*.jpg;*.jpeg;*.png;*.bmp;*.gif";


                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    picboxrestock.Image = Image.FromFile(openFileDialog.FileName);
                    picboxrestock.SizeMode = PictureBoxSizeMode.Zoom;
                    isImageFromFile = true;
                }
            }
        }

        private void bttn_adst_Click(object sender, EventArgs e)//ปุ่มaddสินค้า 
        {
            try
            {
                using (OpenFileDialog openFileDialog = new OpenFileDialog())
                {
                    openFileDialog.Title = "เลือกรูปภาพ";
                    openFileDialog.Filter = "Image Files|*.jpg;*.jpeg;*.png;*.bmp;*.gif"; // กรองเฉพาะไฟล์รูปภาพ

                    if (openFileDialog.ShowDialog() == DialogResult.OK) // ถ้าผู้ใช้เลือกไฟล์
                    {
                        byte[] imageBytes = File.ReadAllBytes(openFileDialog.FileName); // อ่านไฟล์ภาพเป็น byte array

                        using (MySqlConnection conn = new MySqlConnection(connectionString))
                        {
                            conn.Open();


                            string sql = "INSERT INTO menu_cookie (name_cookie, amount, price, pic_cookie) VALUES (@name, @amount, @price, @image)";

                            using (MySqlCommand cmd = new MySqlCommand(sql, conn))
                            {

                                cmd.Parameters.AddWithValue("@name", namepd.Text);
                                cmd.Parameters.AddWithValue("@amount", amountpd.Text);
                                cmd.Parameters.AddWithValue("@price", pricepd.Text);
                                cmd.Parameters.AddWithValue("@image", imageBytes);

                                int rowsAffected = cmd.ExecuteNonQuery();

                                if (rowsAffected > 0)
                                {
                                    MessageBox.Show("เพิ่มสินค้าสำเร็จ");
                                    LoadCookieData();


                                    label_id.Text = "";
                                    namepd.Clear();
                                    amountpd.Clear();
                                    pricepd.Clear();
                                    picboxrestock.Image = null;
                                }
                                else
                                {
                                    MessageBox.Show("ไม่สามารถเพิ่มสินค้าได้");
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("เกิดข้อผิดพลาด: " + ex.Message);
            }
        }



        private void button1_Click(object sender, EventArgs e)
        {


            label_id.Text = "";
            namepd.Clear();

            amountpd.Clear();

            pricepd.Clear();

            picboxrestock.Image = null;
        }



        private void pricepd_KeyPress(object sender, KeyPressEventArgs e)
        {

            if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar))
            {

                e.Handled = true;
            }
        }


        private void amountpd_KeyPress(object sender, KeyPressEventArgs e)
        {

            if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar))
            {
                e.Handled = true;
            }
        }

        private void pricepd_Leave(object sender, EventArgs e)
        {

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

        private void datarestcok_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {

        }

        private void Form14_Load(object sender, EventArgs e)
        {
            LoadCookieData();
        }

        private void pictureBox1_Click(object sender, EventArgs e)
        {

        }
    }
}

