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
using System.Xml;
using System.Text.RegularExpressions;


namespace AmazonTest
{
    public partial class Form1 : Form
    {
        AWSLogger awsLogger;
        System.Timers.Timer timerAdaptPrice;       // 自动调价计时器
        System.Timers.Timer timerUpdateInventory;  // 更新库存定时器
        System.Timers.Timer timerMonitorListing;   // 监控跟卖定时器
        System.Timers.Timer timerUpdateShippingStatus;   // 更新投递状态
        // System.Timers.Timer timerRequestFeedback;   // 发送feedback request
        System.Timers.Timer timerUpdateHolidayPrompt; // 更新邮件中节日内容
        System.Timers.Timer timerScheduled; // 定时执行
        bool requestFeedbackRunning;  // 请求评论功能是否正在运行
        private string holidayPrompt;   // 节日提醒内容
        private string marketId;   // 当前市场id

        public Form1()
        {
            // 把所有的控件合法性线程检查全部都给禁止掉了
            //System.Windows.Forms.Control.CheckForIllegalCrossThreadCalls = false;
            requestFeedbackRunning = false;
            UpdateHolidayPrompt();

            InitializeComponent();
            tb_list_price.Text = "9.99";
            marketId = GlobalConfig.Instance.MarketID_US;  // 初始默认为US
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
            //CreateGetHttpResponse("https://tools.usps.com/go/TrackConfirmAction?qtc_tLabels1=LS117905710CN");
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
            //Console.WriteLine(EUB.Instance.doRequest());
            //getFBAOrders();
            //SendFeedbackRequestEmail("FBM");
            //UpdateProductModel();
            //CorrectSKUName();
            //MarketplaceWebServiceSamples.SubmitFeed("childProductFeedTemplate.xml", GlobalConfig.Instance.UpdateProductFeedType);
            //MarketplaceWebServiceSamples.SubmitFeed("parentProductFeedTemplate.xml", GlobalConfig.Instance.UpdateProductFeedType);
            //MarketplaceWebServiceSamples.SubmitFeed("relationshipTemplate.xml", GlobalConfig.Instance.CreateRelationshipFeedType);            
            //MarketplaceWebServiceSamples.RunSample();
            //System.Diagnostics.Process.Start(System.Environment.CurrentDirectory + "\\" + "createVariation\\parentProductFeedTemplate.xml");
            //getFBAOrders(GlobalConfig.Instance.MarketID_US);
            //SendFeedbackRequestEmail(GlobalConfig.Instance.MarketID_US);
            //AdaptPrice.MonitorListing();
            //SendFeedbackRequestEmail(GlobalConfig.Instance.MarketID_US);
            //getFBAOrders(GlobalConfig.Instance.MarketID_US);  // 拉取美国站
            //update_inventory(GlobalConfig.Instance.MarketID_US);
            //update_inventory(GlobalConfig.Instance.MarketID_CA);

        }


        private void Form1_Load(object sender, EventArgs e)
        {

            // *** 暂停
            //timerAdaptPrice = new System.Timers.Timer();
            //timerAdaptPrice.Interval = 15 * 60 * 1000;  // 间隔时间为15min
            //timerAdaptPrice.Elapsed += new System.Timers.ElapsedEventHandler(adaptPrice);

            timerUpdateInventory = new System.Timers.Timer();
            timerUpdateInventory.Interval = 2 * 60 * 60 * 1000;  // 间隔时间为2h
            timerUpdateInventory.Elapsed += new System.Timers.ElapsedEventHandler(updateInventory);

            timerUpdateShippingStatus = new System.Timers.Timer();
            timerUpdateShippingStatus.Interval = 24 * 60 * 60 * 1000;  // 间隔时间为24h
            timerUpdateShippingStatus.Elapsed += new System.Timers.ElapsedEventHandler(UpdateShipingStatus);

            // 废弃
            //timerRequestFeedback = new System.Timers.Timer();
            //timerRequestFeedback.Interval = 24 * 60 * 60 * 1000;  // 间隔时间为24h
            //timerRequestFeedback.Elapsed += new System.Timers.ElapsedEventHandler(RequestFeedback);

            timerUpdateHolidayPrompt = new System.Timers.Timer();
            timerUpdateHolidayPrompt.Interval = 24 * 60 * 60 * 1000;  // 间隔时间为24h
            timerUpdateHolidayPrompt.Elapsed += new System.Timers.ElapsedEventHandler(UpdateHolidayPrompt);

            timerScheduled = new System.Timers.Timer();
            timerScheduled.Interval = 1 * 60 * 1000;  // 间隔时间为1min
            timerScheduled.Elapsed += new System.Timers.ElapsedEventHandler(ScheduleTask);

            timerMonitorListing = new System.Timers.Timer();
            timerMonitorListing.Interval = 1 * 60 * 60 * 1000;  // 间隔时间为1h
            timerMonitorListing.Elapsed += new System.Timers.ElapsedEventHandler(MonitorListing);

            // 显示未发货订单
            dgv_unshipped_order_BoundDS();



            // debug状态下不执行下面的语句
#if (DEBUG)
                return;
#endif

            // 更新货运状态
            Thread threadUpdateShipingStatus = new Thread(UpdateShipingStatus);
            threadUpdateShipingStatus.Start();

            // 跟卖监测
            Thread threadMonitorListing = new Thread(MonitorListing);
            threadMonitorListing.Start();

            // 发送feedback request
            //Thread threadSendFeedbackRequestEmail = new Thread(SendFeedbackRequestEmail);
            //threadSendFeedbackRequestEmail.Start();

            // 启动定时器
            //timerAdaptPrice.Enabled = true;
            timerUpdateInventory.Enabled = true;
            timerUpdateShippingStatus.Enabled = true;
            //timerRequestFeedback.Enabled = true;
            timerUpdateHolidayPrompt.Enabled = true;
            timerScheduled.Enabled = true;
            timerMonitorListing.Enabled = true;
        }



        private void button1_Click(object sender, EventArgs e)
        {
            //timerAdaptPrice.Enabled = true;
            //timerUpdateInventory.Enabled = true;
            //timerUpdateShippingStatus.Enabled = true;
            //timerRequestFeedback.Enabled = true;
        }

        private void button2_Click(object sender, EventArgs e)
        {
            //timerAdaptPrice.Enabled = false;
            timerUpdateInventory.Enabled = false;
            timerUpdateShippingStatus.Enabled = false;
            //timerRequestFeedback.Enabled = false;
            timerUpdateHolidayPrompt.Enabled = false;
        }


        private void UpdateHolidayPrompt(object sender, System.Timers.ElapsedEventArgs e)
        {
            UpdateHolidayPrompt();
        }

        private void ScheduleTask(object sender, System.Timers.ElapsedEventArgs e)
        {
            int hour = DateTime.Now.Hour;
            int minute = DateTime.Now.Minute;

            // 每天21:00发送请求评论,拉取fba订单
            if (hour == 21 && minute == 0)
            {
                SendFeedbackRequestEmail(GlobalConfig.Instance.MarketID_US);
                getFBAOrders(GlobalConfig.Instance.MarketID_US);  // 拉取美国站
                // UpdateProductModel(); // 设置机型
            }
            else if (hour == 21 && minute == 30)
            {
                //SendFeedbackRequestEmail(GlobalConfig.Instance.MarketID_CA);
                getFBAOrders(GlobalConfig.Instance.MarketID_CA);  // 拉取加拿大站
            }
        }


        private void UpdateProductModel()
        {
            // 设置机型
            string cmdFormat = "update t_product set model='{0}' where sku like '%{1}%'";
            List<string> cmdList = new List<string>();
            cmdList.Add(string.Format(cmdFormat, "IP5", "-IP5-"));
            cmdList.Add(string.Format(cmdFormat, "IP6", "-IP6-"));
            cmdList.Add(string.Format(cmdFormat, "IP6P", "-IP6P-"));
            int affectedRowsCount = MySqlHelper.ExecuteNonQuery(MySqlHelper.Conn, System.Data.CommandType.Text, String.Join(";", cmdList), null);
        }


        private void UpdateHolidayPrompt()
        {
            // 根据月份，修改节日的提醒内容
            int month = DateTime.Now.Month;
            int day = DateTime.Now.Day;
            switch (month)
            {
                case 1:
                    if (day == 1)
                    {
                        holidayPrompt = "New Year's Day";
                    }
                    else
                    {
                        holidayPrompt = "the past New Year's Day";
                    }
                    break;
                case 2:
                    if (day == 12)
                    {
                        holidayPrompt = "Lincoln's Birthday";
                    }
                    else if (day == 14)
                    {
                        holidayPrompt = "St. Valentine's Day";
                    }
                    else if (day == 18)
                    {
                        holidayPrompt = "Washington's Birthday";
                    }
                    else if (day < 14)
                    {
                        holidayPrompt = "the coming St. Valentine's Day";
                    }
                    else
                    {
                        holidayPrompt = "the past St. Valentine's Day";
                    }
                    break;
                case 3:
                    if (day == 17)
                    {
                        holidayPrompt = "St. Patrick's Day";
                    }
                    else if (day < 17)
                    {
                        holidayPrompt = "the coming St. Patrick's Day";
                    }
                    else
                    {
                        holidayPrompt = "the coming April Fool's Day";
                    }
                    break;
                case 4:
                    if (day == 1)
                    {
                        holidayPrompt = "April Fool's Day";
                    }
                    else
                    {
                        holidayPrompt = "Easter Day";
                    }
                    break;
                case 5:
                    holidayPrompt = "Mother's Day";
                    break;
                case 6:
                    if (day <= 14)
                    {
                        holidayPrompt = "Flag Day";
                    }
                    else
                    {
                        holidayPrompt = "Father's Day";
                    }
                    break;
                case 7:
                    holidayPrompt = "Independence Day";
                    break;
                case 8:
                    holidayPrompt = "the coming Labor Day";
                    break;
                case 9:
                    holidayPrompt = "Labor Day";
                    break;
                case 10:
                    holidayPrompt = "Columbus Day";
                    break;
                case 11:
                    if (day <= 10)
                    {
                        holidayPrompt = "Holloween";
                    }
                    else if (day <= 23)
                    {
                        holidayPrompt = "the coming Thanksgiving Day";
                    }
                    else
                    {
                        holidayPrompt = "Thanksgiving Day";
                    }
                    break;
                case 12:
                    if (day == 25)
                    {
                        holidayPrompt = "Christmas";
                    }
                    else if (day < 25)
                    {
                        holidayPrompt = "the coming Christmas";
                    }
                    else
                    {
                        holidayPrompt = "the coming New Year's Day";
                    }
                    break;
                default:
                    break;
            }
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
                //*** 从数据库中读取未发货订单, 按seller、model、sku排序
                DataSet ds = MySqlHelper.GetDataSet(MySqlHelper.Conn, System.Data.CommandType.Text,
                    string.Format("select a.*, b.sku, b.order_item_id, b.quantity, c.seller from t_order_basic as a, t_order_product as b, t_product as c where a.market_id='{0}' and a.order_id=b.order_id and a.order_status=0 and a.order_type=0 and b.sku=c.sku order by c.model,c.seller,b.sku;", marketId), null);

                //*** 从数据库中读取每个订单的数量
                DataSet dsOrderCount = MySqlHelper.GetDataSet(MySqlHelper.Conn, System.Data.CommandType.Text,
                    string.Format("select a.order_id, sum(b.quantity) as order_count from t_order_basic as a, t_order_product as b where a.market_id='{0}' and a.order_id=b.order_id and a.order_status=0 and a.order_type=0 group by b.order_id;", marketId), null);
                Dictionary<string, int> orderCountDict = new Dictionary<string, int>();
                foreach (DataRow dr in dsOrderCount.Tables[0].Rows)
                {
                    orderCountDict.Add(dr["order_id"].ToString(), Int32.Parse(dr["order_count"].ToString()));
                }

                string srcFilePath = System.Environment.CurrentDirectory + "\\shippingInfo\\shiptemplate.xls";


                //*** read shipping template
                Microsoft.Office.Interop.Excel.Application excel = new Microsoft.Office.Interop.Excel.ApplicationClass();
                excel.Visible = false;
                Workbook wBook = excel.Workbooks.Open(srcFilePath, Type.Missing, Type.Missing, Type.Missing, Type.Missing, Type.Missing, Type.Missing, Type.Missing, Type.Missing, Type.Missing, Type.Missing, Type.Missing, Type.Missing, Type.Missing, Type.Missing);
                Worksheet wSheet = wBook.Sheets[1] as Worksheet; //第一个sheet页

                //*** write order info
                int rowIndex = 2;
                // E邮宝业务类型
                //string eubBuzType = GlobalConfig.Instance.EubBuzType;   
                string eubBuzType = GlobalConfig.Instance.GetCommonConfigValue("eubBuzType");

                // 1. 先处理只包含1个的            
                foreach (DataRow dr in ds.Tables[0].Rows)
                {
                    string order_id = dr["order_id"].ToString();
                    if (orderCountDict[order_id] == 1)
                    {
                        wSheet.Cells[rowIndex, 1] = order_id;
                        wSheet.Cells[rowIndex, 2] = dr["order_item_id"].ToString();
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
                        wSheet.Cells[rowIndex, 15] = dr["seller"].ToString();
                        wSheet.Cells[rowIndex, 20] = eubBuzType;
                        ++rowIndex;
                    }
                }

                // 2. 再处理包含2-3个的
                foreach (DataRow dr in ds.Tables[0].Rows)
                {
                    string order_id = dr["order_id"].ToString();
                    if (orderCountDict[order_id] == 2 || orderCountDict[order_id] == 3)
                    {
                        wSheet.Cells[rowIndex, 1] = order_id;
                        wSheet.Cells[rowIndex, 2] = dr["order_item_id"].ToString();
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
                }

                // 3. 最后处理超过3个以上的
                foreach (DataRow dr in ds.Tables[0].Rows)
                {
                    string order_id = dr["order_id"].ToString();
                    if (orderCountDict[order_id] > 3)
                    {
                        wSheet.Cells[rowIndex, 1] = order_id;
                        wSheet.Cells[rowIndex, 2] = dr["order_item_id"].ToString();
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
                }

                //****设置禁止弹出保存和覆盖的询问提示框 
                excel.DisplayAlerts = false;
                excel.AlertBeforeOverwriting = true;

                //string

                //保存
                DateTime now = DateTime.Now;
                //string destFilePath = String.Format(System.Environment.CurrentDirectory + "\\shippingInfo\\ship_{0}.xls", now.ToString(GlobalConfig.Instance.TimeFormat));
                string destFilePath = String.Format(System.Environment.CurrentDirectory + "\\shippingInfo\\ship_{0}_{1}.xls", marketId, now.ToString(GlobalConfig.Instance.GetCommonConfigValue("timeFormat")));
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

        //private void getUnshippedOrders(object param)
        private void getUnshippedOrders(string market_id)
        {
            //string market_id = (string)param;
            //*** 获取订单文件
            //string fileName = String.Format("orderReport/{0}_report_{1}.txt", market_id, DateTime.Now.ToString(GlobalConfig.Instance.TimeFormat));
            //MarketplaceWebServiceSamples.GetReport(GlobalConfig.Instance.UnshippedOrderReportType, fileName, DateTime.Now, DateTime.Now);
            string fileName = String.Format("orderReport/{0}_report_{1}.txt", market_id, DateTime.Now.ToString(GlobalConfig.Instance.GetCommonConfigValue("timeFormat")));
            MarketplaceWebServiceSamples.GetReport(market_id, GlobalConfig.Instance.GetCommonConfigValue("unshippedOrderReportType"), fileName, DateTime.Now, DateTime.Now);

            //string fileName = "orderReport/report_20151111_124828.txt";
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
                        string[] date = words[3].Split(new char[] { '-', 'T', ':' }, StringSplitOptions.RemoveEmptyEntries);
                        // 转换成本地时间
                        DateTime payments_date = Convert.ToDateTime(string.Format("{0}-{1}-{2} {3}:{4}:{5}", date[0], date[1], date[2], date[3], date[4], date[5])).AddHours(int.Parse(GlobalConfig.Instance.GetConfigValue(market_id, "timeDifference")));
                        string payments_date_local = payments_date.ToString("yyyy-MM-dd HH:mm:ss");

                        orderCmdTextList.Add(String.Format("('{0}', '{1}', '{2}', '{3}', '{4}', '{5}', '{6}', '{7}', '{8}', '{9}', '{10}', '{11}', '{12}', {13}, {14}, '{15}')",
                            words[0], payments_date_local, words[8], words[7], words[9], words[16],
                            words[17], words[18], words[19], words[20], words[21], words[22], words[23], default_order_status, default_order_type, market_id));
                        orderProductCmdTextList.Add(String.Format("('{0}', '{1}', '{2}', {3}, '{4}', '{5}')", words[0], words[1], words[10], words[12], words[11], market_id)); // order_id order_item_id sku quantity product_name market_id
                        line = f.ReadLine();
                    }
                    f.Close();
                    if (orderCmdTextList.Count > 0)
                    {
                        //*** 更新到数据库
                        string cmdText = String.Format("insert ignore into t_order_basic (order_id, payments_date, buyer_name, buyer_email, buyer_phone, recipient_name, ship_address_1, ship_address_2," +
                            "ship_address_3, ship_city, ship_state, ship_postal_code, ship_country, order_status, order_type, market_id)" +
                            " values {0};", String.Join(",", orderCmdTextList));
                        cmdText += String.Format("insert ignore into t_order_product (order_id, order_item_id, sku, quantity, product_name, market_id) values {0}", String.Join(",", orderProductCmdTextList));
                        int affectedRowsCount = MySqlHelper.ExecuteNonQuery(MySqlHelper.Conn, System.Data.CommandType.Text, cmdText, null);
                        Console.WriteLine("Affected Rows: " + affectedRowsCount);

                        // 更新库存
                        update_inventory(market_id);

                        // 确认订单
                        confirm_order(market_id);

                        // 记录上一次拉取时间 INSERT t_product (sku, asin, quantity) values {0} ON DUPLICATE KEY UPDATE
                        MySqlHelper.ExecuteNonQuery(MySqlHelper.Conn, System.Data.CommandType.Text, 
                            String.Format("insert t_config (config_name, config_value, market_id) values ('{0}', '{1}', '{2}') ON DUPLICATE KEY UPDATE config_value='{3}';", 
                            "get_order_latest_request_date", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), market_id, DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")), null);

                        // 绑定数据源
                        dgv_unshipped_order_BoundDS();

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


        private void getFBAOrders(string market_id)
        {
            // 读取上一次请求报告的时间
            DataSet ds = MySqlHelper.GetDataSet(MySqlHelper.Conn, System.Data.CommandType.Text,
                string.Format("select config_value from t_config where config_name='fba_order_latest_request_date' and market_id='{0}'", market_id), null);
            DateTime startDate = Convert.ToDateTime(ds.Tables[0].Rows[0]["config_value"]);
            DateTime endDate = DateTime.Now;

            // 同一天请求一次就好了
            if ((startDate.Month == endDate.Month) && (startDate.Day == endDate.Day))
            {
                return;
            }


            //*** 获取FBA订单文件
            //string fileName = String.Format("fbaOrderReport/report_{0}.txt", DateTime.Now.ToString(GlobalConfig.Instance.TimeFormat));
            //MarketplaceWebServiceSamples.GetReport(GlobalConfig.Instance.UnshippedFBAOrderReportType, fileName, startDate, endDate);
            string fileName = String.Format("fbaOrderReport/{0}_report_{1}.txt", market_id, DateTime.Now.ToString(GlobalConfig.Instance.GetCommonConfigValue("timeFormat")));
            MarketplaceWebServiceSamples.GetReport(market_id, GlobalConfig.Instance.GetCommonConfigValue("unshippedFBAOrderReportType"), fileName, startDate, endDate);

            //string fileName = "fbaOrderReport/report_20151024_174551.txt";
            StreamReader f = File.OpenText(fileName);
            string line = f.ReadLine();
            // 校验第一行
            if (line != null)
            {
                string[] words = line.Split('\t');
                string[] expectWords = { "amazon-order-id", "merchant-order-id", "shipment-id", "shipment-item-id", "amazon-order-item-id",
                    "merchant-order-item-id", "purchase-date", "payments-date", "shipment-date", "reporting-date", "buyer-email", "buyer-name",
                    "buyer-phone-number", "sku", "product-name", "quantity-shipped", "currency", "item-price", "item-tax", "shipping-price",
                    "shipping-tax", "gift-wrap-price", "gift-wrap-tax", "ship-service-level", "recipient-name", "ship-address-1", "ship-address-2",
                    "ship-address-3", "ship-city", "ship-state", "ship-postal-code", "ship-country", "ship-phone-number", "bill-address-1", "bill-address-2",
                    "bill-address-3", "bill-city", "bill-state", "bill-postal-code", "bill-country", "item-promotion-discount", "ship-promotion-discount",
                    "carrier", "tracking-number", "estimated-arrival-date", "fulfillment-center-id", "fulfillment-channel", "sales-channel" };
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
                        ship_city ship_state ship_postal_code ship_country track_id estimated_arrival_date ship_service_level carrier
                        */
                        // 将utc格式的时间转换成mysql的datetime格式 ：2015-09-09T23:29:17-07:00
                        string[] date = words[7].Split(new char[] { '-', 'T', ':' }, StringSplitOptions.RemoveEmptyEntries);
                        // 转换成本地时间
                        DateTime payments_date = Convert.ToDateTime(string.Format("{0}-{1}-{2} {3}:{4}:{5}", date[0], date[1], date[2], date[3], date[4], date[5])).AddHours(int.Parse(GlobalConfig.Instance.GetConfigValue(market_id, "timeDifference")));
                        string payments_date_local = payments_date.ToString("yyyy-MM-dd HH:mm:ss");

                        date = words[44].Split(new char[] { '-', 'T', ':' }, StringSplitOptions.RemoveEmptyEntries);
                        DateTime estimated_arrival_date = Convert.ToDateTime(string.Format("{0}-{1}-{2} {3}:{4}:{5}", date[0], date[1], date[2], date[3], date[4], date[5])).AddHours(int.Parse(GlobalConfig.Instance.GetConfigValue(market_id, "timeDifference")));
                        string estimated_arrival_date_local = estimated_arrival_date.ToString("yyyy-MM-dd");

                        orderCmdTextList.Add(String.Format("('{0}', '{1}', '{2}', '{3}', '{4}', '{5}', '{6}', '{7}', '{8}', '{9}', '{10}', '{11}', '{12}', '{13}', '{14}', '{15}', '{16}', '{17}')",
                            words[0], payments_date_local, words[11], words[10], words[12], words[24],
                            words[25], words[26], words[27], words[28], words[29], words[30], words[31], words[43], estimated_arrival_date_local, words[23], words[42], market_id));
                        orderProductCmdTextList.Add(String.Format("('{0}', '{1}', '{2}', {3}, '{4}', '{5}')", words[0], words[4], words[13], words[15], words[14], market_id)); // order_id order_item_id sku quantity product_name market_id
                        line = f.ReadLine();
                    }
                    f.Close();
                    if (orderCmdTextList.Count > 0)
                    {
                        //*** 更新到数据库
                        string cmdText = String.Format("insert ignore into t_order_basic_fba (order_id, payments_date, buyer_name, buyer_email, buyer_phone, recipient_name, ship_address_1, ship_address_2," +
                            "ship_address_3, ship_city, ship_state, ship_postal_code, ship_country, track_id, estimated_arrival_date, ship_service_level, carrier, market_id)" +
                            " values {0};", String.Join(",", orderCmdTextList));
                        cmdText += String.Format("insert ignore into t_order_product (order_id, order_item_id, sku, quantity, product_name, market_id) values {0};", String.Join(",", orderProductCmdTextList));

                        // 更新请求报告的时间
                        cmdText += string.Format("update t_config set config_value='{0}' where config_name='fba_order_latest_request_date' and market_id='{1}';", endDate.ToString("yyyy-MM-dd HH:mm:ss"), market_id);
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
            string market_id = GlobalConfig.Instance.MarketID_CA;
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
                    "ship-phone-number", "item-promotion-discount", "item-promotion-id", "ship-promotion-discount", "ship-promotion-id", "delivery-start-date", "delivery-end-date", "delivery-time-zone", "delivery-Instructions" };
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

                        orderCmdTextList.Add(String.Format("('{0}', '{1}', '{2}', '{3}', '{4}', '{5}', '{6}', '{7}', '{8}', '{9}', '{10}', '{11}', '{12}', {13}, {14}, '{15}')",
                            words[0], payments_date_local, words[5], words[4], words[24], words[16],
                            words[17], words[18], words[19], words[20], words[21], words[22], words[23], default_order_status, default_order_type, market_id));
                        orderProductCmdTextList.Add(String.Format("('{0}', '{1}', '{2}', {3}, '{4}', '{5}')", words[0], words[1], words[7], words[9], words[8], market_id)); // order_id order_item_id sku quantity product_name market_id
                        line = f.ReadLine();
                    }
                    f.Close();
                    if (orderCmdTextList.Count > 0)
                    {
                        //*** 更新到数据库
                        string cmdText = String.Format("insert ignore into t_order_basic (order_id, payments_date, buyer_name, buyer_email, buyer_phone, recipient_name, ship_address_1, ship_address_2," +
                            "ship_address_3, ship_city, ship_state, ship_postal_code, ship_country, order_status, order_type, market_id)" +
                            " values {0};", String.Join(",", orderCmdTextList));
                        cmdText += String.Format("insert ignore into t_order_product (order_id, order_item_id, sku, quantity, product_name, market_id) values {0}", String.Join(",", orderProductCmdTextList));
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

        private void SendFeedbackRequestEmail(string market_id)
        {
            SendFeedbackRequestEmail("FBA", market_id);
            SendFeedbackRequestEmail("FBM", market_id);
        }

        private void SendFeedbackRequestEmail(string fulfillType, string market_id)
        {
            Console.WriteLine("SendFeedbackRequestEmail");
            //if (!requestFeedbackRunning)  // 同一时间只有一个在运行，避免重复发送邮件
            if (true)
            {
                requestFeedbackRunning = true;
                DataSet ds;
                string table_name;
                if (fulfillType == "FBA")
                {
                    table_name = "t_order_basic_fba";
                    string cmdText = String.Format("select order_id, buyer_name, buyer_email from t_order_basic_fba where is_feedback=0 and feedback_request_count=0 and estimated_arrival_date<='{0}' and market_id='{1}'", DateTime.Now.ToString("yyyy-MM-dd"), market_id);
                    ds = MySqlHelper.GetDataSet(MySqlHelper.Conn, System.Data.CommandType.Text, cmdText, null);
                }
                else
                {
                    table_name = "t_order_basic";
                    // 从数据库读取投递成功的订单列表：投递成功 & 已经到了1天
                    string cmdText = String.Format("select order_id, buyer_name, buyer_email from t_order_basic where is_feedback=0 and is_delivered=1 and feedback_request_count=0 and deliver_date<='{0}' and market_id='{1}'", DateTime.Now.AddDays(-1).ToString("yyyy-MM-dd"), market_id);
                    ds = MySqlHelper.GetDataSet(MySqlHelper.Conn, System.Data.CommandType.Text, cmdText, null);
                }
                StreamReader f = File.OpenText("feedbackRequestEmailTemplate.html");
                string emailTemplate = f.ReadToEnd();
                f.Close();
                // 替换公共配置
                //emailTemplate = emailTemplate.Replace("${seller_id}", GlobalConfig.Instance.SellerId).Replace("${marketplace_id}", GlobalConfig.Instance.MarketplaceId).Replace("${holiday_prompt}", holidayPrompt);
                emailTemplate = emailTemplate.Replace("${seller_id}", GlobalConfig.Instance.GetConfigValue(market_id, "sellerId")).Replace("${marketplace_id}", GlobalConfig.Instance.GetConfigValue(market_id, "marketplaceId")).Replace("${holiday_prompt}", holidayPrompt);
                string subjectTemplate = "Please Help by Leaving Feedback/Review for Your Amazon Order {0}";
                string now = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                int sendCount = 1;
                foreach (DataRow dr in ds.Tables[0].Rows)
                {
                    // 查询订单的商品列表
                    DataSet ds1 = MySqlHelper.GetDataSet(MySqlHelper.Conn, System.Data.CommandType.Text,
                    String.Format("select product_name, asin from t_order_product as a, t_product as b where a.order_id='{0}' and a.sku=b.sku", dr["order_id"]), null);
                    string product_list = "";
                    foreach (DataRow dr1 in ds1.Tables[0].Rows)
                    {
                        // 请求review
                        // product_list += String.Format("<li><a href='https://www.amazon.com/review/create-review?asin={0}' target='_blank'> Leave Product Review</a>: {1}</li>", dr1["asin"], dr1["product_name"]);
                        product_list += String.Format("<li>{0}</li>", dr1["product_name"]);
                    }
                    string body = emailTemplate.Replace("${order_id}", dr["order_id"].ToString()).Replace("${product_list}", product_list).Replace("${buyer_name}", dr["buyer_name"].ToString());
                    if (SendEmail.Instance.sendEmail(dr["buyer_email"].ToString(), String.Format(subjectTemplate, dr["order_id"]), body))
                    {
                        // 回写数据库
                        MySqlHelper.ExecuteNonQuery(MySqlHelper.Conn, System.Data.CommandType.Text,
                            String.Format("update {0} set feedback_request_count=feedback_request_count+1, last_feedback_request_time=curtime() where order_id='{1}'", table_name, dr["order_id"].ToString()), null);
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
            foreach (DataRow dr in ds.Tables[0].Rows)
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
            SendFeedbackRequestEmail(GlobalConfig.Instance.MarketID_US);
        }


        private void MonitorListing(object sender, System.Timers.ElapsedEventArgs e)
        {
            MonitorListing();
        }

        private void MonitorListing()
        {
            AdaptPrice.MonitorListing();
            label_monitor_prompt.Text = "最近更新时间：" + DateTime.Now.ToString(GlobalConfig.Instance.GetCommonConfigValue("timeFormat"));

        }

        private void updateInventory(object sender, System.Timers.ElapsedEventArgs e)
        //private void updateInventory()
        {
            //*** 获取库存报告文件
            //string fileName = String.Format("inventoryReport/report_{0}.txt", DateTime.Now.ToString(GlobalConfig.Instance.TimeFormat));
            //MarketplaceWebServiceSamples.GetReport(GlobalConfig.Instance.InventoryReportType, fileName, DateTime.Now, DateTime.Now);
            string fileName = String.Format("inventoryReport/report_{0}.txt", DateTime.Now.ToString(GlobalConfig.Instance.GetCommonConfigValue("timeFormat")));
            MarketplaceWebServiceSamples.GetReport(GlobalConfig.Instance.MarketID_US, GlobalConfig.Instance.GetCommonConfigValue("inventoryReportType"), fileName, DateTime.Now, DateTime.Now);
            //string fileName = "inventoryReport/report_20151003_143614.txt";
            StreamReader f = File.OpenText(fileName);
            string line = f.ReadLine();
            // 校验第一行
            if (line != null)
            {
                string[] words = line.Split('\t');
                string[] expectWords = { "sku", "asin", "price", "quantity" };
                if (string.Join(",", words) == string.Join(",", expectWords))
                {
                    line = f.ReadLine();
                    List<string> cmdTextList = new List<string>();
                    while (line != null)
                    {
                        words = line.Split('\t');
                        cmdTextList.Add(String.Format("('{0}', '{1}', {2})", words[0], words[1], words[3] == "" ? "0" : words[3]));
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
            get_order_report();
            //DialogResult dlg = MessageBox.Show("正在从亚马逊拉取未发货订单入库，请稍等...", "提示", MessageBoxButtons.OK);
        }

        private void get_order_report()
        {
            string msgBoxText = String.Format("当前站点：{0}  确定要从亚马逊拉取订单吗？", marketId);
            DialogResult dlgResult = MessageBox.Show(msgBoxText, "确认", MessageBoxButtons.OKCancel);
            if (dlgResult == DialogResult.OK)
            {
                // 显示提示文字：正在拉取订单，请稍等。。。
                System.Windows.Forms.Label label;
                if (marketId == GlobalConfig.Instance.MarketID_CA)
                {
                    label = label_get_order_time_ca;
                }
                else
                {
                    label = label_get_order_time;
                }
                label.Text = "正在拉取订单，请稍等。。。";

                // 从亚马逊拉取未发货订单入库
                getUnshippedOrders(marketId);

                // 多线程 有bug
                //Thread thread = new Thread(getUnshippedOrders);
                //thread.Start(marketId);
            }
        }

        private void dgv_unshipped_order_BoundDS()
        {
            //*** 从数据库中读取未发货订单
            DataSet ds = MySqlHelper.GetDataSet(MySqlHelper.Conn, System.Data.CommandType.Text,
                string.Format("select a.order_id, b.order_item_id, a.payments_date, a.confirmed_date, b.sku, b.quantity, a.track_id, a.buyer_name, a.recipient_name, a.ship_country, a.ship_state, a.ship_city, a.ship_address_1, a.ship_address_2, a.ship_address_3 from t_order_basic as a, t_order_product as b where a.market_id='{0}' and a.order_id=b.order_id and a.order_status=0 and a.order_type=0", marketId), null);
            DataGridView dgv;
            System.Windows.Forms.Label label;
            if (marketId == GlobalConfig.Instance.MarketID_CA)
            {
                dgv = dgv_unshipped_order_ca;
                label = label_get_order_time_ca;
            }
            else
            {
                dgv = dgv_unshipped_order;
                label = label_get_order_time;
            }

            // 绑定到datagridview
            dgv.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgv.DataSource = ds;
            dgv.DataMember = "Table";
            dgv.ReadOnly = true;
            dgv.AllowUserToDeleteRows = false;
            dgv.AllowUserToAddRows = false;
            for (int i = 0; i < 14; ++i)
            {
                dgv.AutoResizeColumn(i);
            }

            // 显示上一次拉取时间
            ds = MySqlHelper.GetDataSet(MySqlHelper.Conn, System.Data.CommandType.Text, String.Format("select config_value from t_config where config_name='{0}' and market_id='{1}'", "get_order_latest_request_date", marketId), null);
            string latest_date = "";
            if (ds.Tables[0].Rows.Count > 0)
            {
                latest_date = ds.Tables[0].Rows[0]["config_value"].ToString();
            }
            label.Text = String.Format("最近更新时间：{0}", latest_date);
        }

        private void dgv_fba_order_BoundDS()
        {
            //*** 从数据库中读取未发货订单
            DataSet ds = MySqlHelper.GetDataSet(MySqlHelper.Conn, System.Data.CommandType.Text,
                string.Format("select a.order_id, b.order_item_id, a.payments_date, b.sku, b.quantity, a.estimated_arrival_date, a.carrier, a.track_id, a.ship_service_level, a.buyer_name, a.recipient_name from t_order_basic_fba as a, t_order_product as b where a.order_id=b.order_id and a.market_id='{0}'", marketId), null);

            DataGridView dgv;
            if (marketId == GlobalConfig.Instance.MarketID_CA)
            {
                dgv = dgv_fba_order_ca;
            }
            else
            {
                dgv = dgv_fba_order;
            }

            // 绑定到datagridview
            dgv.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgv.DataSource = ds;
            dgv.DataMember = "Table";
            dgv.ReadOnly = true;
            dgv.AllowUserToDeleteRows = false;
            dgv.AllowUserToAddRows = false;
            dgv.AutoResizeColumn(0);
        }

        private void dgv_fake_shipped_order_BoundDS()
        {
            //*** 从数据库中读取假发货订单
            DataSet ds = MySqlHelper.GetDataSet(MySqlHelper.Conn, System.Data.CommandType.Text,
                string.Format("select a.order_id, b.sku, a.payments_date, a.confirmed_date, a.track_id,a.buyer_name,a.recipient_name  from t_order_basic as a, t_order_product as b where a.order_type=1 and a.order_id=b.order_id and a.market_id='{0}'", marketId), null);

            DataGridView dgv;
            if (marketId == GlobalConfig.Instance.MarketID_CA)
            {
                dgv = dgv_fake_unshipped_order_ca;
            }
            else
            {
                dgv = dgv_fake_unshipped_order;
            }

            // 绑定到datagridview
            dgv.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgv.DataSource = ds;
            dgv.DataMember = "Table";
            dgv.ReadOnly = true;
            dgv.AllowUserToDeleteRows = false;
            dgv.AllowUserToAddRows = false;
            dgv.Columns[1].DefaultCellStyle.Format = "yyyy-MM-dd";
            for (int i = 0; i < 6; ++i)
            {
                dgv.AutoResizeColumn(i);
            }
        }

        private void dgv_observed_order_BoundDS()
        {
            //*** 从数据库中读取待观察订单
            DataSet ds = MySqlHelper.GetDataSet(MySqlHelper.Conn, System.Data.CommandType.Text,
                string.Format("select * from t_order_basic where order_status=0 and order_type=2 and is_finished=0 and market_id='{0}'", marketId), null);

            DataGridView dgv;
            if (marketId == GlobalConfig.Instance.MarketID_CA)
            {
                dgv = dgv_observed_order_ca;
            }
            else
            {
                dgv = dgv_observed_order;
            }

            // 绑定到datagridview
            dgv.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgv.DataSource = ds;
            dgv.DataMember = "Table";
            dgv.ReadOnly = true;
            dgv.AllowUserToDeleteRows = false;
            dgv.AllowUserToAddRows = false;
            dgv.AutoResizeColumn(0);
        }

        private void dgv_ship_failed_order_BoundDS()
        {
            //*** 从数据库中读取发货失败订单
            DataSet ds = MySqlHelper.GetDataSet(MySqlHelper.Conn, System.Data.CommandType.Text,
                string.Format("select order_id,payments_date,track_id,buyer_name,recipient_name,ship_country,ship_state,ship_city,ship_address_1,ship_address_2,ship_address_3 from t_order_basic where is_delivered=2 and is_finished=0 and market_id='{0}'", marketId), null);

            DataGridView dgv;
            if (marketId == GlobalConfig.Instance.MarketID_CA)
            {
                dgv = dgv_ship_failed_order_ca;
            }
            else
            {
                dgv = dgv_ship_failed_order;
            }

            // 绑定到datagridview
            dgv.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgv.DataSource = ds;
            dgv.DataMember = "Table";
            dgv.ReadOnly = true;
            dgv.AllowUserToDeleteRows = false;
            dgv.AllowUserToAddRows = false;
            for (int i = 0; i < 5; ++i)
            {
                dgv.AutoResizeColumn(i);
            }
        }


        private void dgv_listing_offer_BoundDS()
        {
            //*** 从数据库中读取未发货订单
            DataSet ds = MySqlHelper.GetDataSet(MySqlHelper.Conn, System.Data.CommandType.Text,
                string.Format("select a.sku, b.current_offer_number, sum(quantity) as sales_count from t_order_product as a, t_product_offer_number as b where a.sku=b.sku and a.market_id='{0}' and b.market_id='{0}' and a.sku not like '%_G' and b.current_offer_number>b.default_offer_number group by a.sku order by sales_count desc;", marketId, marketId), null);
            DataGridView dgv = dgv_listing_offer;
            if (marketId == GlobalConfig.Instance.MarketID_CA)
            {
                //dgv = dgv_listing_offer_ca;
            }

            // 绑定到datagridview
            dgv.SelectionMode = DataGridViewSelectionMode.CellSelect;
            dgv.DataSource = ds;
            dgv.DataMember = "Table";
            dgv.ReadOnly = true;
            dgv.AllowUserToDeleteRows = false;
            dgv.AllowUserToAddRows = false;
            for (int i = 0; i < 2; ++i)
            {
                dgv.AutoResizeColumn(i);
            }
        }


        private void btn_tag_observed_Click(object sender, EventArgs e)
        {
            tag_observed();
        }


        private void tag_observed()
        {
            // 标记为待观察订单
            List<string> observedOrderList = new List<string>();
            List<string> observedOrderIdList = new List<string>();

            DataGridView dgv;
            if (marketId == GlobalConfig.Instance.MarketID_CA)
            {
                dgv = dgv_unshipped_order_ca;
            }
            else
            {
                dgv = dgv_unshipped_order;
            }
            foreach (DataGridViewRow c in dgv.SelectedRows)
            {
                observedOrderList.Add(String.Format("{0} {1} {2}", c.Cells["order_id"].Value.ToString(), c.Cells["buyer_name"].Value.ToString(), c.Cells["sku"].Value.ToString()));
                observedOrderIdList.Add(String.Format("'{0}'", c.Cells["order_id"].Value.ToString()));
            }

            mark_as_observed(observedOrderList, observedOrderIdList);

            // 刷新datagridview
            dgv_unshipped_order_BoundDS();
        }


        private void mark_as_observed(List<string> observedOrderList, List<string> observedOrderIdList)
        {
            // 弹出确认对话框
            string msgBoxText = String.Format("确定要将以下订单标记为待观察吗？\n{0}", String.Join("\n", observedOrderList));
            DialogResult dlgResult = MessageBox.Show(msgBoxText, "确认", MessageBoxButtons.OKCancel);
            if (dlgResult == DialogResult.OK)
            {
                // 写入数据库
                string cmdText = String.Format("update t_order_basic set order_type=2 where order_id in ({0}) and market_id='{1}'", String.Join(",", observedOrderIdList), marketId);
                int affectedRowsCount = MySqlHelper.ExecuteNonQuery(MySqlHelper.Conn, System.Data.CommandType.Text, cmdText, null);
                Console.WriteLine("Affected Rows: " + affectedRowsCount);
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
                case 4:
                    dgv_fba_order_BoundDS();
                    break;
                case 8:
                    dgv_listing_offer_BoundDS();
                    break;
                default:
                    break;
            }
        }

        private void btn_ship_order_Click(object sender, EventArgs e)
        {
            confirm_ship_order();
        }


        private void confirm_ship_order()
        {
            // 从数据库获取待发货订单的数量
            DataSet ds = MySqlHelper.GetDataSet(MySqlHelper.Conn, System.Data.CommandType.Text,
                String.Format("select order_id, track_id, confirmed_date from t_order_basic where order_status=0 and order_type=0 and is_confirmed=1 and market_id='{0}'", marketId), null);

            // 弹出确认对话框
            string msgBoxText = String.Format("请确定此次待发货订单数：{0}个？", ds.Tables[0].Rows.Count);
            DialogResult dlgResult = MessageBox.Show(msgBoxText, "确认", MessageBoxButtons.OKCancel);
            if (dlgResult == DialogResult.OK)
            {
                // 在亚马逊确认发货
                //DateTime now = DateTime.Now.AddHours(-15);  // 转成亚马逊的当地时间
                DateTime now = DateTime.Now.AddHours(-int.Parse(GlobalConfig.Instance.GetConfigValue(marketId, "timeDifference")));  // 转成亚马逊的当地时间
                //string fileName = String.Format("confirmOrderFeed/confirmOrderFeed_{0}.txt", now.ToString(GlobalConfig.Instance.TimeFormat));
                string fileName = String.Format("confirmOrderFeed/{0}_confirmOrderFeed_{1}.txt", marketId, now.ToString(GlobalConfig.Instance.GetCommonConfigValue("timeFormat")));

                //*** 生成feed的txt文件
                //*** order-id	order-item-id	quantity	ship-date	carrier-code	carrier-name	tracking-number	ship-method
                //string ship_date = now.ToString(GlobalConfig.Instance.ShipDateFormat);
                //string rowFormat = "{0}\t\t\t"+ ship_date +"\t"+ GlobalConfig.Instance.ShipCarrierCode + "\t"+ GlobalConfig.Instance.ShipCarrierName + "\t{1}\t";
                //string ship_date = now.ToString(GlobalConfig.Instance.GetCommonConfigValue("shipDateFormat"));
                string rowFormat = "{0}\t\t\t{1}\t" + GlobalConfig.Instance.GetConfigValue(marketId, "shipCarrierCode") + "\t" + GlobalConfig.Instance.GetConfigValue(marketId, "shipCarrierName") + "\t{2}\t";
                List<string> orderList = new List<string>();

                // 标题行
                orderList.Add(String.Format("{0}\t{1}\t{2}\t{3}\t{4}\t{5}\t{6}\t{7}", "order-id", "order-item-id", "quantity", "ship-date", "carrier-code", "carrier-name", "tracking-number", "ship-method"));

                foreach (DataRow dr in ds.Tables[0].Rows)
                {
                    //Datetime confirmed_date = (DateTime)dr["confirmed_date"];
                    orderList.Add(String.Format(rowFormat, dr["order_id"], ((DateTime)dr["confirmed_date"]).ToString(GlobalConfig.Instance.GetCommonConfigValue("shipDateFormat")), dr["track_id"]));
                }
                System.IO.File.WriteAllLines(fileName, orderList);

                //*** 调用submit feed方法
                //MarketplaceWebServiceSamples.SubmitFeed(fileName, GlobalConfig.Instance.ConfirmOrderFeedType);
                MarketplaceWebServiceSamples.SubmitFeed(marketId, fileName, GlobalConfig.Instance.GetCommonConfigValue("confirmOrderFeedType"));

                // 写入数据库
                string cmdText = string.Format("update t_order_basic set order_status=1 where order_status=0 and order_type=0 and market_id='{0}'", marketId);
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
            else if (responseContent.IndexOf("status-alert") != -1) // 投递失败
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
            update_track_id();
        }

        private void update_track_id()
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
                    int start_pos = 0;
                    while (words[start_pos].Trim() == "")
                    {
                        ++start_pos;
                    }
                    // 获取亚马逊的订单id
                    cmdList.Add(String.Format("update t_order_basic set track_id='{0}' where order_id='{1}' and market_id='{2}'", words[start_pos + 1], words[start_pos], marketId));
                    line = f.ReadLine();
                }
                string cmdText = String.Join(";", cmdList);
                int affectedRowsCount = MySqlHelper.ExecuteNonQuery(MySqlHelper.Conn, System.Data.CommandType.Text, cmdText, null);
                MessageBox.Show("更新个数：" + affectedRowsCount, "确认", MessageBoxButtons.OK);

                // 刷新datagridview
                dgv_unshipped_order_BoundDS();
            }
        }

        private void btn_tag_fake_shipped_Click(object sender, EventArgs e)
        {
            tag_fake_shipped();
        }

        private void tag_fake_shipped()
        {
            // 标记为假发货订单
            List<string> fakeShippedOrderList = new List<string>();
            List<string> fakeShippedOrderIdList = new List<string>();

            DataGridView dgv;
            if (marketId == GlobalConfig.Instance.MarketID_CA)
            {
                dgv = dgv_unshipped_order_ca;
            }
            else
            {
                dgv = dgv_unshipped_order;
            }
            foreach (DataGridViewRow c in dgv.SelectedRows)
            {
                fakeShippedOrderList.Add(String.Format("{0} {1} {2}", c.Cells["order_id"].Value.ToString(), c.Cells["buyer_name"].Value.ToString(), c.Cells["sku"].Value.ToString()));
                fakeShippedOrderIdList.Add(String.Format("'{0}'", c.Cells["order_id"].Value.ToString()));
            }

            mark_as_fake_shipped(fakeShippedOrderList, fakeShippedOrderIdList);

            // 刷新datagridview
            dgv_unshipped_order_BoundDS();
        }

        private void mark_as_fake_shipped(List<string> fakeShippedOrderList, List<string> fakeShippedOrderIdList)
        {
            // 弹出确认对话框
            string msgBoxText = String.Format("确定要将以下订单标记为假发货吗？\n{0}", String.Join("\n", fakeShippedOrderList));
            DialogResult dlgResult = MessageBox.Show(msgBoxText, "确认", MessageBoxButtons.OKCancel);
            if (dlgResult == DialogResult.OK)
            {
                // 写入数据库
                string cmdText = String.Format("update t_order_basic set order_type=1 where order_id in ({0}) and market_id='{1}'", String.Join(",", fakeShippedOrderIdList), marketId);
                int affectedRowsCount = MySqlHelper.ExecuteNonQuery(MySqlHelper.Conn, System.Data.CommandType.Text, cmdText, null);
            }
        }

        private void btn_show_order_detail_Click(object sender, EventArgs e)
        {
            show_order_detail("");
        }

        private void show_order_detail(string market_id)
        {
            // 按照sku归类，数量从多到少排列，排除掉包含多个不同货物的订单
            DataSet ds;
            DataSet ds_order_count;
            DataSet ds_large_order_count;
            DataSet ds_sku_count;
            DataSet model_ds;
            DataSet seller_ds;
            DataSet seller_model_ds;
            if (market_id == "") // 统计所有站点
            {
                // 获取每个sku的销售数量
                ds = MySqlHelper.GetDataSet(MySqlHelper.Conn, System.Data.CommandType.Text,
                    "select b.sku, sum(quantity) as total from t_order_basic as a, t_order_product as b where a.order_id=b.order_id and a.order_status=0 and a.order_type=0 group by b.sku order by total desc", null);

                // 获取订单总数
                ds_order_count = MySqlHelper.GetDataSet(MySqlHelper.Conn, System.Data.CommandType.Text, "select count(*) from t_order_basic where order_status=0 and order_type=0;", null);

                // 获取多个订单总数
                ds_large_order_count = MySqlHelper.GetDataSet(MySqlHelper.Conn, System.Data.CommandType.Text,
                    "select b.order_id from t_order_basic as a, t_order_product as b where a.order_id=b.order_id and a.order_status=0 and a.order_type=0 group by b.order_id having sum(quantity)>1;", null);

                // 从数据库中读取未发货订单货物总数
                ds_sku_count = MySqlHelper.GetDataSet(MySqlHelper.Conn, System.Data.CommandType.Text,
                    "select sum(b.quantity) as total from t_order_basic as a, t_order_product as b where a.order_status=0 and a.order_type=0 and a.order_id=b.order_id;", null);

                // 统计各个机型的销售数量
                model_ds = MySqlHelper.GetDataSet(MySqlHelper.Conn, System.Data.CommandType.Text,
                    "select c.model, sum(b.quantity) as total from t_order_basic as a, t_order_product as b, t_product as c where a.order_status=0 and a.order_type=0 and a.order_id=b.order_id and b.sku=c.sku group by c.model order by total desc;", null);

                // 统计各个商家的销售数量
                seller_ds = MySqlHelper.GetDataSet(MySqlHelper.Conn, System.Data.CommandType.Text,
                    "select d.seller_name, sum(b.quantity) as total from t_order_basic as a, t_order_product as b, t_product as c, t_seller as d where a.order_status=0 and a.order_type=0 and a.order_id=b.order_id and b.sku=c.sku and c.seller=d.seller_id group by c.seller order by total desc;", null);

                // 统计各个商家各个型号的销量
                seller_model_ds = MySqlHelper.GetDataSet(MySqlHelper.Conn, System.Data.CommandType.Text,
                    "select sum(b.quantity) as total, c.model, c.seller, d.seller_name from t_order_basic as a, t_order_product as b, t_product as c, t_seller as d where a.order_id=b.order_id and a.order_status=0 and a.order_type=0 and b.sku=c.sku and c.seller=d.seller_id group by c.seller, c.model order by c.seller;", null);

            }
            else  // 统计指定站点
            {
                // 获取每个sku的销售数量
                ds = MySqlHelper.GetDataSet(MySqlHelper.Conn, System.Data.CommandType.Text,
                    string.Format("select b.sku, sum(quantity) as total from t_order_basic as a, t_order_product as b where a.order_id=b.order_id and a.market_id='{0}' and a.order_status=0 and a.order_type=0 group by b.sku order by total desc", market_id), null);

                // 获取订单总数
                ds_order_count = MySqlHelper.GetDataSet(MySqlHelper.Conn, System.Data.CommandType.Text, string.Format("select count(*) from t_order_basic where order_status=0 and order_type=0 and market_id='{0}';", market_id), null);

                // 获取多个订单总数
                ds_large_order_count = MySqlHelper.GetDataSet(MySqlHelper.Conn, System.Data.CommandType.Text,
                    string.Format("select b.order_id from t_order_basic as a, t_order_product as b where a.market_id='{0}' and a.order_id=b.order_id and a.order_status=0 and a.order_type=0  group by b.order_id having sum(quantity)>1;", market_id), null);

                // 从数据库中读取未发货订单货物总数
                ds_sku_count = MySqlHelper.GetDataSet(MySqlHelper.Conn, System.Data.CommandType.Text,
                    string.Format("select sum(b.quantity) as total from t_order_basic as a, t_order_product as b where a.market_id='{0}' and a.order_status=0 and a.order_type=0 and a.order_id=b.order_id;", market_id), null);

                // 统计各个机型的销售数量
                model_ds = MySqlHelper.GetDataSet(MySqlHelper.Conn, System.Data.CommandType.Text,
                    string.Format("select c.model, sum(b.quantity) as total from t_order_basic as a, t_order_product as b, t_product as c where a.market_id='{0}' and a.order_status=0 and a.order_type=0 and a.order_id=b.order_id and b.sku=c.sku group by c.model order by total desc;", market_id), null);

                // 统计各个商家的销售数量
                seller_ds = MySqlHelper.GetDataSet(MySqlHelper.Conn, System.Data.CommandType.Text,
                    string.Format("select d.seller_name, sum(b.quantity) as total from t_order_basic as a, t_order_product as b, t_product as c, t_seller as d where a.market_id='{0}' and a.order_status=0 and a.order_type=0 and a.order_id=b.order_id and b.sku=c.sku and c.seller=d.seller_id group by c.seller order by total desc;", market_id), null);

                // 统计各个商家各个型号的销量
                seller_model_ds = MySqlHelper.GetDataSet(MySqlHelper.Conn, System.Data.CommandType.Text,
                    string.Format("select sum(b.quantity) as total, c.model, c.seller, d.seller_name from t_order_basic as a, t_order_product as b, t_product as c, t_seller as d where a.market_id='{0}' and a.order_id=b.order_id and a.order_status=0 and a.order_type=0 and b.sku=c.sku and c.seller=d.seller_id group by c.seller, c.model order by c.seller;", market_id), null);

            }
            DateTime now = DateTime.Now;
            string fileName = String.Format("packOrderDetail\\{0}_packOrderDetail_{1}.txt", market_id, now.ToString(GlobalConfig.Instance.GetCommonConfigValue("timeFormat")));

            //*** 生成txt文件
            List<string> skuCountList = new List<string>();

            skuCountList.Add("订单总数：" + ds_order_count.Tables[0].Rows[0][0].ToString() + " 个");
            skuCountList.Add("超过1个货物的订单总数：" + ds_large_order_count.Tables[0].Rows.Count + " 个");
            /*
            skuCountList.Add("-----------------------------------------------");
            skuCountList.Add("-----------------------------------------------");
            skuCountList.Add("物流包裹SKU归类统计");
            skuCountList.Add("-----------------------------------------------");
            skuCountList.Add("-----------------------------------------------");
            foreach (DataRow dr in ds.Tables[0].Rows)
            {
                skuCountList.Add(String.Format("{0,-20}\t{1}", dr["sku"], dr["total"]));
            }
            */


            skuCountList.Add("-----------------------------------------------");
            skuCountList.Add("-----------------------------------------------");
            skuCountList.Add("SKU汇总统计");
            skuCountList.Add(String.Format("SKU总个数：{0}个", ds_sku_count.Tables[0].Rows[0]["total"]));
            skuCountList.Add("-----------------------------------------------");
            skuCountList.Add("-----------------------------------------------");


            foreach (DataRow dr in ds.Tables[0].Rows)
            {
                skuCountList.Add(String.Format("{0,-20}\t{1}", dr["sku"], dr["total"]));
            }

            // 机型汇总统计
            skuCountList.Add("-----------------------------------------------");
            skuCountList.Add("-----------------------------------------------");
            skuCountList.Add("机型汇总统计");
            skuCountList.Add("-----------------------------------------------");
            skuCountList.Add("-----------------------------------------------");

            foreach (DataRow dr in model_ds.Tables[0].Rows)
            {
                skuCountList.Add(String.Format("{0,-10}\t{1}", dr["model"], dr["total"]));
            }

            // 商家汇总统计
            skuCountList.Add("-----------------------------------------------");
            skuCountList.Add("-----------------------------------------------");
            skuCountList.Add("商家汇总统计");
            skuCountList.Add("-----------------------------------------------");
            skuCountList.Add("-----------------------------------------------");

            foreach (DataRow dr in seller_ds.Tables[0].Rows)
            {
                skuCountList.Add(String.Format("{0,-10}\t{1}", dr["seller_name"], dr["total"]));
            }

            // 商家各型号汇总统计
            skuCountList.Add("-----------------------------------------------");
            skuCountList.Add("-----------------------------------------------");
            skuCountList.Add("商家各型号汇总统计");
            skuCountList.Add("-----------------------------------------------");
            skuCountList.Add("-----------------------------------------------");

            foreach (DataRow dr in seller_model_ds.Tables[0].Rows)
            {
                skuCountList.Add(String.Format("{0}#{1,-10}\t{2, -10}\t{3}", dr["seller"], dr["seller_name"], dr["model"], dr["total"]));
            }

            System.IO.File.WriteAllLines(fileName, skuCountList);

            // 打开文件
            System.Diagnostics.Process.Start("notepad.exe", System.Environment.CurrentDirectory + "\\" + fileName);
        }

        private void btn_get_fba_order_Click(object sender, EventArgs e)
        {
            DialogResult dlg = MessageBox.Show("正在从亚马逊拉取FBA订单入库，请稍等...", "提示", MessageBoxButtons.OK);
            //getFBAOrders();
            dgv_fba_order_BoundDS();
        }

        private void dgv_fba_order_RowPostPaint(object sender, DataGridViewRowPostPaintEventArgs e)
        {
            SolidBrush solidBrush = new SolidBrush(dgv_fba_order.RowHeadersDefaultCellStyle.ForeColor);
            e.Graphics.DrawString((e.RowIndex + 1).ToString(), e.InheritedRowStyle.Font, solidBrush, e.RowBounds.Location.X + 15, e.RowBounds.Location.Y + 5);
        }

        private void btn_modify_price_Click(object sender, EventArgs e)
        {
            modify_price();
        }


        private void modify_price()
        {
            string sku = tb_sku.Text.ToString();
            string price = tb_price.Text.ToString();
            string list_price = tb_list_price.Text.ToString();

            if (marketId == GlobalConfig.Instance.MarketID_CA)
            {
                sku = tb_sku_ca.Text.ToString();
                price = tb_price_ca.Text.ToString();
                list_price = tb_list_price_ca.Text.ToString();
            }
            // 弹出确认对话框
            string msgBoxText = String.Format("当前站点：{0}  确定要将 {1} 的价格改为 {2} 吗？", marketId, sku, price);
            DialogResult dlgResult = MessageBox.Show(msgBoxText, "确认", MessageBoxButtons.OKCancel);
            if (dlgResult == DialogResult.OK)
            {
                //*** 生成feed的xml文件
                DateTime now = DateTime.Now;
                string fileName = String.Format("priceFeed/{0}_{1}_{2}.xml", marketId, sku, now.ToString(GlobalConfig.Instance.GetCommonConfigValue("timeFormat")));
                File.Copy("priceFeed/priceFeedTemplate.xml", fileName);
                XmlDocument xmlDocument = new XmlDocument();
                xmlDocument.Load(fileName);
                var root = xmlDocument.DocumentElement;
                string currency = GlobalConfig.Instance.GetConfigValue(marketId, "currency");
                string priceFormat = "<MessageID>1</MessageID><Price><SKU>{0}</SKU><StandardPrice currency=\"{1}\">{2:N2}</StandardPrice><Sale><StartDate>{3}</StartDate><EndDate>{4}</EndDate><SalePrice currency=\"{5}\">{6:N2}</SalePrice></Sale></Price>";
                XmlElement newElement = xmlDocument.CreateElement("Message");
                //newElement.InnerXml = String.Format(priceFormat, sku, list_price, GlobalConfig.Instance.SaleStartDate, GlobalConfig.Instance.SaleEndDate, price);
                newElement.InnerXml = String.Format(priceFormat, sku, currency, list_price, GlobalConfig.Instance.GetCommonConfigValue("saleStartDate"), GlobalConfig.Instance.GetCommonConfigValue("saleEndDate"), currency, price);
                root.AppendChild(newElement);
                xmlDocument.Save(fileName);

                //*** 调用submit feed方法
                MarketplaceWebServiceSamples.SubmitFeed(marketId, fileName, GlobalConfig.Instance.GetCommonConfigValue("adaptPriceFeedType"));

                MessageBox.Show("已修改，请检查是否修改成功！", "确认", MessageBoxButtons.OK);
            }
        }


        // 入库
        private void button3_Click(object sender, EventArgs e)
        {
            string sku = tb_inbound_sku.Text.ToString();
            string quantity = tb_inbound_quantity.Text.ToString();
            // 弹出确认对话框
            string msgBoxText = String.Format("{0} 进货 {1} 个？", sku, quantity);
            DialogResult dlgResult = MessageBox.Show(msgBoxText, "确认", MessageBoxButtons.OKCancel);
            if (dlgResult == DialogResult.OK)
            {
                int affectedRowsCount = MySqlHelper.ExecuteNonQuery(MySqlHelper.Conn, System.Data.CommandType.Text,
                    String.Format("update t_product set inventory=inventory+{0} where sku='{1}'", quantity, sku), null);
                if (affectedRowsCount != 1)
                {
                    MessageBox.Show("更新失败，请检查SKU是否存在！", "确认", MessageBoxButtons.OK);
                }
                else
                {
                    // 保存进货记录
                    MySqlHelper.ExecuteNonQuery(MySqlHelper.Conn, System.Data.CommandType.Text,
                    String.Format("insert into t_inbound_record(sku, total, create_date) values('{0}',{1},'{2}');", sku, quantity, DateTime.Now.ToShortDateString()), null);
                    tb_inbound_quantity.Text = "";
                    tb_inbound_sku.Text = "";
                }


            }
        }

        // 已废弃
        private void button4_Click(object sender, EventArgs e)
        {

        }

        // 更新库存
        private void button6_Click(object sender, EventArgs e)
        {
            //update_inventory();
        }

        // 更新库存
        private void update_inventory(string market_id)
        {
            //*** 从数据库中读取未发货订单的各个sku的数量
            DataSet ds = MySqlHelper.GetDataSet(MySqlHelper.Conn, System.Data.CommandType.Text,
                string.Format("select b.sku, sum(b.quantity) as sku_sum from t_order_basic as a, t_order_product as b where a.market_id='{0}' and a.order_status=0 and a.order_type=0 and a.is_outbound=0 and a.order_id=b.order_id group by b.sku;", market_id), null);


            // 更新库存，更新相应sku的阈值
            List<string> cmdTextList = new List<string>();
            foreach (DataRow dr in ds.Tables[0].Rows)
            {
                // 进货量 = 阈值*2 - 当前库存
                cmdTextList.Add(String.Format("update t_product set inventory=inventory-{0}, inventory_threshold={1} where sku='{2}'", dr["sku_sum"], Int32.Parse(dr["sku_sum"].ToString()) * 2, dr["sku"]));
            }

            // 销量单独统计，假发货列表中的也要统计进来
            DataSet ds_sku_total = MySqlHelper.GetDataSet(MySqlHelper.Conn, System.Data.CommandType.Text,
                "select b.sku, sum(b.quantity) as sku_sum from t_order_basic as a, t_order_product as b where ((a.order_status=0 and a.order_type=0) or (a.order_type=1)) and a.order_id=b.order_id group by b.sku;", null);
            foreach (DataRow dr in ds_sku_total.Tables[0].Rows)
            {
                cmdTextList.Add(String.Format("update t_product set today_sales={0} where sku='{1}'", Int32.Parse(dr["sku_sum"].ToString()), dr["sku"]));
            }

            // 将表单标记为outbound 
            cmdTextList.Add(string.Format("update t_order_basic set is_outbound=1 where order_type=0 and order_status=0 and market_id='{0}'", market_id));

            // 写入数据库
            int affectedRowsCount = MySqlHelper.ExecuteNonQuery(MySqlHelper.Conn, System.Data.CommandType.Text, String.Join(";", cmdTextList), null);
            Console.WriteLine("Affected Rows: " + affectedRowsCount);
            // MessageBox.Show("已更新，请获取进货报告！", "确认", MessageBoxButtons.OK);
        }


        // 获取进货报告
        private void button5_Click(object sender, EventArgs e)
        {

            get_restock_report();

        }

        private void get_restock_report()
        {
            // 生成今日缺货数量
            //DataSet ds = MySqlHelper.GetDataSet(MySqlHelper.Conn, System.Data.CommandType.Text,
            //    "select a.sku, a.inventory, a.inventory_threshold, b.seller_name, b.seller_addr, a.seller_price from t_product as a left join t_seller as b on a.seller=b.seller_id where a.inventory<a.inventory_threshold order by b.seller_addr, b.seller_name, a.sku", null);

            DataSet ds = GetRestockDataSet();

            DateTime now = DateTime.Now;
            string fileName = String.Format("restockReport\\restockReport_{0}.txt", now.ToString(GlobalConfig.Instance.GetCommonConfigValue("timeFormat")));

            //*** 生成txt文件
            List<string> restockList = new List<string>();
            string rowTemplate = "{0,-5}\t{1,-20}\t{2,-10}\t{3,-10}\t{4,-10}";
            restockList.Add(String.Format(rowTemplate, "序号", "SKU", "库存", "厂家名称", "厂家地址"));
            int index = 0;
            foreach (DataRow dr in ds.Tables[0].Rows)
            {
                restockList.Add("------------------------------------------------------------------------------------------------------");
                ++index;
                // SKU 库存 厂家名称 厂家地址  
                restockList.Add(String.Format(rowTemplate, index, dr["sku"], dr["inventory"].ToString(), dr["seller_name"], dr["seller_addr"]));
            }

            System.IO.File.WriteAllLines(fileName, restockList);

            // 打开文件
            System.Diagnostics.Process.Start("notepad.exe", System.Environment.CurrentDirectory + "\\" + fileName);
        }

        private void dgv_observed_order_RowPostPaint(object sender, DataGridViewRowPostPaintEventArgs e)
        {
            SolidBrush solidBrush = new SolidBrush(dgv_observed_order.RowHeadersDefaultCellStyle.ForeColor);
            e.Graphics.DrawString((e.RowIndex + 1).ToString(), e.InheritedRowStyle.Font, solidBrush, e.RowBounds.Location.X + 15, e.RowBounds.Location.Y + 5);
        }

        private void dgv_fake_unshipped_order_RowPostPaint(object sender, DataGridViewRowPostPaintEventArgs e)
        {
            SolidBrush solidBrush = new SolidBrush(dgv_fake_unshipped_order.RowHeadersDefaultCellStyle.ForeColor);
            e.Graphics.DrawString((e.RowIndex + 1).ToString(), e.InheritedRowStyle.Font, solidBrush, e.RowBounds.Location.X + 15, e.RowBounds.Location.Y + 5);
        }

        private void dgv_ship_failed_order_RowPostPaint(object sender, DataGridViewRowPostPaintEventArgs e)
        {
            SolidBrush solidBrush = new SolidBrush(dgv_ship_failed_order.RowHeadersDefaultCellStyle.ForeColor);
            e.Graphics.DrawString((e.RowIndex + 1).ToString(), e.InheritedRowStyle.Font, solidBrush, e.RowBounds.Location.X + 15, e.RowBounds.Location.Y + 5);
        }

        private void button4_Click_1(object sender, EventArgs e)
        {
            tag_ship_failed_order_as_finished();
        }


        private void tag_ship_failed_order_as_finished()
        {
            // 将发货失败订单标记为已处理
            List<string> orderList = new List<string>();
            List<string> orderIdList = new List<string>();

            DataGridView dgv;
            if (marketId == GlobalConfig.Instance.MarketID_CA)
            {
                dgv = dgv_ship_failed_order_ca;
            }
            else
            {
                dgv = dgv_ship_failed_order;
            }

            foreach (DataGridViewRow c in dgv.SelectedRows)
            {
                orderList.Add(String.Format("{0} {1}", c.Cells["order_id"].Value.ToString(), c.Cells["buyer_name"].Value.ToString()));
                orderIdList.Add(String.Format("'{0}'", c.Cells["order_id"].Value.ToString()));
            }

            // 弹出确认对话框
            string msgBoxText = String.Format("确定要将以下发货失败订单标记为已处理吗？\n{0}", String.Join("\n", orderList));
            DialogResult dlgResult = MessageBox.Show(msgBoxText, "确认", MessageBoxButtons.OKCancel);
            if (dlgResult == DialogResult.OK)
            {
                // 写入数据库
                string cmdText = String.Format("update t_order_basic set is_finished=1 where order_id in ({0})", String.Join(",", orderIdList));
                int affectedRowsCount = MySqlHelper.ExecuteNonQuery(MySqlHelper.Conn, System.Data.CommandType.Text, cmdText, null);
                Console.WriteLine("Affected Rows: " + affectedRowsCount);

                // 刷新datagridview
                dgv_ship_failed_order_BoundDS();
            }
        }

        private void button7_Click(object sender, EventArgs e)
        {
            tag_fake_shipped_order_as_finished();
        }


        private void tag_fake_shipped_order_as_finished()
        {
            // 将假发货订单标记为已处理
            List<string> orderList = new List<string>();
            List<string> orderIdList = new List<string>();
            List<string> fakeOrderList = new List<string>();
            // 标题行 order-id	order-item-id	quantity	ship-date	carrier-code	carrier-name	tracking-number	ship-method
            fakeOrderList.Add(String.Format("{0}\t{1}\t{2}\t{3}\t{4}\t{5}\t{6}\t{7}", "order-id", "order-item-id", "quantity", "ship-date", "carrier-code", "carrier-name", "tracking-number", "ship-method"));
            //string rowFormat = "{0}\t\t\t\t\t\t{1}\t";
            //DateTime now = DateTime.Now.AddHours(-15);  // 转成亚马逊的当地时间

            //*** order-id	order-item-id	quantity	ship-date	carrier-code	carrier-name	tracking-number	ship-method
            string rowFormat = "{0}\t\t\t{1}\t" + GlobalConfig.Instance.GetConfigValue(marketId, "shipCarrierCode") + "\t" + GlobalConfig.Instance.GetConfigValue(marketId, "shipCarrierName") + "\t{2}\t";

            DataGridView dgv;
            if (marketId == GlobalConfig.Instance.MarketID_CA)
            {
                dgv = dgv_fake_unshipped_order_ca;
            }
            else
            {
                dgv = dgv_fake_unshipped_order;
            }
            foreach (DataGridViewRow c in dgv.SelectedRows)
            {
                orderList.Add(String.Format("{0} {1}", c.Cells["order_id"].Value.ToString(), c.Cells["buyer_name"].Value.ToString()));
                orderIdList.Add(String.Format("'{0}'", c.Cells["order_id"].Value.ToString()));
                DateTime confirmed_date = (DateTime)c.Cells["confirmed_date"].Value;
                fakeOrderList.Add(String.Format(rowFormat, c.Cells["order_id"].Value.ToString(), confirmed_date.ToString("yyyy-MM-dd"), c.Cells["track_id"].Value.ToString()));
            }

            // 弹出确认对话框
            string msgBoxText = String.Format("确定要将以下假发货订单标记为已发货吗？\n{0}", String.Join("\n", orderList));
            DialogResult dlgResult = MessageBox.Show(msgBoxText, "确认", MessageBoxButtons.OKCancel);
            if (dlgResult == DialogResult.OK)
            {
                // 在亚马逊确认发货
                string fileName = String.Format("confirmFakeOrderFeed/{0}_confirmFakeOrderFeed_{1}.txt", marketId, DateTime.Now.ToString(GlobalConfig.Instance.GetCommonConfigValue("timeFormat")));

                //*** 生成feed的txt文件                
                System.IO.File.WriteAllLines(fileName, fakeOrderList);

                //*** 调用submit feed方法
                MarketplaceWebServiceSamples.SubmitFeed(marketId, fileName, GlobalConfig.Instance.GetCommonConfigValue("confirmOrderFeedType"));

                // 写入数据库
                string cmdText = String.Format("update t_order_basic set order_type=0, order_status=1 where order_id in ({0}) and market_id='{1}'", String.Join(",", orderIdList), marketId);
                int affectedRowsCount = MySqlHelper.ExecuteNonQuery(MySqlHelper.Conn, System.Data.CommandType.Text, cmdText, null);
                Console.WriteLine("Affected Rows: " + affectedRowsCount);

                // 刷新datagridview
                dgv_fake_shipped_order_BoundDS();
            }
        }

        private void button8_Click(object sender, EventArgs e)
        {
            tag_observed_order_as_finished();
        }


        private void tag_observed_order_as_finished()
        {
            // 将待观察订单标记为已处理
            List<string> orderList = new List<string>();
            List<string> orderIdList = new List<string>();

            DataGridView dgv;
            if (marketId == GlobalConfig.Instance.MarketID_CA)
            {
                dgv = dgv_observed_order_ca;
            }
            else
            {
                dgv = dgv_observed_order;
            }
            foreach (DataGridViewRow c in dgv.SelectedRows)
            {
                orderList.Add(String.Format("{0} {1}", c.Cells["order_id"].Value.ToString(), c.Cells["buyer_name"].Value.ToString()));
                orderIdList.Add(String.Format("'{0}'", c.Cells["order_id"].Value.ToString()));
            }

            // 弹出确认对话框
            string msgBoxText = String.Format("确定要将以下待观察订单标记为已处理吗？\n{0}", String.Join("\n", orderList));
            DialogResult dlgResult = MessageBox.Show(msgBoxText, "确认", MessageBoxButtons.OKCancel);
            if (dlgResult == DialogResult.OK)
            {
                // 写入数据库
                string cmdText = String.Format("update t_order_basic set is_finished=1 where order_id in ({0}) and market_id='{1}'", String.Join(",", orderIdList), marketId);
                int affectedRowsCount = MySqlHelper.ExecuteNonQuery(MySqlHelper.Conn, System.Data.CommandType.Text, cmdText, null);
                Console.WriteLine("Affected Rows: " + affectedRowsCount);

                // 刷新datagridview
                dgv_observed_order_BoundDS();
            }
        }




        // 生成进货集合列表
        private void button10_Click(object sender, EventArgs e)
        {
            create_restock_folder();
        }


        private void create_restock_folder()
        {
            //文件夹结构：日期-》市场-》编号-商家#地址-》型号-》
            string parent_path = @"F:\百度云同步盘\进货图片列表\进货" + DateTime.Now.ToString("yyyyMMdd");
            if (Directory.Exists(parent_path))
            {
                DirectoryInfo di = new DirectoryInfo(parent_path);
                di.Delete(true);
            }
            Directory.CreateDirectory(parent_path);
            string src_path = @"F:\百度云同步盘\商品图片\All Of The Cases\{0}.jpg";

            // 获取进货列表
            DataSet ds = GetRestockDataSet();
            foreach (DataRow dr in ds.Tables[0].Rows)
            {
                //文件夹结构：日期-》市场-》编号-商家#地址-》型号-》
                string child_path = String.Format(@"{0}\{1}\{2}-{3} # {4}\{5}", parent_path, dr["building_name"], dr["seller_id"], dr["seller_name"], dr["seller_addr"], dr["model"]);
                CreateDirectoryWithCheck(child_path);
                // 图片命名：进货数量#sku
                string dest_file = String.Format(@"{0}\{1}#{2}.jpg", child_path, dr["inventory"], dr["sku"]);
                // 拷贝文件 
                File.Copy(String.Format(src_path, dr["sku"]), dest_file);
            }

            MessageBox.Show("已生成进货集合列表，请先生成水印，再同步到百度云！", "确认", MessageBoxButtons.OK);
        }


        private void CreateDirectoryWithCheck(string path)
        {
            if (!Directory.Exists(path))
            {
                // Create the directory it does not exist.
                Directory.CreateDirectory(path);
            }
        }


        private DataSet GetRestockDataSet()
        {
            return MySqlHelper.GetDataSet(MySqlHelper.Conn, System.Data.CommandType.Text,
                "select a.sku, a.inventory, a.model, b.seller_id, b.seller_name, b.building_name, b.seller_addr from t_product as a left join t_seller as b on a.seller=b.seller_id where a.inventory<0 order by b.seller_addr, a.seller, a.model, a.sku", null);

        }


        private void CorrectSKUName()
        {
            DirectoryInfo TheFolder = new DirectoryInfo(@"F:\百度云同步盘\商品图片\All Of The Cases");
            foreach (FileInfo NextFile in TheFolder.GetFiles())
            {
                if (NextFile.Name.Contains("-1.jpg"))
                {
                    NextFile.MoveTo(NextFile.Directory + @"/" + NextFile.Name.TrimEnd("-1.jpg".ToCharArray()) + ".jpg");
                }

            }
        }

        private void button9_Click(object sender, EventArgs e)
        {

        }


        // 生成配货图片集合
        private void button9_Click_1(object sender, EventArgs e)
        {
            create_match_folder(GlobalConfig.Instance.MarketID_US);
            create_match_folder(GlobalConfig.Instance.MarketID_CA);
        }


        private void create_match_folder(string market_id)
        {
            //文件夹结构：日期-》编号-商家-》型号-》
            string parent_path = @"F:\百度云同步盘\配货图片列表\配货_" + DateTime.Now.ToString("yyyyMMdd") + @"\" + market_id;
            if (Directory.Exists(parent_path))
            {
                DirectoryInfo di = new DirectoryInfo(parent_path);
                di.Delete(true);
            }
            Directory.CreateDirectory(parent_path);
            string src_path = @"F:\百度云同步盘\商品图片\All Of The Cases\{0}.jpg";

            // 获取配货列表
            DataSet ds = MySqlHelper.GetDataSet(MySqlHelper.Conn, System.Data.CommandType.Text,
               string.Format("select c.sku, sum(b.quantity) as total, c.model, c.seller, d.seller_name from t_order_basic as a, t_order_product as b, t_product as c, t_seller as d where a.market_id='{0}' and a.order_id=b.order_id and ((a.order_status=0 and a.order_type=0) or (a.order_type=1)) and b.sku=c.sku and c.seller=d.seller_id and c.inventory+c.today_sales>0 group by c.seller, c.model, c.sku;", market_id), null);


            foreach (DataRow dr in ds.Tables[0].Rows)
            {
                //文件夹结构：日期-》编号-商家-》型号-》
                string child_path = String.Format(@"{0}\{1}-{2}\{3}", parent_path, dr["seller"], dr["seller_name"], dr["model"]);
                CreateDirectoryWithCheck(child_path);
                // 图片命名：配货数量#sku
                string dest_file = String.Format(@"{0}\{1}#{2}.jpg", child_path, dr["total"], dr["sku"]);
                // 拷贝文件 
                File.Copy(String.Format(src_path, dr["sku"]), dest_file);
            }

            MessageBox.Show("已生成配货集合列表，请先生成水印，再同步到百度云！", "确认", MessageBoxButtons.OK);
        }

        private void tabPage6_Click(object sender, EventArgs e)
        {

        }


        //  一键入库
        private void button12_Click(object sender, EventArgs e)
        {
            // 弹出确认对话框
            DialogResult dlgResult = MessageBox.Show("一键入库：将进货列表中的所有sku按缺货数量入库。确认操作？", "确认", MessageBoxButtons.OKCancel);
            if (dlgResult == DialogResult.OK)
            {
                DataSet ds = GetRestockDataSet();
                foreach (DataRow dr in ds.Tables[0].Rows)
                {
                    int affectedRowsCount = MySqlHelper.ExecuteNonQuery(MySqlHelper.Conn, System.Data.CommandType.Text,
                    String.Format("update t_product set inventory=0 where sku='{0}'", dr["sku"]), null);
                    if (affectedRowsCount == 1)
                    {
                        // 保存进货记录
                        MySqlHelper.ExecuteNonQuery(MySqlHelper.Conn, System.Data.CommandType.Text,
                            String.Format("insert into t_inbound_record(sku, total, create_date) values('{0}',{1},'{2}');", dr["sku"], Math.Abs(int.Parse(dr["inventory"].ToString())), DateTime.Now.ToShortDateString()), null);
                    }
                }
                MessageBox.Show("操作成功，请录入其他情况（缺货sku & 多进货sku）", "确认", MessageBoxButtons.OK);
            }
        }


        // 今日进货统计
        private void button11_Click(object sender, EventArgs e)
        {
            DataSet ds = MySqlHelper.GetDataSet(MySqlHelper.Conn, System.Data.CommandType.Text,
                "select a.sku, sum(a.total) as total, c.seller_name, c.seller_addr from t_inbound_record as a, t_product as b, t_seller as c where a.create_date=curdate() and a.sku=b.sku and b.seller=c.seller_id group by a.sku order by c.seller_addr, c.seller_name, b.model, b.sku", null);

            DateTime now = DateTime.Now;
            string fileName = String.Format("inboundReport\\inboundReport_{0}.txt", now.ToString(GlobalConfig.Instance.GetCommonConfigValue("timeFormat")));

            //*** 生成txt文件
            List<string> restockList = new List<string>();
            string rowTemplate = "{0,-5}\t{1,-20}\t{2,-10}\t{3,-10}\t{4,-10}";
            restockList.Add(String.Format(rowTemplate, "序号", "SKU", "进货数量", "厂家名称", "厂家地址"));
            int index = 0;
            foreach (DataRow dr in ds.Tables[0].Rows)
            {
                restockList.Add("------------------------------------------------------------------------------------------------------");
                ++index;
                // SKU 进货数量 厂家名称 厂家地址  
                restockList.Add(String.Format(rowTemplate, index, dr["sku"], dr["total"].ToString(), dr["seller_name"], dr["seller_addr"]));
            }

            System.IO.File.WriteAllLines(fileName, restockList);

            // 打开文件
            System.Diagnostics.Process.Start("notepad.exe", System.Environment.CurrentDirectory + "\\" + fileName);
        }


        private void submitFeed(string market_id, string filename, string feedType)
        {
            System.Windows.Forms.TextBox tb_current;
            if (market_id == GlobalConfig.Instance.MarketID_CA)
            {
                tb_current = tb_feedMissionId_ca;
            }
            else
            {
                tb_current = tb_feedMissionId;
            }
            tb_current.Text = "";
            string feedSubmissionId = MarketplaceWebServiceSamples.SubmitFeed(market_id, filename, feedType);
            if (feedSubmissionId == "")
            {
                MessageBox.Show("请求失败，请重试", "确认", MessageBoxButtons.OK);
            }
            else
            {
                MessageBox.Show("请求成功，请稍后获取请求结果", "确认", MessageBoxButtons.OK);
                tb_current.Text = feedSubmissionId;
            }
        }

        // 创建父体
        private void button13_Click(object sender, EventArgs e)
        {
            create_variation_parent();
        }

        private void create_variation_parent()
        {
            string filename;
            if (marketId == GlobalConfig.Instance.MarketID_CA)
            {
                filename = tb_variation_parent_filename_ca.Text.ToString();
            }
            else
            {
                filename = tb_variation_parent_filename.Text.ToString();
            }
            if (filename == "")
            {
                MessageBox.Show("请先生成XML文件", "确认", MessageBoxButtons.OK);
            }
            else
            {
                DialogResult dlgResult = MessageBox.Show("确认操作？", "确认", MessageBoxButtons.OKCancel);
                if (dlgResult == DialogResult.OK)
                {
                    submitFeed(marketId, filename, GlobalConfig.Instance.GetCommonConfigValue("updateProductFeedType"));
                }
            }
        }

        // 子类加size-color
        private void button14_Click(object sender, EventArgs e)
        {
            append_size_color_to_child();
        }

        private void append_size_color_to_child()
        {
            string filename;
            if (marketId == GlobalConfig.Instance.MarketID_CA)
            {
                filename = tb_variation_child_filename_ca.Text.ToString();
            }
            else
            {
                filename = tb_variation_child_filename.Text.ToString();
            }
            if (filename == "")
            {
                MessageBox.Show("请先生成XML文件", "确认", MessageBoxButtons.OK);
            }
            else
            {
                DialogResult dlgResult = MessageBox.Show("确认操作？", "确认", MessageBoxButtons.OKCancel);
                if (dlgResult == DialogResult.OK)
                {
                    submitFeed(marketId, filename, GlobalConfig.Instance.GetCommonConfigValue("updateProductFeedType"));
                }
            }
        }

        // 子类绑定到父类
        private void button15_Click(object sender, EventArgs e)
        {
            bound_child_with_parent();
        }


        private void bound_child_with_parent()
        {
            string filename;
            if (marketId == GlobalConfig.Instance.MarketID_CA)
            {
                filename = tb_variation_bound_filename_ca.Text.ToString();
            }
            else
            {
                filename = tb_variation_bound_filename.Text.ToString();
            }
            if (filename == "")
            {
                MessageBox.Show("请先生成XML文件", "确认", MessageBoxButtons.OK);
            }
            else
            {
                DialogResult dlgResult = MessageBox.Show("确认操作？", "确认", MessageBoxButtons.OKCancel);
                if (dlgResult == DialogResult.OK)
                {
                    submitFeed(marketId, filename, GlobalConfig.Instance.GetCommonConfigValue("createRelationshipFeedType"));
                }
            }
        }


        // 获取feed执行结果
        private void button16_Click(object sender, EventArgs e)
        {
            get_variation_result();
        }


        private void get_variation_result()
        {
            string feedMissionId;
            if (marketId == GlobalConfig.Instance.MarketID_CA)
            {
                feedMissionId = tb_feedMissionId_ca.Text.ToString();
            }
            else
            {
                feedMissionId = tb_feedMissionId.Text.ToString();
            }
            if (feedMissionId != "")
            {
                DialogResult dlgResult = MessageBox.Show("确认操作？", "确认", MessageBoxButtons.OKCancel);
                if (dlgResult == DialogResult.OK)
                {
                    string fileName = String.Format("feedSubmissionResult\\feedSubmissionResult_{0}_{1}.xml", DateTime.Now.ToString(GlobalConfig.Instance.GetCommonConfigValue("timeFormat")), marketId);
                    MarketplaceWebServiceSamples.GetFeedSubmissionResult(marketId, feedMissionId, fileName);
                    // 打开文件
                    System.Diagnostics.Process.Start(System.Environment.CurrentDirectory + "\\" + fileName);
                }
            }
            else
            {
                MessageBox.Show("FeedMissionId不能为空", "确认", MessageBoxButtons.OK);
            }
        }


        // 生成子类变体模板
        private void button17_Click(object sender, EventArgs e)
        {
            create_variation_size_color();
        }

        private void create_variation_size_color()
        {
            DialogResult dlgResult = MessageBox.Show("确认操作？", "确认", MessageBoxButtons.OKCancel);

            System.Windows.Forms.TextBox tb_current;
            System.Windows.Forms.TextBox tb_variation_child_filename_current;
            if (marketId == GlobalConfig.Instance.MarketID_CA)
            {
                tb_current = tb_variation_data_ca;
                tb_variation_child_filename_current = tb_variation_child_filename_ca;
            }
            else
            {
                tb_current = tb_variation_data;
                tb_variation_child_filename_current = tb_variation_child_filename;
            }

            if (dlgResult == DialogResult.OK && tb_current.Text.ToString() != "")
            {
                //*** 生成feed的xml文件
                DateTime now = DateTime.Now;
                string fileName = String.Format("createVariation\\childProductFeed_{0}_{1}.xml", now.ToString(GlobalConfig.Instance.GetCommonConfigValue("timeFormat")), marketId);
                File.Copy("createVariation\\childProductFeedTemplate.xml", fileName);
                XmlDocument xmlDocument = new XmlDocument();
                xmlDocument.Load(fileName);
                var root = xmlDocument.DocumentElement;

                string feedFormat = "<MessageID>{0}</MessageID><OperationType>PartialUpdate</OperationType><Product><SKU>{1}</SKU><ProductData><ToysBaby><ProductType>BabyProducts</ProductType><CustomerPackageType>Standard Packaging</CustomerPackageType><VariationData><Parentage>child</Parentage><VariationTheme>Size-Color</VariationTheme></VariationData><Size>{2}</Size><SizeMap>{3}</SizeMap><Color>{4}</Color><ColorMap>{5}</ColorMap></ToysBaby></ProductData></Product>";
                int index = 1;

                string[] sArray = Regex.Split(tb_current.Text.ToString(), "\r\n");
                foreach (string line in sArray)
                {
                    // sku color size
                    string[] sItem = line.Split('\t');
                    if (sItem.Count() == 3)
                    {
                        XmlElement newElement = xmlDocument.CreateElement("Message");
                        newElement.InnerXml = String.Format(feedFormat, index, sItem[0], sItem[2], sItem[2], sItem[1], sItem[1]);
                        root.AppendChild(newElement);
                        ++index;
                    }

                }
                xmlDocument.Save(fileName);
                tb_variation_child_filename_current.Text = fileName;
                MessageBox.Show("文件已生成，请检查文件内容", "确认", MessageBoxButtons.OK);
            }
        }


        // 生成绑定变体关系模板
        private void button18_Click(object sender, EventArgs e)
        {
            create_bound_variation();
        }


        private void create_bound_variation()
        {
            DialogResult dlgResult = MessageBox.Show("确认操作？", "确认", MessageBoxButtons.OKCancel);
            System.Windows.Forms.TextBox tb_current;
            System.Windows.Forms.TextBox tb_variation_bound_filename_current;
            if (marketId == GlobalConfig.Instance.MarketID_CA)
            {
                tb_current = tb_variation_data_ca;
                tb_variation_bound_filename_current = tb_variation_bound_filename_ca;
            }
            else
            {
                tb_current = tb_variation_data;
                tb_variation_bound_filename_current = tb_variation_bound_filename;
            }
            if (dlgResult == DialogResult.OK && tb_current.Text.ToString() != "")
            {
                //*** 生成feed的xml文件
                DateTime now = DateTime.Now;
                string fileName = String.Format("createVariation\\relationshipFeed_{0}_{1}.xml", now.ToString(GlobalConfig.Instance.GetCommonConfigValue("timeFormat")), marketId);
                File.Copy("createVariation\\relationshipTemplate.xml", fileName);
                XmlDocument xmlDocument = new XmlDocument();
                xmlDocument.Load(fileName);
                var root = xmlDocument.DocumentElement;

                string feedFormat = "<Relation><SKU>{0}</SKU><Type>Variation</Type></Relation>";
                string parent_sku = "";
                string feedContent = "";

                string[] sArray = Regex.Split(tb_current.Text.ToString(), "\r\n");
                foreach (string line in sArray)
                {
                    // sku parent_sku
                    string[] sItem = line.Split('\t');
                    if (sItem.Count() == 2)
                    {
                        feedContent += String.Format(feedFormat, sItem[0]);
                        parent_sku = sItem[1];
                    }
                }

                XmlElement newElement = xmlDocument.CreateElement("Message");
                newElement.InnerXml = String.Format("<MessageID>1</MessageID><OperationType>Update</OperationType><Relationship><ParentSKU>{0}</ParentSKU>{1}</Relationship>", parent_sku, feedContent);
                root.AppendChild(newElement);
                xmlDocument.Save(fileName);
                tb_variation_bound_filename_current.Text = fileName;
                MessageBox.Show("文件已生成，请检查文件内容", "确认", MessageBoxButtons.OK);
            }
        }

        private void tb_variation_data_TextChanged(object sender, EventArgs e)
        {

        }

        // 查看创建父体模板内容
        private void button19_Click(object sender, EventArgs e)
        {
            if (tb_variation_parent_filename.Text.ToString() == "")
            {
                MessageBox.Show("文件名不能为空", "确认", MessageBoxButtons.OK);
            }
            else
            {
                System.Diagnostics.Process.Start(System.Environment.CurrentDirectory + "\\" + tb_variation_parent_filename.Text.ToString());
            }

        }

        // 查看更新子类变体模板内容
        private void button20_Click(object sender, EventArgs e)
        {
            if (tb_variation_child_filename.Text.ToString() == "")
            {
                MessageBox.Show("文件名不能为空", "确认", MessageBoxButtons.OK);
            }
            else
            {
                System.Diagnostics.Process.Start(System.Environment.CurrentDirectory + "\\" + tb_variation_child_filename.Text.ToString());
            }
        }

        // 查看绑定变体模板内容
        private void button21_Click(object sender, EventArgs e)
        {
            if (tb_variation_bound_filename.Text.ToString() == "")
            {
                MessageBox.Show("文件名不能为空", "确认", MessageBoxButtons.OK);
            }
            else
            {
                System.Diagnostics.Process.Start(System.Environment.CurrentDirectory + "\\" + tb_variation_bound_filename.Text.ToString());
            }
        }

        private void button22_Click(object sender, EventArgs e)
        {


        }

        private void tabControl2_Click(object sender, EventArgs e)
        {

        }

        private void tabControl2_Selected(object sender, TabControlEventArgs e)
        {
            switch (e.TabPageIndex)
            {
                case 0:
                    marketId = GlobalConfig.Instance.MarketID_US;
                    break;
                case 1:
                    marketId = GlobalConfig.Instance.MarketID_CA;
                    break;
                default:
                    break;
            }
            dgv_unshipped_order_BoundDS();  // 显示未发货订单列表
        }

        private void button2_Click_1(object sender, EventArgs e)
        {
            get_order_report();
        }

        private void button26_Click(object sender, EventArgs e)
        {
            tag_observed();
        }


        private void button25_Click(object sender, EventArgs e)
        {
            // 生成EUB的订单文件
            fillShipFile();
            MessageBox.Show("导出成功！", "提示", MessageBoxButtons.OK);
        }


        // 生成皇家物流的订单文件
        private void fillShipFile_PFC()
        {
            try
            {
                //*** 从数据库中读取未发货订单, 按seller、model、sku排序
                DataSet ds = MySqlHelper.GetDataSet(MySqlHelper.Conn, System.Data.CommandType.Text,
                    string.Format("select a.*, b.sku, b.order_item_id, b.quantity, c.seller from t_order_basic as a, t_order_product as b, t_product as c where a.market_id='{0}' and a.order_id=b.order_id and a.order_status=0 and a.order_type=0 and b.sku=c.sku order by c.model,c.seller,b.sku;", marketId), null);

                //*** 从数据库中读取每个订单的数量
                DataSet dsOrderCount = MySqlHelper.GetDataSet(MySqlHelper.Conn, System.Data.CommandType.Text,
                    string.Format("select a.order_id, sum(b.quantity) as order_count from t_order_basic as a, t_order_product as b where a.market_id='{0}' and a.order_id=b.order_id and a.order_status=0 and a.order_type=0 group by b.order_id;", marketId), null);
                Dictionary<string, int> orderCountDict = new Dictionary<string, int>();
                foreach (DataRow dr in dsOrderCount.Tables[0].Rows)
                {
                    orderCountDict.Add(dr["order_id"].ToString(), Int32.Parse(dr["order_count"].ToString()));
                }

                string srcFilePath = System.Environment.CurrentDirectory + "\\shippingInfo\\shiptemplate_pfc.xls";


                //*** read shipping template
                Microsoft.Office.Interop.Excel.Application excel = new Microsoft.Office.Interop.Excel.ApplicationClass();
                excel.Visible = false;
                Workbook wBook = excel.Workbooks.Open(srcFilePath, Type.Missing, Type.Missing, Type.Missing, Type.Missing, Type.Missing, Type.Missing, Type.Missing, Type.Missing, Type.Missing, Type.Missing, Type.Missing, Type.Missing, Type.Missing, Type.Missing);
                Worksheet wSheet = wBook.Sheets[1] as Worksheet; //第一个sheet页

                //*** write order info
                int rowIndex = 2;


                // 1. 先处理只包含1个的            
                foreach (DataRow dr in ds.Tables[0].Rows)
                {
                    string order_id = dr["order_id"].ToString();
                    if (orderCountDict[order_id] == 1)
                    {
                        wSheet.Cells[rowIndex, 1] = order_id;
                        wSheet.Cells[rowIndex, 3] = "HKAPOST";   // 香港平邮
                        wSheet.Cells[rowIndex, 5] = dr["recipient_name"].ToString();
                        wSheet.Cells[rowIndex, 6] = dr["ship_country"].ToString();
                        wSheet.Cells[rowIndex, 7] = dr["ship_address_1"].ToString();
                        wSheet.Cells[rowIndex, 8] = dr["ship_address_2"].ToString();
                        wSheet.Cells[rowIndex, 9] = dr["ship_city"].ToString();
                        wSheet.Cells[rowIndex, 10] = dr["ship_state"].ToString();
                        wSheet.Cells[rowIndex, 11] = "'" + dr["ship_postal_code"].ToString();
                        wSheet.Cells[rowIndex, 12] = "'" + dr["buyer_phone"].ToString();
                        wSheet.Cells[rowIndex, 16] = dr["sku"].ToString();
                        wSheet.Cells[rowIndex, 17] = "Phone case#" + dr["seller"].ToString() + "  " + dr["sku"].ToString();
                        wSheet.Cells[rowIndex, 18] = "手机壳";
                        wSheet.Cells[rowIndex, 20] = dr["quantity"].ToString();
                        wSheet.Cells[rowIndex, 21] = 1;
                        wSheet.Cells[rowIndex, 22] = 0.03;
                        ++rowIndex;
                    }
                }

                // 2. 再处理包含2-3个的
                foreach (DataRow dr in ds.Tables[0].Rows)
                {
                    string order_id = dr["order_id"].ToString();
                    if (orderCountDict[order_id] == 2 || orderCountDict[order_id] == 3)
                    {
                        wSheet.Cells[rowIndex, 1] = order_id;
                        wSheet.Cells[rowIndex, 3] = "HKAPOST";   // 香港平邮
                        wSheet.Cells[rowIndex, 5] = dr["recipient_name"].ToString();
                        wSheet.Cells[rowIndex, 6] = dr["ship_country"].ToString();
                        wSheet.Cells[rowIndex, 7] = dr["ship_address_1"].ToString();
                        wSheet.Cells[rowIndex, 8] = dr["ship_address_2"].ToString();
                        wSheet.Cells[rowIndex, 9] = dr["ship_city"].ToString();
                        wSheet.Cells[rowIndex, 10] = dr["ship_state"].ToString();
                        wSheet.Cells[rowIndex, 11] = "'" + dr["ship_postal_code"].ToString();
                        wSheet.Cells[rowIndex, 12] = "'" + dr["buyer_phone"].ToString();
                        wSheet.Cells[rowIndex, 16] = dr["sku"].ToString();
                        wSheet.Cells[rowIndex, 17] = "Phone case# " + dr["seller"].ToString() + "  " + dr["sku"].ToString();
                        wSheet.Cells[rowIndex, 18] = "手机壳#1";
                        wSheet.Cells[rowIndex, 20] = dr["quantity"].ToString();
                        wSheet.Cells[rowIndex, 21] = 1;
                        wSheet.Cells[rowIndex, 22] = 0.03;
                        ++rowIndex;
                    }
                }

                // 3. 最后处理超过3个以上的
                foreach (DataRow dr in ds.Tables[0].Rows)
                {
                    string order_id = dr["order_id"].ToString();
                    if (orderCountDict[order_id] > 3)
                    {
                        wSheet.Cells[rowIndex, 1] = order_id;
                        wSheet.Cells[rowIndex, 3] = "HKAPOST";   // 香港平邮
                        wSheet.Cells[rowIndex, 5] = dr["recipient_name"].ToString();
                        wSheet.Cells[rowIndex, 6] = dr["ship_country"].ToString();
                        wSheet.Cells[rowIndex, 7] = dr["ship_address_1"].ToString();
                        wSheet.Cells[rowIndex, 8] = dr["ship_address_2"].ToString();
                        wSheet.Cells[rowIndex, 9] = dr["ship_city"].ToString();
                        wSheet.Cells[rowIndex, 10] = dr["ship_state"].ToString();
                        wSheet.Cells[rowIndex, 11] = "'" + dr["ship_postal_code"].ToString();
                        wSheet.Cells[rowIndex, 12] = "'" + dr["buyer_phone"].ToString();
                        wSheet.Cells[rowIndex, 16] = dr["sku"].ToString();
                        wSheet.Cells[rowIndex, 17] = "Phone case# " + dr["seller"].ToString() + "  " + dr["sku"].ToString();
                        wSheet.Cells[rowIndex, 18] = "手机壳#1";
                        wSheet.Cells[rowIndex, 20] = dr["quantity"].ToString();
                        wSheet.Cells[rowIndex, 21] = 1;
                        wSheet.Cells[rowIndex, 22] = 0.03;
                        ++rowIndex;
                    }
                }

                //****设置禁止弹出保存和覆盖的询问提示框 
                excel.DisplayAlerts = false;
                excel.AlertBeforeOverwriting = true;

                //string

                //保存
                DateTime now = DateTime.Now;
                //string destFilePath = String.Format(System.Environment.CurrentDirectory + "\\shippingInfo\\ship_{0}.xls", now.ToString(GlobalConfig.Instance.TimeFormat));
                string destFilePath = String.Format(System.Environment.CurrentDirectory + "\\shippingInfo\\ship_pfc_{0}_{1}.xls", marketId, now.ToString(GlobalConfig.Instance.GetCommonConfigValue("timeFormat")));
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

        private void button23_Click(object sender, EventArgs e)
        {
            update_track_id();
        }

        private void button1_Click_1(object sender, EventArgs e)
        {
            //create_match_folder();
        }

        private void button22_Click_1(object sender, EventArgs e)
        {
            tag_fake_shipped();
        }

        private void button24_Click(object sender, EventArgs e)
        {
            confirm_ship_order();
        }

        private void button27_Click(object sender, EventArgs e)
        {
            tag_observed_order_as_finished();
        }

        private void button28_Click(object sender, EventArgs e)
        {
            tag_fake_shipped_order_as_finished();
        }

        private void button29_Click(object sender, EventArgs e)
        {
            tag_ship_failed_order_as_finished();
        }

        private void tabControl3_Selected(object sender, TabControlEventArgs e)
        {

        }

        private void button30_Click(object sender, EventArgs e)
        {
            modify_price();
        }

        private void button38_Click(object sender, EventArgs e)
        {
            create_variation_size_color();
        }

        private void dgv_unshipped_order_ca_RowPostPaint(object sender, DataGridViewRowPostPaintEventArgs e)
        {
            SolidBrush solidBrush = new SolidBrush(dgv_unshipped_order_ca.RowHeadersDefaultCellStyle.ForeColor);
            e.Graphics.DrawString((e.RowIndex + 1).ToString(), e.InheritedRowStyle.Font, solidBrush, e.RowBounds.Location.X + 15, e.RowBounds.Location.Y + 5);
        }

        private void dgv_observed_order_ca_RowPostPaint(object sender, DataGridViewRowPostPaintEventArgs e)
        {
            SolidBrush solidBrush = new SolidBrush(dgv_observed_order_ca.RowHeadersDefaultCellStyle.ForeColor);
            e.Graphics.DrawString((e.RowIndex + 1).ToString(), e.InheritedRowStyle.Font, solidBrush, e.RowBounds.Location.X + 15, e.RowBounds.Location.Y + 5);
        }

        private void dgv_fake_unshipped_order_ca_RowPostPaint(object sender, DataGridViewRowPostPaintEventArgs e)
        {
            SolidBrush solidBrush = new SolidBrush(dgv_fake_unshipped_order_ca.RowHeadersDefaultCellStyle.ForeColor);
            e.Graphics.DrawString((e.RowIndex + 1).ToString(), e.InheritedRowStyle.Font, solidBrush, e.RowBounds.Location.X + 15, e.RowBounds.Location.Y + 5);
        }

        private void dgv_ship_failed_order_ca_RowPostPaint(object sender, DataGridViewRowPostPaintEventArgs e)
        {
            SolidBrush solidBrush = new SolidBrush(dgv_ship_failed_order_ca.RowHeadersDefaultCellStyle.ForeColor);
            e.Graphics.DrawString((e.RowIndex + 1).ToString(), e.InheritedRowStyle.Font, solidBrush, e.RowBounds.Location.X + 15, e.RowBounds.Location.Y + 5);
        }

        private void dgv_fba_order_ca_RowPostPaint(object sender, DataGridViewRowPostPaintEventArgs e)
        {
            SolidBrush solidBrush = new SolidBrush(dgv_fba_order_ca.RowHeadersDefaultCellStyle.ForeColor);
            e.Graphics.DrawString((e.RowIndex + 1).ToString(), e.InheritedRowStyle.Font, solidBrush, e.RowBounds.Location.X + 15, e.RowBounds.Location.Y + 5);
        }

        private void button40_Click(object sender, EventArgs e)
        {
            //confirm_order();
        }

        private void confirm_order(string market_id)
        {
            // 从数据库获取待确认订单的数量
            DataSet ds = MySqlHelper.GetDataSet(MySqlHelper.Conn, System.Data.CommandType.Text,
                String.Format("select order_id, track_id, order_type  from t_order_basic where order_status=0 and is_confirmed=0 and order_type in (0, 1) and market_id='{0}'", market_id), null);

            // 弹出确认对话框
            //string msgBoxText = String.Format("请确定此次待确认订单数：{0}个？", ds.Tables[0].Rows.Count);
            //DialogResult dlgResult = MessageBox.Show(msgBoxText, "确认", MessageBoxButtons.OKCancel);
            //if (dlgResult == DialogResult.OK)
            {
                // 在亚马逊确认发货
                //DateTime now = DateTime.Now.AddHours(-15);  // 转成亚马逊的当地时间
                DateTime now = DateTime.Now.AddHours(-int.Parse(GlobalConfig.Instance.GetConfigValue(market_id, "timeDifference")));  // 转成亚马逊的当地时间
                //string fileName = String.Format("confirmOrderFeed/confirmOrderFeed_{0}.txt", now.ToString(GlobalConfig.Instance.TimeFormat));
                string fileName = String.Format("confirmOrderFeed/{0}_confirmOrderFeed_{1}.txt", market_id, now.ToString(GlobalConfig.Instance.GetCommonConfigValue("timeFormat")));

                //*** 生成feed的txt文件
                //*** order-id	order-item-id	quantity	ship-date	carrier-code	carrier-name	tracking-number	ship-method
                //string ship_date = now.ToString(GlobalConfig.Instance.ShipDateFormat);
                //string rowFormat = "{0}\t\t\t"+ ship_date +"\t"+ GlobalConfig.Instance.ShipCarrierCode + "\t"+ GlobalConfig.Instance.ShipCarrierName + "\t{1}\t";
                string ship_date = now.ToString(GlobalConfig.Instance.GetCommonConfigValue("shipDateFormat"));
                string rowFormat = "{0}\t\t\t" + ship_date + "\t" + GlobalConfig.Instance.GetConfigValue(market_id, "shipCarrierCode") + "\t" + GlobalConfig.Instance.GetConfigValue(market_id, "shipCarrierName") + "\t{1}\t";
                List<string> orderList = new List<string>();

                // 标题行
                orderList.Add(String.Format("{0}\t{1}\t{2}\t{3}\t{4}\t{5}\t{6}\t{7}", "order-id", "order-item-id", "quantity", "ship-date", "carrier-code", "carrier-name", "tracking-number", "ship-method"));

                foreach (DataRow dr in ds.Tables[0].Rows)
                {
                    // 不写入track id
                    orderList.Add(String.Format(rowFormat, dr["order_id"], ""));
                }
                System.IO.File.WriteAllLines(fileName, orderList);

                //*** 调用submit feed方法
                //MarketplaceWebServiceSamples.SubmitFeed(fileName, GlobalConfig.Instance.ConfirmOrderFeedType);
                MarketplaceWebServiceSamples.SubmitFeed(market_id, fileName, GlobalConfig.Instance.GetCommonConfigValue("confirmOrderFeedType"));

                // 写入数据库
                string cmdText = string.Format("update t_order_basic set is_confirmed=1,confirmed_date='{0}' where order_status=0 and is_confirmed=0 and order_type=0 and market_id='{1}'", ship_date, market_id);
                int affectedRowsCount = MySqlHelper.ExecuteNonQuery(MySqlHelper.Conn, System.Data.CommandType.Text, cmdText, null);
                Console.WriteLine("Affected Rows: " + affectedRowsCount);

                // 刷新datagridview
                //dgv_unshipped_order_BoundDS();
            }
        }

        private void button41_Click(object sender, EventArgs e)
        {
            //confirm_order();
        }

        private void button1_Click_2(object sender, EventArgs e)
        {
            show_order_detail(marketId);
        }

        private void dgv_listing_offer_RowPostPaint(object sender, DataGridViewRowPostPaintEventArgs e)
        {
            SolidBrush solidBrush = new SolidBrush(dgv_listing_offer.RowHeadersDefaultCellStyle.ForeColor);
            e.Graphics.DrawString((e.RowIndex + 1).ToString(), e.InheritedRowStyle.Font, solidBrush, e.RowBounds.Location.X + 15, e.RowBounds.Location.Y + 5);
        }

        private void button37_Click(object sender, EventArgs e)
        {
            create_bound_variation();
        }

        private void button39_Click(object sender, EventArgs e)
        {
            get_variation_result();
        }

        private void button35_Click(object sender, EventArgs e)
        {
            if (tb_variation_parent_filename_ca.Text.ToString() == "")
            {
                MessageBox.Show("文件名不能为空", "确认", MessageBoxButtons.OK);
            }
            else
            {
                System.Diagnostics.Process.Start(System.Environment.CurrentDirectory + "\\" + tb_variation_parent_filename_ca.Text.ToString());
            }
        }

        private void button36_Click(object sender, EventArgs e)
        {
            create_variation_parent();
        }

        private void button33_Click(object sender, EventArgs e)
        {
            if (tb_variation_child_filename_ca.Text.ToString() == "")
            {
                MessageBox.Show("文件名不能为空", "确认", MessageBoxButtons.OK);
            }
            else
            {
                System.Diagnostics.Process.Start(System.Environment.CurrentDirectory + "\\" + tb_variation_child_filename_ca.Text.ToString());
            }
        }

        private void button34_Click(object sender, EventArgs e)
        {
            append_size_color_to_child();
        }

        private void button31_Click(object sender, EventArgs e)
        {
            if (tb_variation_bound_filename_ca.Text.ToString() == "")
            {
                MessageBox.Show("文件名不能为空", "确认", MessageBoxButtons.OK);
            }
            else
            {
                System.Diagnostics.Process.Start(System.Environment.CurrentDirectory + "\\" + tb_variation_bound_filename_ca.Text.ToString());
            }
        }

        private void button32_Click(object sender, EventArgs e)
        {
            bound_child_with_parent();
        }

        private void button6_Click_1(object sender, EventArgs e)
        {
            // 生成皇家物流的订单文件
            fillShipFile_PFC();
            MessageBox.Show("导出成功！", "提示", MessageBoxButtons.OK);
        }

        private void button40_Click_1(object sender, EventArgs e)
        {
            DataSet ds = MySqlHelper.GetDataSet(MySqlHelper.Conn, System.Data.CommandType.Text,
                    String.Format("select b.order_id, count(*) as sub_order_count from t_order_basic as a, t_order_product as b where a.order_id=b.order_id and a.order_status=0 and a.order_type=0 and a.market_id='{0}' group by b.order_id having count(*)>1;", marketId), null);
           

            DateTime now = DateTime.Now;
            string fileName = String.Format("packOrderDetail\\{0}_mutilOrderDetail_{1}.txt", marketId, now.ToString(GlobalConfig.Instance.GetCommonConfigValue("timeFormat")));

            //*** 生成txt文件
            List<string> multiOrderCountList = new List<string>();

            multiOrderCountList.Add(String.Format("{0}\t\t\t{1, 10}", "订单号", "包含的子订单个数"));

            foreach (DataRow dr in ds.Tables[0].Rows)
            {
                multiOrderCountList.Add(String.Format("{0}\t{1, 10}", dr["order_id"], dr["sub_order_count"]));
            }

            System.IO.File.WriteAllLines(fileName, multiOrderCountList);

            // 打开文件
            System.Diagnostics.Process.Start("notepad.exe", System.Environment.CurrentDirectory + "\\" + fileName);
        }
    }
}
