using System;
using System.ComponentModel.Design;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.OLE.Interop;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.Win32;
using Task = System.Threading.Tasks.Task;

namespace AllLoggerVisualStudioExtension {
    [ProvideAutoLoad(UIContextGuids.SolutionExists)]
    [PackageRegistration(UseManagedResourcesOnly = true)]
    [InstalledProductRegistration("#110", "#112", "1.0", IconResourceID = 400)]
    [Guid(VSPackage.PackageGuidString)]
    [SuppressMessage("StyleCop.CSharp.DocumentationRules",
        "SA1650:ElementDocumentationMustBeSpelledCorrectly", Justification =
            "pkgdef, VS and vsixmanifest are valid VS terms")]
    public sealed class VSPackage : Package {
        public const string PackageGuidString =
            "3babd3fc-49e6-4fde-a0b2-ce76ea5a6f80";

        public VSPackage() { }

        #region Package Members

        protected override void Initialize() {
            KeyMonitor.Initialize(this);
            base.Initialize();
        }

        #endregion
    }
}