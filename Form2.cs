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

namespace donyadasproject
{
    public partial class Form2 : Form
    {
        public Form2()
        {
            InitializeComponent();
        }

        private void linkstaff_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            Form5 newForm = new Form5();
            newForm.Show();

            // ปิดฟอร์มปัจจุบัน
            this.Close();
        }

        private void bttnlg1_Click(object sender, EventArgs e)
        {
            Form3 newForm = new Form3();
            newForm.Show();


            this.Hide();
        }

        private void bttnsg1_Click(object sender, EventArgs e)
        {
            Form4 newForm = new Form4();
            newForm.Show();


            this.Hide();
        }

        private void pictureBox1_Click(object sender, EventArgs e)
        {

        }

        private void linkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            Form16 newForm = new Form16();
            newForm.Show();
            this.Close();       
        }
    }
}
