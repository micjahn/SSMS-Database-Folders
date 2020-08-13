namespace SsmsDatabaseFolders
{
    using System;
    using System.Reflection;
    using System.Windows.Forms;

    using Microsoft.SqlServer.Management.UI.VSIntegration.ObjectExplorer;

    /// <summary>
    /// Used to organize Databases in Object Explorer into groups
    /// </summary>
    public class ObjectExplorerExtender : IObjectExplorerExtender
    {
        public const string DatabaseFolderNodeTag = "DatabaseFolder";

        private IDatabaseFolderOptions Options { get; }
        private IServiceProvider Package { get; }
        private Func<string, string> GetStringFromResources { get;  }

        private string LocalizedReadonly;

        public ObjectExplorerExtender(IServiceProvider package, IDatabaseFolderOptions options, Func<string, string> getStringFromResources)
        {
            Package = package;
            Options = options;
            GetStringFromResources = getStringFromResources;
        }

        public TreeView GetObjectExplorerTreeView()
        {
            var objectExplorerService = (IObjectExplorerService)Package.GetService(typeof(IObjectExplorerService));
            if (objectExplorerService != null)
            {
                var oesTreeProperty = objectExplorerService.GetType().GetProperty("Tree", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.IgnoreCase);
                if (oesTreeProperty != null)
                    return (TreeView)oesTreeProperty.GetValue(objectExplorerService, null);
            }

            return null;
        }

        private INodeInformation GetNodeInformation(TreeNode node)
        {
            INodeInformation result = null;
            IServiceProvider serviceProvider = node as IServiceProvider;
            if (serviceProvider != null)
            {
                result = (serviceProvider.GetService(typeof(INodeInformation)) as INodeInformation);
            }
            return result;
        }

        public bool GetNodeExpanding(TreeNode node)
        {
            var lazyNode = node as ILazyLoadingNode;
            if (lazyNode != null)
                return lazyNode.Expanding;
            else
                return false;
        }

        public string GetNodeUrnPath(TreeNode node)
        {
            var ni = GetNodeInformation(node);
            return GetNodeUrnPath(ni);
        }

        private string GetNodeUrnPath(INodeInformation nodeInformation)
        {
            if (nodeInformation != null)
                return nodeInformation.UrnPath;
            else
                return null;
        }

        public void ReorganizeDatabaseNodes(TreeNode node)
        {
            if (!Options.GroupDatabasesByName && !Options.SeparateReadonlyDatabases)
                return;
            if (node.Nodes.Count <= 1)
                return;
            if (NodeIsADatabaseFolder(node))
                return;
            if (!NodeIsDatabasesFolderNode(node))
                return;

            LocalizedReadonly = GetStringFromResources("FolderNameReadonly");

            node.TreeView.BeginUpdate();

            var indexForInsertNextDatabaseFolder = -1;
            var indexForInsertGlobalReadonlyDatabaseFolder = -1;

            for (var nodeIndex = 0; nodeIndex < node.Nodes.Count; nodeIndex++)
            {
                var childNode = node.Nodes[nodeIndex];

                if (NodeIsADatabaseFolder(childNode))
                {
                    continue;
                }

                var childNodeInformation = GetNodeInformation(childNode);
                var childNodeUrnPath = GetNodeUrnPath(childNodeInformation);
                if (childNodeUrnPath != "Server/Database")
                    continue;

                if (indexForInsertNextDatabaseFolder < 0)
                    indexForInsertGlobalReadonlyDatabaseFolder = indexForInsertNextDatabaseFolder = nodeIndex;

                var databaseFolder = GetDatabaseFolderNameFromNode(childNode, childNodeInformation);

                var databaseFolderNode = GetOrCreateFolderNode(node, databaseFolder, childNodeInformation, indexForInsertGlobalReadonlyDatabaseFolder, ref indexForInsertNextDatabaseFolder);
                if (databaseFolderNode == null)
                    continue;

                node.Nodes.Remove(childNode);
                databaseFolderNode.Nodes.Add(childNode);
                nodeIndex--;
            }

            node.TreeView.EndUpdate();
        }

        private bool NodeIsDatabasesFolderNode(TreeNode node)
        {
            if (node?.Parent != null)
            {
                var urnPath = GetNodeUrnPath(node);
                return urnPath == "Server/DatabasesFolder";
            }
            return false;
        }

        private bool NodeIsADatabaseFolder(TreeNode node)
        {
            return node.Tag != null && node.Tag.ToString() == DatabaseFolderNodeTag;
        }

        private String GetDatabaseFolderNameFromNode(TreeNode node, INodeInformation nodeInformation)
        {
            var ni = nodeInformation ?? GetNodeInformation(node);
            if (ni != null)
            {
                var indexOfUnderscore = ni.InvariantName.IndexOf('_');
                if (indexOfUnderscore > 0)
                    return ni.InvariantName.Substring(0, indexOfUnderscore);
            }
            return null;
        }

        private TreeNode GetOrCreateFolderNode(TreeNode rootNode, string databaseFolder, INodeInformation childNodeInformation, int indexForInsertGlobalReadonlyDatabaseFolder, ref int indexForInsertNextDatabaseFolder)
        {
            TreeNode databaseFolderNode = null;

            if (Options.GroupDatabasesByName && !String.IsNullOrEmpty(databaseFolder))
            {
                if (rootNode.Nodes.ContainsKey(databaseFolder))
                    databaseFolderNode = rootNode.Nodes[databaseFolder];
                else
                {
                    databaseFolderNode = new DatabaseFolderTreeNode(rootNode)
                    {
                        Name = databaseFolder,
                        Text = databaseFolder,
                        Tag = DatabaseFolderNodeTag,
                        ImageIndex = rootNode.ImageIndex,
                        SelectedImageIndex = rootNode.ImageIndex
                    };
                    rootNode.Nodes.Insert(indexForInsertNextDatabaseFolder, databaseFolderNode);
                    indexForInsertNextDatabaseFolder++;
                }
            }

            if (Options.SeparateReadonlyDatabases)
            {
                var readonlyValue = childNodeInformation["ReadOnly"];
                if (readonlyValue is bool && (bool) readonlyValue)
                {
                    if ((databaseFolderNode ?? rootNode).Nodes.ContainsKey(LocalizedReadonly))
                        databaseFolderNode = (databaseFolderNode ?? rootNode).Nodes[LocalizedReadonly];
                    else
                    {
                        var readonlyFolderNode = new DatabaseFolderTreeNode(rootNode)
                        {
                            Name = LocalizedReadonly,
                            Text = LocalizedReadonly,
                            Tag = DatabaseFolderNodeTag,
                            ImageIndex = rootNode.ImageIndex,
                            SelectedImageIndex = rootNode.ImageIndex
                        };
                        if (databaseFolderNode != null)
                        {
                            databaseFolderNode.Nodes.Insert(0, readonlyFolderNode);
                        }
                        else
                        {
                            rootNode.Nodes.Insert(indexForInsertGlobalReadonlyDatabaseFolder, readonlyFolderNode);
                        }
                        databaseFolderNode = readonlyFolderNode;
                    }
                }
            }
            return databaseFolderNode;
        }
    }
}
