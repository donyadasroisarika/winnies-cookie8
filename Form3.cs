using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using MySql.Data.MySqlClient;

namespace donyadasproject
{
    public partial class Form3 : Form
    {
        // สตริงสำหรับเชื่อมต่อฐานข้อมูล MySQL
        private string connectionString = "Server=localhost;Database=dearcookie;Uid=root;Pwd=;";

        // ตัวแปรสำหรับควบคุมสถานะการแสดงรหัสผ่าน
        private bool isPasswordVisible = false;
        public Form3()
        {
            InitializeComponent();   // สร้างองค์ประกอบ UI ของฟอร์ม

        }

        private void bttnlg2_Click(object sender, EventArgs e)
        {

            string username = tbmem.Text; // ดึงชื่อผู้ใช้จาก textbox
            string password = tbpassmem.Text; // ดึงชื่อผู้ใช้จาก textbox



            // ตรวจสอบ username และ password
            using (MySqlConnection conn = new MySqlConnection(connectionString))
            {
                try
                {
                    conn.Open();

                    string query = "SELECT * FROM member_cookie WHERE user_member = @username AND pass_member = @password";

                    using (MySqlCommand cmd = new MySqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@username", tbmem.Text.Trim());
                        cmd.Parameters.AddWithValue("@password", tbpassmem.Text.Trim());

                        using (MySqlDataReader reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                // ดึงข้อมูลจาก reader (ตรวจสอบว่ามีคอลัมน์ตามนี้ใน DB)
                                
                                string tel_user = reader["phone_member"].ToString();
                                string name_user = reader["user_member"].ToString();
                             

                                MessageBox.Show("Login Successful!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);

                                Form6 newForm = new Form6
                                {
                                    phoneform6 = tel_user,
                                    nameform6 = name_user,
                                };

                                newForm.Show();
                                this.Hide(); // ซ่อนหน้าจอ login
                            }
                            else
                            {
                                MessageBox.Show("Invalid username or password.", "Login Failed", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                            }
                        }
                    }
                }
                 catch (Exception ex)
                {
                    MessageBox.Show("Error: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                conn.Close();
            }
   
        }

        private void pictureBox1_Click(object sender, EventArgs e)
        {

        }

        private void tbmem_TextChanged(object sender, EventArgs e)
        {

        }

        private void tbpassmem_TextChanged(object sender, EventArgs e)
        {

        }

        private void bttnmem_Click(object sender, EventArgs e)
        {
           Form2 f2 = new Form2();
            f2.Show();
            this.Close();


           
        }

        // ฟังก์ชันนี้จะถูกเรียกเมื่อมีการคลิกลิงก์ showpasswow (LinkLabel)
        private void showpasswow_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            isPasswordVisible = !isPasswordVisible; // สลับค่าตัวแปร isPasswordVisible ระหว่าง true กับ false  // ถ้าเดิมเป็น false (ซ่อนรหัสผ่าน) → จะเป็น true (แสดงรหัสผ่าน)

            tbpassmem.UseSystemPasswordChar = !isPasswordVisible;// ตั้งค่าให้ TextBox (tbpassmem) แสดงหรือซ่อนรหัสผ่าน
                                                                 // ถ้า isPasswordVisible เป็น true → ไม่ใช้สัญลักษณ์รหัสผ่าน (แสดงตัวอักษร)
                                                                 // ถ้า isPasswordVisible เป็น false → ใช้สัญลักษณ์ ● แทนตัวอักษร

            showpasswow.Text = isPasswordVisible ? "ซ่อนรหัสผ่าน" : "แสดงรหัสผ่าน";// เปลี่ยนข้อความของลิงก์ showpasswow ให้ตรงกับสถานะการแสดงรหัสผ่าน
                                                                                   // ถ้า isPasswordVisible เป็น true → แสดงข้อความ "ซ่อนรหัสผ่าน"
                                                                                   // ถ้าเป็น false → แสดงข้อความ "แสดงรหัสผ่าน"
        }
    }
}
 