using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Mail;

namespace AmazonTest
{
    public sealed class SendEmail
    {
        private static readonly SendEmail instance = new SendEmail();
        private List<System.Net.Mail.MailMessage> mailMsgList;
        private List<SmtpClient> smtpClientList;

        private SendEmail()
        {
            mailMsgList = new List<MailMessage>();
            smtpClientList = new List<SmtpClient>();

            // 新浪邮箱
            {
                MailMessage mailMsg = new MailMessage();
                mailMsg.From = new MailAddress("upright_buz@163.com", "UprightBuz", System.Text.Encoding.UTF8);
                mailMsg.SubjectEncoding = System.Text.Encoding.UTF8;//邮件标题编码
                mailMsg.BodyEncoding = System.Text.Encoding.UTF8;//邮件内容编码    
                mailMsg.IsBodyHtml = true;//是否是HTML邮件    
                mailMsg.Priority = MailPriority.High;//邮件优先级


                SmtpClient smtpClient = new SmtpClient();
                smtpClient.Host = "smtp.163.com";
                smtpClient.Port = 25;
                smtpClient.UseDefaultCredentials = true;
                smtpClient.DeliveryMethod = System.Net.Mail.SmtpDeliveryMethod.Network;
                smtpClient.Credentials = new System.Net.NetworkCredential("upright_buz", "asmvuzranpwtihud");

                mailMsgList.Add(mailMsg);
                smtpClientList.Add(smtpClient);
            }

            // 阿里邮箱
            {
                MailMessage mailMsg = new MailMessage();
                mailMsg.From = new MailAddress("liujun@uprightsz.com", "UprightBuz", System.Text.Encoding.UTF8);
                mailMsg.SubjectEncoding = System.Text.Encoding.UTF8;//邮件标题编码
                mailMsg.BodyEncoding = System.Text.Encoding.UTF8;//邮件内容编码    
                mailMsg.IsBodyHtml = true;//是否是HTML邮件    
                mailMsg.Priority = MailPriority.High;//邮件优先级


                SmtpClient smtpClient = new SmtpClient();
                smtpClient.Host = "smtp.mxhichina.com";
                smtpClient.Port = 25;
                smtpClient.UseDefaultCredentials = true;
                smtpClient.DeliveryMethod = System.Net.Mail.SmtpDeliveryMethod.Network;
                smtpClient.Credentials = new System.Net.NetworkCredential("liujun@uprightsz.com", "Upright123");

                mailMsgList.Add(mailMsg);
                smtpClientList.Add(smtpClient);
            }

        }

        public static SendEmail Instance
        {
            get
            {
                return instance;
            }
        }

        public bool sendEmail(string receiver, string subject, string body)
        {
            try
            {
                // 先用新浪邮箱发送
                sendEmail(receiver, subject, body, mailMsgList[0], smtpClientList[0]);
            }
            catch (System.Net.Mail.SmtpException ex)
            {
                Console.WriteLine(ex.Message);
                // 用阿的邮箱发送
                try
                {
                    sendEmail(receiver, subject, body, mailMsgList[1], smtpClientList[1]);
                }
                catch (SmtpException ex1)
                {
                    Console.WriteLine(ex1.Message);
                }
            }
            return true;
        }

        public void sendEmail(string receiver, string subject, string body, MailMessage mailMsg, SmtpClient smtpClient)
        {
            mailMsg.To.Clear();
            mailMsg.To.Add(receiver);
            mailMsg.Subject = subject;//邮件标题  
            mailMsg.Body = body;//邮件内容
            object userState = mailMsg;
            smtpClient.Send(mailMsg);
        }
    }
}
