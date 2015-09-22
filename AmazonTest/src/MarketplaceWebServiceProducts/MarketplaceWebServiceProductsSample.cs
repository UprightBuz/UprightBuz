/*******************************************************************************
 * Copyright 2009-2015 Amazon Services. All Rights Reserved.
 * Licensed under the Apache License, Version 2.0 (the "License"); 
 *
 * You may not use this file except in compliance with the License. 
 * You may obtain a copy of the License at: http://aws.amazon.com/apache2.0
 * This file is distributed on an "AS IS" BASIS, WITHOUT WARRANTIES OR 
 * CONDITIONS OF ANY KIND, either express or implied. See the License for the 
 * specific language governing permissions and limitations under the License.
 *******************************************************************************
 * Marketplace Web Service Products
 * API Version: 2011-10-01
 * Library Version: 2015-02-13
 * Generated: Tue Feb 10 14:34:55 PST 2015
 */

using MarketplaceWebServiceProducts.Model;
using System;
using System.Collections.Generic;
using AmazonTest.src;
using System.IO;
using System.Text;
using System.Data;
using System.Xml;
//using MarketplaceWebService;
//using MarketplaceWebService.Model;
//using MarketplaceWebService.Samples;

namespace MarketplaceWebServiceProducts {

    /// <summary>
    /// Runnable sample code to demonstrate usage of the C# client.
    ///
    /// To use, import the client source as a console application,
    /// and mark this class as the startup object. Then, replace
    /// parameters below with sensible values and run.
    /// </summary>
    public class MarketplaceWebServiceProductsSample {

        public static void GetSkuPrice(Dictionary<string, Dictionary<string, float>> skuDict)
        { 
            // Create a configuration object
            MarketplaceWebServiceProductsConfig config = new MarketplaceWebServiceProductsConfig();
            config.ServiceURL = GlobalConfig.Instance.ServiceURL;
            // Set other client connection configurations here if needed
            // Create the client itself
            MarketplaceWebServiceProducts client = new MarketplaceWebServiceProductsClient(GlobalConfig.Instance.AppName, GlobalConfig.Instance.AppVersion, GlobalConfig.Instance.AccessKey, GlobalConfig.Instance.SecretKey, config);

            MarketplaceWebServiceProductsSample sample = new MarketplaceWebServiceProductsSample(client, GlobalConfig.Instance.SellerId, "", GlobalConfig.Instance.MarketplaceId);
                

            // Uncomment the operation you'd like to test here
            // TODO: Modify the request created in the Invoke method to be valid

            try 
            {
                //*** todo: 这里需要对list做切片，每组20个
                List<string> skuList = new List<string>(skuDict.Keys);
                IMWSResponse response = null;
                int count = GlobalConfig.Instance.SkuPriceCount;
                int current_index;
                for (current_index = 0; current_index < skuList.Count; current_index += count){
                    int current_count = (current_index + count)  > skuList.Count ? skuList.Count - current_index : count;
                    List<string> skuRangeList = skuList.GetRange(current_index, current_count);
                    //{
                    //    // 获取购物车价格
                    //    response = sample.InvokeGetCompetitivePricingForSKU(skuRangeList);
                    //    XmlDocument xmlDocument = new XmlDocument();
                    //    xmlDocument.LoadXml(response.ToXML());
                    //    XmlNamespaceManager nsmgr = new XmlNamespaceManager(xmlDocument.NameTable);
                    //    nsmgr.AddNamespace("mws", xmlDocument.GetElementsByTagName("GetCompetitivePricingForSKUResponse")[0].Attributes["xmlns"].Value);
                    //    XmlNodeList xnList = xmlDocument.SelectNodes("//mws:GetCompetitivePricingForSKUResult", nsmgr);
                    //    foreach (XmlNode xn in xnList)
                    //    {
                    //        string competitive_price = xn.SelectSingleNode(".//mws:LandedPrice/mws:Amount", nsmgr).InnerText;
                    //        skuDict[xn.Attributes["SellerSKU"].Value].Add("competitive_price", float.Parse(competitive_price));

                    //    }
                    //}

                    // 获取当前最低价
                    {
                        response = sample.InvokeGetLowestOfferListingsForSKU(skuRangeList);
                        XmlDocument xmlDocument = new XmlDocument();
                        xmlDocument.LoadXml(response.ToXML());
                        XmlNamespaceManager nsmgr = new XmlNamespaceManager(xmlDocument.NameTable);
                        nsmgr.AddNamespace("mws", xmlDocument.GetElementsByTagName("GetLowestOfferListingsForSKUResponse")[0].Attributes["xmlns"].Value);
                        XmlNodeList xnList = xmlDocument.SelectNodes("//mws:GetLowestOfferListingsForSKUResult", nsmgr);
                        foreach (XmlNode xn in xnList)
                        {
                            XmlNode target = xn.SelectSingleNode(".//mws:LandedPrice/mws:Amount", nsmgr);
                            if (target != null)
                            {
                                string lowest_price = target.InnerText;

                                skuDict[xn.Attributes["SellerSKU"].Value].Add("lowest_price", float.Parse(lowest_price));
                            }
                        }
                    }

                    // 获取我的价格
                    {
                        response = sample.InvokeGetMyPriceForSKU(skuRangeList);
                        XmlDocument xmlDocument = new XmlDocument();
                        xmlDocument.LoadXml(response.ToXML());
                        XmlNamespaceManager nsmgr = new XmlNamespaceManager(xmlDocument.NameTable);
                        nsmgr.AddNamespace("mws", xmlDocument.GetElementsByTagName("GetMyPriceForSKUResponse")[0].Attributes["xmlns"].Value);
                        XmlNodeList xnList = xmlDocument.SelectNodes("//mws:GetMyPriceForSKUResult", nsmgr);
                        foreach (XmlNode xn in xnList)
                        {
                            XmlNode target = xn.SelectSingleNode(".//mws:LandedPrice/mws:Amount", nsmgr);
                            if (target != null)
                            {
                                string my_price = target.InnerText;
                                skuDict[xn.Attributes["SellerSKU"].Value].Add("my_price", float.Parse(my_price));
                            }
                        }
                    }

                    // Restore rate: 10 items every second
                    System.Threading.Thread.Sleep(GlobalConfig.Instance.SkuPriceWaitTime);
                }
 

                ////取根结点         
                ////*** todo: 获取购物车的sellerId，判断自己是否抢到购物车
                ////XmlNodeList xnList = xmlDocument.GetElementsByTagName("GetCompetitivePricingForSKUResult");
                
                    

                //// response = sample.InvokeGetCompetitivePricingForASIN();
                //// response = sample.InvokeGetCompetitivePricingForSKU();
                //// response = sample.InvokeGetLowestOfferListingsForASIN();
                //// response = sample.InvokeGetLowestOfferListingsForSKU();
                //// response = sample.InvokeGetMatchingProduct();
                //// response = sample.InvokeGetMatchingProductForId();
                //// response = sample.InvokeGetMyPriceForASIN();
                //// response = sample.InvokeGetMyPriceForSKU();
                //// response = sample.InvokeGetProductCategoriesForASIN();
                //// response = sample.InvokeGetProductCategoriesForSKU();
                //// response = sample.InvokeGetServiceStatus();
                //// response = sample.InvokeListMatchingProducts();
                //Console.WriteLine("Response:");
                //ResponseHeaderMetadata rhmd = response.ResponseHeaderMetadata;
                //// We recommend logging the request id and timestamp of every call.
                //Console.WriteLine("RequestId: " + rhmd.RequestId);
                //Console.WriteLine("Timestamp: " + rhmd.Timestamp);
                //string responseXml = response.ToXML();
                //Console.WriteLine(responseXml);
            }
            catch (MarketplaceWebServiceProductsException ex)
            {
                // Exception properties are important for diagnostics.
                ResponseHeaderMetadata rhmd = ex.ResponseHeaderMetadata;
                Console.WriteLine("Service Exception:");
                if(rhmd != null)
                {
                    Console.WriteLine("RequestId: " + rhmd.RequestId);
                    Console.WriteLine("Timestamp: " + rhmd.Timestamp);
                }
                Console.WriteLine("Message: " + ex.Message);
                Console.WriteLine("StatusCode: " + ex.StatusCode);
                Console.WriteLine("ErrorCode: " + ex.ErrorCode);
                Console.WriteLine("ErrorType: " + ex.ErrorType);
                throw ex;
            }
        }

        private readonly MarketplaceWebServiceProducts client;
        private readonly string sellerId;
        private readonly string mwsAuthToken;
        private readonly string marketplaceId;

        public MarketplaceWebServiceProductsSample(MarketplaceWebServiceProducts client, string sellerId, string mwsAuthToken, string marketplaceId)
        {
            this.client = client;
            this.sellerId = sellerId;
            this.mwsAuthToken = mwsAuthToken;
            this.marketplaceId = marketplaceId;
        }

        public GetCompetitivePricingForASINResponse InvokeGetCompetitivePricingForASIN(List<string> asinList)
        {
            // Create a request.
            GetCompetitivePricingForASINRequest request = new GetCompetitivePricingForASINRequest();
            string sellerId = "example";
            request.SellerId = sellerId;
            string mwsAuthToken = "example";
            request.MWSAuthToken = mwsAuthToken;
            string marketplaceId = "example";
            request.MarketplaceId = marketplaceId;
            ASINListType ASINList = new ASINListType();
            ASINList.ASIN = asinList;
            request.ASINList = ASINList;
            return this.client.GetCompetitivePricingForASIN(request);
        }

        public GetCompetitivePricingForSKUResponse InvokeGetCompetitivePricingForSKU(List<string> skuList)
        {
            // Create a request.
            GetCompetitivePricingForSKURequest request = new GetCompetitivePricingForSKURequest();
            request.SellerId = this.sellerId;
            request.MWSAuthToken = this.mwsAuthToken;
            request.MarketplaceId = this.marketplaceId;
            SellerSKUListType sellerSKUList = new SellerSKUListType();
            sellerSKUList.SellerSKU = skuList;
            request.SellerSKUList = sellerSKUList;
            return this.client.GetCompetitivePricingForSKU(request);
        }

        public GetLowestOfferListingsForASINResponse InvokeGetLowestOfferListingsForASIN()
        {
            // Create a request.
            GetLowestOfferListingsForASINRequest request = new GetLowestOfferListingsForASINRequest();
            string sellerId = "example";
            request.SellerId = sellerId;
            string mwsAuthToken = "example";
            request.MWSAuthToken = mwsAuthToken;
            string marketplaceId = "example";
            request.MarketplaceId = marketplaceId;
            ASINListType asinList = new ASINListType();
            request.ASINList = asinList;
            string itemCondition = "example";
            request.ItemCondition = itemCondition;
            bool excludeMe = true;
            request.ExcludeMe = excludeMe;
            return this.client.GetLowestOfferListingsForASIN(request);
        }

        public GetLowestOfferListingsForSKUResponse InvokeGetLowestOfferListingsForSKU(List<string> skuList)
        {
            
            // Create a request.
            GetLowestOfferListingsForSKURequest request = new GetLowestOfferListingsForSKURequest();
            request.SellerId = this.sellerId;
            request.MWSAuthToken = this.mwsAuthToken;
            request.MarketplaceId = this.marketplaceId;
            SellerSKUListType sellerSKUList = new SellerSKUListType();
            sellerSKUList.SellerSKU = skuList;
            request.SellerSKUList = sellerSKUList;
            request.ExcludeMe = true;
            return this.client.GetLowestOfferListingsForSKU(request);
        }

        public GetMatchingProductResponse InvokeGetMatchingProduct()
        {
            // Create a request.
            GetMatchingProductRequest request = new GetMatchingProductRequest();
            string sellerId = "example";
            request.SellerId = sellerId;
            string mwsAuthToken = "example";
            request.MWSAuthToken = mwsAuthToken;
            string marketplaceId = "example";
            request.MarketplaceId = marketplaceId;
            ASINListType asinList = new ASINListType();
            request.ASINList = asinList;
            return this.client.GetMatchingProduct(request);
        }

        public GetMatchingProductForIdResponse InvokeGetMatchingProductForId()
        {
            // Create a request.
            GetMatchingProductForIdRequest request = new GetMatchingProductForIdRequest();
            string sellerId = "example";
            request.SellerId = sellerId;
            string mwsAuthToken = "example";
            request.MWSAuthToken = mwsAuthToken;
            string marketplaceId = "example";
            request.MarketplaceId = marketplaceId;
            string idType = "example";
            request.IdType = idType;
            IdListType idList = new IdListType();
            request.IdList = idList;
            return this.client.GetMatchingProductForId(request);
        }

        public GetMyPriceForASINResponse InvokeGetMyPriceForASIN()
        {
            // Create a request.
            GetMyPriceForASINRequest request = new GetMyPriceForASINRequest();
            string sellerId = "example";
            request.SellerId = sellerId;
            string mwsAuthToken = "example";
            request.MWSAuthToken = mwsAuthToken;
            string marketplaceId = "example";
            request.MarketplaceId = marketplaceId;
            ASINListType asinList = new ASINListType();
            request.ASINList = asinList;
            return this.client.GetMyPriceForASIN(request);
        }

        public GetMyPriceForSKUResponse InvokeGetMyPriceForSKU(List<string> skuList)
        {
            // Create a request.
            GetMyPriceForSKURequest request = new GetMyPriceForSKURequest();
            request.SellerId = this.sellerId;
            request.MWSAuthToken = this.mwsAuthToken;
            request.MarketplaceId = this.marketplaceId;
            SellerSKUListType sellerSKUList = new SellerSKUListType();
            sellerSKUList.SellerSKU = skuList;
            request.SellerSKUList = sellerSKUList;
            return this.client.GetMyPriceForSKU(request);
        }

        public GetProductCategoriesForASINResponse InvokeGetProductCategoriesForASIN()
        {
            // Create a request.
            GetProductCategoriesForASINRequest request = new GetProductCategoriesForASINRequest();
            string sellerId = "example";
            request.SellerId = sellerId;
            string mwsAuthToken = "example";
            request.MWSAuthToken = mwsAuthToken;
            string marketplaceId = "example";
            request.MarketplaceId = marketplaceId;
            string asin = "example";
            request.ASIN = asin;
            return this.client.GetProductCategoriesForASIN(request);
        }

        public GetProductCategoriesForSKUResponse InvokeGetProductCategoriesForSKU()
        {
            // Create a request.
            GetProductCategoriesForSKURequest request = new GetProductCategoriesForSKURequest();
            string sellerId = "example";
            request.SellerId = sellerId;
            string mwsAuthToken = "example";
            request.MWSAuthToken = mwsAuthToken;
            string marketplaceId = "example";
            request.MarketplaceId = marketplaceId;
            string sellerSKU = "example";
            request.SellerSKU = sellerSKU;
            return this.client.GetProductCategoriesForSKU(request);
        }

        public GetServiceStatusResponse InvokeGetServiceStatus()
        {
            // Create a request.
            GetServiceStatusRequest request = new GetServiceStatusRequest();
            string sellerId = "example";
            request.SellerId = sellerId;
            string mwsAuthToken = "example";
            request.MWSAuthToken = mwsAuthToken;
            return this.client.GetServiceStatus(request);
        }

        public ListMatchingProductsResponse InvokeListMatchingProducts()
        {
            // Create a request.
            ListMatchingProductsRequest request = new ListMatchingProductsRequest();
            string sellerId = "example";
            request.SellerId = sellerId;
            string mwsAuthToken = "example";
            request.MWSAuthToken = mwsAuthToken;
            string marketplaceId = "example";
            request.MarketplaceId = marketplaceId;
            string query = "example";
            request.Query = query;
            string queryContextId = "example";
            request.QueryContextId = queryContextId;
            return this.client.ListMatchingProducts(request);
        }


    }
}
