using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace donyadasproject
{
    public partial class Form11 : Form
    {
        private string _shownamestaff;

        public string namestaff11
        {
            get { return _shownamestaff; }
            set { _shownamestaff = value; label2.Text = value; }
        }

        public string _showphonestaff;
       
        public Form11()
        {
            InitializeComponent();
        }
        
        private void pictureBox1_Click(object sender, EventArgs e)
        {

        }

        private void reanddeletestock_Click(object sender, EventArgs e)
        {


            Form14 newForm = new Form14
            {
                namestaff14 = label2.Text
            };
            newForm.Show();
            this.Close();
        }

        private void mystaff_Click(object sender, EventArgs e)
        {
            
        }

        private void backtomain_Click(object sender, EventArgs e)
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

        private void label2_Click(object sender, EventArgs e)
        {

        }

        private void saleshis_Click(object sender, EventArgs e)
        {
            Form13 newForm = new Form13
            {
                namestaff13 = label2.Text
            };
            newForm.Show();

            this.Hide();
        }
    }
    }

