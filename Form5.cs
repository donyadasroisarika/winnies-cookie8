using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.OleDb;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using MySql.Data.MySqlClient;
using ZXing;

namespace donyadasproject
{
    public partial class Form5 : Form
    {
        private string connectionString = "Server=localhost;Database=dearcookie;Uid=root;Pwd=;";


        public Form5()
        {
            InitializeComponent();
        }

        private void bttnlgstaff_Click(object sender, EventArgs e)
        {
            string username = tbstaff.Text;
            string password = tbpassstaff.Text;

            using (MySqlConnection conn = new MySqlConnection(connectionString))
            {
                try
                {
                    conn.Open();
                    string query = "SELECT * FROM owner WHERE user_owner = @username AND pass_owner = @password";
                    MySqlCommand cmd = new MySqlCommand(query, conn);
                    cmd.Parameters.AddWithValue("@username", username);
                    cmd.Parameters.AddWithValue("@password", password);

                    using (MySqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            string name_user = reader["user_owner"].ToString();

                            MessageBox.Show("Login Successful!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);

                            Form11 newForm = new Form11
                            {
                                namestaff11 = name_user
                            };
                            newForm.Show();

                            this.Hide();
                        }
                        else
                        {
                            MessageBox.Show("Invalid username or password.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
       

}
    


    private void tbstaff_TextChanged(object sender, EventArgs e)
    {

    }


    private void tbpassstaff_TextChanged(object sender, EventArgs e)
    {

    }

        private void bttnbackstaff_Click(object sender, EventArgs e)
        {
            Form2 f2 = new Form2();
            f2.Show();

            this.Close();
        }

        private void pictureBox1_Click(object sender, EventArgs e)
        {

        }
        private bool isPasswordVisible = false;
        private void showpassstaff_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            isPasswordVisible = !isPasswordVisible;

            tbpassstaff.UseSystemPasswordChar = !isPasswordVisible;

            showpassstaff.Text = isPasswordVisible ? "ซ่อนรหัสผ่าน" : "แสดงรหัสผ่าน";
        }
    }

    

      
    }

