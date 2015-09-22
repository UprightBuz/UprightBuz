using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using MarketplaceWebServiceOrders;
using MarketplaceWebServiceProducts;
using MarketplaceWebService.Samples;
using System.IO;
using AmazonTest.src;
using Microsoft.Office.Interop.Excel;
using Microsoft.Office.Core;
using System.Net;
using System.Threading;
//using System.Reflection;


namespace AmazonTest
{
    public partial class Form1 : Form
    {
        AWSLogger awsLogger;
        System.Timers.Timer timerAdaptPrice;       // 自动调价计时器
        System.Timers.Timer timerUpdateInventory;  // 更新库存定时器
        System.Timers.Timer timerUpdateShippingStatus;   // 更新投递状态
        System.Timers.Timer timerRequestFeedback;   // 发送feedback request
        bool requestFeedbackRunning;  // 请求评论功能是否正在运行

        public Form1()
        {
            requestFeedbackRunning = false;
            InitializeComponent();
            // MarketplaceWebServiceOrdersSample.RunSample();   
            // MarketplaceWebServiceProductsSample.RunSample();
            // MarketplaceWebServiceSamples.RunSample();
            // AdaptPrice.RunAdaptPrice();
            // MarketplaceWebServiceSamples.GetInventoryReport();
            // updateInventory();

            // Console.WriteLine(GlobalConfig.Instance.TimeFormat);
            //getUnshippedOrders();
            //fillShipFile("", "");
            //SendEmail.Instance.sendEmail("1064782986@qq.com");
            //CreateGetHttpResponse("https://tools.usps.com/go/TrackConfirmAction?qtc_tLabels1=LS075710627CN");
            //Console.WriteLine(GetShippingStatus("LS075710627CN"));
            //Console.WriteLine(GetShippingStatus("LS057002658CN"));
            //Console.WriteLine(GetShippingStatus("LS092049339CN"));
            //Console.WriteLine(GetShippingStatus("LS075710627fdCN"));
            //string s = EscapeMysqlString("a'b");
            //getHistoryOrders();
            //updateOrderTrackingId();
            //updateShipingStatus();
            //SendFeedbackRequestEmail();
            //string fileName = "confirmOrderFeed/123.txt";
            //MarketplaceWebServiceSamples.SubmitFeed(fileName, GlobalConfig.Instance.ConfirmOrderFeedType);
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            timerAdaptPrice = new System.Timers.Timer();
            timerAdaptPrice.Interval = 15 * 60 * 1000;  // 间隔时间为15min
            timerAdaptPrice.Elapsed += new System.Timers.ElapsedEventHandler(adaptPrice);

            timerUpdateInventory = new System.Timers.Timer();
            timerUpdateInventory.Interval = 2 * 60 * 60 * 1000;  // 间隔时间为2h
            timerUpdateInventory.Elapsed += new System.Timers.ElapsedEventHandler(updateInventory);

            timerUpdateShippingStatus = new System.Timers.Timer();
            timerUpdateShippingStatus.Interval = 24 * 60 * 60 * 1000;  // 间隔时间为24h
            timerUpdateShippingStatus.Elapsed += new System.Timers.ElapsedEventHandler(UpdateShipingStatus);

            timerRequestFeedback = new System.Timers.Timer();
            timerRequestFeedback.Interval = 12 * 60 * 60 * 1000;  // 间隔时间为12h
            timerRequestFeedback.Elapsed += new System.Timers.ElapsedEventHandler(RequestFeedback);

            // 显示未发货订单
            dgv_unshipped_order_BoundDS();

            // 更新货运状态
            Thread threadUpdateShipingStatus = new Thread(UpdateShipingStatus);
            threadUpdateShipingStatus.Start();

            // 发送feedback request
            Thread threadSendFeedbackRequestEmail = new Thread(SendFeedbackRequestEmail);
            threadSendFeedbackRequestEmail.Start();
        }


        private void button1_Click(object sender, EventArgs e)
        {
            timerAdaptPrice.Enabled = true;
            timerUpdateInventory.Enabled = true;
            timerUpdateShippingStatus.Enabled = true;
            timerRequestFeedback.Enabled = true;
        }

        private void button2_Click(object sender, EventArgs e)
        {
            timerAdaptPrice.Enabled = false;
            timerUpdateInventory.Enabled = false;
            timerUpdateShippingStatus.Enabled = false;
            timerRequestFeedback.Enabled = false;
        }

        private void adaptPrice(object sender, System.Timers.ElapsedEventArgs e)
        {
            //*** 执行调价操作
            AdaptPrice.RunAdaptPrice();
        }

        public static bool SaveDataTableToExcel(System.Data.DataTable excelTable, string filePath)
        {
            Microsoft.Office.Interop.Excel.Application app =
                new Microsoft.Office.Interop.Excel.Application();
            try
            {
                app.Visible = false;
                Workbook wBook = app.Workbooks.Add(true);
                Worksheet wSheet = wBook.Worksheets[1] as Worksheet;
                if (excelTable.Rows.Count > 0)
                {
                    int row = 0;
                    row = excelTable.Rows.Count;
                    int col = excelTable.Columns.Count;
                    for (int i = 0; i < row; i++)
                    {
                        for (int j = 0; j < col; j++)
                        {
                            string str = excelTable.Rows[i][j].ToString();
                            wSheet.Cells[i + 2, j + 1] = str;
                        }
                    }
                }

                int size = excelTable.Columns.Count;
                for (int i = 0; i < size; i++)
                {
                    wSheet.Cells[1, 1 + i] = excelTable.Columns[i].ColumnName;
                }
                //设置禁止弹出保存和覆盖的询问提示框 
                app.DisplayAlerts = false;
                app.AlertBeforeOverwriting = false;
                //保存工作簿 
                wBook.Save();
                //保存excel文件 
                app.Save(filePath);
                app.SaveWorkspace(filePath);
                app.Quit();
                app = null;
                return true;
            }
            catch (Exception err)
            {
                MessageBox.Show("导出Excel出错！错误原因：" + err.Message, "提示信息",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return false;
            }
            finally
            {
            }
        }


        private void fillShipFile()
        {
 
            try
            {
                //*** 从数据库中读取未发货订单
                DataSet ds = MySqlHelper.GetDataSet(MySqlHelper.Conn, System.Data.CommandType.Text,
                    "select a.*, b.sku, b.quantity from t_order_basic as a, t_order_product as b where a.order_id=b.order_id and a.order_status=0 and a.order_type=0", null);

                string srcFilePath = System.Environment.CurrentDirectory  + "\\shippingInfo\\shiptemplate.xls";


                //*** read shipping template
                Microsoft.Office.Interop.Excel.Application excel = new Microsoft.Office.Interop.Excel.ApplicationClass();
                excel.Visible = false;
                Workbook wBook = excel.Workbooks.Open(srcFilePath, Type.Missing, Type.Missing, Type.Missing, Type.Missing, Type.Missing, Type.Missing, Type.Missing, Type.Missing, Type.Missing, Type.Missing, Type.Missing, Type.Missing, Type.Missing, Type.Missing);
                Worksheet wSheet = wBook.Sheets[1] as Worksheet; //第一个sheet页

                //*** write order info
                int rowIndex = 2;
                // E邮宝业务类型
                string eubBuzType = GlobalConfig.Instance.EubBuzType;  
                foreach (DataRow dr in ds.Tables[0].Rows)
                {
                    wSheet.Cells[rowIndex, 1] = dr["order_id"].ToString();
                    wSheet.Cells[rowIndex, 3] = dr["sku"].ToString();
                    wSheet.Cells[rowIndex, 4] = dr["quantity"].ToString();
                    wSheet.Cells[rowIndex, 5] = dr["recipient_name"].ToString();
                    wSheet.Cells[rowIndex, 6] = dr["ship_address_1"].ToString();
                    wSheet.Cells[rowIndex, 7] = dr["ship_address_2"].ToString();
                    wSheet.Cells[rowIndex, 8] = dr["ship_address_3"].ToString();
                    wSheet.Cells[rowIndex, 9] = dr["ship_city"].ToString();
                    wSheet.Cells[rowIndex, 10] = dr["ship_state"].ToString();
                    wSheet.Cells[rowIndex, 11] = "'" + dr["ship_postal_code"].ToString();                
                    wSheet.Cells[rowIndex, 12] = dr["ship_country"].ToString();
                    wSheet.Cells[rowIndex, 13] = "'" + dr["buyer_phone"].ToString();
                    wSheet.Cells[rowIndex, 20] = eubBuzType;
                    ++rowIndex;
                }

                //****设置禁止弹出保存和覆盖的询问提示框 
                excel.DisplayAlerts = false;
                excel.AlertBeforeOverwriting = true;

                //string

                //保存
                DateTime now = DateTime.Now;
                string destFilePath = String.Format(System.Environment.CurrentDirectory + "\\shippingInfo\\ship_{0}.xls", now.ToString(GlobalConfig.Instance.TimeFormat)); 
                wSheet.SaveAs(destFilePath, System.Type.Missing, System.Type.Missing, System.Type.Missing, System.Type.Missing, System.Type.Missing, System.Type.Missing, System.Type.Missing, System.Type.Missing, System.Type.Missing);
                wBook.Save();
                ///*****close excel 
                wBook.Close();
                excel.Quit();
                //System.Runtime.InteropServices.Marshal.ReleaseComObject(excel);
                //System.Runtime.InteropServices.Marshal.ReleaseComObject(wSheet);
                //System.Runtime.InteropServices.Marshal.ReleaseComObject(wBook);
                wSheet = null;       
                wBook = null;   
                excel = null;
                //System.GC.Collect(0);

            }
            catch (Exception er)
            {

                Console.WriteLine("Error in shipping File");
            }
          
        
         }

        private void getUnshippedOrders()
        {
            //*** 获取订单文件
            string fileName = String.Format("orderReport/report_{0}.txt", DateTime.Now.ToString(GlobalConfig.Instance.TimeFormat));
            MarketplaceWebServiceSamples.GetReport(GlobalConfig.Instance.UnshippedOrderReportType, fileName);

            //string fileName = "orderReport/230484533016688.txt";
            StreamReader f = File.OpenText(fileName);
            string line = f.ReadLine();
            // 校验第一行
            if (line != null)
            {
                string[] words = line.Split('\t');
                string[] expectWords = { "order-id", "order-item-id", "purchase-date", "payments-date", "reporting-date", "promise-date",
                    "days-past-promise", "buyer-email", "buyer-name", "buyer-phone-number", "sku", "product-name", "quantity-purchased",
                    "quantity-shipped", "quantity-to-ship", "ship-service-level", "recipient-name", "ship-address-1", "ship-address-2",
                    "ship-address-3", "ship-city", "ship-state", "ship-postal-code", "ship-country" };
                if (string.Join(",", words) == string.Join(",", expectWords))
                {
                    line = f.ReadLine();
                    List<string> orderCmdTextList = new List<string>();
                    List<string> orderProductCmdTextList = new List<string>();
                    const int default_order_status = 0;
                    const int default_order_type = 0;
                    while (line != null)
                    {
                        words = EscapeMysqlString(line).Split('\t');
                        /*
                        order_id payments_date buyer_name buyer_email buyer_phone recipient_name ship_address_1 ship_address_2 ship_address_3 
                        ship_city ship_state ship_postal_code ship_country order_status: 0未发货、1已发货 order_type: 0正常、1假发货、2待观察订单
                        */
                        // 将utc格式的时间转换成mysql的datetime格式 ：2015-09-09T23:29:17-07:00
                        string[] date = words[3].Split(new char[] { '-', 'T', ':'}, StringSplitOptions.RemoveEmptyEntries);
                        // 转换成本地时间，美国时间有15个小时的时差
                        DateTime payments_date = Convert.ToDateTime(string.Format("{0}-{1}-{2} {3}:{4}:{5}", date[0], date[1], date[2], date[3], date[4], date[5])).AddHours(15);  
                        string payments_date_local = payments_date.ToString("yyyy-MM-dd HH:mm:ss");

                        orderCmdTextList.Add(String.Format("('{0}', '{1}', '{2}', '{3}', '{4}', '{5}', '{6}', '{7}', '{8}', '{9}', '{10}', '{11}', '{12}', {13}, {14})",
                            words[0], payments_date_local, words[8], words[7], words[9], words[16], 
                            words[17], words[18], words[19], words[20], words[21], words[22], words[23], default_order_status, default_order_type));
                        orderProductCmdTextList.Add(String.Format("('{0}', '{1}', {2}, '{3}')", words[0], words[10], words[12], words[11])); // order_id sku quantity product_name
                        line = f.ReadLine();
                    }
                    f.Close();
                    if (orderCmdTextList.Count > 0)
                    {
                        //*** 更新到数据库
                        string cmdText = String.Format("insert ignore into t_order_basic (order_id, payments_date, buyer_name, buyer_email, buyer_phone, recipient_name, ship_address_1, ship_address_2," +
                            "ship_address_3, ship_city, ship_state, ship_postal_code, ship_country, order_status, order_type)" +
                            " values {0};", String.Join(",", orderCmdTextList));
                        cmdText += String.Format("insert ignore into t_order_product (order_id, sku, quantity, product_name) values {0}", String.Join(",", orderProductCmdTextList));
                        int affectedRowsCount = MySqlHelper.ExecuteNonQuery(MySqlHelper.Conn, System.Data.CommandType.Text, cmdText, null);
                        Console.WriteLine("Affected Rows: " + affectedRowsCount);
                        
                    }
                }
                else
                {
                    // todo 写入log
                }
            }
            else
            {
                // todo 写入log
            }
        }

        private void getHistoryOrders()
        {

            string fileName = "orderReport/276821994016698.txt";
            StreamReader f = File.OpenText(fileName);
            string line = f.ReadLine();
            // 校验第一行
            if (line != null)
            {
                string[] words = line.Split('\t');
                string[] expectWords = { "order-id", "order-item-id", "purchase-date", "payments-date", "buyer-email", "buyer-name", "buyer-phone-number",
                    "sku", "product-name", "quantity-purchased", "currency", "item-price", "item-tax", "shipping-price", "shipping-tax", "ship-service-level",
                    "recipient-name", "ship-address-1", "ship-address-2", "ship-address-3", "ship-city", "ship-state", "ship-postal-code", "ship-country",
                    "ship-phone-number", "delivery-start-date", "delivery-end-date", "delivery-time-zone", "delivery-Instructions" };
                if (string.Join(",", words) == string.Join(",", expectWords))
                {
                    line = f.ReadLine();
                    List<string> orderCmdTextList = new List<string>();
                    List<string> orderProductCmdTextList = new List<string>();
                    const int default_order_status = 1;
                    const int default_order_type = 0;
                    while (line != null)
                    {
                        words = EscapeMysqlString(line).Split('\t');
                        /*
                        order_id payments_date buyer_name buyer_email buyer_phone recipient_name ship_address_1 ship_address_2 ship_address_3 
                        ship_city ship_state ship_postal_code ship_country order_status: 0未发货、1已发货 order_type: 0正常、1假发货、2待观察订单
                        */
                        // 将utc格式的时间转换成mysql的datetime格式 ：2015-09-09T23:29:17-07:00
                        string[] date = words[3].Split(new char[] { '-', 'T', ':' }, StringSplitOptions.RemoveEmptyEntries);
                        // 转换成本地时间，美国时间有15个小时的时差
                        DateTime payments_date = Convert.ToDateTime(string.Format("{0}-{1}-{2} {3}:{4}:{5}", date[0], date[1], date[2], date[3], date[4], date[5])).AddHours(15);
                        string payments_date_local = payments_date.ToString("yyyy-MM-dd HH:mm:ss");

                        orderCmdTextList.Add(String.Format("('{0}', '{1}', '{2}', '{3}', '{4}', '{5}', '{6}', '{7}', '{8}', '{9}', '{10}', '{11}', '{12}', {13}, {14})",
                            words[0], payments_date_local, words[5], words[4], words[24], words[16],
                            words[17], words[18], words[19], words[20], words[21], words[22], words[23], default_order_status, default_order_type));
                        orderProductCmdTextList.Add(String.Format("('{0}', '{1}', {2}, '{3}')", words[0], words[7], words[9], words[8])); // order_id sku quantity product_name
                        line = f.ReadLine();
                    }
                    f.Close();
                    if (orderCmdTextList.Count > 0)
                    {
                        //*** 更新到数据库
                        string cmdText = String.Format("insert ignore into t_order_basic (order_id, payments_date, buyer_name, buyer_email, buyer_phone, recipient_name, ship_address_1, ship_address_2," +
                            "ship_address_3, ship_city, ship_state, ship_postal_code, ship_country, order_status, order_type)" +
                            " values {0};", String.Join(",", orderCmdTextList));
                        cmdText += String.Format("insert ignore into t_order_product (order_id, sku, quantity, product_name) values {0}", String.Join(",", orderProductCmdTextList));
                        int affectedRowsCount = MySqlHelper.ExecuteNonQuery(MySqlHelper.Conn, System.Data.CommandType.Text, cmdText, null);
                        Console.WriteLine("Affected Rows: " + affectedRowsCount);

                    }
                }
                else
                {
                    // todo 写入log
                }
            }
            else
            {
                // todo 写入log
            }
        }

        private void SendFeedbackRequestEmail()
        {
            Console.WriteLine("SendFeedbackRequestEmail");
            if (!requestFeedbackRunning)  // 同一时间只有一个在运行，避免重复发送邮件
            {
                requestFeedbackRunning = true;
                // 从数据库读取投递成功的订单列表：投递成功 & 已经到了2天
                string cmdText = String.Format("select order_id, buyer_name, buyer_email from t_order_basic where is_feedback=0 and is_delivered=1 and feedback_request_count=0 and deliver_date<='{0}'", DateTime.Now.AddDays(-2).ToString("yyyy-MM-dd"));
                DataSet ds = MySqlHelper.GetDataSet(MySqlHelper.Conn, System.Data.CommandType.Text, cmdText, null);
                StreamReader f = File.OpenText("feedbackRequestEmailTemplate.html");
                string emailTemplate = f.ReadToEnd();
                f.Close();
                // 替换公共配置
                emailTemplate = emailTemplate.Replace("${seller_id}", GlobalConfig.Instance.SellerId).Replace("${marketplace_id}", GlobalConfig.Instance.MarketplaceId);
                string subjectTemplate = "Please Help by Leaving Feedback for Your Amazon Order {0}";
                string now = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                int sendCount = 1;
                foreach (DataRow dr in ds.Tables[0].Rows)
                {
                    // 查询订单的商品列表
                    DataSet ds1 = MySqlHelper.GetDataSet(MySqlHelper.Conn, System.Data.CommandType.Text,
                    String.Format("select product_name from t_order_product where order_id='{0}'", dr["order_id"]), null);
                    string product_list = "";
                    foreach (DataRow dr1 in ds1.Tables[0].Rows)
                    {
                        product_list += String.Format("<li>{0}</li>", dr1["product_name"]);
                    }
                    string body = emailTemplate.Replace("${order_id}", dr["order_id"].ToString()).Replace("${product_list}", product_list).Replace("${buyer_name}", dr["buyer_name"].ToString());
                    if (SendEmail.Instance.sendEmail(dr["buyer_email"].ToString(), String.Format(subjectTemplate, dr["order_id"]), body))
                    {
                        // 回写数据库
                        MySqlHelper.ExecuteNonQuery(MySqlHelper.Conn, System.Data.CommandType.Text,
                            String.Format("update t_order_basic set feedback_request_count=feedback_request_count+1, last_feedback_request_time=curtime() where order_id='{0}'", dr["order_id"].ToString()), null);
                        Console.WriteLine(sendCount + " order_id:" + dr["order_id"].ToString());
                        sendCount++;
                        System.Threading.Thread.Sleep(10 * 1000);
                    }
                    else
                    {
                        // todo 进行告警
                        requestFeedbackRunning = false;
                        return;
                    }
                }
                requestFeedbackRunning = false;
            }
        }

        private void UpdateShipingStatus(object sender, System.Timers.ElapsedEventArgs e)
        {
            UpdateShipingStatus();
        }

        private void UpdateShipingStatus()
        {
            Console.WriteLine("UpdateShipingStatus");
            // 从数据库中读取订单：发货中 & 正常订单 & 已经发货7天
            string cmdText = String.Format("select track_id from t_order_basic where order_status=1 and order_type=0 and is_delivered=0 and track_id!='' and payments_date<='{0}'", DateTime.Now.AddDays(-7));
            DataSet ds = MySqlHelper.GetDataSet(MySqlHelper.Conn, System.Data.CommandType.Text, cmdText, null);
            List<string> cmdList = new List<string>();
            foreach(DataRow dr in ds.Tables[0].Rows)
            {
                Console.WriteLine(dr["track_id"]);
                int is_delivered = GetShippingStatus(dr["track_id"].ToString());
                Console.WriteLine(cmdList.Count);
                if (is_delivered != 0)  // 未送达的不需要更新
                {
                    cmdList.Add(String.Format("update t_order_basic set is_delivered={0}, deliver_date='{1}' where track_id='{2}'", is_delivered, DateTime.Now.ToString("yyyy-MM-dd"), dr["track_id"]));
                }             
            }
            if (cmdList.Count > 0)
            {
                // 更新订单投递状态
                MySqlHelper.ExecuteNonQuery(MySqlHelper.Conn, System.Data.CommandType.Text, String.Join(";", cmdList), null);
            }
        }


        private void RequestFeedback(object sender, System.Timers.ElapsedEventArgs e)
        {
            SendFeedbackRequestEmail();
        }


        private void updateInventory(object sender, System.Timers.ElapsedEventArgs e)
        //private void updateInventory()
        {
            //*** 获取库存报告文件
            string fileName = String.Format("inventoryReport/report_{0}.txt", DateTime.Now.ToString(GlobalConfig.Instance.TimeFormat));
            MarketplaceWebServiceSamples.GetReport(GlobalConfig.Instance.InventoryReportType, fileName);
            //string filename = "inventoryReport/report_20150907_233758.txt";
            StreamReader f = File.OpenText(fileName);
            string line = f.ReadLine();
            // 校验第一行
            if (line != null)
            {
                string[] words = line.Split('\t');
                string[] expectWords = { "sku", "asin", "price", "quantity" };
                if (string.Join(",", words)==string.Join(",", expectWords))
                {
                    line = f.ReadLine();
                    List<string> cmdTextList = new List<string>();
                    while (line != null)
                    {
                        words = line.Split('\t');
                        cmdTextList.Add(String.Format("('{0}', '{1}', {2})", words[0], words[1], words[3]));
                        line = f.ReadLine();
                    }
                    f.Close();
                    if (cmdTextList.Count > 0)
                    {
                        //*** 更新到数据库
                        string cmdText = String.Format("INSERT t_product (sku, asin, quantity) values {0} ON DUPLICATE KEY UPDATE asin=VALUES(asin), quantity=VALUES(quantity)", String.Join(",", cmdTextList));
                        int affectedRowsCount = MySqlHelper.ExecuteNonQuery(MySqlHelper.Conn, System.Data.CommandType.Text, cmdText, null);
                        Console.WriteLine("Affected Rows: " + affectedRowsCount);
                    }
                }
                else
                {
                    // todo 写入log
                }
            }
            else
            {
                // todo 写入log
            }
            
        }

        private void Form1_FormClosed(object sender, FormClosedEventArgs e)
        {
            this.Dispose();
            this.Close();
        }

        private void btn_get_order_report_Click(object sender, EventArgs e)
        {
            DialogResult dlg = MessageBox.Show("正在从亚马逊拉取未发货订单入库，请稍等...", "提示", MessageBoxButtons.OK);
            // 从亚马逊拉取未发货订单入库
            getUnshippedOrders();
            // 绑定数据源
            dgv_unshipped_order_BoundDS();
            
        }

        private void dgv_unshipped_order_BoundDS()
        {
            //*** 从数据库中读取未发货订单
            DataSet ds = MySqlHelper.GetDataSet(MySqlHelper.Conn, System.Data.CommandType.Text,
                "select a.*, b.sku, b.quantity from t_order_basic as a, t_order_product as b where a.order_id=b.order_id and a.order_status=0 and a.order_type=0", null);

            // 绑定到datagridview
            dgv_unshipped_order.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgv_unshipped_order.DataSource = ds;
            dgv_unshipped_order.DataMember = "Table";
            dgv_unshipped_order.ReadOnly = true;
            dgv_unshipped_order.AllowUserToDeleteRows = false;
            dgv_unshipped_order.AllowUserToAddRows = false;
            dgv_unshipped_order.AutoResizeColumn(0);
        }

        private void dgv_fake_shipped_order_BoundDS()
        {
            //*** 从数据库中读取假发货订单
            DataSet ds = MySqlHelper.GetDataSet(MySqlHelper.Conn, System.Data.CommandType.Text,
                "select * from t_order_basic where order_status=0 and order_type=1", null);

            // 绑定到datagridview
            dgv_fake_unshipped_order.SelectionMode = DataGridViewSelectionMode.CellSelect;
            dgv_fake_unshipped_order.DataSource = ds;
            dgv_fake_unshipped_order.DataMember = "Table";
            dgv_fake_unshipped_order.ReadOnly = true;
            dgv_fake_unshipped_order.AllowUserToDeleteRows = false;
            dgv_fake_unshipped_order.AllowUserToAddRows = false;
            dgv_fake_unshipped_order.AutoResizeColumn(0);
        }

        private void dgv_observed_order_BoundDS()
        {
            //*** 从数据库中读取待观察订单
            DataSet ds = MySqlHelper.GetDataSet(MySqlHelper.Conn, System.Data.CommandType.Text,
                "select * from t_order_basic where order_status=0 and order_type=2", null);

            // 绑定到datagridview
            dgv_observed_order.SelectionMode = DataGridViewSelectionMode.CellSelect;
            dgv_observed_order.DataSource = ds;
            dgv_observed_order.DataMember = "Table";
            dgv_observed_order.ReadOnly = true;
            dgv_observed_order.AllowUserToDeleteRows = false;
            dgv_observed_order.AllowUserToAddRows = false;
            dgv_observed_order.AutoResizeColumn(0);
        }

        private void dgv_ship_failed_order_BoundDS()
        {
            //*** 从数据库中读取发货失败订单
            DataSet ds = MySqlHelper.GetDataSet(MySqlHelper.Conn, System.Data.CommandType.Text,
                "select * from t_order_basic where is_delivered=2", null);

            // 绑定到datagridview
            dgv_ship_failed_order.SelectionMode = DataGridViewSelectionMode.CellSelect;
            dgv_ship_failed_order.DataSource = ds;
            dgv_ship_failed_order.DataMember = "Table";
            dgv_ship_failed_order.ReadOnly = true;
            dgv_ship_failed_order.AllowUserToDeleteRows = false;
            dgv_ship_failed_order.AllowUserToAddRows = false;
            dgv_ship_failed_order.AutoResizeColumn(0);
        }

        private void btn_tag_observed_Click(object sender, EventArgs e)
        {
            // 标记为待观察订单
            List<string> observedOrderList = new List<string>();
            List<string> observedOrderIdList = new List<string>();
            foreach(DataGridViewRow c in dgv_unshipped_order.SelectedRows)
            {
                observedOrderList.Add(String.Format("{0} {1} {2}", c.Cells["order_id"].Value.ToString(), c.Cells["buyer_name"].Value.ToString(), c.Cells["sku"].Value.ToString()));
                observedOrderIdList.Add(String.Format("'{0}'", c.Cells["order_id"].Value.ToString()));
            }

            // 弹出确认对话框
            string msgBoxText = String.Format("确定要将以下订单标记为待观察吗？\n{0}", String.Join("\n", observedOrderList));
            DialogResult dlgResult = MessageBox.Show(msgBoxText, "确认", MessageBoxButtons.OKCancel);
            if (dlgResult == DialogResult.OK)
            {
                // 写入数据库
                string cmdText = String.Format("update t_order_basic set order_type=2 where order_id in ({0})", String.Join(",", observedOrderIdList));
                int affectedRowsCount = MySqlHelper.ExecuteNonQuery(MySqlHelper.Conn, System.Data.CommandType.Text, cmdText, null);
                Console.WriteLine("Affected Rows: " + affectedRowsCount);

                // 刷新datagridview
                dgv_unshipped_order_BoundDS();
            }
        }


        private void btn_export_eub_excel_Click(object sender, EventArgs e)
        {
            // 生成eub的订单文件
            fillShipFile();
            MessageBox.Show("导出成功！", "提示", MessageBoxButtons.OK);
        }

        private void dgv_unshipped_order_RowPostPaint(object sender, DataGridViewRowPostPaintEventArgs e)
        {
            SolidBrush solidBrush = new SolidBrush(dgv_unshipped_order.RowHeadersDefaultCellStyle.ForeColor);
            e.Graphics.DrawString((e.RowIndex + 1).ToString(), e.InheritedRowStyle.Font, solidBrush, e.RowBounds.Location.X + 15, e.RowBounds.Location.Y + 5);
        }

        private void tabControl1_Selected(object sender, TabControlEventArgs e)
        {
            switch (e.TabPageIndex)
            {
                case 0:
                    dgv_unshipped_order_BoundDS();
                    break;
                case 1:
                    dgv_observed_order_BoundDS();
                    break;
                case 2:
                    dgv_fake_shipped_order_BoundDS();
                    break;
                case 3:
                    dgv_ship_failed_order_BoundDS();
                    break;
                default:
                    break;
            }
        }

        private void btn_ship_order_Click(object sender, EventArgs e)
        {
            // 从数据库获取待发货订单的数量
            DataSet ds = MySqlHelper.GetDataSet(MySqlHelper.Conn, System.Data.CommandType.Text,
                "select order_id, track_id, order_type  from t_order_basic where order_status=0 and order_type in (0, 1)", null);
            
            // 弹出确认对话框
            string msgBoxText = String.Format("请确定此次待发货订单数：{0}个？", ds.Tables[0].Rows.Count);
            DialogResult dlgResult = MessageBox.Show(msgBoxText, "确认", MessageBoxButtons.OKCancel);
            if (dlgResult == DialogResult.OK)
            {
                // 在亚马逊确认发货
                DateTime now = DateTime.Now;
                string fileName = String.Format("confirmOrderFeed/confirmOrderFeed_{0}.txt", now.ToString(GlobalConfig.Instance.TimeFormat));

                //*** 生成feed的txt文件
                //*** order-id	order-item-id	quantity	ship-date	carrier-code	carrier-name	tracking-number	ship-method
                string ship_date = now.ToString(GlobalConfig.Instance.ShipDateFormat);
                string rowFormat = "{0}\t\t\t"+ ship_date +"\t"+ GlobalConfig.Instance.ShipCarrierCode + "\t"+ GlobalConfig.Instance.ShipCarrierName + "\t{1}\t";
                List<string> orderList = new List<string>();

                // 标题行
                orderList.Add(String.Format("{0}\t{1}\t{2}\t{3}\t{4}\t{5}\t{6}\t{7}", "order-id", "order-item-id", "quantity", "ship-date", "carrier-code", "carrier-name", "tracking-number", "ship-method"));

                foreach (DataRow dr in ds.Tables[0].Rows)
                {
                    if (int.Parse(dr["order_type"].ToString()) == 0)
                    {
                        orderList.Add(String.Format(rowFormat, dr["order_id"], dr["track_id"]));
                    }
                    else  
                    {
                        // 假发货订单不写入track id
                        orderList.Add(String.Format(rowFormat, dr["order_id"], ""));
                    }
                }
                System.IO.File.WriteAllLines(fileName, orderList);
            
                //*** 调用submit feed方法
                MarketplaceWebServiceSamples.SubmitFeed(fileName, GlobalConfig.Instance.ConfirmOrderFeedType);
                
                // 写入数据库
                string cmdText = "update t_order_basic set order_status=1 where order_status=0 and order_type in (0, 1)";
                int affectedRowsCount = MySqlHelper.ExecuteNonQuery(MySqlHelper.Conn, System.Data.CommandType.Text, cmdText, null);
                Console.WriteLine("Affected Rows: " + affectedRowsCount);

                // 刷新datagridview
                dgv_unshipped_order_BoundDS();
            }
        }


        public static int GetShippingStatus(string track_id)
        {
            string url = String.Format("https://tools.usps.com/go/TrackConfirmAction?qtc_tLabels1=${0}", track_id);
            string responseContent = CreateGetHttpResponse(url);
            if (responseContent.IndexOf("status-delivered") != -1)  // 投递成功
            {
                return 1;
            }
            else if(responseContent.IndexOf("status-alert") != -1) // 投递失败
            {
                return 2;
            }
            else
            {
                return 0; // 投递中
            }

        }

        /// <summary>  
        /// 创建GET方式的HTTP请求  
        /// </summary>  
        public static string CreateGetHttpResponse(string url)
        {
            try
            {
                WebRequest wReq = WebRequest.Create(url);
                WebResponse wResp = wReq.GetResponse();
                Stream respStream = wResp.GetResponseStream();
                using (System.IO.StreamReader reader = new System.IO.StreamReader(respStream))
                {
                    return reader.ReadToEnd();
                }
            }
            catch (System.Exception ex)
            {
                //errorMsg = ex.Message;
            }
            return "";
        }

        public string EscapeMysqlString(string str)
        {
            return str.Replace("'", "\\'");
        }

        private void btn_update_track_id_Click(object sender, EventArgs e)
        {
            // 弹出确认对话框
            DialogResult dlgResult = MessageBox.Show("确定要将订单的tracking id入库吗？", "确认", MessageBoxButtons.OKCancel);
            if (dlgResult == DialogResult.OK)
            {
                StreamReader f = File.OpenText("orderTrackId/order_track_id_now.txt");
                string line = f.ReadLine();
                List<string> cmdList = new List<string>();
                while (line != null)
                {
                    string[] words = line.Split('\t');
                    cmdList.Add(String.Format("update t_order_basic set track_id='{0}' where order_id='{1}'", words[1], words[0]));
                    line = f.ReadLine();
                }
                string cmdText = String.Join(";", cmdList);
                int affectedRowsCount = MySqlHelper.ExecuteNonQuery(MySqlHelper.Conn, System.Data.CommandType.Text, cmdText, null);
                MessageBox.Show("更新个数："+affectedRowsCount, "确认", MessageBoxButtons.OK);

                // 刷新datagridview
                dgv_unshipped_order_BoundDS();
            }
            
        }

        private void btn_tag_fake_shipped_Click(object sender, EventArgs e)
        {
            // 标记为假发货订单
            List<string> fakeShippedOrderList = new List<string>();
            List<string> fakeShippedOrderIdList = new List<string>();
            foreach (DataGridViewRow c in dgv_unshipped_order.SelectedRows)
            {
                fakeShippedOrderList.Add(String.Format("{0} {1} {2}", c.Cells["order_id"].Value.ToString(), c.Cells["buyer_name"].Value.ToString(), c.Cells["sku"].Value.ToString()));
                fakeShippedOrderIdList.Add(String.Format("'{0}'", c.Cells["order_id"].Value.ToString()));
            }

            // 弹出确认对话框
            string msgBoxText = String.Format("确定要将以下订单标记为假发货吗？\n{0}", String.Join("\n", fakeShippedOrderList));
            DialogResult dlgResult = MessageBox.Show(msgBoxText, "确认", MessageBoxButtons.OKCancel);
            if (dlgResult == DialogResult.OK)
            {
                // 写入数据库
                string cmdText = String.Format("update t_order_basic set order_type=1 where order_id in ({0})", String.Join(",", fakeShippedOrderIdList));
                int affectedRowsCount = MySqlHelper.ExecuteNonQuery(MySqlHelper.Conn, System.Data.CommandType.Text, cmdText, null);
                Console.WriteLine("Affected Rows: " + affectedRowsCount);

                // 刷新datagridview
                dgv_unshipped_order_BoundDS();
            }
        }
    }
}
