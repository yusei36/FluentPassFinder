using KeePass.Util.Spr;
using KeePassLib;
using System.Drawing;

namespace FluentPassFinderContracts
{
    public interface IPluginHostProxy
    {
        void CopyToClipboard(string strToCopy, bool bSprCompile, bool bIsEntryInfo, PwEntry peEntryInfo);
        string GetPlaceholderValue(string placeholder, SprContext context);
        Image GetBuildInIcon(PwIcon nuildInIconId); 
        PwDatabase[] GetPwDatabases();
    }
}
