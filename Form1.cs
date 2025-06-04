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
    public partial class Form1 : Form
    {
        private MySqlConnection databaseConnection()
        {
            string connectionString = "datasource=127.0.0.1;port=3306;username=root;password=;database=mycookie;";

            MySqlConnection conn = new MySqlConnection(connectionString);

            return conn;
        }
        public Form1()
        {
            InitializeComponent();
        }

        private void bttnmain_Click(object sender, EventArgs e)
        {
            
            Form2 newForm = new Form2();
            newForm.Show();


            this.Hide();
        }

        private void mainpic_Click(object sender, EventArgs e)
        {

        }
    }
}
