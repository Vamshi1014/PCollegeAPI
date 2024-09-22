namespace Flyurdreamcommands.Models.Datafields
{
    public class Email
    {
        public string MailTo { get; set; }
        public string Bcc { get; set; }
        public string Name { get; set; }
        public string Subject { get; set; }
        public string Message { get; set; }
        public string SmtpHost { get; set; }
        public string Password { get; set; }
        public int Port { get; set; }
        public string MailFrom { get; set; }
        public DateTime sentDateTime { get; set; }
        public bool archiveFlag  { get; set; }
    }
  
}
