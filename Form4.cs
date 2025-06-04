using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;
using MySql.Data.MySqlClient;

namespace donyadasproject
{
    public partial class Form4 : Form
    {
        private string connectionString = "Server=localhost;Database=dearcookie;Uid=root;Pwd=;";

        public Form4()
        {
            InitializeComponent();
        }


        private void pictureBox1_Click(object sender, EventArgs e)
        {

        }
        private void tbsg_TextChanged(object sender, EventArgs e)
        {

        }

        private void tbphonenew_TextChanged(object sender, EventArgs e)
        {

        }

        private void tbpassnew_TextChanged(object sender, EventArgs e)
        {

        }

        private void label3_Click(object sender, EventArgs e)
        {

        }

        private void bttnbacknewmem_Click(object sender, EventArgs e)
        {
            Form2 newForm = new Form2();
            newForm.Show();


            this.Hide();
        }

        private void bttnsg2_Click(object sender, EventArgs e)
        {
            string username = tbsg.Text.Trim();
            string password = tbpassnew.Text.Trim();
            string phone = tbphonenew.Text.Trim();

            // ✅ ตรวจสอบเบอร์โทร
            if (!Regex.IsMatch(phone, @"^0\d{9}$"))
            {
                MessageBox.Show("เบอร์โทรต้องเป็นตัวเลข 10 หลักและขึ้นต้นด้วย 0", "ข้อมูลไม่ถูกต้อง", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // ✅ ตรวจสอบรหัสผ่าน
            if (password.Length < 8)
            {
                MessageBox.Show("รหัสผ่านต้องมีอย่างน้อย 8 ตัวอักษร", "ข้อมูลไม่ถูกต้อง", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            using (MySqlConnection conn = new MySqlConnection(connectionString))
            {
                try
                {
                    conn.Open();

                    // ✅ ตรวจสอบว่าเบอร์โทรซ้ำหรือไม่
                    string checkQuery = "SELECT COUNT(*) FROM member_cookie WHERE phone_member = @phone";
                    using (MySqlCommand checkCmd = new MySqlCommand(checkQuery, conn))
                    {
                        checkCmd.Parameters.AddWithValue("@phone", phone);
                        int count = Convert.ToInt32(checkCmd.ExecuteScalar());

                        if (count > 0)
                        {
                            MessageBox.Show("เบอร์โทรนี้มีอยู่ในระบบแล้ว", "ข้อมูลซ้ำ", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                            return;
                        }
                    }

                    // ✅ บันทึกข้อมูล
                    string insertQuery = "INSERT INTO member_cookie (user_member, pass_member, phone_member) VALUES (@username, @password, @phone)";
                    using (MySqlCommand insertCmd = new MySqlCommand(insertQuery, conn))
                    {
                        insertCmd.Parameters.AddWithValue("@username", username);
                        insertCmd.Parameters.AddWithValue("@password", password);
                        insertCmd.Parameters.AddWithValue("@phone", phone);

                        int rowsInserted = insertCmd.ExecuteNonQuery();

                        if (rowsInserted > 0)
                        {
                            MessageBox.Show("sign up succesful!", "succesful", MessageBoxButtons.OK, MessageBoxIcon.Information);

                            Form3 newForm = new Form3();
                            newForm.Show();
                            this.Close();
                        }
                        else
                        {
                            MessageBox.Show("ไม่สามารถสมัครสมาชิกได้", "ล้มเหลว", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("เกิดข้อผิดพลาด: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

    }
}
    


                
        




                


            

            

        
    

