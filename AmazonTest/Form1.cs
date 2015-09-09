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

namespace AmazonTest
{
    public partial class Form1 : Form
    {
        AWSLogger awsLogger;
        System.Timers.Timer timerAdaptPrice;       // 自动调价计时器
        System.Timers.Timer timerUpdateInventory;  // 更新库存定时器

        public Form1()
        {
            InitializeComponent();
            // MarketplaceWebServiceOrdersSample.RunSample();   
            // MarketplaceWebServiceProductsSample.RunSample();
            // MarketplaceWebServiceSamples.RunSample();
            // AdaptPrice.RunAdaptPrice();
            // MarketplaceWebServiceSamples.GetInventoryReport();
            // updateInventory();

            // Console.WriteLine(GlobalConfig.Instance.TimeFormat);
            
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            timerAdaptPrice = new System.Timers.Timer();
            timerAdaptPrice.Interval = 15 * 60 * 1000;  // 间隔时间为15min
            timerAdaptPrice.Elapsed += new System.Timers.ElapsedEventHandler(adaptPrice);

            timerUpdateInventory = new System.Timers.Timer();
            timerUpdateInventory.Interval = 2 * 60 * 60 * 1000;  // 间隔时间为2h
            timerUpdateInventory.Elapsed += new System.Timers.ElapsedEventHandler(updateInventory);
        }

        private void button1_Click(object sender, EventArgs e)
        {
            timerAdaptPrice.Enabled = true;
            timerUpdateInventory.Enabled = true;
        }

        private void button2_Click(object sender, EventArgs e)
        {
            timerAdaptPrice.Enabled = false;
            timerUpdateInventory.Enabled = false;
        }

        private void adaptPrice(object sender, System.Timers.ElapsedEventArgs e)
        {
            //*** 执行调价操作
            AdaptPrice.RunAdaptPrice();
        }

        private void updateInventory(object sender, System.Timers.ElapsedEventArgs e)
        //private void updateInventory()
        {
            //*** 获取库存报告文件
            string filename = MarketplaceWebServiceSamples.GetInventoryReport();
            //string filename = "inventoryReport/report_20150907_233758.txt";
            StreamReader f = File.OpenText(filename);
            string line = f.ReadLine();
            // 校验第一行
            if (line != null)
            {
                string[] words = line.Split(new char[] { '\t', ' ' });
                string[] expectWords = { "sku", "asin", "price", "quantity" };
                if (string.Join(",", words)==string.Join(",", expectWords))
                {
                    line = f.ReadLine();
                    List<string> cmdTextList = new List<string>();
                    while (line != null)
                    {
                        words = line.Split(new char[] { '\t', ' ' });
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
    }
}
