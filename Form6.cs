using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using MySql.Data.MySqlClient;

namespace donyadasproject
{
    public partial class Form6 : Form

    {
        private string _shownamecus;
        public string  nameform6
        {
            get { return _shownamecus; }
            set { _shownamecus = value; lbname_cus.Text = value; }
        }

        public string _showphonecus;
        public string phoneform6
        {
            get { return _showphonecus; }
            set { _showphonecus = value; lbphonecus.Text = value; }
        }
        

        public Form6()
        {
            InitializeComponent();
        }

        private void load_name(string phone)
        {
            
                string connectionString = "server=localhost;user id=root;password=;database=dearcookie;";
                string name = string.Empty;

                using (MySqlConnection conn = new MySqlConnection(connectionString))
                {
                    string query = "SELECT user_member FROM member_cookie WHERE phone_member = @phone";
                    MySqlCommand cmd = new MySqlCommand(query, conn);
                    cmd.Parameters.AddWithValue("@phone", phone);

                    try
                    {
                        conn.Open();
                        object result = cmd.ExecuteScalar();

                        if (result != null)
                        {
                            name = result.ToString();
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("เกิดข้อผิดพลาด: " + ex.Message);
                    }
                }

            lbname_cus.Text = name;
            lbphonecus.Text = phone;


        }

        private void button3_Click(object sender, EventArgs e)
        {
            Form10 newForm = new Form10();
            newForm.phoneform10 = lbphonecus.Text;
            newForm.Show();

            this.Close();
        }

        private void pictureBox1_Click(object sender, EventArgs e)
        {

        }

        private void lbname_cus_Click(object sender, EventArgs e)
        {

        }

        private void lbphonecus_Click(object sender, EventArgs e)
        {

        }

        private void orderhis_Click(object sender, EventArgs e)
        {
            Form7 newForm = new Form7();
            newForm.phoneform7 = lbphonecus.Text;
            newForm.Show();

            this.Close();
        }

       

        private void Form6_Load(object sender, EventArgs e)
        {
            load_name(phoneform6);

        }

        private void bttnbackstaff_Click(object sender, EventArgs e)
        {
            DialogResult result = MessageBox.Show(
        "คุณต้องการออกจากระบบใช่หรือไม่?",
        "ยืนยันการออกจากระบบ",
        MessageBoxButtons.YesNo,
        MessageBoxIcon.Question);

            if (result == DialogResult.Yes)
            {
                // กลับไปหน้า Login หรือปิดโปรแกรม
                Form2 loginForm = new Form2(); // เปลี่ยนเป็นหน้าล็อกอินของคุณ
                loginForm.Show();
                this.Close(); // ปิดหน้าปัจจุบัน
            }
        }
    }
}
