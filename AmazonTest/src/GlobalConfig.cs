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

        private GlobalConfig() {           
            IniReader iniReader = new IniReader(System.Environment.CurrentDirectory + "\\config.ini");
            string awsSection = "AWS_US";
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
        }

        public string AccessKey{ get{return accessKey;} }
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

        public static GlobalConfig Instance
        {
            get
            {
                return instance;
            }
        }
    }
}
