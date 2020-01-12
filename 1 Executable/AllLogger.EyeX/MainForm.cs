using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Web.Script.Serialization;
using System.Windows.Forms;
using WK.Libraries.SharpClipboardNS;

// 需要用Nuget安装包SharpClipboard
namespace AllLogger.EyeX {
    public partial class MainForm : Form {
        #region 公开方法

        /// <summary>
        ///     窗口构造函数。
        /// </summary>
        public MainForm() {
            InitializeComponent();

            windowTitleTimer.Interval = 500;
            windowTitleTimer.Tick += WindowTitleTimerOnTick;
        }

        #endregion

        #region 私有变量

        /// <summary>
        ///     矩形类型，用于存储窗体矩形信息。
        /// </summary>
        private struct Rect {
            public int Left;
            public int Top;
            public int Right;
            public int Bottom;
            public long Timestamp;

            public override bool Equals(object obj) {
                if (!(obj is Rect))
                    return false;

                var rect = (Rect) obj;
                return Top == rect.Top && Right == rect.Right &&
                       Bottom == rect.Bottom && Left == rect.Left;
            }

            public override int GetHashCode() {
                return (Top + "," + Right + "," + Bottom + "," + Left)
                    .GetHashCode();
            }
        }

        /// <summary>
        ///     活动窗口标题。
        /// </summary>
        private StringBuilder windowTitleName = new StringBuilder(1024);

        /// <summary>
        ///     用于检测活动窗口标题变化的定时器。
        /// </summary>
        private readonly Timer windowTitleTimer = new Timer();

        /// <summary>
        ///     DWMWINDOWATTRIBUTE
        ///     <see cref="https://msdn.microsoft.com/en-us/library/windows/desktop/aa969530(v=vs.85).aspx" />
        /// </summary>
        private enum DwmWindowAttribute {
            /// <summary>
            ///     DWMWA_EXTENDED_FRAME_BOUNDS
            /// </summary>
            DwmwaExtendedFrameBounds = 9
        }

        /******** 尝试一下获取鼠标点击事件的元素 ********/

        #region 鼠标钩子相关变量

        private enum MouseMessages {
            WM_LBUTTONDOWN = 0x0201,
            WM_LBUTTONUP = 0x0202,
            WM_MOUSEMOVE = 0x0200,
            WM_MOUSEWHEEL = 0x020A,
            WM_RBUTTONDOWN = 0x0204,
            WM_RBUTTONUP = 0x0205
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct POINT {
            public readonly int x;
            public readonly int y;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct MSLLHOOKSTRUCT {
            public readonly POINT pt;
            public readonly uint mouseData;
            public readonly uint flags;
            public readonly uint time;
            public readonly IntPtr dwExtraInfo;
        }

        private const int WH_MOUSE_LL = 14;

        private static readonly LowLevelMouseProc
            _mouseProc = MouseHookCallback;

        private static IntPtr _mouseHookId = IntPtr.Zero;

        #endregion

        #region 键盘钩子相关变量
        
        private const int WH_KEYBOARD_LL = 13;
        private const int WM_KEYDOWN = 0x0100;
        private static IntPtr _keyboardHookId = IntPtr.Zero;

        private static readonly LowLevelKeyboardProc _keyboardProc =
            KeyboardHookCallback;

        #endregion

        /// <summary>
        ///     1970时间。
        /// </summary>
        private static readonly DateTime dt1970 =
            TimeZone.CurrentTimeZone.ToLocalTime(new DateTime(1970, 1, 1));

        /// <summary>
        ///     记录起始时间。
        /// </summary>
        private DateTime startTime;

        /// <summary>
        ///     视线停留记录对象。
        /// </summary>
        private StreamWriter fixationWriter;

        /// <summary>
        ///     键盘输入记录对象。
        /// </summary>
        private static StreamWriter keyboardWriter;

        /// <summary>
        ///     鼠标输入记录对象。
        /// </summary>
        private static StreamWriter mouseWriter;

        /// <summary>
        ///     剪贴板输入记录对象。
        /// </summary>
        private static StreamWriter clipboardWriter;

        /// <summary>
        ///     活动窗口输入记录对象。
        /// </summary>
        private static StreamWriter windowTitleWriter;

        /// <summary>
        ///     Json序列化工具。
        /// </summary>
        private static readonly JavaScriptSerializer jsonSerializer =
            new JavaScriptSerializer();

        /// <summary>
        ///     剪贴板。
        /// </summary>
        private readonly SharpClipboard clipboard = new SharpClipboard();

        #endregion

        #region 私有方法

        /// <summary>
        ///     获取活动窗体id。
        /// </summary>
        /// <returns>活动窗体id。</returns>
        [DllImport("user32.dll")]
        private static extern int GetForegroundWindow();

        /// <summary>
        ///     获得Dwm窗体属性。
        /// </summary>
        /// <param name="hwnd">窗体id。</param>
        /// <param name="dwAttribute">属性标识。</param>
        /// <param name="pvAttribute">作为输出参数的属性值。</param>
        /// <param name="cbAttribute">输出属性值得大小。</param>
        /// <returns>是否成功。</returns>
        [DllImport(@"dwmapi.dll")]
        private static extern int DwmGetWindowAttribute(int hwnd,
            int dwAttribute, out Rect pvAttribute, int cbAttribute);

        /// <summary>
        ///     获得窗体标题。
        /// </summary>
        /// <param name="hWnd">窗体id。</param>
        /// <param name="lpString">用于写入窗体标题的StringBuilder。</param>
        /// <param name="nMaxCount">窗体标题的最大长度。</param>
        /// <returns></returns>
        [DllImport("user32.dll")]
        private static extern int GetWindowText(int hWnd,
            StringBuilder lpString, int nMaxCount);

        /// <summary>
        ///     获得窗体标题长度。
        /// </summary>
        /// <param name="hWnd">窗体id。</param>
        /// <returns>窗体标题长度。</returns>
        [DllImport("user32.dll")]
        private static extern int GetWindowTextLength(int hWnd);

        /// <summary>
        ///     浏览按钮事件处理函数。
        /// </summary>
        private void btnLogPath_Click(object sender, EventArgs e) {
            if (folderBrowserDialog.ShowDialog() == DialogResult.OK)
                txbLogPath.Text = folderBrowserDialog.SelectedPath;
        }

        #region 鼠标钩子

        /// <summary>
        ///     设置鼠标钩子
        /// </summary>
        private static IntPtr SetHook(LowLevelMouseProc proc) {
            using (var curProcess = Process.GetCurrentProcess())
            using (var curModule = curProcess.MainModule) {
                return SetWindowsHookEx(WH_MOUSE_LL, proc,
                    GetModuleHandle(curModule.ModuleName), 0);
            }
        }

        /// <summary>
        ///     鼠标钩子回调
        /// </summary>
        private static IntPtr MouseHookCallback(int nCode, IntPtr wParam,
            IntPtr lParam) {
            if (
                nCode >=
                0 // &&MouseMessages.WM_LBUTTONDOWN == (MouseMessages)wParam
            ) {
                var hookStruct =
                    (MSLLHOOKSTRUCT) Marshal.PtrToStructure(lParam,
                        typeof(MSLLHOOKSTRUCT));
                mouseWriter.WriteLine(jsonSerializer.Serialize(new {
                    hookStruct.pt.x, hookStruct.pt.y,
                    activity = (MouseMessages) wParam,
                    Timestamp = (long) DateTime.Now.Subtract(dt1970)
                        .TotalMilliseconds
                }));
                mouseWriter.Flush();
            }

            return CallNextHookEx(_mouseHookId, nCode, wParam, lParam);
        }

        #region 鼠标必要函数集

        private delegate IntPtr LowLevelMouseProc(int nCode, IntPtr wParam,
            IntPtr lParam);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr SetWindowsHookEx(int idHook,
            LowLevelMouseProc lpfn, IntPtr hMod, uint dwThreadId);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool UnhookWindowsHookEx(IntPtr hhk);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode,
            IntPtr wParam, IntPtr lParam);

        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr GetModuleHandle(string lpModuleName);

        #endregion

        #endregion

        #region 键盘钩子

        /// <summary>
        ///     键盘钩子设置函数
        /// </summary>
        private static IntPtr SetHook(LowLevelKeyboardProc proc) {
            using (var curProcess = Process.GetCurrentProcess())
            using (var curModule = curProcess.MainModule) {
                return SetWindowsHookEx(WH_KEYBOARD_LL, proc,
                    GetModuleHandle(curModule.ModuleName), 0);
            }
        }

        /// <summary>
        ///     键盘钩子回调函数
        /// </summary>
        private static IntPtr KeyboardHookCallback(int nCode, IntPtr wParam,
            IntPtr lParam) {
            if (nCode >= 0 // && wParam == (IntPtr)WM_KEYDOW
            ) {
                var vkCode = Marshal.ReadInt32(lParam);
                var kc = new KeysConverter();
                var realKey = (Keys) vkCode;
                var keyChar = kc.ConvertToString(realKey);
                keyboardWriter.WriteLine(jsonSerializer.Serialize(new {
                    Activity = wParam,
                    Timestamp = (long) DateTime.Now.Subtract(dt1970)
                        .TotalMilliseconds,
                    KeyValue = keyChar
                }));
                keyboardWriter.Flush();
            }

            return CallNextHookEx(_keyboardHookId, nCode, wParam, lParam);
        }

        #region 键盘钩子使用必要函数集

        private delegate IntPtr LowLevelKeyboardProc(int nCode, IntPtr wParam,
            IntPtr lParam);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr SetWindowsHookEx(int idHook,
            LowLevelKeyboardProc lpfn, IntPtr hMod, uint dwThreadId);

        #endregion

        #endregion

        /// <summary>
        /// 活动窗口标题变化定时器事件处理函数。
        /// </summary>
        private void WindowTitleTimerOnTick(object sender, EventArgs e) {
            var windowTitleId = GetForegroundWindow();
            var newWindowTitleName = new StringBuilder(512);
            GetWindowText(windowTitleId, newWindowTitleName, 512);

            if (!newWindowTitleName.Equals(windowTitleName)) {
                windowTitleName = newWindowTitleName;
                windowTitleWriter.WriteLine(jsonSerializer.Serialize(new {
                    Timestamp = (long) DateTime.Now.Subtract(dt1970)
                        .TotalMilliseconds,
                    Title = newWindowTitleName.ToString()
                }));
            }

            windowTitleWriter.Flush();
        }

        /// <summary>
        ///     剪贴板内容变化事件处理函数。
        /// </summary>
        private void Clipboard_ClipboardChanged(object sender,
            SharpClipboard.ClipboardChangedEventArgs e) {
            // Is the content copied of text type?
            if (e.ContentType == SharpClipboard.ContentTypes.Text) {
                // Get the cut/copied text.
                clipboardWriter.WriteLine(jsonSerializer.Serialize(new {
                    Timestamp = (long) DateTime.Now.Subtract(dt1970)
                        .TotalMilliseconds,
                    content = e.Content.ToString()
                }));
                clipboardWriter.Flush();
            }
        }

        /// <summary>
        ///     开始按钮事件处理函数。
        /// </summary>
        private void btnStart_Click(object sender, EventArgs e) {
            if (!Verify())
                return;

            txbLogPath.Enabled = false;
            btnLogPath.Enabled = false;
            btnStart.Enabled = false;
            btnStop.Enabled = true;
            toolStripStatusLabel.Text = "正在记录日志...";
            toolStripProgressBar.Style = ProgressBarStyle.Marquee;
            Hide();
            notifyIcon.ShowBalloonTip(2000, "AllLogger.EyeX", "开始记录日志",
                ToolTipIcon.Info);
            notifyIcon.Text = "AllLogger.EyeX: 正在记录日志";

            startTime = DateTime.Now;
            fixationWriter = new StreamWriter(
                txbLogPath.Text + "\\fixation." +
                startTime.ToString("yyyy-MM-dd-hh-mm-ss") + ".log");
            keyboardWriter = new StreamWriter(
                txbLogPath.Text + "\\keyBoard." +
                startTime.ToString("yyyy-MM-dd-hh-mm-ss") + ".log");
            mouseWriter = new StreamWriter(
                txbLogPath.Text + "\\mouse." +
                startTime.ToString("yyyy-MM-dd-hh-mm-ss") + ".log");
            clipboardWriter = new StreamWriter(
                txbLogPath.Text + "\\clipboard." +
                startTime.ToString("yyyy-MM-dd-hh-mm-ss") + ".log");
            windowTitleWriter = new StreamWriter(
                txbLogPath.Text + "\\windowTitle." +
                startTime.ToString("yyyy-MM-dd-hh-mm-ss") + ".log");

            windowTitleTimer.Start();

            clipboard.ClipboardChanged += Clipboard_ClipboardChanged;

            _mouseHookId = SetHook(_mouseProc); // 设置鼠标输入事件钩子
            _keyboardHookId = SetHook(_keyboardProc); // 设置键盘输入事件钩子
        }

        /// <summary>
        ///     验证用户输入与环境。
        /// </summary>
        /// <returns>是否通过验证。</returns>
        private bool Verify() {
            if (string.IsNullOrWhiteSpace(txbLogPath.Text)) {
                MessageBox.Show("尚未指定日志文件路径。", "AllLogger.EyeX",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }

            return true;
        }

        /// <summary>
        ///     停止按钮事件处理函数。
        /// </summary>
        private void btnStop_Click(object sender, EventArgs e) {
            fixationWriter.Flush();
            fixationWriter.Close();

            windowTitleTimer.Stop();
            windowTitleWriter.Flush();
            windowTitleWriter.Close();

            UnhookWindowsHookEx(_keyboardHookId); // 解绑键盘钩子
            keyboardWriter.Flush();
            keyboardWriter.Close();

            UnhookWindowsHookEx(_mouseHookId); // 解绑鼠标钩子
            mouseWriter.Flush();
            mouseWriter.Close();

            clipboard.ClipboardChanged -= Clipboard_ClipboardChanged;
            clipboardWriter.Flush();
            clipboardWriter.Close();

            notifyIcon.Text = "AllLogger.EyeX";
            notifyIcon.ShowBalloonTip(2000, "AllLogger.EyeX", "日志记录结束",
                ToolTipIcon.Info);
            toolStripProgressBar.Style = ProgressBarStyle.Blocks;
            toolStripStatusLabel.Text = "";
            btnStop.Enabled = false;
            btnStart.Enabled = true;
            btnLogPath.Enabled = true;
            txbLogPath.Enabled = true;
        }

        /// <summary>
        ///     通知栏按钮事件处理函数。
        /// </summary>
        private void notifyIcon_Click(object sender, EventArgs e) {
            Visible = !Visible;
        }

        #endregion
    }
}