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
    public partial class Form16 : Form
    {
        public Form16()
        {
            InitializeComponent();
        }

        private void bttnbacknewmem_Click(object sender, EventArgs e)
        {
            Form2 loginForm = new Form2(); // เปลี่ยนเป็นหน้าล็อกอินของคุณ
            loginForm.Show();
            this.Close(); // ปิดหน้าปัจจุบัน
        }

        private void pictureBox1_Click(object sender, EventArgs e)
        {

        }
    }
}
