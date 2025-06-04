using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using MySql.Data.MySqlClient;
using static Mysqlx.Expect.Open.Types.Condition.Types;

namespace donyadasproject
{
    public partial class Form10 : Form
    {

        string connectionString = "server=localhost;user id=root;password=;database=dearcookie;";

        
        private string _showphonecus;

        
        public string phoneform10
        {
            

            
            set
            {
               
                _showphonecus = value;

               
                label1.Text = value;

               
                LoadCartData();
            }
        }

        public Form10() // ปก ฟอร์ม
        {
            InitializeComponent();
            LoadMenuData();
            
        }

        private void LoadMenuData()
        {
          
            using (MySqlConnection conn = new MySqlConnection(connectionString))
            {
                string query = "SELECT * FROM menu_cookie"; 
                MySqlDataAdapter adapter = new MySqlDataAdapter(query, conn); 
                DataTable dt = new DataTable(); 
                adapter.Fill(dt); 
                datamenu.DataSource = dt;
            }

            
            datamenu.Columns["id"].Visible = false;
            datamenu.Columns["pic_cookie"].Visible = false;

            
            ((DataGridViewTextBoxColumn)datamenu.Columns["price"]).DefaultCellStyle.Format = "N2";

           
            datamenu.SelectionChanged += DataGridViewMenu_SelectionChanged;

            
            LoadCartData();
        }

        private void datamenu_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
          
            if (e.RowIndex >= 0)
            {
                DataGridViewRow row = datamenu.Rows[e.RowIndex]; 

             
                pdname.Text = row.Cells["name_cookie"].Value.ToString();

             
                pdprice.Text = string.Format("{0:0.00}", row.Cells["price"].Value);

               
                pdstock.Text = row.Cells["amount"].Value.ToString();

                
                if (row.Cells["pic_cookie"].Value != DBNull.Value)
                {
                    byte[] imageBytes = (byte[])row.Cells["pic_cookie"].Value;

                    using (MemoryStream ms = new MemoryStream(imageBytes))
                    {
                        showmenu.Image = Image.FromStream(ms);
                        showmenu.SizeMode = PictureBoxSizeMode.Zoom; 
                    }
                }
                else
                {
                    
                    showmenu.Image = null;
                }
            }
        }

        
        private void LoadCartData() 
        {
           
            using (MySqlConnection conn = new MySqlConnection(connectionString))
            {
                
                string query = "SELECT * FROM cart_cookie WHERE name_customer = @name_customer";

                
                using (MySqlCommand cmd = new MySqlCommand(query, conn))
                {
                   
                    cmd.Parameters.AddWithValue("@name_customer", label1.Text);

                    
                    using (MySqlDataAdapter adapter = new MySqlDataAdapter(cmd))
                    {
                      
                        DataTable dt = new DataTable();

                       
                        adapter.Fill(dt);

                        
                        datacart.DataSource = dt;
                    }
                }
            } 
        


        datacart.Columns["id"].Visible = false;

            // ปรับความกว้างของคอลัมน์
            datacart.Columns["name"].Width = 155;
            datacart.Columns["amount"].Width = 40;
            datacart.Columns["price"].Width = 40;
            datacart.Columns["total"].Width = 60;
            
            
            
            // จัดชิดขวาสำหรับตัวเลขให้ดูอ่านง่าย
            datacart.Columns["amount"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
            datacart.Columns["price"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
            datacart.Columns["total"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;


            // จัดให้ราคามีทศนิยม
            datacart.Columns["price"].DefaultCellStyle.Format = "N2";
            datacart.Columns["total"].DefaultCellStyle.Format = "N2";
        }



        private void Addbt_Click(object sender, EventArgs e)
        {
           
            if (string.IsNullOrWhiteSpace(qty_input.Text))
            {
                MessageBox.Show("กรุณากรอกจำนวน");
                return;
            }
           
            if (!(int.TryParse(qty_input.Text, out int qty) && qty > 0))
            {
                MessageBox.Show("กรุณากรอกตัวเลขที่ไม่เป็นลบและมากกว่า0");
                return;
            }
           
            if (pdname.Text == "flavor")
            {
                MessageBox.Show("กรุณาเลือกสินค้าก่อน");
                return;
            }

           
            string name = pdname.Text;

            
            decimal price = decimal.Parse(pdprice.Text);

            
            int stock = int.Parse(pdstock.Text);

           
            decimal total = qty * price;


            if (qty > stock)
            {
                MessageBox.Show("จำนวนที่เลือกมากกว่าจำนวนในสต็อก");
                return;
            }

            bool inserted = InsertCartItem(name, qty, price ,total,label1.Text);
            if (!inserted)
            {
                MessageBox.Show("ไม่สามารถเพิ่มสินค้าลงตะกร้าได้");
                return;
            }

            bool updated = UpdateStock(name, qty);
            if (!updated)
            {
                MessageBox.Show("ไม่สามารถตัดสต็อกได้");
                return;
            }

            LoadMenuData();
            clear_detail();
            MessageBox.Show("เพิ่มสินค้าลงตะกร้าเรียบร้อยแล้ว\nราคารวม: " + total + " บาท");
        }

        private bool InsertCartItem(string name, int qty, decimal price, decimal total, string name_customer)
        {
            try
            {
                using (MySqlConnection conn = new MySqlConnection(connectionString)) 
                {
                    conn.Open(); 

                
                    string checkQuery = "SELECT amount, total FROM cart_cookie WHERE name = @name AND name_customer = @name_customer";
                    using (MySqlCommand checkCmd = new MySqlCommand(checkQuery, conn)) 
                    {
                        checkCmd.Parameters.AddWithValue("@name", name); 
                        checkCmd.Parameters.AddWithValue("@name_customer", name_customer); 

                        using (MySqlDataReader reader = checkCmd.ExecuteReader()) 
                        {
                            if (reader.Read()) 
                            {
                                int existingQty = Convert.ToInt32(reader["amount"]); 
                                int existingTotal = Convert.ToInt32(reader["total"]);

                                reader.Close(); 

                                int newQty = existingQty + qty; 
                                decimal newTotal = existingTotal + total;

                               
                                string updateQuery = "UPDATE cart_cookie SET amount = @newQty, total = @newTotal WHERE name = @name AND name_customer = @name_customer";
                                using (MySqlCommand updateCmd = new MySqlCommand(updateQuery, conn)) 
                                {
                                    updateCmd.Parameters.AddWithValue("@newQty", newQty); 
                                    updateCmd.Parameters.AddWithValue("@newTotal", newTotal); 
                                    updateCmd.Parameters.AddWithValue("@name", name);
                                    updateCmd.Parameters.AddWithValue("@name_customer", name_customer);
                                    updateCmd.ExecuteNonQuery(); 
                                }
                            }
                            else 
                            {
                                reader.Close();

                               
                                string insertQuery = "INSERT INTO cart_cookie (name, amount, price, total, name_customer) VALUES (@name, @amount, @price, @total, @name_customer)";
                                using (MySqlCommand insertCmd = new MySqlCommand(insertQuery, conn))
                                {
                                    insertCmd.Parameters.AddWithValue("@name", name); 
                                    insertCmd.Parameters.AddWithValue("@amount", qty); 
                                    insertCmd.Parameters.AddWithValue("@price", price); 
                                    insertCmd.Parameters.AddWithValue("@total", total); 
                                    insertCmd.Parameters.AddWithValue("@name_customer", name_customer); 
                                    insertCmd.ExecuteNonQuery(); 
                                }
                            }
                        }
                    }
                }

                return true; 
            }
            catch (Exception ex) 
            {
                MessageBox.Show("Error: " + ex.Message); 
                return false;
            }
        }


        private bool UpdateStock(string name, int qty)  
        {
            try
            {
                using (MySqlConnection conn = new MySqlConnection(connectionString)) 
                {
                    conn.Open();  

                    
                    string updateStockQuery = "UPDATE menu_cookie SET amount = amount - @qty WHERE name_cookie = @name AND amount >= @qty";

                    using (MySqlCommand cmd = new MySqlCommand(updateStockQuery, conn))  
                    {
                        cmd.Parameters.AddWithValue("@qty", qty); 
                        cmd.Parameters.AddWithValue("@name", name); 

                        int rowsAffected = cmd.ExecuteNonQuery();  

                        if (rowsAffected == 0)  
                        {
                            MessageBox.Show("จำนวนสินค้าในสต็อกไม่เพียงพอ หรือไม่พบสินค้า");
                            return false;
                        }
                    }
                }
                return true;  
            }
            catch (Exception ex)
            {
                MessageBox.Show("เกิดข้อผิดพลาดขณะตัดสต็อก: " + ex.Message); 
                return false;
            }
        }

        
        private bool RestoreStock(string name, int qty)
        {
            try
            {
                
                using (MySqlConnection conn = new MySqlConnection(connectionString))
                {
                   
                    conn.Open();

                
                    string query = "UPDATE menu_cookie SET amount = amount + @qty WHERE name_cookie = @name";

                   
                    using (MySqlCommand cmd = new MySqlCommand(query, conn))
                    {
                        
                        cmd.Parameters.AddWithValue("@qty", qty);

                      
                        cmd.Parameters.AddWithValue("@name", name);

                        
                        cmd.ExecuteNonQuery();
                    }
                }

              
                return true;
            }
            catch (Exception ex)
            {
                
                MessageBox.Show("เกิดข้อผิดพลาดขณะคืนสต็อก: " + ex.Message);

               
                return false;
            }
        }


        private void deletebt_Click(object sender, EventArgs e)
        {
           
            if (datacart.SelectedRows.Count == 0)
            {
                MessageBox.Show("กรุณาเลือกสินค้าที่ต้องการลบออกจากตะกร้า");  
                return;  
            }

            DataGridViewRow selectedRow = datacart.SelectedRows[0];  

            int id = Convert.ToInt32(selectedRow.Cells["id"].Value);  

            string name = selectedRow.Cells["name"].Value.ToString();  

            int qty = Convert.ToInt32(selectedRow.Cells["amount"].Value);  

            bool deleted = DeleteCartItem(id); 
            if (!deleted)
            {
                MessageBox.Show("ไม่สามารถลบสินค้าออกจากตะกร้าได้");
                return;
            }


            bool restored = RestoreStock(name, qty); // เรียกใช้ฟังก์ชัน RestoreStock เพื่อเพิ่มสต็อกสินค้ากลับคืน โดยส่งชื่อสินค้า (name) และจำนวน (qty) ที่ลบออกไป
                                                     // ฟังก์ชันจะคืนค่า true หากคืนสต็อกสำเร็จ หรือ false หากเกิดปัญหา



            if (!restored)
            {
                MessageBox.Show("ไม่สามารถคืนสต็อกสินค้าได้");
                return;
            }

           
            LoadMenuData();

            
            clear_detail();

            
            MessageBox.Show("ลบสินค้าออกจากตะกร้าเรียบร้อยแล้ว");
        }

        
        private bool DeleteCartItem(int id)
        {
            try
            {

                using (MySqlConnection conn = new MySqlConnection(connectionString)) 
                {
                    conn.Open(); 

                    string query = "DELETE FROM cart_cookie WHERE id = @id";  

                    using (MySqlCommand cmd = new MySqlCommand(query, conn)) 
                    {
                        cmd.Parameters.AddWithValue("@id", id); 

                        cmd.ExecuteNonQuery(); 
                    }
                }

                return true;
            }
            catch (Exception ex)
            {
               
                MessageBox.Show("เกิดข้อผิดพลาดขณะลบสินค้า: " + ex.Message);

              
                return false;
            }
        }


        private void clear_detail() //เคลียสินค้า 
        {
            label2.Text = "";  
            showmenu.Image = null;  
            pdname.Text = "flavor";  
            pdprice.Text = "price";
            pdstock.Text = "quantity";
            qty_input.Text = "";  
        }







        private void DataGridViewMenu_SelectionChanged(object sender, EventArgs e)
        {
           
        }
        private void showmenu_Click(object sender, EventArgs e)
        {

        }

        private void nextforpay_Click(object sender, EventArgs e)
        {
            
            using (MySqlConnection conn = new MySqlConnection(connectionString))
            {
                
                string query = "SELECT 1 FROM cart_cookie WHERE name_customer = @name_customer LIMIT 1";

                using (MySqlCommand cmd = new MySqlCommand(query, conn))
                {
                   
                    cmd.Parameters.AddWithValue("@name_customer", label1.Text);

                    try
                    {
                        conn.Open(); 

                        using (MySqlDataReader reader = cmd.ExecuteReader())
                        {
                            
                            if (!reader.HasRows)
                            {
                                MessageBox.Show("กรุณาเลือกสินค้าก่อน", "ไม่มีสินค้า", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                                return;
                            }
                        }

                        
                        Form15 newForm = new Form15();
                        newForm.phoneform15 = label1.Text; 
                        newForm.Show();
                        this.Close(); 
                    }
                    catch (Exception ex)
                    {
                  
                        MessageBox.Show("เกิดข้อผิดพลาด: " + ex.Message);
                    }
                }
            }
        }


        private void pictureBox1_Click(object sender, EventArgs e)
        {

        }
        private void btedit_Click(object sender, EventArgs e)
        {
            
            if (string.IsNullOrWhiteSpace(qty_input.Text))
            {
                MessageBox.Show("กรุณากรอกจำนวน");
                return;
            }

           
            if (!(int.TryParse(qty_input.Text, out int qty) && qty > 0))
            {
                MessageBox.Show("กรุณากรอกตัวเลขที่ไม่เป็นลบและมากกว่า 0");
                return;
            }

            
            DataGridViewRow selectedRow = datacart.SelectedRows[0];
            int cartId = Convert.ToInt32(selectedRow.Cells["id"].Value);  
            string name = selectedRow.Cells["name"].Value.ToString();  
            int oldQty = Convert.ToInt32(selectedRow.Cells["amount"].Value); 
            int price = Convert.ToInt32(selectedRow.Cells["price"].Value);  

           
            string input = qty_input.Text;

           
            if (!int.TryParse(input, out int newQty) || newQty <= 0)
            {
                MessageBox.Show("กรุณากรอกจำนวนที่ถูกต้อง");
                return;
            }

            
            int newTotal = newQty * price;

            
            int qtyDiff = newQty - oldQty;

            
            if (qtyDiff > 0)
            {
                int stock = GetStock(name);  
                if (qtyDiff > stock)  
                {
                    MessageBox.Show("จำนวนใหม่เกินจำนวนในสต็อก");
                    return;
                }
            }

          
            using (MySqlConnection conn = new MySqlConnection(connectionString))
            {
                conn.Open();

                
                string updateCartQuery = "UPDATE cart_cookie SET amount = @amount, total = @total WHERE id = @id";
                using (MySqlCommand cmd = new MySqlCommand(updateCartQuery, conn))
                {
                    cmd.Parameters.AddWithValue("@amount", newQty);
                    cmd.Parameters.AddWithValue("@total", newTotal);
                    cmd.Parameters.AddWithValue("@id", cartId);
                    cmd.ExecuteNonQuery();
                }

               
                string updateStockQuery = "UPDATE menu_cookie SET amount = amount - @diff WHERE name_cookie = @name";
                using (MySqlCommand cmd = new MySqlCommand(updateStockQuery, conn))
                {
                    cmd.Parameters.AddWithValue("@diff", qtyDiff);
                    cmd.Parameters.AddWithValue("@name", name);
                    cmd.ExecuteNonQuery();
                }
            }

           
            LoadMenuData();

            clear_detail();

      
            MessageBox.Show("แก้ไขจำนวนสินค้าเรียบร้อยแล้ว");
        }



        private int GetStock(string name)
        {
            
            using (MySqlConnection conn = new MySqlConnection(connectionString))
            {
                conn.Open();  
               
                string query = "SELECT amount FROM menu_cookie WHERE name_cookie = @name";

                using (MySqlCommand cmd = new MySqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@name", name); 

                    
                    object result = cmd.ExecuteScalar();

                    
                    return result != null ? Convert.ToInt32(result) : 0;
                }
            }
        }


        private void datacart_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0) 
            {
            
                DataGridViewRow selectedRow = datacart.Rows[e.RowIndex];

             
                string name = selectedRow.Cells["name"].Value.ToString();
                string id_edit = selectedRow.Cells["id"].Value.ToString();

              
                using (MySqlConnection conn = new MySqlConnection(connectionString))
                {
                    conn.Open();  
                  
                    string cartQuery = "SELECT amount, price, total FROM cart_cookie WHERE name = @name LIMIT 1";
                    using (MySqlCommand cartCmd = new MySqlCommand(cartQuery, conn))
                    {
                        cartCmd.Parameters.AddWithValue("@name", name);  

                        using (MySqlDataReader cartReader = cartCmd.ExecuteReader()) 
                        {
                            if (cartReader.Read()) 
                            {
                                label2.Text = id_edit;  
                                pdname.Text = name;  
                                pdprice.Text = cartReader["price"].ToString();  
                                pdstock.Text = cartReader["amount"].ToString();  
                            }
                        }
                    }

                    
                    string picQuery = "SELECT pic_cookie FROM menu_cookie WHERE name_cookie = @name LIMIT 1";
                    using (MySqlCommand picCmd = new MySqlCommand(picQuery, conn))
                    {
                        picCmd.Parameters.AddWithValue("@name", name);  
                        using (MySqlDataReader picReader = picCmd.ExecuteReader()) 
                        {
                            if (picReader.Read() && picReader["pic_cookie"] != DBNull.Value) 
                            {
                                byte[] imageBytes = (byte[])picReader["pic_cookie"];  

                                using (MemoryStream ms = new MemoryStream(imageBytes)) 
                                {
                                    showmenu.Image = Image.FromStream(ms);  
                                    showmenu.SizeMode = PictureBoxSizeMode.Zoom;  
                                }
                            }
                            else
                            {
                                showmenu.Image = null;  
                            }
                        }
                    }
                }
            }
        }


        private void backtomb_Click_1(object sender, EventArgs e)
        {
            Form6 newForm = new Form6();
            newForm.Show();
            this.Close();

        }

        private void label1_Click(object sender, EventArgs e)
        {

        }

        private void qty_input_TextChanged(object sender, EventArgs e)
        {

        }

        private void panel1_Paint(object sender, PaintEventArgs e)
        {

        }

        private void pdname_Click(object sender, EventArgs e)
        {

        }

        private void pdprice_Click(object sender, EventArgs e)
        {

        }

        private void label4_Click(object sender, EventArgs e)
        {

        }
    }
    }
   

    

