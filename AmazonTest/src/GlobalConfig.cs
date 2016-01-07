using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace AmazonTest.src
{
    public sealed class GlobalConfig
    {
        private static readonly GlobalConfig instance = new GlobalConfig();
        private string accessKey;
        private string secretKey;
        private string appName;
        private string appVersion;
        private string serviceURL;
        private string sellerId;
        private string marketplaceId;
        private int skuPriceCount;
        private int skuPriceWaitTime;
        private float adaptRange;
        private string saleStartDate;
        private string saleEndDate;
        private string timeFormat;
        private string inventoryReportType;
        private string unshippedOrderReportType;
        private string historyOrderReportType;
        private string eubBuzType;
        private string adaptPriceFeedType;
        private string confirmOrderFeedType;
        private string shipCarrierCode;
        private string shipCarrierName;
        private string shipDateFormat;
        private string unshippedFBAOrderReportType;
        private string updateProductFeedType;
        private string createRelationshipFeedType;
        private IniReader iniReader;

        private GlobalConfig() {           
            iniReader = new IniReader(System.Environment.CurrentDirectory + "\\config.ini");
            /*string awsSection = "AWS_US";
            accessKey = iniReader.ReadValue(awsSection, "accessKey");
            secretKey = iniReader.ReadValue(awsSection, "secretKey");
            appName = iniReader.ReadValue(awsSection, "appName");
            appVersion = iniReader.ReadValue(awsSection, "appVersion");
            serviceURL = iniReader.ReadValue(awsSection, "serviceURL");
            sellerId = iniReader.ReadValue(awsSection, "sellerId");
            marketplaceId = iniReader.ReadValue(awsSection, "marketplaceId");
            skuPriceCount = int.Parse(iniReader.ReadValue(awsSection, "skuPriceCount"));
            skuPriceWaitTime = int.Parse(iniReader.ReadValue(awsSection, "skuPriceWaitTime"));
            adaptRange = float.Parse(iniReader.ReadValue(awsSection, "adaptRange"));
            saleStartDate = iniReader.ReadValue(awsSection, "saleStartDate");
            saleEndDate = iniReader.ReadValue(awsSection, "saleEndDate");
            timeFormat = iniReader.ReadValue(awsSection, "timeFormat");
            inventoryReportType = iniReader.ReadValue(awsSection, "inventoryReportType");
            unshippedOrderReportType = iniReader.ReadValue(awsSection, "unshippedOrderReportType");
            historyOrderReportType = iniReader.ReadValue(awsSection, "historyOrderReportType");
            eubBuzType = iniReader.ReadValue(awsSection, "eubBuzType");
            adaptPriceFeedType = iniReader.ReadValue(awsSection, "adaptPriceFeedType");
            confirmOrderFeedType = iniReader.ReadValue(awsSection, "confirmOrderFeedType");
            shipCarrierCode = iniReader.ReadValue(awsSection, "shipCarrierCode");
            shipCarrierName = iniReader.ReadValue(awsSection, "shipCarrierName");
            shipDateFormat = iniReader.ReadValue(awsSection, "shipDateFormat");
            unshippedFBAOrderReportType = iniReader.ReadValue(awsSection, "unshippedFBAOrderReportType");
            updateProductFeedType = iniReader.ReadValue(awsSection, "updateProductFeedType");
            createRelationshipFeedType = iniReader.ReadValue(awsSection, "createRelationshipFeedType");*/
        }

        /*public string AccessKey{ get{return accessKey;} }
        public string SecretKey { get { return secretKey; } }
        public string AppVersion { get { return appVersion; } }
        public string AppName { get { return appName; } }
        public string ServiceURL { get { return serviceURL; } }
        public string SellerId { get { return sellerId; } }
        public string MarketplaceId { get { return marketplaceId; } }
        public int SkuPriceCount { get { return skuPriceCount; } }
        public int SkuPriceWaitTime { get { return skuPriceWaitTime; } }
        public float AdaptRange { get { return adaptRange; } }
        public string SaleStartDate { get { return saleStartDate; } }
        public string SaleEndDate { get { return saleEndDate; } }
        public string TimeFormat { get { return timeFormat; } }
        public string InventoryReportType { get { return inventoryReportType; } }
        public string UnshippedOrderReportType { get { return unshippedOrderReportType; } }
        public string HistoryOrderReportType { get { return historyOrderReportType; } }
        public string EubBuzType { get { return eubBuzType; } }
        public string AdaptPriceFeedType { get { return adaptPriceFeedType; } }
        public string ConfirmOrderFeedType { get { return confirmOrderFeedType; } }
        public string ShipCarrierCode { get { return shipCarrierCode; } }
        public string ShipCarrierName { get { return shipCarrierName; } }
        public string ShipDateFormat { get { return shipDateFormat; } }
        public string UnshippedFBAOrderReportType { get { return unshippedFBAOrderReportType; } }
        public string UpdateProductFeedType { get { return updateProductFeedType; } }
        public string CreateRelationshipFeedType { get { return createRelationshipFeedType; } }
        */
        public string MarketID_US { get { return "US"; } }   // 美
        public string MarketID_CA { get { return "CA"; } }   // 加
        public string MarketID_UK { get { return "UK"; } }   // 英
        public string MarketID_DE { get { return "DE"; } }   // 德
        public string MarketID_FR { get { return "FR"; } }   // 法
        public string MarketID_ES { get { return "ES"; } }   // 西
        public string MarketID_IT { get { return "IT"; } }   // 意

        // 获取各个站点的配置项
        public string GetConfigValue(string section, string key)
        {
            return iniReader.ReadValue(section, key);
        }

        // 获取通用配置项
        public string GetCommonConfigValue(string key)
        {
            return GetConfigValue("COMMON", key);
        }

        public static GlobalConfig Instance
        {
            get
            {
                return instance;
            }
        }
    }
}
