using System.Data;
using System.Net;
using System.Net.Mail;
using Flyurdreamcommands.Constants;
using Flyurdreamcommands.Models.Databasemodel;
using Flyurdreamcommands.Models.Datafields;
using Flyurdreamcommands.Repositories.Abstract;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Flyurdreamcommands.Repositories.Concrete
{
    public class EmailService : DataRepositoryBase, IEmailService
    {
        private readonly IConfiguration _config;
        private readonly ILogger _logger;
        public EmailService(IConfiguration config, ILogger<EmailService> logger) : base(logger, config)
        {
            _config = config;
            _logger = logger;
        }
        public void EmailNotification(User user, Email mailArgs, string[] attachmentPaths = null)
        {
            try
            {
               
                // Read the content of the email template file

                string rootDirectory = AppDomain.CurrentDomain.BaseDirectory;
                string userVerificationTemplatePath = Path.Combine(rootDirectory, "EmailTemplate", "useregistration.html");

                // Now you can use these paths to read the contents of the template files or perform other operations

                string emailTemplate = File.ReadAllText(userVerificationTemplatePath);

                // Replace placeholders in the template with actual values
                emailTemplate = emailTemplate.Replace("{{FirstName}}", user.FirstName)
                                             .Replace("{{VerificationUrl}}", user.VerificationUrl);
                using (MailMessage mailMessage = new MailMessage())
                {
                    mailArgs.SmtpHost= _config["EmailSettings:SmtpServer"];
                    mailArgs.Port = Convert.ToInt16(_config["EmailSettings:Port"]);                    
                    mailArgs.MailFrom = _config["EmailSettings:NoReplyEmail"];                    
                    mailArgs.Password = _config["EmailSettings:Password"];                    
                    mailMessage.From = new MailAddress(mailArgs.MailFrom);
                    mailMessage.To.Add(mailArgs.MailTo);
                    if (!string.IsNullOrEmpty(mailArgs.Bcc))
                        mailMessage.Bcc.Add(mailArgs.Bcc);
                    mailMessage.Subject = mailArgs.Subject;
                    mailMessage.Body = emailTemplate;
                    mailMessage.IsBodyHtml = true;

                    // Attach files if available
                    if (attachmentPaths != null && attachmentPaths.Length > 0)
                    {
                        foreach (string attachmentPath in attachmentPaths)
                        {
                            if (File.Exists(attachmentPath))
                            {
                                Attachment attachment = new Attachment(attachmentPath);
                                mailMessage.Attachments.Add(attachment);
                            }
                        }
                    }         

                    using (SmtpClient smtpClient = new SmtpClient(mailArgs.SmtpHost, mailArgs.Port))
                    {
                        smtpClient.UseDefaultCredentials = false;
                        smtpClient.Credentials = new NetworkCredential(mailArgs.MailFrom, mailArgs.Password);                        
                        smtpClient.EnableSsl = true; // Enable SSL/TLS
                         smtpClient.Send(mailMessage);
                        _logger.LogInformation(Const.EmailSentSuccess);
                    }
                }

            }
            catch (Exception ex)
            {
                _logger.LogError($"Error sending email: {ex.Message}");
                throw ;
            }

            finally {
                
            }

         }

        public void SendOtpEmial(UserPasscode userPasscode, Email mailArgs, string[] attachmentPaths = null)
        {
            try
            {

                // Read the content of the email template file

                string rootDirectory = AppDomain.CurrentDomain.BaseDirectory;
                string userVerificationTemplatePath = Path.Combine(rootDirectory, "EmailTemplate", "OTPGenertion_forgotpassword.html");

                // Now you can use these paths to read the contents of the template files or perform other operations

                string emailTemplate = File.ReadAllText(userVerificationTemplatePath);

                // Replace placeholders in the template with actual values
                emailTemplate = emailTemplate.Replace("{{FirstName}}", userPasscode.User.FirstName)
                                             .Replace("{{Passcode}}", userPasscode.Passcode.OTP);
                using (MailMessage mailMessage = new MailMessage())
                {
                    mailArgs.SmtpHost = _config["EmailSettings:SmtpServer"];
                    ;
                    mailArgs.Port = Convert.ToInt16(_config["EmailSettings:Port"]);
                    mailArgs.MailFrom = _config["EmailSettings:NoReplyEmail"];
                    mailArgs.Password = _config["EmailSettings:Password"];
                    mailMessage.From = new MailAddress(mailArgs.MailFrom);
                    mailMessage.To.Add(mailArgs.MailTo);
                    if (!string.IsNullOrEmpty(mailArgs.Bcc))
                        mailMessage.Bcc.Add(mailArgs.Bcc);
                    mailMessage.Subject = mailArgs.Subject;
                    mailMessage.Body = emailTemplate;
                    mailMessage.IsBodyHtml = true;

                    // Attach files if available
                    if (attachmentPaths != null && attachmentPaths.Length > 0)
                    {
                        foreach (string attachmentPath in attachmentPaths)
                        {
                            if (File.Exists(attachmentPath))
                            {
                                Attachment attachment = new Attachment(attachmentPath);
                                mailMessage.Attachments.Add(attachment);
                            }
                        }
                    }

                    using (SmtpClient smtpClient = new SmtpClient(mailArgs.SmtpHost, mailArgs.Port))
                    {
                        smtpClient.UseDefaultCredentials = false;
                        smtpClient.Credentials = new NetworkCredential(mailArgs.MailFrom, mailArgs.Password);
                        smtpClient.EnableSsl = true; // Enable SSL/TLS
                        smtpClient.Send(mailMessage);
                        _logger.LogInformation(Const.EmailSentSuccess);
                    }
                }

            }
            catch (Exception ex)
            {
                _logger.LogError($"Error sending email: {ex.Message}");
                throw;
            }

            finally
            {

            }

        }

        public void InsertEmailHistory(int userId, Email email)
        {
            try
            {
                // Create and open the connection to the database
                using (SqlConnection connection = new SqlConnection(ConfigurationData.DbConnectionString))
                {
                    // Create a SqlCommand object to execute the stored procedure
                    using (SqlCommand command = new SqlCommand("InsertEmail", connection))
                    {
                        // Specify that the SqlCommand is a stored procedure
                        command.CommandType = CommandType.StoredProcedure;
                        // Add parameters to the SqlCommand object
                        command.Parameters.AddWithValue("@SenderEmail", email.MailFrom);
                        command.Parameters.AddWithValue("@RecipientEmail", email.MailTo);
                        command.Parameters.AddWithValue("@Subject", email.Subject);
                        command.Parameters.AddWithValue("@Body", email.Message);
                        command.Parameters.AddWithValue("@SentDateTime", DateTime.Now);               
                        command.Parameters.AddWithValue("@ArchiveFlag", email.archiveFlag);
                        command.Parameters.AddWithValue("@UserId", userId);

                        // Open the connection
                        connection.Open();
                        // Execute the stored procedure
                        command.ExecuteNonQuery();
                    }
                }
            }
            catch
            { throw; }

        }

    }
}
