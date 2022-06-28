namespace AndoIt.Common.Interface
{
    public interface IMailSender
    {
        string Name { get; set; }
        string SmtpServer { get; set; }
        string User { get; set; }
        string Password { get; set; }
        string From { get; set; }
        string To { get; set; }
        string ReplyTo { get; set; }
        string Subject { get; set; }
        string Text { get; set; }

        void Send(string subject = null, string text = null);
    }
}
