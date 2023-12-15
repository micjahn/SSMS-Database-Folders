using System.Windows.Forms;

namespace SsmsDatabaseFolders
{
    public interface IObjectExplorerExtender
    {
        bool GetNodeExpanding(TreeNode node);
        TreeView GetObjectExplorerTreeView();
        void ReorganizeDatabaseNodes(TreeNode node);
        void RefreshDatabaseNode();
    }
}
