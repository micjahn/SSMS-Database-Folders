namespace SsmsDatabaseFolders
{
    using System.ComponentModel;

    using Localization;

    using Microsoft.VisualStudio.Shell;

    public class DatabaseFolderOptions : DialogPage, IDatabaseFolderOptions
    {
        [CategoryResources(nameof(DatabaseFolderOptions) + "DatabaseFolderDisplayOptions")]
        [DisplayNameResources(nameof(DatabaseFolderOptions) + nameof(GroupDatabasesByName))]
        [DescriptionResources(nameof(DatabaseFolderOptions) + nameof(GroupDatabasesByName))]
        [DefaultValue(true)]
        public bool GroupDatabasesByName { get; set; } = true;

        [CategoryResources(nameof(DatabaseFolderOptions) + "DatabaseFolderDisplayOptions")]
        [DisplayNameResources(nameof(DatabaseFolderOptions) + nameof(SeparateReadonlyDatabases))]
        [DescriptionResources(nameof(DatabaseFolderOptions) + nameof(SeparateReadonlyDatabases))]
        [DefaultValue(true)]
        public bool SeparateReadonlyDatabases { get; set; } = false;
    }
}
