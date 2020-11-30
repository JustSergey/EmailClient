using System;
using System.IO;
using System.Windows.Forms;

namespace EmailClient
{
    public partial class MainForm : Form
    {
        const string name = "Sergey Marchenko";
        const string smtp_address = "smtp.gmail.com";
        const int smtp_port = 465;
        const string imap_address = "imap.gmail.com";
        const int imap_port = 993;
        const string body_text_path = "bodyText.txt";
        const string credential_path = "credential.txt";

        string userName, password;
        MailClient mailClient;
        string bodyText;

        public MainForm()
        {
            InitializeComponent();
            mailClient = new MailClient(smtp_address, smtp_port, imap_address, imap_port);
            (userName, password) = ("", "");
            if (File.Exists(credential_path))
            {
                string[] credential = File.ReadAllLines(credential_path);
                if (credential.Length > 1)
                {
                    userName = credential[0];
                    password = credential[1];
                }
            }
            if (File.Exists(body_text_path))
                bodyText = File.ReadAllText(body_text_path);
            else
                bodyText = "Ответ";
        }

        private void connectButton_Click(object sender, EventArgs e)
        {
            if (mailClient.Connect(userName, password, out string error))
            {
                startButton.Enabled = true;
                connectButton.Enabled = false;
            }
            else
                MessageBox.Show(this, error);
        }

        private void MainForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            mailClient.Disconnect();
        }

        private void startButton_Click(object sender, EventArgs e)
        {
            updateTimer.Enabled = true;
            startButton.Enabled = false;
        }

        private void updateTimer_Tick(object sender, EventArgs e)
        {
            mailClient.Update(name, bodyText);
        }
    }
}
