using System;
using System.Data;
using System.Windows.Forms;
using System.Collections.Generic;
using MySql.Data.MySqlClient;
using System.Reflection.Emit;
using Saladpuk.PromptPay.Facades;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.ProgressBar;
using System.Diagnostics;
using System.Drawing.Printing;
using System.Drawing;
using System.IO;
using System.Xml.Linq;
using ZXing.Common;
using ZXing.OneD;
using ZXing;
using iTextSharp.text;
using iTextSharp.text.pdf;

namespace donyadasproject
{
    public partial class Form15 : Form
    {
        string connectionString = "server=localhost;user id=root;password=;database=dearcookie;";

        // ตัว constructor ของฟอร์ม Form15
        public Form15()
        {
            InitializeComponent();          
            this.Load += new EventHandler(Form15_Load);  
        }

        private string _showphonecus;     

      
        public string phoneform15
        {
            get { return _showphonecus; }  
            set
            {
                _showphonecus = value;     
                label1.Text = value;     


               
                 
                LoadCartData();
            }
        }


       
        private void Form15_Load(object sender, EventArgs e)
        {
            LoadCartData(); 
            decimal totalWithVatIncluded = 0; 

           
            foreach (DataGridViewRow row in datacart.Rows)
            {
               
                if (row.Cells["total"].Value != null && decimal.TryParse(row.Cells["total"].Value.ToString(), out decimal value))
                {
                    
                    totalWithVatIncluded += value;
                }
            }

            
        

        // ถอด VAT จากยอดรวม
        decimal vat = totalWithVatIncluded * 7 / 100;
            decimal netTotal = totalWithVatIncluded - vat;

            // แสดงผลใน Label
            label2.Text = netTotal.ToString("N2");       // ยอดไม่รวม VAT
            label3.Text = vat.ToString("N2");            // VAT 7%
            label4.Text = totalWithVatIncluded.ToString("N2");   // ยอดรวม (รวม VAT แล้ว)

            // สร้าง QR Code จากยอดรวมจริง (รวม VAT)
            string qrcode = PPay.DynamicQR
                .MobileNumber("0970824980")
                .Amount((double)totalWithVatIncluded)
                .CreateCreditTransferQrCode();

            BarcodeWriter BW = new BarcodeWriter
            {
                Format = BarcodeFormat.QR_CODE,
                Options = new EncodingOptions
                {
                    Width = 200,
                    Height = 200,
                    PureBarcode = true
                }
            };

            Bitmap QrcodeBitmap = BW.Write(qrcode);
            pictureBox2.Image = QrcodeBitmap;
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
            datacart.Columns["name_customer"].Visible = false;

            // ปรับความกว้างของคอลัมน์
            datacart.Columns["name"].Width = 155;
            datacart.Columns["amount"].Width = 40;
            datacart.Columns["price"].Width = 40;
            datacart.Columns["total"].Width = 60;

            // จัดชิดขวาสำหรับตัวเลขให้ดูอ่านง่าย
            datacart.Columns["amount"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
            datacart.Columns["price"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
            datacart.Columns["total"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
        }



        // เหตุการณ์เมื่อกดปุ่ม button1
        private void button1_Click(object sender, EventArgs e)
        {
            SaveToSalesHistory();  
        }

       
        private string GenerateOrderCode()
        {
            string orderCode = ""; 
                                    
            string query = "SELECT MAX(order_code) FROM sales_history WHERE order_code LIKE 'WNCK-%'";

            
            using (MySqlConnection conn = new MySqlConnection(connectionString))
            {
                conn.Open();  

              
                using (MySqlCommand cmd = new MySqlCommand(query, conn))
                {
                   
                    object result = cmd.ExecuteScalar();

                    if (result != null && result != DBNull.Value)  
                    {
                        string lastCode = result.ToString();    
                        int number = int.Parse(lastCode.Substring(5));  
                        orderCode = $"WNCK-{(number + 1).ToString("D3")}";  
                    }
                    else  
                    {
                        orderCode = "WNCK-001";  
                    }
                }
            }

            order_id = orderCode;  
            return orderCode;      
        }


        private string order_id = "";

        
        private void SaveToSalesHistory()
        {
            
            string connectionString = "datasource=127.0.0.1;port=3306;username=root;password=;database=dearcookie;charset=utf8;";

           
            string orderCode = GenerateOrderCode();

            
            var cartItems = new List<(string productName, int amount, decimal price, decimal total)>();

           
            using (MySqlConnection conn = new MySqlConnection(connectionString))
            {
                conn.Open();

                
                string selectCart = @"SELECT name AS product_name, amount, price, total 
                              FROM cart_cookie 
                              WHERE name_customer = @phone";

                
                using (MySqlCommand selectCmd = new MySqlCommand(selectCart, conn))
                {
                    selectCmd.Parameters.AddWithValue("@phone", label1.Text);

                    
                    using (MySqlDataReader reader = selectCmd.ExecuteReader())
                    {
                        while (reader.Read())  // วนลูปอ่านข้อมูลทุกแถว
                        {
                            // เพิ่มข้อมูลแต่ละรายการลงในลิสต์ cartItems
                            cartItems.Add((
                                reader["product_name"].ToString(),            
                                Convert.ToInt32(reader["amount"]),            
                                Convert.ToDecimal(reader["price"]),           
                                Convert.ToDecimal(reader["total"])            
                            ));
                        }
                    }
                }



              
                string receiptPosition = createpdf();
 foreach (var item in cartItems)
                {
                    
                    string insert = @"INSERT INTO sales_history 
        (order_code, product_name, amount, price, total, phone_customer, sale_datetime, receipt_position) 
        VALUES (@order_code, @product_name, @amount, @price, @total, @phone, NOW(), @receipt_position)";

                   
                    using (MySqlCommand insertCmd = new MySqlCommand(insert, conn))
                    {
                       
                        insertCmd.Parameters.AddWithValue("@order_code", orderCode);          
                        insertCmd.Parameters.AddWithValue("@product_name", item.productName);  
                        insertCmd.Parameters.AddWithValue("@amount", item.amount);             
                        insertCmd.Parameters.AddWithValue("@price", item.price);              
                        insertCmd.Parameters.AddWithValue("@total", item.total);            
                        insertCmd.Parameters.AddWithValue("@phone", label1.Text);             
                        insertCmd.Parameters.AddWithValue("@receipt_position", receiptPosition);

                        insertCmd.ExecuteNonQuery();  
                    }
                }


               
                string deleteQuery = "DELETE FROM cart_cookie WHERE name_customer = @phone";

                
                using (MySqlCommand deleteCmd = new MySqlCommand(deleteQuery, conn))
                {
                   
                    deleteCmd.Parameters.AddWithValue("@phone", label1.Text);

                   
                    deleteCmd.ExecuteNonQuery();
                }

               
                MessageBox.Show($"Order Code: {orderCode}");

                
                conn.Close();

                
                Form10 newForm = new Form10();
                newForm.phoneform10 = label1.Text;
                newForm.Show();
                this.Close();

            }
        }

        private string createpdf() //โดยเลือกข้อมูลรายละเอียดออเดอร์จากดาต้าเบส 
        {
            string receiptPosition = "";
            string query = "SELECT * FROM sales_history WHERE order_code = @order_code";

            using (MySqlConnection connection = new MySqlConnection(connectionString))
            {
                connection.Open();

                // ดึงข้อมูลจากฐานข้อมูล cart_cookie
                using (MySqlCommand command = new MySqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@order_code", order_id);

                    string userName = "";
                    string userEmail = "";
                    string userTel = label1.Text;
                    string userAddress = "";

                    // ดึงข้อมูลจากฐานข้อมูล member
                    string userQuery = "SELECT * FROM member_cookie WHERE phone_member = @phone";
                    using (MySqlCommand userCommand = new MySqlCommand(userQuery, connection))
                    {
                        userCommand.Parameters.AddWithValue("@phone", label1.Text);
                        using (MySqlDataReader memberreader = userCommand.ExecuteReader())
                        {
                            while (memberreader.Read())
                            {
                                userName = memberreader["user_member"].ToString();
                            }
                        }
                    }

                    // กำหนดฟอนต์
                    BaseFont sarabun = BaseFont.CreateFont(@"D:\winniecookies\font\THSarabunNew.ttf", BaseFont.IDENTITY_H, BaseFont.EMBEDDED); // ฟอนต์ Sarabun
                    iTextSharp.text.Font fontNormal = new iTextSharp.text.Font(sarabun, 16, iTextSharp.text.Font.NORMAL);
                    iTextSharp.text.Font fontBold = new iTextSharp.text.Font(sarabun, 16, iTextSharp.text.Font.BOLD);
                    BaseColor myColor = new BaseColor(4, 23, 92);

                    string fileName = $"WNCK_{DateTime.Now:yyyyMMddHHmmss}.pdf";
                    receiptPosition = @"D://winniecookies//bill//" + fileName;     // ใส่ตำแหน่งใบเสร็จตามจริง เช่น เครื่อง, เคาน์เตอร์

                    // สร้าง Document และ Writer
                    Document doc = new Document(PageSize.A4, 50, 50, 50, 50);
                    PdfWriter writer = PdfWriter.GetInstance(doc, new FileStream(receiptPosition, FileMode.Create));
                    doc.Open();

                    // หัวใบเสร็จ
                    // กล่องสีด้านบน
                    PdfPTable topBar = new PdfPTable(1);
                    topBar.WidthPercentage = 100;

                    PdfPCell topCell = new PdfPCell(new Phrase("")); // ไม่มีข้อความ
                    topCell.BackgroundColor = myColor;
                    topCell.Border = PdfPCell.NO_BORDER;
                    topCell.FixedHeight = 20f;
                    topBar.AddCell(topCell);

                    doc.Add(topBar);

                    var fontTitle = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 16);
                    var fontSubTitle = FontFactory.GetFont(FontFactory.HELVETICA, 12);
                    Paragraph company = new Paragraph("Winnie's Cookies", fontTitle);
                    company.Alignment = Element.ALIGN_CENTER;
                    doc.Add(company);

                    Paragraph address1 = new Paragraph("101/99 pixxies hollow", fontNormal);
                    address1.Alignment = Element.ALIGN_RIGHT;
                    doc.Add(address1);

                    Paragraph address2 = new Paragraph("Neverland 1604", fontNormal);
                    address2.Alignment = Element.ALIGN_RIGHT;
                    doc.Add(address2);

                    doc.Add(iTextSharp.text.Chunk.NEWLINE);

                    
                    PdfPTable infoTable = new PdfPTable(3);

                   
                    infoTable.WidthPercentage = 100;

                    
                    
                    infoTable.AddCell(CreateCell($"เลขคำสั่งซื้อ : {order_id}", fontNormal, PdfPCell.NO_BORDER));

                    
                    infoTable.AddCell(CreateCell($"", fontNormal, PdfPCell.NO_BORDER));

                    
                    infoTable.AddCell(CreateCell($"วันที่ : {DateTime.Now:dd MM yyyy}", fontNormal, PdfPCell.NO_BORDER));

                    
                    doc.Add(infoTable);

                    
                    doc.Add(iTextSharp.text.Chunk.NEWLINE);

                    
                    doc.Add(new Paragraph("ข้อมูลผู้ซื้อ", fontBold));

                    
                    PdfPTable customerTable = new PdfPTable(1);

                   
                    customerTable.WidthPercentage = 100;

                    
                    customerTable.AddCell(CreateCell($"ชื่อ : {userName}", fontNormal, PdfPCell.NO_BORDER));

                   
                    customerTable.AddCell(CreateCell($"เบอร์โทรศัพท์: {userTel}", fontNormal, PdfPCell.NO_BORDER));

                    
                    doc.Add(customerTable);


                    // ตารางสินค้า

                    BaseColor headerTextColor = BaseColor.WHITE;

                    PdfPTable productTable = new PdfPTable(4);
                    productTable.WidthPercentage = 100;
                    productTable.SetWidths(new float[] { 4, 1, 2, 2 });

                    productTable.AddCell(CreateCell("สินค้า", fontBold, PdfPCell.BOX, Element.ALIGN_CENTER, myColor, headerTextColor));
                    productTable.AddCell(CreateCell("จำนวน", fontBold, PdfPCell.BOX, Element.ALIGN_CENTER, myColor, headerTextColor));
                    productTable.AddCell(CreateCell("ราคาต่อหน่วย", fontBold, PdfPCell.BOX, Element.ALIGN_CENTER, myColor, headerTextColor));
                    productTable.AddCell(CreateCell("รวม", fontBold, PdfPCell.BOX, Element.ALIGN_CENTER, myColor, headerTextColor));

                    double subtotal = 0;

                    using (MySqlConnection conn = new MySqlConnection(connectionString))
                    {
                        conn.Open();

                       
                        string selectCart = @"SELECT name AS product_name, amount, price, total 
                              FROM cart_cookie 
                              WHERE name_customer = @phone";
                        using (MySqlCommand selectCmd = new MySqlCommand(selectCart, conn))
                        {
                            selectCmd.Parameters.AddWithValue("@phone", label1.Text);    
                            using (MySqlDataReader reader = selectCmd.ExecuteReader())
                            
                            {
                                while (reader.Read()) 
                                {
                                   
                                    string pdfproductname = reader["product_name"].ToString(); 
                                    string pdfamount = Convert.ToDecimal(reader["amount"]).ToString("N0"); 
                                    string pdfprice = Convert.ToDecimal(reader["price"]).ToString("N2"); 
                                    string pdftotal = Convert.ToDecimal(reader["total"]).ToString("N2");  
                                    double unittotal = Convert.ToDouble(reader["total"]);
                                    subtotal += unittotal;
                                    productTable.AddCell(CreateCell(pdfproductname, fontNormal, PdfPCell.BOX, Element.ALIGN_LEFT)); 
                                    productTable.AddCell(CreateCell(pdfamount, fontNormal, PdfPCell.BOX, Element.ALIGN_CENTER));     
                                    productTable.AddCell(CreateCell(pdfprice, fontNormal, PdfPCell.BOX, Element.ALIGN_RIGHT));      
                                    productTable.AddCell(CreateCell(pdftotal, fontNormal, PdfPCell.BOX, Element.ALIGN_RIGHT));      

                                }
                            }
                        }
                    }
                    doc.Add(productTable);  
                    double total = subtotal; 
                    double vat = total * 7 / 100;
                    double netSubtotal = total - vat;  


                    PdfPTable summaryTable = new PdfPTable(4);

                    summaryTable.AddCell(CreateEmptyCell(2, PdfPCell.TOP_BORDER)); 
                    summaryTable.AddCell(CreateCell("ยอดสุทธิ (ก่อน VAT)", fontNormal, PdfPCell.BOX, Element.ALIGN_CENTER)); 
                    summaryTable.AddCell(CreateCell($"{netSubtotal.ToString("N2")} บาท", fontNormal, PdfPCell.BOX, Element.ALIGN_RIGHT)); 

                    summaryTable.AddCell(CreateEmptyCell(2));
                    summaryTable.AddCell(CreateCell("VAT 7%", fontNormal, PdfPCell.BOX, Element.ALIGN_CENTER)); 
                    summaryTable.AddCell(CreateCell($"{vat.ToString("N2")} บาท", fontNormal, PdfPCell.BOX, Element.ALIGN_RIGHT)); 
                    summaryTable.AddCell(CreateEmptyCell(2)); 
                    PdfPCell totalValue = new PdfPCell(new Phrase("ยอดรวม (รวม VAT)", new iTextSharp.text.Font(fontBold.BaseFont, fontBold.Size, fontBold.Style, BaseColor.WHITE))) // สร้างเซลล์สำหรับแสดงยอดรวมรวม VAT พร้อมตั้งค่าฟอนต์ตัวหนา สีขาว
                    {
                        BackgroundColor = myColor, 
                        HorizontalAlignment = Element.ALIGN_CENTER
                    };
                    summaryTable.AddCell(totalValue); // เพิ่มเซลล
                    summaryTable.AddCell(CreateCell($"{total.ToString("N2")} บาท", fontNormal, PdfPCell.BOX, Element.ALIGN_RIGHT)); 

                    doc.Add(summaryTable); 

                    doc.Add(iTextSharp.text.Chunk.NEWLINE); 
                    doc.Add(new Paragraph("", fontBold)); 




                    doc.Close();
                    Process.Start(receiptPosition);  



                }
                
            }
            return receiptPosition;
        }
        private PdfPCell CreateEmptyCell(int colspan, int border = PdfPCell.NO_BORDER)
        {
            PdfPCell cell = new PdfPCell(new Phrase(" ")); 
            if (border != null) cell.Border = border; 
            cell.Colspan = colspan; 
            return cell; 
        }

        private void AddCell(PdfPTable table, string text, iTextSharp.text.Font font, BaseColor textColor = default, BaseColor bgColor = default, int align = Element.ALIGN_LEFT)
        {
            PdfPCell cell = new PdfPCell(new Phrase(text, font)); 
            if (textColor != default) cell.Phrase.Font.Color = textColor;
            if (bgColor != default) cell.BackgroundColor = bgColor;
            table.AddCell(cell);
        }

        private PdfPCell CreateCell(string text, iTextSharp.text.Font baseFont, int border, int align = Element.ALIGN_LEFT, BaseColor bgColor = null, BaseColor textColor = null)
        {
            iTextSharp.text.Font font = (textColor != null)
                ? new iTextSharp.text.Font(baseFont.BaseFont, baseFont.Size, baseFont.Style, textColor) 
                : baseFont; 
            PdfPCell cell = new PdfPCell(new Phrase(text, font)); 
            cell.Border = border; 
            cell.HorizontalAlignment = align; 
            if (bgColor != null) cell.BackgroundColor = bgColor; 
            return cell; 
        }

        private void pictureBox1_Click(object sender, EventArgs e)
        {

        }

        private void dataGridView1_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
        }

        private void backtomb_Click(object sender, EventArgs e)
        {
            Form10 newForm = new Form10();
            newForm.phoneform10 = label1.Text;
            newForm.Show();

            this.Close();
        }

        private void Form15_Load_1(object sender, EventArgs e)
        {

        }

        private void label3_Click(object sender, EventArgs e)
        {

        }


    }
}


//using MySql.Data.MySqlClient;
//using System;
//using System.Collections.Generic;
//using System.ComponentModel;
//using System.Data;
//using System.Drawing;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;
//using System.Windows.Forms;
//using Saladpuk;
//using ZXing;
//using Saladpuk.PromptPay.Facades;
//using ZXing.Common;
//using System.IO;
//using System.Data.SqlClient;
//using iTextSharp.text.pdf;
//using iTextSharp.text;
//using static System.Windows.Forms.VisualStyles.VisualStyleElement;
//using System.Diagnostics;
//using Mysqlx.Crud;
//using static System.Windows.Forms.VisualStyles.VisualStyleElement.TrackBar;
//using System;
//using System.IO;
//using iTextSharp.text;
//using iTextSharp.text.pdf;
//using System.Security.Cryptography;

//namespace Part_Furnitur
//{
//    public partial class Form6 : Form
//    {
//        public Form6()
//        {
//            InitializeComponent();
//        }
//        private string _username;
//        public string usernameqrcode
//        {
//            get { return _username; }
//            set { _username = value; username_qrcode.Text = value; }
//        }

//        private string _tel;
//        public string telqrcode
//        {
//            get { return _tel; }
//            set { _tel = value; tel_qrcode.Text = value; }
//        }
//        private void Form6_Load(object sender, EventArgs e)
//        {
//            string connectionString = "datasource=127.0.0.1;port=3306;username=root;password=;database=the'part furnitur;charset=utf8;";
//            string query = "SELECT * FROM cart_product WHERE name_ctm = @namectm"; // เปลี่ยน @namectm ให้เป็น parameter
//            MessageBox.Show("load");
//            using (MySqlConnection myConn = new MySqlConnection(connectionString))
//            {
//                double totalSum = 0;
//                try
//                {
//                    myConn.Open();
//                    // ใช้ MySqlCommand แทน MySqlDataAdapter เพื่อเพิ่ม parameter
//                    using (MySqlCommand cmd = new MySqlCommand(query, myConn))
//                    {
//                        // กำหนดค่าให้กับ parameter @namectm
//                        cmd.Parameters.AddWithValue("@namectm", tel_qrcode.Text);  // หรือชื่อผู้ใช้ที่ต้องการดึงข้อมูล
//                        MessageBox.Show("load1");
//                        MySqlDataReader reader = cmd.ExecuteReader();
//                        while (reader.Read())
//                        {
//                            // Assuming 'total_product' is the column for price, read it and add to totalSum
//                            if (reader["total_product"] != DBNull.Value)
//                            {
//                                totalSum += Convert.ToDouble(reader["total_product"]);
//                            }
//                        }
//                        // แสดงผลรวมใน Label
//                        qrcode_total_price.Text = totalSum.ToString("N2");
//                        double vat = (totalSum * 7 / 100);
//                        label2.Text = vat.ToString("N2") + "฿";
//                        label3.Text = (vat + totalSum).ToString("N2") + "฿";
//                        string qrcode = PPay.DynamicQR.MobileNumber("0952659854").Amount(vat + totalSum).CreateCreditTransferQrCode();
//                        BarcodeWriter BW = new BarcodeWriter { Format = BarcodeFormat.QR_CODE, Options = new EncodingOptions { Width = 200, Height = 200, PureBarcode = true } };
//                        Bitmap QrcodeBitmap = BW.Write(qrcode);
//                        pictureBox1.Image = QrcodeBitmap;
//                    }
//                    myConn.Close();
//                }
//                catch (Exception ex)
//                {
//                    MessageBox.Show("เกิดข้อผิดพลาด: " + ex.Message);
//                }
//            }
//        }

//        private void button4_Click(object sender, EventArgs e)
//        {

//        }

//        private void button3_Click(object sender, EventArgs e)
//        {
//            {
//                card c = new card();
//                c.telcart = telqrcode.ToString();
//                c.username_shop = _username;
//                c.Show();
//                this.Hide();
//            }
//        }

//        private void button5_Click(object sender, EventArgs e)
//        {
//            Login login = new Login();
//            login.Show();
//            this.Hide();
//        }
//        private byte[] selectedImageBytes;

//        private void button1_Click(object sender, EventArgs e)
//        {
//            using (OpenFileDialog ofd = new OpenFileDialog())
//            {
//                ofd.Title = "เลือกรูปภาพเพื่ออัปโหลด";
//                ofd.Filter = "Image Files|*.jpg;*.jpeg;*.png;*.bmp";

//                if (ofd.ShowDialog() == DialogResult.OK)
//                {
//                    string imagePath = ofd.FileName;
//                    selectedImageBytes = File.ReadAllBytes(imagePath);
//                    label1.Text = "✅";
//                    label1.ForeColor = Color.Green;
//                    MessageBox.Show("เลือกรูปภาพเรียบร้อยแล้ว!", "สำเร็จ", MessageBoxButtons.OK, MessageBoxIcon.Information);
//                }
//            }
//        }
//        private string order_id = "";
//        private void button2_Click(object sender, EventArgs e)
//        {
//            string connectionString = "datasource=127.0.0.1;port=3306;username=root;password=;database=the'part furnitur;charset=utf8;";


//            if (selectedImageBytes == null)
//            {
//                MessageBox.Show("กรุณาเลือกรูปใบเสร็จก่อนยืนยันการสั่งซื้อ!");
//                return;
//            }

//            string query = @"SELECT MAX(order_id) FROM ( SELECT order_id FROM order_product UNION SELECT order_id FROM history ) AS combined WHERE order_id LIKE @order_id";
//            string OrderId = "";

//            using (MySqlConnection conn = new MySqlConnection(connectionString))
//            {
//                conn.Open();

//                using (MySqlCommand cmd = new MySqlCommand(query, conn))
//                {
//                    cmd.Parameters.AddWithValue("@order_id", "ORD" + "%");

//                    object result = cmd.ExecuteScalar();
//                    if (result != DBNull.Value && result != null)
//                    {
//                        // มี order_id อยู่แล้ว -> ดึงเลขล่าสุดมาบวก
//                        MessageBox.Show(result.ToString());
//                        string lastOrderId = result.ToString(); // e.g. ORD-005
//                        string lastNumberStr = lastOrderId.Substring(lastOrderId.Length - 3); // "005"
//                        int lastNumber = int.Parse(lastNumberStr);
//                        int nextNumber = lastNumber + 1;

//                        OrderId = $"ORD-{nextNumber.ToString("D3")}"; //D3 คือถ้าค่ามา 1 จะแสดงเป็น 001 ก็คือทำให้ครบ3หลัก
//                    }
//                    else
//                    {
//                        // ยังไม่มี order เริ่มที่ 001
//                        OrderId = $"ORD-001";
//                    }
//                }
//            }


//            using (MySqlConnection conn = new MySqlConnection(connectionString))
//            {
//                conn.Open();

//                // 1. ดึงข้อมูลทั้งหมดจาก cart แล้วเก็บไว้ใน List
//                MySqlCommand getCartCmd = new MySqlCommand("SELECT id, name_ctm, name_product, type_product, quantiy_product, price_product, total_product FROM cart_product WHERE name_ctm = @namectm", conn);
//                getCartCmd.Parameters.AddWithValue("@namectm", tel_qrcode.Text);

//                MySqlDataReader reader = getCartCmd.ExecuteReader();

//                // สร้าง list ไว้เก็บข้อมูลที่อ่านจาก reader
//                var cartItems = new List<(int id, string NameCtm, string NameProduct, string TypeProduct, int Quantity, decimal Price, decimal Total)>();

//                while (reader.Read())
//                {

//                    cartItems.Add((
//                        Convert.ToInt32(reader["id"]),
//                        reader["name_ctm"].ToString(),
//                        reader["name_product"].ToString(),
//                        reader["type_product"].ToString(),
//                        Convert.ToInt32(reader["quantiy_product"]),
//                        Convert.ToDecimal(reader["price_product"]),
//                        Convert.ToDecimal(reader["total_product"])
//                    ));
//                }

//                reader.Close(); // ปิด reader ก่อน insert
//                // 2. แทรกข้อมูลเข้า order_product
//                foreach (var item in cartItems)
//                {
//                    MySqlCommand insertCmd = new MySqlCommand(@"
//            INSERT INTO order_product 
//            (order_id, name_ctm, name_product, type_product, quantity_product, price_product, total_product, receipt_image, order_date)
//            VALUES 
//            (@order_id, @name_ctm, @name_product, @type_product, @quantity, @price, @total, @receipt_image, NOW())", conn);

//                    order_id = OrderId;
//                    insertCmd.Parameters.AddWithValue("@order_id", OrderId);
//                    insertCmd.Parameters.AddWithValue("@name_ctm", item.NameCtm);
//                    insertCmd.Parameters.AddWithValue("@name_product", item.NameProduct);
//                    insertCmd.Parameters.AddWithValue("@type_product", item.TypeProduct);
//                    insertCmd.Parameters.AddWithValue("@quantity", item.Quantity);
//                    insertCmd.Parameters.AddWithValue("@price", item.Price);
//                    insertCmd.Parameters.AddWithValue("@total", item.Total);
//                    insertCmd.Parameters.AddWithValue("@receipt_image", selectedImageBytes);

//                    insertCmd.ExecuteNonQuery();
//                }
//                createpdf();

//                // 3. ล้าง cart ของผู้ใช้คนนั้น
//                MySqlCommand clearCartCmd = new MySqlCommand("DELETE FROM cart_product WHERE name_ctm = @namectm", conn);
//                clearCartCmd.Parameters.AddWithValue("@namectm", tel_qrcode.Text);
//                clearCartCmd.ExecuteNonQuery();

//                MessageBox.Show("ยืนยันคำสั่งซื้อและบันทึกข้อมูลเรียบร้อยแล้ว!", "สำเร็จ", MessageBoxButtons.OK, MessageBoxIcon.Information);
//            }
//        }

//        

//        private void AddCell(PdfPTable table, string text, iTextSharp.text.Font font)
//        {
//            PdfPCell cell = new PdfPCell(new Phrase(text, font));
//            table.AddCell(cell);
//        }

//        private PdfPCell CreateNoBorderCell(string text, iTextSharp.text.Font font)
//        {
//            PdfPCell cell = new PdfPCell(new Phrase(text, font));
//            cell.Border = iTextSharp.text.Rectangle.NO_BORDER;
//            return cell;
//        }
//    }
//}
