using KeePassLib;

namespace KeePassEntrySearcherContracts
{
    public interface IPluginDataProvider
    {
        PwDatabase[] GetPwDatabases();
    }
}