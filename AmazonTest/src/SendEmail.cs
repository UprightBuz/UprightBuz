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
        private System.Net.Mail.MailMessage mailMsg;
        private SmtpClient smtpClient;

        private SendEmail()
        {
            mailMsg = new System.Net.Mail.MailMessage();
            mailMsg.From = new MailAddress("liujun@uprightsz.com", "UprightBuz", System.Text.Encoding.UTF8);             
            mailMsg.SubjectEncoding = System.Text.Encoding.UTF8;//邮件标题编码
            mailMsg.BodyEncoding = System.Text.Encoding.UTF8;//邮件内容编码    
            mailMsg.IsBodyHtml = true;//是否是HTML邮件    
            mailMsg.Priority = MailPriority.High;//邮件优先级    

            smtpClient = new SmtpClient();
            // smtpClient.Host = "smtp.163.com";
            smtpClient.Host = "smtp.mxhichina.com";
            smtpClient.Port = 25;
            smtpClient.UseDefaultCredentials = true;
            smtpClient.DeliveryMethod = System.Net.Mail.SmtpDeliveryMethod.Network;
            smtpClient.Credentials = new System.Net.NetworkCredential("liujun@uprightsz.com", "Upright123");
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
            mailMsg.To.Clear();
            mailMsg.To.Add(receiver);
            mailMsg.Subject = subject;//邮件标题  
            mailMsg.Body = body;//邮件内容
            object userState = mailMsg;

            try

            {
                smtpClient.Send(mailMsg);
                return true;
            }
            catch (System.Net.Mail.SmtpException ex)
            {
                Console.WriteLine(ex.Message);
                return false;
            }
        }
    }
}
