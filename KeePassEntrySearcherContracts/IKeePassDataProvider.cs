using KeePassLib;

namespace KeePassEntrySearcherContracts
{
    public interface IKeePassDataProvider
    {
        PwDatabase[] GetPwDatabases();
    }
}