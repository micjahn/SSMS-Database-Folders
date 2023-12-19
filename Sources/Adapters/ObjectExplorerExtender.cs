namespace SsmsDatabaseFolders
{
    using System;
    using System.Collections.Generic;
    using System.Reflection;
    using System.Text.RegularExpressions;
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

        public void RefreshDatabaseNode()
        {
            var objectExplorer = (IObjectExplorerService)Package.GetService(typeof(IObjectExplorerService));
            var treeView = GetObjectExplorerTreeView();

            if (treeView != null)
            {
                foreach (TreeNode node in treeView.Nodes)
                {
                    if (RefreshDatabaseNodebyNode(objectExplorer, node))
                        break;
                }
            }
        }

        private bool RefreshDatabaseNodebyNode(IObjectExplorerService objectExplorer, TreeNode node)
        {
            if (NodeIsDatabasesFolderNode(node))
            {
                var nodeInformation = GetNodeInformation(node);
                objectExplorer.SynchronizeTree(nodeInformation);
                SendKeys.Send("{F5}");
                return true;
            }

            foreach (TreeNode childNode in node.Nodes)
            {
                if (RefreshDatabaseNodebyNode(objectExplorer, childNode))
                    return true;
            }
            return false;
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
            if (!Options.IsEnabledAndAnOptionSet)
                return;
            if (node.Nodes.Count <= 1)
                return;
            if (NodeIsADatabaseFolder(node))
                return;
            if (!NodeIsDatabasesFolderNode(node))
                return;

            LocalizedReadonly = GetStringFromResources("FolderNameReadonly");

            node.TreeView.BeginUpdate();

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

                if (indexForInsertGlobalReadonlyDatabaseFolder < 0)
                    indexForInsertGlobalReadonlyDatabaseFolder = nodeIndex;

                var databaseFolderNode = GetOrCreateFolderNode(node, childNode, childNodeInformation, indexForInsertGlobalReadonlyDatabaseFolder);
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

        private IEnumerable<String> GetDatabaseFolderNameFromNode(TreeNode node, INodeInformation nodeInformation)
        {
            var ni = nodeInformation ?? GetNodeInformation(node);
            if (ni != null)
            {
                if (Options.RegularExpressions.Count > 0)
                {
                    foreach (var pattern in Options.RegularExpressions)
                    {
                        if (String.IsNullOrEmpty(pattern))
                            continue;

                        var match = Regex.Match(ni.InvariantName, pattern, RegexOptions.IgnoreCase);
                        if (match.Groups.Count > 1)
                        {
                            for (var groupIndex = 1; groupIndex < match.Groups.Count; groupIndex++)
                            {
                                var group = match.Groups[groupIndex];
                                if (!string.IsNullOrEmpty(group.Value))
                                    yield return group.Value;
                            }
                            yield break;
                        }
                    }
                }
                else
                {
                    var indexOfUnderscore = ni.InvariantName.IndexOf('_');
                    if (indexOfUnderscore > 0)
                        yield return ni.InvariantName.Substring(0, indexOfUnderscore);
                }
            }
        }

        private TreeNode GetOrCreateFolderNode(TreeNode rootNode, TreeNode childNode, INodeInformation childNodeInformation, int indexForInsertGlobalReadonlyDatabaseFolder)
        {
            TreeNode databaseFolderNode = null;

            if (Options.GroupDatabasesByName)
            {
                var currentNode = rootNode;
                foreach (var databaseFolder in GetDatabaseFolderNameFromNode(childNode, childNodeInformation))
                {
                    var newIndex = currentNode == rootNode ? indexForInsertGlobalReadonlyDatabaseFolder : 0;
                    if (currentNode.Nodes.ContainsKey(databaseFolder))
                        databaseFolderNode = currentNode.Nodes[databaseFolder];
                    else
                    {
                        for (; newIndex < currentNode.Nodes.Count; newIndex++)
                        {
                            var nodeAtIndex = currentNode.Nodes[newIndex];
                            if ((nodeAtIndex.Name != LocalizedReadonly &&
                                String.CompareOrdinal(nodeAtIndex.Name, databaseFolder) > 0) ||
                                !NodeIsADatabaseFolder(nodeAtIndex))
                                break;
                        }
                        databaseFolderNode = new DatabaseFolderTreeNode(currentNode)
                        {
                            Name = databaseFolder,
                            Text = databaseFolder,
                            Tag = DatabaseFolderNodeTag,
                            ImageIndex = currentNode.ImageIndex,
                            SelectedImageIndex = currentNode.ImageIndex
                        };
                        currentNode.Nodes.Insert(newIndex, databaseFolderNode);
                    }
                    currentNode = databaseFolderNode;
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
