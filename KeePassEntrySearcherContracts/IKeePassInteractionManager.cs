using KeePassLib;
using System.Drawing;
using System.Windows.Threading;

namespace KeePassEntrySearcherContracts
{
    public interface IKeePassInteractionManager
    {
        Dispatcher PluginHostDispatcher { get; }

        void CopyToClipboard(string strToCopy, bool bSprCompile, bool bIsEntryInfo, PwEntry peEntryInfo);

        Image GetBuildInIcon(PwIcon nuildInIconId);
    }
}
