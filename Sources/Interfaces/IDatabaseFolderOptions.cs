namespace SsmsDatabaseFolders
{
    using System.Collections.Generic;

    public interface IDatabaseFolderOptions
    {
        bool IsEnabledAndAnOptionSet { get; }
        bool Enabled { get; }
        bool GroupDatabasesByName { get; }
        bool SeparateReadonlyDatabases { get; }
        List<string> RegularExpressions { get; }
        List<ICustomFolderConfiguration> CustomFolderConfigurations { get; }
    }

    public interface ICustomFolderConfiguration
    {
        string CustomFolderName { get; }

        List<string> RegularExpressions { get; }

        bool UseOtherGroupingMethodsInside { get; }
    }
}
