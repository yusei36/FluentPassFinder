using KeePassLib;

namespace FluentPassFinderContracts
{
    public interface IPluginDataProvider
    {
        PwDatabase[] GetPwDatabases();
    }
}