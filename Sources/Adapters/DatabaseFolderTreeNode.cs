namespace SsmsDatabaseFolders
{
    using System;
    using System.Drawing;
    using System.Windows.Forms;

    using Microsoft.SqlServer.Management.UI.VSIntegration.ObjectExplorer;

    internal class DatabaseFolderTreeNode : HierarchyTreeNode, INodeWithMenu, IServiceProvider
    {
        readonly object parent;

        public DatabaseFolderTreeNode(TreeNode parent)
        {
            this.parent = parent;
        }

        public override Icon Icon => (parent as INodeWithIcon)?.Icon;

        public override Icon SelectedIcon => (parent as INodeWithIcon)?.SelectedIcon;

        public override bool ShowPolicyHealthState
        {
            get => false;
            set { }
        }

        public override int State => (parent as INodeWithIcon)?.State ?? 0;

        public object GetService(Type serviceType)
        {
            return (parent as IServiceProvider)?.GetService(serviceType);
        }


        public void DoDefaultAction()
        {
            (parent as INodeWithMenu)?.DoDefaultAction();
        }

        public void ShowContextMenu(Point screenPos)
        {
            (parent as INodeWithMenu)?.ShowContextMenu(screenPos);
        }
    }
}
