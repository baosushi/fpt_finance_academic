using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Linq;
using System.Web;
using System.Reflection;
using DataService.Model;
using System.Net.Mail;
using System.Net;
using System.Threading.Tasks;

namespace CaptstoneProject.Models
{

    public static class DataContextExtensions
    {
        public static DB_Finance_AcademicEntities BulkInsert<T>(this DB_Finance_AcademicEntities context, T entity, int count, int batchSize) where T : class
        {
            context.Set<T>().Add(entity);

            if (count % batchSize == 0)
            {
                context.SaveChanges();
                context.Dispose();
                context = new DB_Finance_AcademicEntities();

                // This is optional, allow to tracking
                //context.Configuration.AutoDetectChangesEnabled = false;
            }
            return context;
        }
    }

    public static class Utils
    {
        public static DateTime ToDateTime(this string datetime)
        {
            try
            {
                return DateTime.ParseExact(datetime, "dd/MM/yyyy", CultureInfo.InvariantCulture);
            }
            catch
            {
                throw new Exception("Chuỗi ngày tháng không đúng định dạng");
            }
        }

        public static DateTime GetEndOfDate(this DateTime value)
        {
            return new DateTime(value.Year, value.Month, value.Day, 23, 59, 59);
        }

        public static DateTime GetStartOfDate(this DateTime value)
        {
            return new DateTime(value.Year, value.Month, value.Day, 0, 0, 0);
        }

        public static string GetEnumDisplayName(this Enum enumValue)
        {
            return enumValue.GetType()
                            .GetMember(enumValue.ToString())
                            .First()
                            .GetCustomAttribute<DisplayAttribute>().GetName();
        }

        //
        // Summary:
        //     Sends the specified message to an SMTP server for delivery.
        //
        // Parameters:
        //   sender:
        //     A System.String that represents the address of the sender of the e-mail message.
        //
        //   recipient:
        //     A System.String that represents the address of the recipient of the e-mail message.
        //
        //   password:
        //     A System.String that contains the sender email password.
        //
        //   content:
        //     A System.String that contains the message body.
        public static async Task SendEmail(string sender, string recipient, string password, string content)
        {
            using (SmtpClient client = new SmtpClient("smtp.gmail.com", 587))
            {
                // Configure the client
                client.EnableSsl = true;
                client.UseDefaultCredentials = false;
                client.Credentials = new NetworkCredential(sender, password);
                System.Net.ServicePointManager.ServerCertificateValidationCallback = delegate (object s,
                        System.Security.Cryptography.X509Certificates.X509Certificate certificate,
                        System.Security.Cryptography.X509Certificates.X509Chain chain,
                        System.Net.Security.SslPolicyErrors sslPolicyErrors)
                {
                    return true;
                };
                // A client has been created, now you need to create a MailMessage object
                MailMessage message = new MailMessage(
                                         sender, // From field
                                         recipient, // Recipient field
                                         "Course Registration Payment Confirm", // Subject of the email message
                                         content // Email message body
                                      );
                message.IsBodyHtml = true;
                message.BodyEncoding = System.Text.Encoding.UTF8;

                // Send the message
                client.SendCompleted += (s, e) =>
                {
                    SmtpClient callbackClient = s as SmtpClient;
                    callbackClient.Dispose();
                };

                await client.SendMailAsync(message);
            }
        }
    }
}