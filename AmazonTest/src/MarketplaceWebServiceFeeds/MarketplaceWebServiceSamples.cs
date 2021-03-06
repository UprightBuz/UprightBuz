/******************************************************************************* 
 *  Copyright 2009 Amazon Services.
 *  Licensed under the Apache License, Version 2.0 (the "License"); 
 *  
 *  You may not use this file except in compliance with the License. 
 *  You may obtain a copy of the License at: http://aws.amazon.com/apache2.0
 *  This file is distributed on an "AS IS" BASIS, WITHOUT WARRANTIES OR 
 *  CONDITIONS OF ANY KIND, either express or implied. See the License for the 
 *  specific language governing permissions and limitations under the License.
 * ***************************************************************************** 
 * 
 *  Marketplace Web Service CSharp Library
 *  API Version: 2009-01-01
 *  Generated: Mon Mar 16 17:31:42 PDT 2009 
 * 
 */

using System;
using System.Xml.Serialization;
using System.Collections.Generic;
using MarketplaceWebService;
using MarketplaceWebService.Mock;
using MarketplaceWebService.Model;
using System.IO;
using AmazonTest.src;

namespace MarketplaceWebService.Samples
{

    /// <summary>
    /// Marketplace Web Service  Samples
    /// </summary>
    public class MarketplaceWebServiceSamples 
    {
    
       /**
        * Samples for Marketplace Web Service functionality
        */

        public static void GetReport(string market_id, string reportType, string fileName, DateTime startDate, DateTime endDate)
        {
            /************************************************************************
            * Instantiate  Implementation of Marketplace Web Service 
            ***********************************************************************/

            MarketplaceWebServiceConfig config = new MarketplaceWebServiceConfig();

            /************************************************************************
             * The application name and version are included in each MWS call's
             * HTTP User-Agent field. These are required fields.
             ***********************************************************************/



            /************************************************************************
             * All MWS requests must contain the seller's merchant ID and 
             * marketplace ID.
             ***********************************************************************/

            //config.ServiceURL = GlobalConfig.Instance.ServiceURL;
            config.ServiceURL = GlobalConfig.Instance.GetConfigValue(market_id, "serviceURL");

            //config.SetUserAgentHeader(GlobalConfig.Instance.AppName, GlobalConfig.Instance.AppVersion, "C#");
            //MarketplaceWebService service = new MarketplaceWebServiceClient(GlobalConfig.Instance.AccessKey, GlobalConfig.Instance.SecretKey, config);
            config.SetUserAgentHeader(GlobalConfig.Instance.GetCommonConfigValue("appName"), GlobalConfig.Instance.GetCommonConfigValue("appVersion"), "C#");
            MarketplaceWebService service = new MarketplaceWebServiceClient(GlobalConfig.Instance.GetConfigValue(market_id, "accessKey"), GlobalConfig.Instance.GetConfigValue(market_id, "secretKey"), config);

            //*** 1. Submit a report request using the RequestReport operation. This is a request to Amazon MWS to generate a specific report.            
            RequestReportRequest reportRequest = new RequestReportRequest();
            //reportRequest.Merchant = GlobalConfig.Instance.SellerId;
            reportRequest.Merchant = GlobalConfig.Instance.GetConfigValue(market_id, "sellerId");
            // request.MWSAuthToken = "<Your MWS Auth Token>"; // Optional
            reportRequest.MarketplaceIdList = new IdList();
            //reportRequest.MarketplaceIdList.Id = new List<string>( new string [] { GlobalConfig.Instance.MarketplaceId } );
            reportRequest.MarketplaceIdList.Id = new List<string>(new string[] { GlobalConfig.Instance.GetConfigValue(market_id, "marketplaceId") });

            reportRequest.ReportType = reportType;

            if (startDate != null)
            {
                reportRequest.StartDate = startDate;
                reportRequest.EndDate = endDate;
            }
            
            // @TODO: set additional request parameters here
            // request.ReportOptions = "ShowSalesChannel=true"; 
            string reportRequestId = RequestReportSample.InvokeRequestReport(service, reportRequest);

            Dictionary<string, string> requestInfo = new Dictionary<string, string>();
            requestInfo["ReportProcessingStatus"] = "";
            requestInfo["GeneratedReportId"] = "";

            /************************************************************************
             * Uncomment to invoke Get Report Request List Action
             ***********************************************************************/
            {
                GetReportRequestListRequest request = new GetReportRequestListRequest();
                request.Merchant = reportRequest.Merchant;
                //request.MWSAuthToken = "<Your MWS Auth Token>"; // Optional
                // @TODO: set additional request parameters here
                request.ReportRequestIdList = new IdList();
                request.ReportRequestIdList.Id = new List<string>(new string[] { reportRequestId });
                while(requestInfo["ReportProcessingStatus"] != "_DONE_" && requestInfo["GeneratedReportId"] == "")
                {
                    if(!GetReportRequestListSample.InvokeGetReportRequestList(service, request, requestInfo))
                    {
                        // todo �쳣����
                        requestInfo["ReportProcessingStatus"] = "_DONE_";
                        requestInfo["GeneratedReportId"] = "";
                        break;
                    }
                    //*** Request every 60s 
                    System.Threading.Thread.Sleep(1 * 60 * 1000);
                }
            }

            //*** 2. Using the GetReportList operation and include the ReportRequestId for the report requested. 
            //*** The operation returns a ReportId that you can then pass to the GetReport operation 50193016685
            {
                if (requestInfo["ReportProcessingStatus"] == "_DONE_" && requestInfo["GeneratedReportId"] == "")
                {
                    
                    GetReportListRequest getReportListRequest = new GetReportListRequest();
                    getReportListRequest.Merchant = reportRequest.Merchant;
                    //request.MWSAuthToken = "<Your MWS Auth Token>"; // Optional
                    getReportListRequest.ReportRequestIdList = new IdList();
                    getReportListRequest.ReportRequestIdList.Id = new List<string>(new string[] { reportRequestId });
                    string reportId = "";
                    while (reportId == "")
                    {
                        //*** Request every 60s 
                        System.Threading.Thread.Sleep(1 * 60 * 1000);
                        reportId = GetReportListSample.InvokeGetReportList(service, getReportListRequest);
                    }
                    requestInfo["reportId"] = reportId;
                }
            }
            

            //*** 3. Submit a request using the GetReport operation to receive a specific report. 
            //*** You include in the request the GeneratedReportId or the ReportId for the report you want to receive. 
            //*** You then process the Content-MD5 header to confirm that the report was not corrupted during transmission.
            GetReportRequest getReportRequest = new GetReportRequest();
            getReportRequest.Merchant = reportRequest.Merchant;
            // request.MWSAuthToken = "<Your MWS Auth Token>"; // Optional

            // Note that depending on the type of report being downloaded, a report can reach 
            // sizes greater than 1GB. For this reason we recommend that you _always_ program to
            // MWS in a streaming fashion. Otherwise, as your business grows you may silently reach
            // the in-memory size limit and have to re-work your solution.
            // NOTE: Due to Content-MD5 validation, the stream must be read/write.
            if (requestInfo["GeneratedReportId"] != "" || requestInfo["reportId"] != "")
            {
                getReportRequest.ReportId = requestInfo["GeneratedReportId"] != "" ? requestInfo["GeneratedReportId"] : requestInfo["reportId"];
                getReportRequest.Report = File.Open(fileName, FileMode.OpenOrCreate, FileAccess.ReadWrite);
                GetReportSample.InvokeGetReport(service, getReportRequest);
                getReportRequest.Report.Close();
            }
        }

        public static string SubmitFeed(string market_id, string fileName, string feedType)
        {
            //IniReader iniReader = new IniReader(System.Environment.CurrentDirectory + "\\config.ini");
            //string awsSection = "AWS_US";

            //String accessKeyId = iniReader.ReadValue(awsSection, "accessKey");
            //String secretAccessKey = iniReader.ReadValue(awsSection, "secretKey");

            /************************************************************************
            * Instantiate  Implementation of Marketplace Web Service 
            ***********************************************************************/

            MarketplaceWebServiceConfig config = new MarketplaceWebServiceConfig();

            /************************************************************************
             * The application name and version are included in each MWS call's
             * HTTP User-Agent field. These are required fields.
             ***********************************************************************/

            //string applicationName = iniReader.ReadValue(awsSection, "appName"); ;
            //string applicationVersion = iniReader.ReadValue(awsSection, "appVersion");

            /************************************************************************
             * All MWS requests must contain the seller's merchant ID and 
             * marketplace ID.
             ***********************************************************************/
            //string merchantId = iniReader.ReadValue(awsSection, "sellerId");
            //string marketplaceId = iniReader.ReadValue(awsSection, "marketplaceId");

            // United States:
            //config.ServiceURL = GlobalConfig.Instance.ServiceURL;

            //config.SetUserAgentHeader(GlobalConfig.Instance.AppName, GlobalConfig.Instance.AppVersion, "C#");
            config.ServiceURL = GlobalConfig.Instance.GetConfigValue(market_id, "serviceURL");

            config.SetUserAgentHeader(GlobalConfig.Instance.GetCommonConfigValue("appName"), GlobalConfig.Instance.GetCommonConfigValue("appVersion"), "C#");

            //MarketplaceWebService service = new MarketplaceWebServiceClient(GlobalConfig.Instance.AccessKey, GlobalConfig.Instance.SecretKey, config);
            MarketplaceWebService service = new MarketplaceWebServiceClient(GlobalConfig.Instance.GetConfigValue(market_id, "accessKey"), GlobalConfig.Instance.GetConfigValue(market_id, "secretKey"), config);

            SubmitFeedRequest request = new SubmitFeedRequest();
            request.Merchant = GlobalConfig.Instance.GetConfigValue(market_id, "sellerId");
            // request.MWSAuthToken = iniReader.ReadValue(awsSection, "MWSAuthToken"); // Optional
            request.MarketplaceIdList = new IdList();
            request.MarketplaceIdList.Id = new List<string>(new string[] { GlobalConfig.Instance.GetConfigValue(market_id, "marketplaceId") });

            // MWS exclusively offers a streaming interface for uploading your feeds. This is because 
            // feed sizes can grow to the 1GB+ range - and as your business grows you could otherwise 
            // silently reach the feed size where your in-memory solution will no longer work, leaving you 
            // puzzled as to why a solution that worked for a long time suddenly stopped working though 
            // you made no changes. For the same reason, we strongly encourage you to generate your feeds to 
            // local disk then upload them directly from disk to MWS.

            request.FeedContent = File.Open(fileName, FileMode.Open, FileAccess.Read);

            // Calculating the MD5 hash value exhausts the stream, and therefore we must either reset the
            // position, or create another stream for the calculation.
            request.ContentMD5 = MarketplaceWebServiceClient.CalculateContentMD5(request.FeedContent);
            request.FeedContent.Position = 0;

            request.FeedType = feedType;

            string feedSubmissionId = SubmitFeedSample.InvokeSubmitFeed(service, request);
            request.FeedContent.Close();
            return feedSubmissionId;
        }


        public static void GetFeedSubmissionResult(string market_id, string feedSubmissionId, string fileName)
        {
            //IniReader iniReader = new IniReader(System.Environment.CurrentDirectory + "\\config.ini");
            //string awsSection = "AWS_US";

            String accessKeyId = GlobalConfig.Instance.GetConfigValue(market_id, "accessKey");
            String secretAccessKey = GlobalConfig.Instance.GetConfigValue(market_id, "secretKey");

            /************************************************************************
            * Instantiate  Implementation of Marketplace Web Service 
            ***********************************************************************/

            MarketplaceWebServiceConfig config = new MarketplaceWebServiceConfig();

            /************************************************************************
             * The application name and version are included in each MWS call's
             * HTTP User-Agent field. These are required fields.
             ***********************************************************************/

            string applicationName = GlobalConfig.Instance.GetCommonConfigValue("appName");
            string applicationVersion = GlobalConfig.Instance.GetCommonConfigValue("appVersion");

            /************************************************************************
             * All MWS requests must contain the seller's merchant ID and 
             * marketplace ID.
             ***********************************************************************/
            string merchantId = GlobalConfig.Instance.GetConfigValue(market_id, "sellerId");
            string marketplaceId = GlobalConfig.Instance.GetConfigValue(market_id, "marketplaceId");

            // United States:
            config.ServiceURL = GlobalConfig.Instance.GetConfigValue(market_id, "serviceURL");
            //
            // United Kingdom:
            // config.ServiceURL = "https://mws.amazonservices.co.uk";
            //
            // Germany:
            // config.ServiceURL = "https://mws.amazonservices.de";
            //
            // France:
            // config.ServiceURL = "https://mws.amazonservices.fr";
            //
            // Japan:
            // config.ServiceURL = "https://mws.amazonservices.jp";
            //
            // China:
            // config.ServiceURL = "https://mws.amazonservices.com.cn";
            //
            // Canada:
            // config.ServiceURL = "https://mws.amazonservices.ca";
            //
            // Italy:
            // config.ServiceURL = "https://mws.amazonservices.it";
            //
            config.SetUserAgentHeader(
                applicationName,
                applicationVersion,
                "C#"
                );
            MarketplaceWebService service = new MarketplaceWebServiceClient(accessKeyId, secretAccessKey, config);

            /************************************************************************
             * Uncomment to invoke Get Feed Submission Result Action
             ***********************************************************************/
            {
                GetFeedSubmissionResultRequest request = new GetFeedSubmissionResultRequest();
                request.Merchant = merchantId;
                //request.MWSAuthToken = "<Your MWS Auth Token>"; // Optional

                // Note that depending on the size of the feed sent in, and the number of errors and warnings,
                // the result can reach sizes greater than 1GB. For this reason we recommend that you _always_ 
                // program to MWS in a streaming fashion. Otherwise, as your business grows you may silently reach
                // the in-memory size limit and have to re-work your solution.
                // NOTE: Due to Content-MD5 validation, the stream must be read/write.
                request.FeedSubmissionId = feedSubmissionId;
                request.FeedSubmissionResult = File.Open(fileName, FileMode.OpenOrCreate, FileAccess.ReadWrite);

                GetFeedSubmissionResultSample.InvokeGetFeedSubmissionResult(service, request);
                request.FeedSubmissionResult.Close();
            }
        }

        public static void RunSample()
        {
            Console.WriteLine("===========================================");
            Console.WriteLine("Welcome to Marketplace Web Service Samples!");
            Console.WriteLine("===========================================");

            Console.WriteLine("To get started:");
            Console.WriteLine("===========================================");
            Console.WriteLine("  - Fill in your AWS credentials");
            Console.WriteLine("  - Uncomment sample you're interested in trying");
            Console.WriteLine("  - Set request with desired parameters");
            Console.WriteLine("  - Hit F5 to run!");
            Console.WriteLine();

            Console.WriteLine("===========================================");
            Console.WriteLine("Samples Output");
            Console.WriteLine("===========================================");
            Console.WriteLine();

            /************************************************************************
             * Access Key ID and Secret Acess Key ID, obtained from:
             * http://aws.amazon.com
             * 
             * IMPORTANT: Your Secret Access Key is a secret, and should be known 
             * only by you and AWS. You should never include your Secret Access Key 
             * in your requests to AWS. You should never e-mail your Secret Access Key 
             * to anyone. It is important to keep your Secret Access Key confidential 
             * to protect your account.
             ***********************************************************************/

            IniReader iniReader = new IniReader(System.Environment.CurrentDirectory + "\\config.ini");
            string awsSection = "AWS_US";

            String accessKeyId = iniReader.ReadValue(awsSection, "accessKey");
            String secretAccessKey = iniReader.ReadValue(awsSection, "secretKey");

            /************************************************************************
            * Instantiate  Implementation of Marketplace Web Service 
            ***********************************************************************/

            MarketplaceWebServiceConfig config = new MarketplaceWebServiceConfig();

            /************************************************************************
             * The application name and version are included in each MWS call's
             * HTTP User-Agent field. These are required fields.
             ***********************************************************************/

            string applicationName = iniReader.ReadValue(awsSection, "appName"); ;
            string applicationVersion = iniReader.ReadValue(awsSection, "appVersion"); ;

            //MarketplaceWebService service =
            //    new MarketplaceWebServiceClient(
            //        accessKeyId,
            //        secretAccessKey,
            //        applicationName,
            //        applicationVersion,
            //        config);


            /************************************************************************
             * All MWS requests must contain the seller's merchant ID and 
             * marketplace ID.
             ***********************************************************************/
            string merchantId = iniReader.ReadValue(awsSection, "sellerId");
            string marketplaceId = iniReader.ReadValue(awsSection, "marketplaceId");
                       
            /************************************************************************
             * Uncomment to configure the client instance. Configuration settings
             * include:
             *
             *  - MWS Service endpoint URL
             *  - Proxy Host and Proxy Port
             *  - User Agent String to be sent to Marketplace Web Service  service
             *
             ***********************************************************************/
            //MarketplaceWebServiceConfig config = new MarketplaceWebServiceConfig();
            //config.ProxyHost = "https://PROXY_URL";
            //config.ProxyPort = 9090;
            //
            // IMPORTANT: Uncomment out the appropiate line for the country you wish 
            // to sell in:
            // 
            // United States:
            config.ServiceURL = "https://mws.amazonservices.com";
            //
            // United Kingdom:
            // config.ServiceURL = "https://mws.amazonservices.co.uk";
            //
            // Germany:
            // config.ServiceURL = "https://mws.amazonservices.de";
            //
            // France:
            // config.ServiceURL = "https://mws.amazonservices.fr";
            //
            // Japan:
            // config.ServiceURL = "https://mws.amazonservices.jp";
            //
            // China:
            // config.ServiceURL = "https://mws.amazonservices.com.cn";
            //
            // Canada:
            // config.ServiceURL = "https://mws.amazonservices.ca";
            //
            // Italy:
            // config.ServiceURL = "https://mws.amazonservices.it";
            //
            config.SetUserAgentHeader(
                applicationName,
                applicationVersion,
                "C#"
                );
            MarketplaceWebService service = new MarketplaceWebServiceClient(accessKeyId, secretAccessKey, config);


            /************************************************************************
             * Uncomment to try out Mock Service that simulates Marketplace Web Service 
             * responses without calling Marketplace Web Service  service.
             *
             * Responses are loaded from local XML files. You can tweak XML files to
             * experiment with various outputs during development
             *
             * XML files available under MarketplaceWebService\Mock tree
             *
             ***********************************************************************/
            // MarketplaceWebService service = new MarketplaceWebServiceMock();


            /************************************************************************
             * Uncomment to invoke Get Report Action
             ***********************************************************************/
            {
                // GetReportRequest request = new GetReportRequest();
                // request.Merchant = merchantId;
                // request.MWSAuthToken = "<Your MWS Auth Token>"; // Optional

                // Note that depending on the type of report being downloaded, a report can reach 
                // sizes greater than 1GB. For this reason we recommend that you _always_ program to
                // MWS in a streaming fashion. Otherwise, as your business grows you may silently reach
                // the in-memory size limit and have to re-work your solution.
                // NOTE: Due to Content-MD5 validation, the stream must be read/write.
                // request.ReportId = "REPORT_ID";
                // request.Report = File.Open("report.xml", FileMode.OpenOrCreate, FileAccess.ReadWrite );
                // GetReportSample.InvokeGetReport(service, request);
            }

            /************************************************************************
             * Uncomment to invoke Get Report Schedule Count Action
             ***********************************************************************/
            {
                //GetReportScheduleCountRequest request = new GetReportScheduleCountRequest();
                //request.Merchant = merchantId;
                //request.MWSAuthToken = "<Your MWS Auth Token>"; // Optional
                // @TODO: set additional request parameters here
                //GetReportScheduleCountSample.InvokeGetReportScheduleCount(service, request);
            }


            /************************************************************************
             * Uncomment to invoke Get Report Request List By Next Token Action
             ***********************************************************************/
            {
                //GetReportRequestListByNextTokenRequest request = new GetReportRequestListByNextTokenRequest();
                //request.Merchant = merchantId;
                //request.MWSAuthToken = "<Your MWS Auth Token>"; // Optional
                //request.NextToken = "NextToken from GetReportRequestList";
                // @TODO: set additional request parameters here
                //GetReportRequestListByNextTokenSample.InvokeGetReportRequestListByNextToken(service, request);
            }

            /************************************************************************
             * Uncomment to invoke Update Report Acknowledgements Action
             ***********************************************************************/
            {
                //UpdateReportAcknowledgementsRequest request = new UpdateReportAcknowledgementsRequest();
                //request.Merchant = merchantId;
                //request.MWSAuthToken = "<Your MWS Auth Token>"; // Optional
                //request.WithReportIdList(new IdList().WithId("REPORT_ID"));
                // @TODO: set additional request parameters here
                //UpdateReportAcknowledgementsSample.InvokeUpdateReportAcknowledgements(service, request);
            }


            /************************************************************************
             * Uncomment to invoke Submit Feed Action
             ***********************************************************************/
            {
                //SubmitFeedRequest request = new SubmitFeedRequest();
                //request.Merchant = merchantId;
                // request.MWSAuthToken = iniReader.ReadValue(awsSection, "MWSAuthToken"); // Optional
                //request.MarketplaceIdList = new IdList();
                //request.MarketplaceIdList.Id = new List<string>(new string[] { marketplaceId });

                // MWS exclusively offers a streaming interface for uploading your feeds. This is because 
                // feed sizes can grow to the 1GB+ range - and as your business grows you could otherwise 
                // silently reach the feed size where your in-memory solution will no longer work, leaving you 
                // puzzled as to why a solution that worked for a long time suddenly stopped working though 
                // you made no changes. For the same reason, we strongly encourage you to generate your feeds to 
                // local disk then upload them directly from disk to MWS.

                //request.FeedContent = File.Open("feed.xml", FileMode.Open, FileAccess.Read);

                // Calculating the MD5 hash value exhausts the stream, and therefore we must either reset the
                // position, or create another stream for the calculation.
                //request.ContentMD5 = MarketplaceWebServiceClient.CalculateContentMD5(request.FeedContent);
                //request.FeedContent.Position = 0;

                //request.FeedType = "_POST_PRODUCT_PRICING_DATA_";

                //SubmitFeedSample.InvokeSubmitFeed(service, request);
            }


            /************************************************************************
             * Uncomment to invoke Get Report Count Action
             ***********************************************************************/
            {
                // GetReportCountRequest request = new GetReportCountRequest();
                // request.Merchant = merchantId;
                // request.MWSAuthToken = "<Your MWS Auth Token>"; // Optional
                // @TODO: set additional request parameters here
                // GetReportCountSample.InvokeGetReportCount(service, request);
            }

            /************************************************************************
             * Uncomment to invoke Get Feed Submission List By Next Token Action
             ***********************************************************************/
            //{
            //    GetFeedSubmissionListByNextTokenRequest request = new GetFeedSubmissionListByNextTokenRequest();
            //    request.Merchant = merchantId;
            //    // request.MWSAuthToken = "<Your MWS Auth Token>"; // Optional
            //    request.NextToken = "NextToken from GetFeedSubmissionList";
            //    // @TODO: set additional request parameters here
            //    GetFeedSubmissionListByNextTokenSample.InvokeGetFeedSubmissionListByNextToken(service, request);
            //}

            /************************************************************************
             * Uncomment to invoke Cancel Feed Submissions Action
             ***********************************************************************/
            {
                //CancelFeedSubmissionsRequest request = new CancelFeedSubmissionsRequest();
                //request.Merchant = merchantId;
                //request.MWSAuthToken = "<Your MWS Auth Token>"; // Optional
                // @TODO: set additional request parameters here
                //CancelFeedSubmissionsSample.InvokeCancelFeedSubmissions(service, request);
            }

            /************************************************************************
             * Uncomment to invoke Request Report Action
             ***********************************************************************/
            {
                //RequestReportRequest request = new RequestReportRequest();
                //request.Merchant = merchantId;
                //request.MWSAuthToken = "<Your MWS Auth Token>"; // Optional
                //request.MarketplaceIdList = new IdList();
                //request.MarketplaceIdList.Id = new List<string>( new string [] { marketplaceId } );

                //request.ReportType = "Desired Report Type";
                // @TODO: set additional request parameters here
                //request.ReportOptions = "ShowSalesChannel=true"; 
                //RequestReportSample.InvokeRequestReport(service, request);
            }

            /************************************************************************
             * Uncomment to invoke Get Feed Submission Count Action
             ***********************************************************************/
            {
                //GetFeedSubmissionCountRequest request = new GetFeedSubmissionCountRequest();
                //request.Merchant = merchantId;
                //request.MWSAuthToken = "<Your MWS Auth Token>"; // Optional
                // @TODO: set additional request parameters here
                //GetFeedSubmissionCountSample.InvokeGetFeedSubmissionCount(service, request);
            }

            /************************************************************************
             * Uncomment to invoke Cancel Report Requests Action
             ***********************************************************************/
            {
                //CancelReportRequestsRequest request = new CancelReportRequestsRequest();
                //request.Merchant = merchantId;
                //request.MWSAuthToken = "<Your MWS Auth Token>"; // Optional
                // @TODO: set additional request parameters here
                //CancelReportRequestsSample.InvokeCancelReportRequests(service, request);
            }

            /************************************************************************
             * Uncomment to invoke Get Report List Action
             ***********************************************************************/
            {
                //GetReportListRequest request = new GetReportListRequest();
                //request.Merchant = merchantId;
                //request.MWSAuthToken = "<Your MWS Auth Token>"; // Optional
                // @TODO: set additional request parameters here
                //GetReportListSample.InvokeGetReportList(service, request);
            }

            /************************************************************************
             * Uncomment to invoke Get Feed Submission Result Action
             ***********************************************************************/
            {
                GetFeedSubmissionResultRequest request = new GetFeedSubmissionResultRequest();
                request.Merchant = merchantId;
                //request.MWSAuthToken = "<Your MWS Auth Token>"; // Optional

                // Note that depending on the size of the feed sent in, and the number of errors and warnings,
                // the result can reach sizes greater than 1GB. For this reason we recommend that you _always_ 
                // program to MWS in a streaming fashion. Otherwise, as your business grows you may silently reach
                // the in-memory size limit and have to re-work your solution.
                // NOTE: Due to Content-MD5 validation, the stream must be read/write.aprtwin
                request.FeedSubmissionId = "57612016782";
                request.FeedSubmissionResult = File.Open("feedSubmissionResult12.xml", FileMode.OpenOrCreate, FileAccess.ReadWrite);

                GetFeedSubmissionResultSample.InvokeGetFeedSubmissionResult(service, request);
                request.FeedSubmissionResult.Close();
            }

            /************************************************************************
             * Uncomment to invoke Get Feed Submission List Action
             ***********************************************************************/
            {
                //GetFeedSubmissionListRequest request = new GetFeedSubmissionListRequest();
                //request.Merchant = merchantId;
                ////request.MWSAuthToken = "<Your MWS Auth Token>"; // Optional
                //// @TODO: set additional request parameters here
                //GetFeedSubmissionListSample.InvokeGetFeedSubmissionList(service, request);
            }

            /************************************************************************
             * Uncomment to invoke Get Report Request List Action
             ***********************************************************************/
            {
                //GetReportRequestListRequest request = new GetReportRequestListRequest();
                //request.Merchant = merchantId;
                //request.MWSAuthToken = "<Your MWS Auth Token>"; // Optional
                // @TODO: set additional request parameters here
                //GetReportRequestListSample.InvokeGetReportRequestList(service, request);
            }

            /************************************************************************
             * Uncomment to invoke Get Report Schedule List By Next Token Action
             ***********************************************************************/
            {
                //GetReportScheduleListByNextTokenRequest request = new GetReportScheduleListByNextTokenRequest();
                //request.Merchant = merchantId;
                //request.MWSAuthToken = "<Your MWS Auth Token>"; // Optional
                //request.NextToken = "NextToken from GetReportScheduleList";
                // @TODO: set additional request parameters here
                //GetReportScheduleListByNextTokenSample.InvokeGetReportScheduleListByNextToken(service, request);
            }

            /************************************************************************
             * Uncomment to invoke Get Report List By Next Token Action
             ***********************************************************************/
            {
                // GetReportListByNextTokenRequest request = new GetReportListByNextTokenRequest();
                // request.Merchant = merchantId;
                // request.MWSAuthToken = "<Your MWS Auth Token>"; // Optional
                //request.NextToken = "NextToken from GetReportList";
                // @TODO: set additional request parameters here
                // GetReportListByNextTokenSample.InvokeGetReportListByNextToken(service, request);
            }

            /************************************************************************
             * Uncomment to invoke Manage Report Schedule Action
             ***********************************************************************/
            {
                //ManageReportScheduleRequest request = new ManageReportScheduleRequest();
                //request.Merchant = merchantId;
                //request.MWSAuthToken = "<Your MWS Auth Token>"; // Optional
                //request.ReportType = "Report Type";
                //request.Schedule = "Schedule";
                // @TODO: set additional request parameters here
                //ManageReportScheduleSample.InvokeManageReportSchedule(service, request);
            }

            /************************************************************************
             * Uncomment to invoke Get Report Request Count Action
             ***********************************************************************/
            {
                //GetReportRequestCountRequest request = new GetReportRequestCountRequest();
                //request.Merchant = merchantId;
                //request.MWSAuthToken = "<Your MWS Auth Token>"; // Optional
                // @TODO: set additional request parameters here
                //GetReportRequestCountSample.InvokeGetReportRequestCount(service, request);
            }

            /************************************************************************
             * Uncomment to invoke Get Report Schedule List Action
             ***********************************************************************/
            {
                //GetReportScheduleListRequest request = new GetReportScheduleListRequest();
                //request.Merchant = merchantId;
                //request.MWSAuthToken = "<Your MWS Auth Token>"; // Optional
                // @TODO: set additional request parameters here
                //GetReportScheduleListSample.InvokeGetReportScheduleList(service, request);
            }


            Console.WriteLine();
            Console.WriteLine("===========================================");
            Console.WriteLine("End of output. You can close this window");
            Console.WriteLine("===========================================");

            // System.Threading.Thread.Sleep(50000);
        }

    }
}
