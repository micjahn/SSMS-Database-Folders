namespace SsmsDatabaseFolders
{
    using System.Collections.Generic;

    public interface IDatabaseFolderOptions
    {
        bool GroupDatabasesByName { get; }
        bool SeparateReadonlyDatabases { get; }
        List<string> RegularExpressions { get; }
    }
}
