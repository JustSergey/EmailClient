using MailKit;
using MailKit.Net.Imap;
using MailKit.Net.Smtp;
using MailKit.Search;
using MimeKit;
using MimeKit.Text;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EmailClient
{
    public class MailClient
    {
        SmtpClient smtpClient;
        ImapClient imapClient;
        string imapAddress;
        int imapPort;
        string smtpAddress;
        int smtpPort;
        string userName;
        string password;
        bool isConnected = false;

        public MailClient(string smtpAddress, int smtpPort, string imapAddress, int imapPort) =>
            (this.smtpAddress, this.smtpPort, this.imapAddress, this.imapPort) = (smtpAddress, smtpPort, imapAddress, imapPort);

        public bool Connect(string userName, string password, out string error)
        {
            this.userName = userName;
            this.password = password;
            error = "";
            try
            {
                imapClient = new ImapClient();
                imapClient.Connect(imapAddress, imapPort, true);
                imapClient.Authenticate(userName, password);
                smtpClient = new SmtpClient();
                smtpClient.Connect(smtpAddress, smtpPort, true);
                smtpClient.Authenticate(userName, password);
                smtpClient.Disconnect(true);
                isConnected = true;
            }
            catch (Exception e) 
            {
                error = e.Message;
            }
            return isConnected;
        }

        public void Disconnect()
        {
            if (isConnected)
            {
                imapClient.Disconnect(true);
                smtpClient.Disconnect(true);
            }
        }

        public void Update(string name, string bodyText)
        {
            if (!isConnected)
                return;

            var inbox = imapClient.Inbox;
            inbox.Open(FolderAccess.ReadWrite);
            var query = SearchQuery.NotAnswered.And(SearchQuery.NotSeen);
            var messages = inbox.Search(query);
            if (messages.Count > 0)
            {
                smtpClient = new SmtpClient();
                smtpClient.Connect(smtpAddress, smtpPort, true);
                smtpClient.Authenticate(userName, password);
                foreach (var uid in messages)
                {
                    var message = inbox.GetMessage(uid);
                    var addresses = message.From;
                    MimeMessage sendMessage = new MimeMessage();
                    sendMessage.From.Add(new MailboxAddress(name, userName));
                    sendMessage.To.AddRange(addresses);
                    sendMessage.Subject = $"Ответ на {message.Subject}";
                    sendMessage.Body = new TextPart(TextFormat.Plain) { Text = bodyText };
                    smtpClient.Send(sendMessage);
                    inbox.AddFlags(uid, MessageFlags.Answered, true);
                }
                smtpClient.Disconnect(true);
            }
        }
    }
}
