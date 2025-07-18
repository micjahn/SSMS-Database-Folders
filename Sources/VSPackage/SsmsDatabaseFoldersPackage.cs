﻿namespace SsmsDatabaseFolders
{
    extern alias Ssms18;
    extern alias Ssms19;
    extern alias Ssms20;
    extern alias Ssms21;
    using Localization;
    using Microsoft.VisualStudio.Shell;
    using Microsoft.VisualStudio.Shell.Interop;
    using System;
    using System.ComponentModel.Design;
    using System.Diagnostics.CodeAnalysis;
    using System.IO;
    using System.Runtime.InteropServices;
    using System.Windows.Forms;

    /// <summary>
    /// This is the class that implements the package exposed by this assembly.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The minimum requirement for a class to be considered a valid package for Visual Studio
    /// is to implement the IVsPackage interface and register itself with the shell.
    /// This package uses the helper classes defined inside the Managed Package Framework (MPF)
    /// to do it: it derives from the Package class that provides the implementation of the
    /// IVsPackage interface and uses the registration attributes defined in the framework to
    /// register itself and its components with the shell. These attributes tell the pkgdef creation
    /// utility what data to put into .pkgdef file.
    /// </para>
    /// <para>
    /// To get loaded into VS, the package must be referred by &lt;Asset Type="Microsoft.VisualStudio.VsPackage" ...&gt; in .vsixmanifest file.
    /// </para>
    /// </remarks>
    [PackageRegistration(UseManagedResourcesOnly = true, AllowsBackgroundLoading = true)]
    [InstalledProductRegistration("#110", "#112", "1.0", IconResourceID = 400)] // Info on this package for Help/About
    [Guid(PackageGuidString)]
    [ProvideAutoLoad("d114938f-591c-46cf-a785-500a82d97410", PackageAutoLoadFlags.BackgroundLoad)] //CommandGuids.ObjectExplorerToolWindowIDString
    [ProvideMenuResource("Menus.ctmenu", 1)]
    [ProvideOptionPage(typeof(DatabaseFolderOptions), "SQL Server Object Explorer", "Database Folders", 114, 116, true)]
    [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1650:ElementDocumentationMustBeSpelledCorrectly", Justification = "pkgdef, VS and vsixmanifest are valid VS terms")]
    public sealed class SsmsDatabaseFoldersPackage : AsyncPackage, IDebugOutput
    {
        /// <summary>
        /// SsmsDatabaseFoldersPackage GUID string.
        /// </summary>
        public const string PackageGuidString = "2D162307-505C-4F59-9E87-E7E579C4BF9D";
        public static readonly Guid PackageGuid = new Guid(PackageGuidString);

        private DatabaseFolderOptions _options;
        public DatabaseFolderOptions Options
        {
            get
            {
                return _options ?? (_options = new DatabaseFolderOptions());
            }
            set
            {
                _options = value;
            }
        }
        
        private IObjectExplorerExtender _objectExplorerExtender;

        private IVsOutputWindowPane _outputWindowPane = null;

        /// <summary>
        /// Initializes a new instance of the <see cref="SsmsDatabaseFoldersPackage"/> class.
        /// </summary>
        public SsmsDatabaseFoldersPackage()
        {
            // Inside this method you can place any initialization code that does not require
            // any Visual Studio service because at this point the package object is created but
            // not sited yet inside Visual Studio environment. The place to do all the other
            // initialization is the Initialize method.
        }

        protected override async System.Threading.Tasks.Task InitializeAsync(System.Threading.CancellationToken cancellationToken, IProgress<ServiceProgressData> progress)
        {
            await this.JoinableTaskFactory.SwitchToMainThreadAsync(cancellationToken);
            // Link with VS options.
            object obj;
            (this as IVsPackage).GetAutomationObject("SQL Server Object Explorer.Database Folders", out obj);
            Options = (DatabaseFolderOptions)obj;

            _objectExplorerExtender = GetObjectExplorerExtender();

            if (_objectExplorerExtender != null)
                AttachTreeViewEvents();

            ToggleDebugOutput();

            var mcs = GetService(typeof(IMenuCommandService)) as OleMenuCommandService;
            if (null != mcs)
            {
                var id = new CommandID(PkgCmdIDList.GuidMenuAndCommandsCmdSet, PkgCmdIDList.cmdidToggleCommand);
                var command = new OleMenuCommand(new EventHandler(CommandToggleDatabaseFolders), id);
                mcs.AddCommand(command);
            }
        }

        private void ToggleDebugOutput()
        {
            if (Options.EnableDebugOutput)
            {
                if (_outputWindowPane == null)
                {
                    // OutputWindowPane for debug messages
                    var outputWindow = (IVsOutputWindow)GetService(typeof(SVsOutputWindow));
                    if (outputWindow != null)
                    {
                        var guidPackage = PackageGuid;
                        outputWindow.CreatePane(PackageGuid, "Database Folders debug output", 1, 0);
                        outputWindow.GetPane(ref guidPackage, out _outputWindowPane);
                    }
                }
            }
            else
            {
                if (_outputWindowPane != null)
                {
                    var outputWindow = (IVsOutputWindow)GetService(typeof(SVsOutputWindow));
                    if (outputWindow != null)
                    {
                        var guidPackage = PackageGuid;
                        outputWindow.DeletePane(ref guidPackage);
                        _outputWindowPane = null;
                    }
                }
            }
        }

        private void CommandToggleDatabaseFolders(object caller, EventArgs args)
        {
            debug_message("toggle database folders");
            Options.Enabled = !Options.Enabled;
            debug_message("Enabled: " + Options.Enabled);
            debug_message("GroupDatabasesByName: " + Options.GroupDatabasesByName);
            debug_message("SeparateReadonlyDatabases: " + Options.SeparateReadonlyDatabases);
            _objectExplorerExtender.RefreshDatabaseNode();
        }

        private IObjectExplorerExtender GetObjectExplorerExtender()
        {
            try
            {
                var ssmsInterfacesPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "SqlWorkbench.Interfaces.dll");

                if (File.Exists(ssmsInterfacesPath))
                {
                    var ssmsInterfacesVersion = System.Diagnostics.FileVersionInfo.GetVersionInfo(ssmsInterfacesPath);

                    switch (ssmsInterfacesVersion.FileMajorPart)
                    {
                        case 21:
                            debug_message("SsmsVersion:21");
                            return new Ssms21::SsmsDatabaseFolders.ObjectExplorerExtender(this, Options, GetLocalizedString);

                        case 20:
                            debug_message("SsmsVersion:20");
                            return new Ssms20::SsmsDatabaseFolders.ObjectExplorerExtender(this, Options, GetLocalizedString);

                        case 19:
                        case 17:
                        case 16:
                            debug_message("SsmsVersion:19");
                            return new Ssms19::SsmsDatabaseFolders.ObjectExplorerExtender(this, Options, GetLocalizedString);

                        case 15:
                            debug_message("SsmsVersion:18");
                            return new Ssms18::SsmsDatabaseFolders.ObjectExplorerExtender(this, Options, GetLocalizedString);

                        default:
                            ActivityLogEntry(__ACTIVITYLOG_ENTRYTYPE.ALE_INFORMATION, String.Format("SqlWorkbench.Interfaces.dll v{0}:{1}", ssmsInterfacesVersion.FileMajorPart, ssmsInterfacesVersion.FileMinorPart));
                            break;
                    }
                }

                ActivityLogEntry(__ACTIVITYLOG_ENTRYTYPE.ALE_WARNING, "Unknown SSMS Version. Defaulting to 20.x.");
                return new Ssms20::SsmsDatabaseFolders.ObjectExplorerExtender(this, Options, GetLocalizedString);
            }
            catch (Exception ex)
            {
                ActivityLogEntry(__ACTIVITYLOG_ENTRYTYPE.ALE_ERROR, ex.ToString());
                return null;
            }
        }

        private void AttachTreeViewEvents()
        {
            var treeView = _objectExplorerExtender.GetObjectExplorerTreeView();
            if (treeView != null)
            {
                treeView.BeforeExpand += new TreeViewCancelEventHandler(ObjectExplorerTreeViewBeforeExpandCallback);
                treeView.AfterExpand += new TreeViewEventHandler(ObjectExplorerTreeViewAfterExpandCallback);
            }
            else
                ActivityLogEntry(__ACTIVITYLOG_ENTRYTYPE.ALE_ERROR, "Object Explorer TreeView == null");
        }

        /// <summary>
        /// Adds new nodes and move items between them
        /// </summary>
        /// <param name="node"></param>
        private void ReorganizeFolders(TreeNode node)
        {
            debug_message("ReorganizeFolders");
            try
            {
                _objectExplorerExtender.ReorganizeDatabaseNodes(node);
            }
            catch (Exception ex)
            {
                ActivityLogEntry(__ACTIVITYLOG_ENTRYTYPE.ALE_ERROR, ex.ToString());
            }
        }


        /// <summary>
        /// After expand node
        /// </summary>
        /// <param name="sender">object explorer</param>
        /// <param name="e">expanding node</param>
        void ObjectExplorerTreeViewAfterExpandCallback(object sender, TreeViewEventArgs e)
        {
            debug_message("\nObjectExplorerTreeViewAfterExpandCallback");
            // Wait for the async node expand to finish or we could miss nodes
            try
            {
                debug_message("Node.Count:{0}", e.Node.GetNodeCount(false));

                if (!Options.GroupDatabasesByName && !Options.SeparateReadonlyDatabases)
                    return;

                if (e.Node.TreeView.InvokeRequired)
                    debug_message("TreeView.InvokeRequired");

                if (_objectExplorerExtender.GetNodeExpanding(e.Node))
                {
                    debug_message("node.Expanding");
                    var waitCount = 0;

                    e.Node.TreeView.Cursor = Cursors.AppStarting;

                    var nodeExpanding = new Timer();
                    nodeExpanding.Interval = 10;
                    EventHandler nodeExpandingEvent = null;
                    nodeExpandingEvent = (object o, EventArgs e2) =>
                    {
                        debug_message("nodeExpanding:{0}", waitCount);
                        waitCount++;
                        if (e.Node.TreeView.InvokeRequired)
                            debug_message("TreeView.InvokeRequired");
                        debug_message("Node.Count:{0}", e.Node.GetNodeCount(false));

                        if (!_objectExplorerExtender.GetNodeExpanding(e.Node))
                        {
                            nodeExpanding.Tick -= nodeExpandingEvent;
                            nodeExpanding.Stop();
                            nodeExpanding.Dispose();

                            ReorganizeFolders(e.Node);

                            e.Node.TreeView.Cursor = Cursors.Default;
                        }
                    };
                    nodeExpanding.Tick += nodeExpandingEvent;
                    nodeExpanding.Start();
                }
            }
            catch (Exception ex)
            {
                ActivityLogEntry(__ACTIVITYLOG_ENTRYTYPE.ALE_ERROR, ex.ToString());
            }
        }

        /// <summary>
        /// Object explorer tree view: event before expand
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void ObjectExplorerTreeViewBeforeExpandCallback(object sender, TreeViewCancelEventArgs e)
        {
            debug_message("\nObjectExplorerTreeViewBeforeExpandCallback");
            try
            {
                if (!Options.GroupDatabasesByName && !Options.SeparateReadonlyDatabases)
                    return;

                debug_message("Node.Count:{0}", e.Node.GetNodeCount(false));

                if (e.Node.GetNodeCount(false) == 1)
                    return;

                if (_objectExplorerExtender.GetNodeExpanding(e.Node))
                {
                    debug_message("node.Expanding");
                    //doing a reorg before expand stops the treeview from jumping
                    ReorganizeFolders(e.Node);
                }
            }
            catch (Exception ex)
            {
                ActivityLogEntry(__ACTIVITYLOG_ENTRYTYPE.ALE_ERROR, ex.ToString());
            }
            
        }

        public void debug_message(string message, params object[] args)
        {
            ToggleDebugOutput();

            if (_outputWindowPane != null)
            {
                if (args == null || args.Length < 1)
                    _outputWindowPane.OutputString(message);
                else
                    _outputWindowPane.OutputString(String.Format(message, args));
                _outputWindowPane.OutputString("\r\n");
            }
        }

        private void ActivityLogEntry(__ACTIVITYLOG_ENTRYTYPE entryType, string message)
        {
            debug_message(message);

            // Logs to %AppData%\Microsoft\VisualStudio\15.0\ActivityLog.XML.
            // Recommended to obtain the activity log just before writing to it. Do not cache or save the activity log for future use.
            var log = GetService(typeof(SVsActivityLog)) as IVsActivityLog;
            if (log == null) return;

            int hr = log.LogEntryGuid(
                (UInt32)entryType,
                this.ToString(),
                message,
                PackageGuid);
        }

        private string GetLocalizedString(string resourceKey)
        {
            return ResourcesAccess.GetString(resourceKey) ?? resourceKey;
        }

        internal static class PkgCmdIDList
        {
            public static Guid GuidMenuAndCommandsCmdSet = new Guid("9797EBA2-2C3C-48F7-B207-AD5999F2101F");
            public const int cmdidToggleCommand = 0x2001;
        }
    }
}
