
namespace SsmsDatabaseFolders
{
    public interface IDebugOutput
    {
        void debug_message(string message, params object[] args);
    }
}
