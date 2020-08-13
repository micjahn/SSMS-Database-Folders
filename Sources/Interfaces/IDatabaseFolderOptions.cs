namespace SsmsDatabaseFolders
{
    public interface IDatabaseFolderOptions
    {
        bool GroupDatabasesByName { get; }
        bool SeparateReadonlyDatabases { get; }
    }
}
