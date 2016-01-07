using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using System.Xml;
using MarketplaceWebServiceProducts;
using MarketplaceWebService.Samples;
using System.IO;

namespace AmazonTest.src
{
    class AdaptPrice
    {
        /// <summary>
        /// ****todo：
        /// 拉取用户SKU列表
        /// 获取最低价格/购物车价格
        /// 获取当前销售价格
        /// 调价逻辑
        /// 发起Feed操作
        /// </summary>
        /// 

        // 监控listing是否有跟卖
        public static void MonitorListing()
        {
            string market_id = GlobalConfig.Instance.MarketID_US;
            DataSet ds = MySqlHelper.GetDataSet(MySqlHelper.Conn, System.Data.CommandType.Text, 
                string.Format("select sku, sum(quantity) as sales_count from t_order_product where sku not like '%_G' and market_id='{0}' group by sku order by sales_count desc;", market_id), null);

            Dictionary<string, int> skuDict = new Dictionary<string, int>();
            foreach (DataRow dr in ds.Tables[0].Rows)
            {
                skuDict.Add(dr["sku"].ToString(), 0);
            }

            //*** 获取sku的相关价格：我的价格，最低价格
            MarketplaceWebServiceProductsSample.GetNumberOfOfferListings(market_id, skuDict);

            // 回写数据库
            List<string> cmdTextList = new List<string>();
            foreach (string sku in skuDict.Keys)
            {
                cmdTextList.Add(String.Format("('{0}', '{1}', {2})", sku, market_id, skuDict[sku]));
            }
            string cmdText = String.Format("insert t_product_offer_number (sku, market_id, current_offer_number) values {0} ON DUPLICATE KEY UPDATE current_offer_number=VALUES(current_offer_number)", String.Join(",", cmdTextList));
            int affectedRowsCount = MySqlHelper.ExecuteNonQuery(MySqlHelper.Conn, System.Data.CommandType.Text, cmdText, null);
            Console.WriteLine("Affected Rows: " + affectedRowsCount);
        }

        public static void RunAdaptPrice()
        {
            string market_id = GlobalConfig.Instance.MarketID_US;
            ///***** get sku list&& Accept price
            /// 
            DataSet ds = MySqlHelper.GetDataSet(MySqlHelper.Conn, System.Data.CommandType.Text, "select * from t_product where quantity>0 and is_adapt_price=1", null);


            Dictionary<string,Dictionary<string,float>> skuDict = new Dictionary<string,Dictionary<string,float>>();
            foreach (DataRow dr in ds.Tables[0].Rows)
            {
                Dictionary<string, float> skuValue = new Dictionary<string, float>();
                skuValue.Add("minimum_price", float.Parse(dr["minimum_price"].ToString()));

                skuDict.Add(dr["sku"].ToString(),skuValue);
            }

            //*** 获取sku的相关价格：我的价格，最低价格
            MarketplaceWebServiceProductsSample.GetSkuPrice(market_id, skuDict);

            //*** 获取需调价sku
            Dictionary<string, float> needAdaptSkuDict = new Dictionary<string, float>();
            foreach (string sku in skuDict.Keys)
            {
                if (skuDict[sku].ContainsKey("my_price") && skuDict[sku].ContainsKey("lowest_price"))
                {
                    if (skuDict[sku]["my_price"] <= skuDict[sku]["minimum_price"])  //*** 我的价格不高于我的可接受价格
                    {
                        needAdaptSkuDict.Add(sku, skuDict[sku]["minimum_price"]);
                    }
                    else //*** 我的价格高于可接受价格
                    {
                        if (skuDict[sku]["my_price"] >= skuDict[sku]["lowest_price"])           //*** 我的价格高于最低价或不是唯一的最低价，下调      
                        {
                            if (skuDict[sku]["lowest_price"] > skuDict[sku]["minimum_price"])  //*** 最低价大于我的可接受价格
                            {
                                needAdaptSkuDict.Add(sku, skuDict[sku]["lowest_price"] - float.Parse(GlobalConfig.Instance.GetCommonConfigValue("adaptRange")));  // 最新价格为当前最低价格 - 调价幅度
                            }
                            else
                            {
                                needAdaptSkuDict.Add(sku, skuDict[sku]["minimum_price"]);
                            }
                        }
                        else
                        {  // 我的价格低于最低价，上调
                            needAdaptSkuDict.Add(sku, skuDict[sku]["lowest_price"] - float.Parse(GlobalConfig.Instance.GetCommonConfigValue("adaptRange")));  // 最新价格为当前最低价格 - 调价幅度
                        }
                    }
                }
            }

            if (needAdaptSkuDict.Count > 0)
            {
                //*** 生成feed的xml文件
                DateTime now = DateTime.Now;
                string fileName = String.Format("priceFeed/priceFeed_{0}.xml", now.ToString(GlobalConfig.Instance.GetCommonConfigValue("timeFormat")));
                File.Copy("priceFeed/priceFeedTemplate.xml", fileName);
                XmlDocument xmlDocument = new XmlDocument();
                xmlDocument.Load(fileName);
                var root = xmlDocument.DocumentElement;

                string priceFormat = "<MessageID>{0}</MessageID><Price><SKU>{1}</SKU><StandardPrice currency=\"USD\">{2:N2}</StandardPrice><Sale><StartDate>{3}</StartDate><EndDate>{4}</EndDate><SalePrice currency=\"USD\">{5:N2}</SalePrice></Sale></Price>";
                int index = 1;
                foreach (string sku in needAdaptSkuDict.Keys)
                {
                    XmlElement newElement = xmlDocument.CreateElement("Message");
                    newElement.InnerXml = String.Format(priceFormat, index, sku, needAdaptSkuDict[sku]*10, GlobalConfig.Instance.GetCommonConfigValue("saleStartDate"), GlobalConfig.Instance.GetCommonConfigValue("saleEndDate"), needAdaptSkuDict[sku]);
                    root.AppendChild(newElement);
                    ++index;                    
                }
                xmlDocument.Save(fileName);

                
                //*** 调用submit feed方法
                MarketplaceWebServiceSamples.SubmitFeed(market_id, fileName, GlobalConfig.Instance.GetCommonConfigValue("adaptPriceFeedType"));
            }


        }

    }
}
