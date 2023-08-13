using KeePassLib;
using System.Drawing;

namespace FluentPassFinderContracts
{
    public interface IPluginHostProxy
    {
        void CopyToClipboard(string strToCopy, bool bSprCompile, bool bIsEntryInfo, PwEntry peEntryInfo);
        Image GetBuildInIcon(PwIcon nuildInIconId); 
        PwDatabase[] GetPwDatabases();
    }
}
