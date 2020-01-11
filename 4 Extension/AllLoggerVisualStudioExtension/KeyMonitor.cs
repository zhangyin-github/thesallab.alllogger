using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using EnvDTE;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor.Commanding.Commands;
using Timer = System.Timers.Timer;

namespace AllLoggerVisualStudioExtension {
    /// <summary>
    /// 按键监控工具。
    /// </summary>
    internal class KeyMonitor {
        /// <summary>
        /// 按键监控工具单件。
        /// </summary>
        public static KeyMonitor Instance { get; private set; }

        /// <summary>
        /// DTE。
        /// </summary>
        private DTE dte;

        /// <summary>
        /// 解决方案事件。
        /// </summary>
        private SolutionEvents solutionEvents;

        /// <summary>
        /// 定时器。
        /// </summary>
        private Timer timer;

        /// <summary>
        /// 按键事件参数。
        /// </summary>
        private TypeCharCommandArgs typeCharCommandArgs;

        /// <summary>
        /// 初始化。
        /// </summary>
        public static void Initialize(Package package) {
            Instance = new KeyMonitor(package);
        }

        /// <summary>
        /// 定时器事件处理函数。
        /// </summary>
        private void TimerOnElapsed(object sender, ElapsedEventArgs e) {
            Save();
        }

        /// <summary>
        /// 按键已按下。
        /// </summary>
        /// <param name="args">按键事件参数。</param>
        public void KeyPressed(TypeCharCommandArgs args) {
            if (timer == null) {
                return;
            }

            typeCharCommandArgs = args;
            timer.Stop();
            timer.Start();
        }

        /// <summary>
        /// 私有的构造函数。
        /// </summary>
        private KeyMonitor(Package package) {
            dte = (DTE) ((IServiceProvider) package).GetService(typeof(DTE));
            solutionEvents = dte.Events.SolutionEvents;
            solutionEvents.Opened += SolutionEventsOnOpened;
        }

        /// <summary>
        /// 解决方案打开事件处理函数。
        /// </summary>
        private void SolutionEventsOnOpened() {
            if (!Directory.Exists(zipRoot)) {
                Directory.CreateDirectory(zipRoot);
            }

            solutionEvents.BeforeClosing += SolutionEventsOnBeforeClosing;

            timer = new Timer();
            timer.Elapsed += TimerOnElapsed;
            timer.Interval = 2000;
            timer.AutoReset = false;
        }

        /// <summary>
        /// 解决方案关闭事件处理函数。
        /// </summary>
        private void SolutionEventsOnBeforeClosing() {
            if (timer != null) {
                timer.Stop();
                timer.Elapsed -= TimerOnElapsed;
                timer = null;
            }

            solutionEvents.BeforeClosing -= SolutionEventsOnBeforeClosing;
        }

        /// <summary>
        /// 压缩工具。
        /// </summary>
        private Zipper zipper = new Zipper();

        /// <summary>
        /// 压缩文件夹根目录。
        /// </summary>
        private string zipRoot =
            Environment.GetFolderPath(Environment.SpecialFolder
                .LocalApplicationData) + "\\AllLoggerVisualStudioExtension\\";

        /// <summary>
        /// 保存文件。
        /// </summary>
        private void Save() {
            typeCharCommandArgs.SubjectBuffer.Properties
                .TryGetProperty<ITextDocument>(typeof(ITextDocument),
                    out ITextDocument textDocument);

            if (textDocument == null) {
                return;
            }

            var documentPath = textDocument.FilePath;
            var documentName = documentPath.Substring(
                documentPath.LastIndexOf("\\", StringComparison.Ordinal) + 1);
            var outputPath = zipRoot + "\\" + DateTime.Now.ToString("yyyyMMddHHmmss") + "-" + documentName;
            textDocument.SaveCopy(outputPath, true);
        }
    }
}